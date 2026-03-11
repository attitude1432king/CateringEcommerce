# Backend Deployment to IIS
# Deploys ASP.NET Core 9.0 API to IIS

param(
    [Parameter(Mandatory=$true)]
    [string]$SourcePath,

    [Parameter(Mandatory=$true)]
    [string]$DestinationPath,

    [Parameter(Mandatory=$true)]
    [string]$AppPoolName,

    [Parameter(Mandatory=$true)]
    [string]$SiteName,

    [Parameter(Mandatory=$true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backend Deployment to IIS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Source: $SourcePath"
Write-Host "Destination: $DestinationPath"
Write-Host "App Pool: $AppPoolName"
Write-Host "Site: $SiteName"
Write-Host "Version: $Version"
Write-Host ""

# Import WebAdministration module
Import-Module WebAdministration -ErrorAction Stop

# Validate source path
if (-not (Test-Path $SourcePath)) {
    Write-Host "[ERROR] Source path does not exist: $SourcePath" -ForegroundColor Red
    exit 1
}

# Create destination directory if it doesn't exist
if (-not (Test-Path $DestinationPath)) {
    Write-Host "[INFO] Creating destination directory..." -ForegroundColor Yellow
    New-Item -Path $DestinationPath -ItemType Directory -Force | Out-Null
}

# Stop application pool
Write-Host "[STEP 1/5] Stopping application pool: $AppPoolName" -ForegroundColor Yellow
try {
    if (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Stop-WebAppPool -Name $AppPoolName -ErrorAction Stop

        # Wait for pool to stop
        $timeout = 30
        $elapsed = 0
        while ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Stopped" -and $elapsed -lt $timeout) {
            Start-Sleep -Seconds 2
            $elapsed += 2
        }

        Write-Host "  ✓ Application pool stopped" -ForegroundColor Green
    } else {
        Write-Host "  ! Application pool not found, will be created" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ! Failed to stop application pool: $_" -ForegroundColor Yellow
}

# Clear destination (keep web.config and appsettings if they exist)
Write-Host "[STEP 2/5] Clearing destination directory..." -ForegroundColor Yellow
try {
    # Backup configuration files
    $webConfigBackup = $null
    $appSettingsBackup = $null

    if (Test-Path "$DestinationPath\web.config") {
        $webConfigBackup = Get-Content "$DestinationPath\web.config" -Raw
    }

    if (Test-Path "$DestinationPath\appsettings.Production.json") {
        $appSettingsBackup = Get-Content "$DestinationPath\appsettings.Production.json" -Raw
    }

    # Remove all files except logs
    Get-ChildItem -Path $DestinationPath -Recurse -Exclude "logs" | Remove-Item -Recurse -Force

    Write-Host "  ✓ Directory cleared" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to clear directory: $_" -ForegroundColor Red
    exit 1
}

# Copy new files
Write-Host "[STEP 3/5] Copying new files..." -ForegroundColor Yellow
try {
    Copy-Item -Path "$SourcePath\*" -Destination $DestinationPath -Recurse -Force
    Write-Host "  ✓ Files copied successfully" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to copy files: $_" -ForegroundColor Red
    exit 1
}

# Restore configuration files if they were backed up
if ($webConfigBackup) {
    $webConfigBackup | Set-Content "$DestinationPath\web.config" -Force
    Write-Host "  ✓ Restored web.config" -ForegroundColor Green
}

if ($appSettingsBackup) {
    $appSettingsBackup | Set-Content "$DestinationPath\appsettings.Production.json" -Force
    Write-Host "  ✓ Restored appsettings.Production.json" -ForegroundColor Green
}

# Ensure web.config exists for ASP.NET Core
Write-Host "[STEP 4/5] Ensuring web.config exists..." -ForegroundColor Yellow
if (-not (Test-Path "$DestinationPath\web.config")) {
    $webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\CateringEcommerce.API.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
      <security>
        <requestFiltering>
          <requestLimits maxAllowedContentLength="524288000" />
        </requestFiltering>
      </security>
    </system.webServer>
  </location>
</configuration>
"@
    $webConfigContent | Set-Content "$DestinationPath\web.config" -Force
    Write-Host "  ✓ web.config created" -ForegroundColor Green
}

# Create logs directory
if (-not (Test-Path "$DestinationPath\logs")) {
    New-Item -Path "$DestinationPath\logs" -ItemType Directory -Force | Out-Null
}

# Set permissions (IIS_IUSRS needs read/execute)
Write-Host "  Setting permissions..." -ForegroundColor Yellow
$acl = Get-Acl $DestinationPath
$permission = "IIS_IUSRS", "ReadAndExecute", "ContainerInherit, ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $DestinationPath $acl
Write-Host "  ✓ Permissions set" -ForegroundColor Green

# Start application pool
Write-Host "[STEP 5/5] Starting application pool: $AppPoolName" -ForegroundColor Yellow
try {
    Start-WebAppPool -Name $AppPoolName -ErrorAction Stop

    # Wait for pool to start
    $timeout = 30
    $elapsed = 0
    while ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Started" -and $elapsed -lt $timeout) {
        Start-Sleep -Seconds 2
        $elapsed += 2
    }

    if ((Get-WebAppPoolState -Name $AppPoolName).Value -eq "Started") {
        Write-Host "  ✓ Application pool started" -ForegroundColor Green
    } else {
        Write-Host "  [ERROR] Failed to start application pool" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "  [ERROR] Failed to start application pool: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Backend Deployment Completed!" -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
