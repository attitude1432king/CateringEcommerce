# Database Initialization Script
# ONE-TIME EXECUTION ONLY - Checks flag before running

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,

    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,

    [Parameter(Mandatory=$true)]
    [string]$ScriptsPath,

    [Parameter(Mandatory=$true)]
    [string]$FlagFile,

    [Parameter(Mandatory=$false)]
    [string]$ConnectionString,

    [Parameter(Mandatory=$false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Initialization Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Server: $ServerName"
Write-Host "Database: $DatabaseName"
Write-Host "Scripts Path: $ScriptsPath"
Write-Host "Flag File: $FlagFile"
Write-Host ""

# Check if database has already been initialized
if ((Test-Path $FlagFile) -and -not $Force) {
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "[INFO] Database already initialized!" -ForegroundColor Yellow
    Write-Host "Flag file exists: $FlagFile" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Database initialization will be SKIPPED." -ForegroundColor Yellow
    Write-Host "This is expected behavior for redeployments." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To force re-initialization (DANGEROUS):" -ForegroundColor Yellow
    Write-Host "  1. Delete flag file: $FlagFile" -ForegroundColor Yellow
    Write-Host "  2. Re-run this script with -Force parameter" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    exit 0
}

if ($Force) {
    Write-Host "[WARNING] Force flag detected - will reinitialize database!" -ForegroundColor Red
    Write-Host "[WARNING] This will DROP ALL EXISTING DATA!" -ForegroundColor Red
    Start-Sleep -Seconds 5
}

# Validate scripts path
if (-not (Test-Path $ScriptsPath)) {
    Write-Host "[ERROR] Scripts path does not exist: $ScriptsPath" -ForegroundColor Red
    exit 1
}

# Build connection string if not provided
if (-not $ConnectionString) {
    $ConnectionString = "Server=$ServerName;Database=master;Integrated Security=True;TrustServerCertificate=True;"
}

Write-Host "[STEP 1/5] Testing database connection..." -ForegroundColor Yellow
try {
    $testQuery = "SELECT @@VERSION"
    $result = Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $testQuery -ErrorAction Stop
    Write-Host "  ✓ Connection successful" -ForegroundColor Green
    Write-Host "  ℹ SQL Server Version: $($result.Column1.Split("`n")[0])" -ForegroundColor Cyan
} catch {
    Write-Host "  [ERROR] Failed to connect to database: $_" -ForegroundColor Red
    exit 1
}

# Create database if it doesn't exist
Write-Host "[STEP 2/5] Creating database if not exists..." -ForegroundColor Yellow
try {
    $createDbQuery = @"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '$DatabaseName')
BEGIN
    CREATE DATABASE [$DatabaseName]
    PRINT 'Database created successfully'
END
ELSE
BEGIN
    PRINT 'Database already exists'
END
"@
    Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $createDbQuery -ErrorAction Stop
    Write-Host "  ✓ Database ready: $DatabaseName" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to create database: $_" -ForegroundColor Red
    exit 1
}

# Switch to application database
$AppConnectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;"

# Execute SQL scripts in order
Write-Host "[STEP 3/5] Executing SQL scripts..." -ForegroundColor Yellow

# Define script execution order
$scriptOrder = @(
    "mastersql.sql",
    "Admin_Tables_Schema.sql",
    "RBAC_Schema.sql",
    "Admin_RBAC_Migration.sql",
    "OrderManagementTables.sql",
    "PartnerApprovalSystem_Tables.sql",
    "Split_Payment_Schema.sql",
    "Supervisor_Management_Schema.sql",
    "InApp_Notifications_Migration.sql",
    "Security_2FA_OAuth_Schema.sql",
    "Wishlist_Feature_Schema.sql",
    "Review_System_Enhancement_Migration.sql",
    "Settings_And_Config_Migration.sql"
)

$executedScripts = 0
$failedScripts = 0

foreach ($scriptName in $scriptOrder) {
    $scriptPath = Join-Path $ScriptsPath $scriptName

    if (Test-Path $scriptPath) {
        Write-Host "  Executing: $scriptName" -ForegroundColor Cyan
        try {
            Invoke-Sqlcmd -ConnectionString $AppConnectionString -InputFile $scriptPath -ErrorAction Stop -QueryTimeout 300
            Write-Host "    ✓ Success" -ForegroundColor Green
            $executedScripts++
        } catch {
            Write-Host "    ✗ Failed: $_" -ForegroundColor Red
            $failedScripts++
        }
    } else {
        Write-Host "  ⊘ Skipped: $scriptName (not found)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "  Summary:" -ForegroundColor Cyan
Write-Host "    Executed: $executedScripts" -ForegroundColor Green
Write-Host "    Failed: $failedScripts" -ForegroundColor $(if ($failedScripts -gt 0) { "Red" } else { "Gray" })

if ($failedScripts -gt 0) {
    Write-Host ""
    Write-Host "  [WARNING] Some scripts failed to execute!" -ForegroundColor Yellow
    Write-Host "  Review errors above and fix manually if needed." -ForegroundColor Yellow
}

# Execute stored procedure scripts
Write-Host "[STEP 4/5] Executing stored procedures..." -ForegroundColor Yellow
$spScripts = Get-ChildItem -Path $ScriptsPath -Filter "*StoredProcedures.sql" | Select-Object -ExpandProperty Name

if ($spScripts.Count -gt 0) {
    foreach ($spScript in $spScripts) {
        $scriptPath = Join-Path $ScriptsPath $spScript
        Write-Host "  Executing: $spScript" -ForegroundColor Cyan
        try {
            Invoke-Sqlcmd -ConnectionString $AppConnectionString -InputFile $scriptPath -ErrorAction Stop -QueryTimeout 300
            Write-Host "    ✓ Success" -ForegroundColor Green
        } catch {
            Write-Host "    ✗ Failed: $_" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "  ⊘ No stored procedure scripts found" -ForegroundColor Yellow
}

# Create deployment history table and mark initialization complete
Write-Host "[STEP 5/5] Marking database as initialized..." -ForegroundColor Yellow
try {
    $markInitializedQuery = @"
-- Create deployment history table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeploymentHistory')
BEGIN
    CREATE TABLE DeploymentHistory (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EventType NVARCHAR(50) NOT NULL,
        Version NVARCHAR(50),
        ExecutedBy NVARCHAR(100),
        ExecutedAt DATETIME DEFAULT GETDATE(),
        Notes NVARCHAR(MAX)
    )
END

-- Insert initialization record
INSERT INTO DeploymentHistory (EventType, Version, ExecutedBy, Notes)
VALUES (
    'DATABASE_INITIALIZED',
    '1.0.0',
    SYSTEM_USER,
    'Initial database setup completed via CI/CD pipeline'
)
"@
    Invoke-Sqlcmd -ConnectionString $AppConnectionString -Query $markInitializedQuery -ErrorAction Stop
    Write-Host "  ✓ Database marked as initialized" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to mark database as initialized: $_" -ForegroundColor Red
    exit 1
}

# Create flag file to prevent re-execution
Write-Host "  Creating flag file..." -ForegroundColor Yellow
try {
    $flagDir = Split-Path $FlagFile -Parent
    if (-not (Test-Path $flagDir)) {
        New-Item -Path $flagDir -ItemType Directory -Force | Out-Null
    }

    $flagContent = @"
Database Initialization Flag File
Created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Server: $ServerName
Database: $DatabaseName
Scripts Executed: $executedScripts
Scripts Failed: $failedScripts

DO NOT DELETE THIS FILE!
This file prevents database re-initialization on subsequent deployments.
"@
    $flagContent | Set-Content -Path $FlagFile -Force
    Write-Host "  ✓ Flag file created: $FlagFile" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to create flag file: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Database Initialization Completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Server: $ServerName" -ForegroundColor Green
Write-Host "Database: $DatabaseName" -ForegroundColor Green
Write-Host "Scripts Executed: $executedScripts" -ForegroundColor Green
Write-Host "Flag File: $FlagFile" -ForegroundColor Green
Write-Host ""
Write-Host "⚠ IMPORTANT: Database will NOT be reinitialized" -ForegroundColor Yellow
Write-Host "  on future deployments unless flag file is deleted." -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Green
