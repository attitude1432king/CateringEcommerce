# Database Initialization Check
# Returns whether database needs initialization

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,

    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,

    [Parameter(Mandatory=$true)]
    [string]$FlagFile
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Initialization Check" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Server: $ServerName"
Write-Host "Database: $DatabaseName"
Write-Host "Flag File: $FlagFile"
Write-Host ""

# Check 1: Flag file exists?
$flagFileExists = Test-Path $FlagFile

Write-Host "[CHECK 1/3] Flag file check..." -ForegroundColor Yellow
if ($flagFileExists) {
    Write-Host "  ✓ Flag file exists: $FlagFile" -ForegroundColor Green
    Write-Host "  → Database has been initialized" -ForegroundColor Cyan
    $needsInit = $false
} else {
    Write-Host "  ✗ Flag file not found" -ForegroundColor Yellow
    Write-Host "  → Database needs initialization" -ForegroundColor Cyan
    $needsInit = $true
}

# Check 2: Database exists?
Write-Host "[CHECK 2/3] Database existence check..." -ForegroundColor Yellow
try {
    $connStr = "Server=$ServerName;Database=master;Integrated Security=True;TrustServerCertificate=True;"
    $query = "SELECT COUNT(*) AS DbExists FROM sys.databases WHERE name = '$DatabaseName'"
    $result = Invoke-Sqlcmd -ConnectionString $connStr -Query $query -ErrorAction Stop

    if ($result.DbExists -eq 1) {
        Write-Host "  ✓ Database exists: $DatabaseName" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Database does not exist" -ForegroundColor Yellow
        $needsInit = $true
    }
} catch {
    Write-Host "  ✗ Cannot connect to SQL Server: $_" -ForegroundColor Red
    Write-Host "  → Assuming database needs initialization" -ForegroundColor Yellow
    $needsInit = $true
}

# Check 3: DeploymentHistory table exists? (if database exists)
if (-not $needsInit) {
    Write-Host "[CHECK 3/3] Deployment history check..." -ForegroundColor Yellow
    try {
        $appConnStr = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;"
        $query = "SELECT COUNT(*) AS TableExists FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DeploymentHistory'"
        $result = Invoke-Sqlcmd -ConnectionString $appConnStr -Query $query -ErrorAction Stop

        if ($result.TableExists -eq 1) {
            Write-Host "  ✓ DeploymentHistory table found" -ForegroundColor Green

            # Check for initialization record
            $initQuery = "SELECT COUNT(*) AS InitCount FROM DeploymentHistory WHERE EventType = 'DATABASE_INITIALIZED'"
            $initResult = Invoke-Sqlcmd -ConnectionString $appConnStr -Query $initQuery -ErrorAction Stop

            if ($initResult.InitCount -gt 0) {
                Write-Host "  ✓ Initialization record found" -ForegroundColor Green
                $needsInit = $false
            } else {
                Write-Host "  ✗ No initialization record found" -ForegroundColor Yellow
                $needsInit = $true
            }
        } else {
            Write-Host "  ✗ DeploymentHistory table not found" -ForegroundColor Yellow
            $needsInit = $true
        }
    } catch {
        Write-Host "  ✗ Error checking deployment history: $_" -ForegroundColor Yellow
        $needsInit = $true
    }
} else {
    Write-Host "[CHECK 3/3] Skipped (database doesn't exist)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Check Result:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($needsInit) {
    Write-Host "✗ DATABASE NEEDS INITIALIZATION" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Database initialization script will run" -ForegroundColor Cyan
    Write-Host "  2. All schema and data will be created" -ForegroundColor Cyan
    Write-Host "  3. Flag file will be created" -ForegroundColor Cyan
    Write-Host ""

    # Set Azure DevOps pipeline variable
    Write-Host "##vso[task.setvariable variable=NeedsInitialization;isOutput=true]true"
} else {
    Write-Host "✓ DATABASE ALREADY INITIALIZED" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Database initialization will be SKIPPED" -ForegroundColor Cyan
    Write-Host "  2. Only application files will be deployed" -ForegroundColor Cyan
    Write-Host ""

    # Set Azure DevOps pipeline variable
    Write-Host "##vso[task.setvariable variable=NeedsInitialization;isOutput=true]false"
}

Write-Host "========================================" -ForegroundColor Cyan

# Exit with success (non-zero exit code would fail pipeline)
exit 0
