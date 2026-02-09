# Backup Current Deployment
# Creates versioned backup before deployment for rollback capability

param(
    [Parameter(Mandatory=$true)]
    [string]$ApiPath,

    [Parameter(Mandatory=$true)]
    [string]$WebPath,

    [Parameter(Mandatory=$true)]
    [string]$BackupPath,

    [Parameter(Mandatory=$true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Pre-Deployment Backup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "API Path: $ApiPath"
Write-Host "Web Path: $WebPath"
Write-Host "Backup Path: $BackupPath"
Write-Host "Version: $Version"
Write-Host ""

# Create backup directory if not exists
if (-not (Test-Path $BackupPath)) {
    Write-Host "[INFO] Creating backup directory..." -ForegroundColor Yellow
    New-Item -Path $BackupPath -ItemType Directory -Force | Out-Null
    Write-Host "  ✓ Backup directory created" -ForegroundColor Green
}

# Create timestamp for backup
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$backupName = "backup_v${Version}_${timestamp}"
$backupFullPath = Join-Path $BackupPath $backupName

Write-Host "[STEP 1/3] Creating backup directory: $backupName" -ForegroundColor Yellow
New-Item -Path $backupFullPath -ItemType Directory -Force | Out-Null
Write-Host "  ✓ Backup directory created" -ForegroundColor Green

# Backup API
Write-Host "[STEP 2/3] Backing up API..." -ForegroundColor Yellow
if (Test-Path $ApiPath) {
    try {
        $apiBackupPath = Join-Path $backupFullPath "api"
        Copy-Item -Path $ApiPath -Destination $apiBackupPath -Recurse -Force

        # Get folder size
        $apiSize = (Get-ChildItem -Path $apiBackupPath -Recurse | Measure-Object -Property Length -Sum).Sum
        $apiSizeMB = [math]::Round($apiSize / 1MB, 2)

        Write-Host "  ✓ API backed up successfully" -ForegroundColor Green
        Write-Host "  ℹ Size: ${apiSizeMB} MB" -ForegroundColor Cyan
    } catch {
        Write-Host "  [ERROR] Failed to backup API: $_" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "  ⊘ API path not found, skipping" -ForegroundColor Yellow
}

# Backup Web
Write-Host "[STEP 3/3] Backing up Web..." -ForegroundColor Yellow
if (Test-Path $WebPath) {
    try {
        $webBackupPath = Join-Path $backupFullPath "web"
        Copy-Item -Path $WebPath -Destination $webBackupPath -Recurse -Force

        # Get folder size
        $webSize = (Get-ChildItem -Path $webBackupPath -Recurse | Measure-Object -Property Length -Sum).Sum
        $webSizeMB = [math]::Round($webSize / 1MB, 2)

        Write-Host "  ✓ Web backed up successfully" -ForegroundColor Green
        Write-Host "  ℹ Size: ${webSizeMB} MB" -ForegroundColor Cyan
    } catch {
        Write-Host "  [ERROR] Failed to backup Web: $_" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "  ⊘ Web path not found, skipping" -ForegroundColor Yellow
}

# Create backup metadata file
Write-Host ""
Write-Host "Creating backup metadata..." -ForegroundColor Yellow
$metadata = @"
Backup Metadata
===============
Version: $Version
Timestamp: $timestamp
Created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

Paths Backed Up:
- API: $ApiPath
- Web: $WebPath

Backup Location: $backupFullPath

Restore Instructions:
---------------------
To restore this backup, run:
.\rollback.ps1 -BackupName "$backupName" -Confirm

Or use the rollback script with the version number:
.\rollback.ps1 -Version "$Version" -Confirm
"@

$metadataPath = Join-Path $backupFullPath "backup_metadata.txt"
$metadata | Set-Content -Path $metadataPath -Force
Write-Host "  ✓ Metadata file created" -ForegroundColor Green

# Cleanup old backups (keep last 10)
Write-Host ""
Write-Host "Cleaning up old backups..." -ForegroundColor Yellow
try {
    $allBackups = Get-ChildItem -Path $BackupPath -Directory | Where-Object { $_.Name -like "backup_*" } | Sort-Object -Property CreationTime -Descending

    if ($allBackups.Count -gt 10) {
        $backupsToDelete = $allBackups | Select-Object -Skip 10

        foreach ($backup in $backupsToDelete) {
            Write-Host "  Deleting old backup: $($backup.Name)" -ForegroundColor Gray
            Remove-Item -Path $backup.FullName -Recurse -Force
        }

        Write-Host "  ✓ Cleaned up $($backupsToDelete.Count) old backup(s)" -ForegroundColor Green
    } else {
        Write-Host "  ℹ No old backups to clean (total: $($allBackups.Count))" -ForegroundColor Cyan
    }
} catch {
    Write-Host "  [WARNING] Failed to cleanup old backups: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Backup Completed Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Backup Name: $backupName" -ForegroundColor Green
Write-Host "Backup Path: $backupFullPath" -ForegroundColor Green
Write-Host ""
Write-Host "This backup can be used for rollback if deployment fails." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green
