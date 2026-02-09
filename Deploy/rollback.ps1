# Rollback to Previous Version
# Restores site from backup

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiPath = "C:\inetpub\wwwroot\CateringAPI",

    [Parameter(Mandatory=$false)]
    [string]$WebPath = "C:\inetpub\wwwroot\CateringWeb",

    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "C:\Deployments\Backups",

    [Parameter(Mandatory=$false)]
    [string]$ApiPool = "CateringEcommerce_API_Pool",

    [Parameter(Mandatory=$false)]
    [string]$WebPool = "CateringEcommerce_Web_Pool",

    [Parameter(Mandatory=$false)]
    [string]$Version,

    [Parameter(Mandatory=$false)]
    [string]$BackupName,

    [Parameter(Mandatory=$false)]
    [switch]$Previous,

    [Parameter(Mandatory=$false)]
    [switch]$Confirm
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Red
Write-Host "ROLLBACK TO PREVIOUS VERSION" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "⚠ WARNING: This will restore a previous deployment" -ForegroundColor Yellow
Write-Host "⚠ All current changes will be LOST" -ForegroundColor Yellow
Write-Host ""

# Import WebAdministration module
Import-Module WebAdministration -ErrorAction Stop

# Determine which backup to restore
$backupToRestore = $null

if ($Previous) {
    Write-Host "[INFO] Searching for most recent backup..." -ForegroundColor Cyan
    $allBackups = Get-ChildItem -Path $BackupPath -Directory | Where-Object { $_.Name -like "backup_*" } | Sort-Object -Property CreationTime -Descending

    if ($allBackups.Count -eq 0) {
        Write-Host "[ERROR] No backups found in: $BackupPath" -ForegroundColor Red
        exit 1
    }

    $backupToRestore = $allBackups[0]
    Write-Host "  ✓ Found backup: $($backupToRestore.Name)" -ForegroundColor Green
}
elseif ($BackupName) {
    Write-Host "[INFO] Looking for backup: $BackupName" -ForegroundColor Cyan
    $backupToRestore = Get-Item -Path (Join-Path $BackupPath $BackupName) -ErrorAction SilentlyContinue

    if (-not $backupToRestore) {
        Write-Host "[ERROR] Backup not found: $BackupName" -ForegroundColor Red
        exit 1
    }

    Write-Host "  ✓ Backup found" -ForegroundColor Green
}
elseif ($Version) {
    Write-Host "[INFO] Searching for backup with version: $Version" -ForegroundColor Cyan
    $allBackups = Get-ChildItem -Path $BackupPath -Directory | Where-Object { $_.Name -like "backup_v${Version}_*" } | Sort-Object -Property CreationTime -Descending

    if ($allBackups.Count -eq 0) {
        Write-Host "[ERROR] No backup found for version: $Version" -ForegroundColor Red
        exit 1
    }

    $backupToRestore = $allBackups[0]
    Write-Host "  ✓ Found backup: $($backupToRestore.Name)" -ForegroundColor Green
}
else {
    Write-Host "[ERROR] Must specify -Previous, -BackupName, or -Version" -ForegroundColor Red
    Write-Host ""
    Write-Host "Usage examples:" -ForegroundColor Cyan
    Write-Host "  .\rollback.ps1 -Previous -Confirm" -ForegroundColor Gray
    Write-Host "  .\rollback.ps1 -Version `"1.0.45`" -Confirm" -ForegroundColor Gray
    Write-Host "  .\rollback.ps1 -BackupName `"backup_v1.0.45_2026-02-06_10-30-00`" -Confirm" -ForegroundColor Gray
    exit 1
}

# Display backup info
Write-Host ""
Write-Host "Backup to restore:" -ForegroundColor Cyan
Write-Host "  Name: $($backupToRestore.Name)" -ForegroundColor White
Write-Host "  Created: $($backupToRestore.CreationTime)" -ForegroundColor White
Write-Host "  Path: $($backupToRestore.FullName)" -ForegroundColor White

