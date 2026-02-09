# IIS Initial Setup Script
# Creates IIS sites and application pools for first-time deployment

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Production",

    [Parameter(Mandatory=$false)]
    [string]$ApiPath = "C:\inetpub\wwwroot\CateringAPI",

    [Parameter(Mandatory=$false)]
    [string]$WebPath = "C:\inetpub\wwwroot\CateringWeb",

    [Parameter(Mandatory=$false)]
    [int]$ApiPort = 5000,

    [Parameter(Mandatory=$false)]
    [int]$WebPort = 80,

    [Parameter(Mandatory=$false)]
    [int]$WebPortHttps = 443
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "IIS Initial Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Environment: $Environment"
Write-Host "API Path: $ApiPath"
Write-Host "Web Path: $WebPath"
Write-Host "API Port: $ApiPort"
Write-Host "Web Port: $WebPort (HTTP), $WebPortHttps (HTTPS)"
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "[ERROR] This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Import WebAdministration module
Write-Host "[STEP 1/8] Loading IIS module..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration -ErrorAction Stop
    Write-Host "  ✓ IIS module loaded" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to load IIS module. Is IIS installed?" -ForegroundColor Red
    Write-Host "  Install IIS using: Install-WindowsFeature -name Web-Server -IncludeManagementTools" -ForegroundColor Yellow
    exit 1
}

# Create directories
Write-Host "[STEP 2/8] Creating directories..." -ForegroundColor Yellow
foreach ($path in @($ApiPath, $WebPath)) {
    if (-not (Test-Path $path)) {
        New-Item -Path $path -ItemType Directory -Force | Out-Null
        Write-Host "  ✓ Created: $path" -ForegroundColor Green
    } else {
        Write-Host "  ℹ Already exists: $path" -ForegroundColor Cyan
    }
}

# Create API Application Pool
Write-Host "[STEP 3/8] Creating API Application Pool..." -ForegroundColor Yellow
$apiPoolName = "CateringEcommerce_API_Pool"

if (Test-Path "IIS:\AppPools\$apiPoolName") {
    Write-Host "  ℹ App Pool already exists: $apiPoolName" -ForegroundColor Cyan
    Stop-WebAppPool -Name $apiPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
} else {
    New-WebAppPool -Name $apiPoolName -Force | Out-Null
    Write-Host "  ✓ Created: $apiPoolName" -ForegroundColor Green
}

# Configure API App Pool
Set-ItemProperty "IIS:\AppPools\$apiPoolName" -Name managedRuntimeVersion -Value ""  # No Managed Code for .NET Core
Set-ItemProperty "IIS:\AppPools\$apiPoolName" -Name managedPipelineMode -Value "Integrated"
Set-ItemProperty "IIS:\AppPools\$apiPoolName" -Name startMode -Value "AlwaysRunning"
Set-ItemProperty "IIS:\AppPools\$apiPoolName" -Name processModel.idleTimeout -Value ([TimeSpan]::FromMinutes(20))
Write-Host "  ✓ Configured API App Pool" -ForegroundColor Green

# Create Web Application Pool
Write-Host "[STEP 4/8] Creating Web Application Pool..." -ForegroundColor Yellow
$webPoolName = "CateringEcommerce_Web_Pool"

if (Test-Path "IIS:\AppPools\$webPoolName") {
    Write-Host "  ℹ App Pool already exists: $webPoolName" -ForegroundColor Cyan
    Stop-WebAppPool -Name $webPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
} else {
    New-WebAppPool -Name $webPoolName -Force | Out-Null
    Write-Host "  ✓ Created: $webPoolName" -ForegroundColor Green
}

# Configure Web App Pool
Set-ItemProperty "IIS:\AppPools\$webPoolName" -Name managedRuntimeVersion -Value ""  # No Managed Code for static site
Set-ItemProperty "IIS:\AppPools\$webPoolName" -Name managedPipelineMode -Value "Integrated"
Write-Host "  ✓ Configured Web App Pool" -ForegroundColor Green

