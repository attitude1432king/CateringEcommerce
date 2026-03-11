# Frontend Deployment to IIS
# Deploys React + Vite Static Site to IIS

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
Write-Host "Frontend Deployment to IIS" -ForegroundColor Cyan
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

# Validate that index.html exists in source
if (-not (Test-Path "$SourcePath\index.html")) {
    Write-Host "[ERROR] index.html not found in source path" -ForegroundColor Red
    exit 1
}

# Create destination directory if it doesn't exist
if (-not (Test-Path $DestinationPath)) {
    Write-Host "[INFO] Creating destination directory..." -ForegroundColor Yellow
    New-Item -Path $DestinationPath -ItemType Directory -Force | Out-Null
}

# Stop website and app pool
Write-Host "[STEP 1/4] Stopping IIS site and app pool..." -ForegroundColor Yellow
try {
    if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
        Stop-Website -Name $SiteName -ErrorAction Stop
        Write-Host "  ✓ Website stopped" -ForegroundColor Green
    } else {
        Write-Host "  ! Website not found, will be created" -ForegroundColor Yellow
    }

    if (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Stop-WebAppPool -Name $AppPoolName -ErrorAction Stop
        Start-Sleep -Seconds 3
        Write-Host "  ✓ App pool stopped" -ForegroundColor Green
    } else {
        Write-Host "  ! App pool not found, will be created" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ! Failed to stop services: $_" -ForegroundColor Yellow
}

# Clear destination directory
Write-Host "[STEP 2/4] Clearing destination directory..." -ForegroundColor Yellow
try {
    # Backup web.config if it exists
    $webConfigBackup = $null
    if (Test-Path "$DestinationPath\web.config") {
        $webConfigBackup = Get-Content "$DestinationPath\web.config" -Raw
    }

    # Remove all files
    Get-ChildItem -Path $DestinationPath -Recurse | Remove-Item -Recurse -Force

    Write-Host "  ✓ Directory cleared" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to clear directory: $_" -ForegroundColor Red
    exit 1
}

# Copy new files
Write-Host "[STEP 3/4] Copying new files..." -ForegroundColor Yellow
try {
    Copy-Item -Path "$SourcePath\*" -Destination $DestinationPath -Recurse -Force
    Write-Host "  ✓ Files copied successfully" -ForegroundColor Green

    # Count files
    $fileCount = (Get-ChildItem -Path $DestinationPath -Recurse -File).Count
    Write-Host "  ℹ Total files: $fileCount" -ForegroundColor Cyan
} catch {
    Write-Host "  [ERROR] Failed to copy files: $_" -ForegroundColor Red
    exit 1
}

# Restore or create web.config for React SPA
Write-Host "  Configuring web.config for SPA routing..." -ForegroundColor Yellow
if ($webConfigBackup) {
    $webConfigBackup | Set-Content "$DestinationPath\web.config" -Force
    Write-Host "  ✓ Restored existing web.config" -ForegroundColor Green
} else {
    $webConfigContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <!-- URL Rewrite for React Router -->
    <rewrite>
      <rules>
        <rule name="React Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/(api)" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>

    <!-- Security Headers -->
    <httpProtocol>
      <customHeaders>
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="SAMEORIGIN" />
        <add name="X-XSS-Protection" value="1; mode=block" />
        <add name="Referrer-Policy" value="strict-origin-when-cross-origin" />
        <add name="Permissions-Policy" value="geolocation=(), microphone=(), camera=()" />
        <remove name="X-Powered-By" />
      </customHeaders>
    </httpProtocol>

    <!-- Static Content Caching -->
    <staticContent>
      <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="7.00:00:00" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff2" />
      <mimeMap fileExtension=".svg" mimeType="image/svg+xml" />
    </staticContent>

    <!-- Compression -->
    <urlCompression doStaticCompression="true" doDynamicCompression="true" />

    <!-- Error Pages -->
    <httpErrors errorMode="Custom" existingResponse="Replace">
      <remove statusCode="404" />
      <error statusCode="404" path="/index.html" responseMode="ExecuteURL" />
    </httpErrors>
  </system.webServer>
</configuration>
"@
    $webConfigContent | Set-Content "$DestinationPath\web.config" -Force
    Write-Host "  ✓ Created web.config" -ForegroundColor Green
}

# Set permissions
Write-Host "  Setting permissions..." -ForegroundColor Yellow
$acl = Get-Acl $DestinationPath
$permission = "IIS_IUSRS", "Read", "ContainerInherit, ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $DestinationPath $acl
Write-Host "  ✓ Permissions set" -ForegroundColor Green

# Start services
Write-Host "[STEP 4/4] Starting IIS site and app pool..." -ForegroundColor Yellow
try {
    Start-WebAppPool -Name $AppPoolName -ErrorAction Stop
    Start-Sleep -Seconds 3
    Write-Host "  ✓ App pool started" -ForegroundColor Green

    Start-Website -Name $SiteName -ErrorAction Stop
    Start-Sleep -Seconds 2
    Write-Host "  ✓ Website started" -ForegroundColor Green

    # Verify site is running
    $siteState = (Get-Website -Name $SiteName).State
    if ($siteState -eq "Started") {
        Write-Host "  ✓ Website is running" -ForegroundColor Green
    } else {
        Write-Host "  [WARNING] Website state: $siteState" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  [ERROR] Failed to start services: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Frontend Deployment Completed!" -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