# Check if metadata file exists
$metadataPath = Join-Path $backupToRestore.FullName "backup_metadata.txt"
if (Test-Path $metadataPath) {
    Write-Host ""
    Write-Host "Backup Metadata:" -ForegroundColor Cyan
    Get-Content $metadataPath | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
}

# Confirmation prompt
if (-not $Confirm) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "⚠ CONFIRMATION REQUIRED" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "Add -Confirm parameter to proceed with rollback" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Example:" -ForegroundColor Cyan
    Write-Host "  .\rollback.ps1 -Previous -Confirm" -ForegroundColor Gray
    Write-Host ""
    exit 0
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "Starting Rollback..." -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

# Stop IIS sites and pools
Write-Host "[STEP 1/4] Stopping IIS sites and app pools..." -ForegroundColor Yellow
try {
    if (Get-WebAppPoolState -Name $ApiPool -ErrorAction SilentlyContinue) {
        Stop-WebAppPool -Name $ApiPool
        Write-Host "  ✓ Stopped: $ApiPool" -ForegroundColor Green
    }

    if (Get-WebAppPoolState -Name $WebPool -ErrorAction SilentlyContinue) {
        Stop-WebAppPool -Name $WebPool
        Write-Host "  ✓ Stopped: $WebPool" -ForegroundColor Green
    }

    Start-Sleep -Seconds 5
} catch {
    Write-Host "  [WARNING] Error stopping pools: $_" -ForegroundColor Yellow
}

# Restore API
Write-Host "[STEP 2/4] Restoring API from backup..." -ForegroundColor Yellow
$apiBackupPath = Join-Path $backupToRestore.FullName "api"

if (Test-Path $apiBackupPath) {
    try {
        # Clear current API directory
        if (Test-Path $ApiPath) {
            Remove-Item -Path "$ApiPath\*" -Recurse -Force
        }

        # Restore from backup
        Copy-Item -Path "$apiBackupPath\*" -Destination $ApiPath -Recurse -Force
        Write-Host "  ✓ API restored successfully" -ForegroundColor Green
    } catch {
        Write-Host "  [ERROR] Failed to restore API: $_" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "  ⊘ API backup not found, skipping" -ForegroundColor Yellow
}

# Restore Web
Write-Host "[STEP 3/4] Restoring Web from backup..." -ForegroundColor Yellow
$webBackupPath = Join-Path $backupToRestore.FullName "web"

if (Test-Path $webBackupPath) {
    try {
        # Clear current Web directory
        if (Test-Path $WebPath) {
            Remove-Item -Path "$WebPath\*" -Recurse -Force
        }

        # Restore from backup
        Copy-Item -Path "$webBackupPath\*" -Destination $WebPath -Recurse -Force
        Write-Host "  ✓ Web restored successfully" -ForegroundColor Green
    } catch {
        Write-Host "  [ERROR] Failed to restore Web: $_" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "  ⊘ Web backup not found, skipping" -ForegroundColor Yellow
}

# Start IIS sites and pools
Write-Host "[STEP 4/4] Starting IIS sites and app pools..." -ForegroundColor Yellow
try {
    Start-WebAppPool -Name $ApiPool
    Write-Host "  ✓ Started: $ApiPool" -ForegroundColor Green

    Start-WebAppPool -Name $WebPool
    Write-Host "  ✓ Started: $WebPool" -ForegroundColor Green

    Start-Sleep -Seconds 5

    # Verify pools are running
    $apiState = (Get-WebAppPoolState -Name $ApiPool).Value
    $webState = (Get-WebAppPoolState -Name $WebPool).Value

    if ($apiState -eq "Started" -and $webState -eq "Started") {
        Write-Host "  ✓ All services started successfully" -ForegroundColor Green
    } else {
        Write-Host "  [WARNING] Some services may not have started correctly" -ForegroundColor Yellow
        Write-Host "    API Pool State: $apiState" -ForegroundColor Yellow
        Write-Host "    Web Pool State: $webState" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  [ERROR] Failed to start services: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Rollback Completed Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Restored Backup: $($backupToRestore.Name)" -ForegroundColor Green
Write-Host ""
Write-Host "⚠ IMPORTANT: Verify the site is working correctly" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Green