# Create API Site
Write-Host "[STEP 5/8] Creating API Site..." -ForegroundColor Yellow
$apiSiteName = "CateringEcommerce_API"

if (Get-Website -Name $apiSiteName -ErrorAction SilentlyContinue) {
    Write-Host "  ℹ Site already exists: $apiSiteName" -ForegroundColor Cyan
    Remove-Website -Name $apiSiteName -ErrorAction SilentlyContinue
}

New-Website -Name $apiSiteName `
    -PhysicalPath $ApiPath `
    -ApplicationPool $apiPoolName `
    -Port $ApiPort `
    -Force | Out-Null

Write-Host "  ✓ Created API Site on port $ApiPort" -ForegroundColor Green

# Create Web Site
Write-Host "[STEP 6/8] Creating Web Site..." -ForegroundColor Yellow
$webSiteName = "CateringEcommerce_Web"

if (Get-Website -Name $webSiteName -ErrorAction SilentlyContinue) {
    Write-Host "  ℹ Site already exists: $webSiteName" -ForegroundColor Cyan
    Remove-Website -Name $webSiteName -ErrorAction SilentlyContinue
}

New-Website -Name $webSiteName `
    -PhysicalPath $WebPath `
    -ApplicationPool $webPoolName `
    -Port $WebPort `
    -Force | Out-Null

Write-Host "  ✓ Created Web Site on port $WebPort" -ForegroundColor Green

# Set permissions
Write-Host "[STEP 7/8] Setting permissions..." -ForegroundColor Yellow

foreach ($path in @($ApiPath, $WebPath)) {
    $acl = Get-Acl $path

    # Add IIS_IUSRS
    $permission = "IIS_IUSRS", "ReadAndExecute", "ContainerInherit, ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.AddAccessRule($accessRule)

    # Add IUSR (for static content)
    $permission = "IUSR", "Read", "ContainerInherit, ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.AddAccessRule($accessRule)

    Set-Acl $path $acl
}

Write-Host "  ✓ Permissions set" -ForegroundColor Green

# Install URL Rewrite Module (if not installed)
Write-Host "[STEP 8/8] Checking URL Rewrite Module..." -ForegroundColor Yellow
$urlRewrite = Get-WindowsFeature -Name Web-Url-Rewrite -ErrorAction SilentlyContinue

if ($urlRewrite -and -not $urlRewrite.Installed) {
    Write-Host "  ⚠ URL Rewrite Module not installed" -ForegroundColor Yellow
    Write-Host "  ℹ Download from: https://www.iis.net/downloads/microsoft/url-rewrite" -ForegroundColor Cyan
} else {
    Write-Host "  ✓ URL Rewrite Module is available" -ForegroundColor Green
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "IIS Setup Completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Created Resources:" -ForegroundColor Cyan
Write-Host "  App Pools:" -ForegroundColor White
Write-Host "    - $apiPoolName (.NET Core)" -ForegroundColor Gray
Write-Host "    - $webPoolName (Static Site)" -ForegroundColor Gray
Write-Host ""
Write-Host "  Sites:" -ForegroundColor White
Write-Host "    - $apiSiteName (Port $ApiPort)" -ForegroundColor Gray
Write-Host "      Path: $ApiPath" -ForegroundColor Gray
Write-Host "    - $webSiteName (Port $WebPort)" -ForegroundColor Gray
Write-Host "      Path: $WebPath" -ForegroundColor Gray
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Configure SSL certificates for HTTPS" -ForegroundColor Gray
Write-Host "  2. Update DNS to point to this server" -ForegroundColor Gray
Write-Host "  3. Run database initialization: .\db-init.ps1" -ForegroundColor Gray
Write-Host "  4. Deploy application files via CI/CD pipeline" -ForegroundColor Gray
Write-Host ""
Write-Host "⚠ Sites are stopped by default - they will start after first deployment" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Green
