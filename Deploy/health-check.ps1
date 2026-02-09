# Health Check Script
# Validates deployment after completion

param(
    [Parameter(Mandatory=$true)]
    [string]$ApiHealthEndpoint,

    [Parameter(Mandatory=$true)]
    [string]$WebHealthEndpoint,

    [Parameter(Mandatory=$false)]
    [int]$Timeout = 60,

    [Parameter(Mandatory=$false)]
    [int]$Retries = 3
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Post-Deployment Health Check" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "API Endpoint: $ApiHealthEndpoint"
Write-Host "Web Endpoint: $WebHealthEndpoint"
Write-Host "Timeout: $Timeout seconds"
Write-Host "Retries: $Retries"
Write-Host ""

$allHealthy = $true

# Function to check HTTP endpoint
function Test-HttpEndpoint {
    param(
        [string]$Url,
        [int]$Timeout,
        [int]$Retries
    )

    $attempt = 0
    $success = $false

    while ($attempt -lt $Retries -and -not $success) {
        $attempt++
        Write-Host "  Attempt $attempt/$Retries..." -ForegroundColor Cyan

        try {
            $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec $Timeout -UseBasicParsing -ErrorAction Stop

            if ($response.StatusCode -eq 200) {
                Write-Host "    ✓ Status Code: 200 OK" -ForegroundColor Green
                $success = $true
                return $true
            } else {
                Write-Host "    ✗ Status Code: $($response.StatusCode)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "    ✗ Error: $($_.Exception.Message)" -ForegroundColor Red

            if ($attempt -lt $Retries) {
                Write-Host "    Waiting 5 seconds before retry..." -ForegroundColor Yellow
                Start-Sleep -Seconds 5
            }
        }
    }

    return $success
}

# Check 1: API Health
Write-Host "[CHECK 1/5] API Health Endpoint" -ForegroundColor Yellow
$apiHealthy = Test-HttpEndpoint -Url $ApiHealthEndpoint -Timeout $Timeout -Retries $Retries

if ($apiHealthy) {
    Write-Host "  ✓ API is healthy" -ForegroundColor Green
} else {
    Write-Host "  ✗ API health check FAILED" -ForegroundColor Red
    $allHealthy = $false
}

Write-Host ""

# Check 2: Web Health
Write-Host "[CHECK 2/5] Web Health Endpoint" -ForegroundColor Yellow
$webHealthy = Test-HttpEndpoint -Url $WebHealthEndpoint -Timeout $Timeout -Retries $Retries

if ($webHealthy) {
    Write-Host "  ✓ Web is healthy" -ForegroundColor Green
} else {
    Write-Host "  ✗ Web health check FAILED" -ForegroundColor Red
    $allHealthy = $false
}

Write-Host ""

# Check 3: IIS Application Pools
Write-Host "[CHECK 3/5] IIS Application Pools" -ForegroundColor Yellow
try {
    Import-Module WebAdministration -ErrorAction Stop

    $apiPool = Get-WebAppPoolState -Name "CateringEcommerce_API_Pool" -ErrorAction SilentlyContinue
    $webPool = Get-WebAppPoolState -Name "CateringEcommerce_Web_Pool" -ErrorAction SilentlyContinue

    if ($apiPool.Value -eq "Started") {
        Write-Host "  ✓ API App Pool: Started" -ForegroundColor Green
    } else {
        Write-Host "  ✗ API App Pool: $($apiPool.Value)" -ForegroundColor Red
        $allHealthy = $false
    }

    if ($webPool.Value -eq "Started") {
        Write-Host "  ✓ Web App Pool: Started" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Web App Pool: $($webPool.Value)" -ForegroundColor Red
        $allHealthy = $false
    }
} catch {
    Write-Host "  ✗ Cannot check IIS pools: $_" -ForegroundColor Yellow
}

Write-Host ""

# Check 4: Critical Files Exist
Write-Host "[CHECK 4/5] Critical Files" -ForegroundColor Yellow
$apiPath = "C:\inetpub\wwwroot\CateringAPI"
$webPath = "C:\inetpub\wwwroot\CateringWeb"

$criticalFiles = @(
    @{ Path = "$apiPath\CateringEcommerce.API.dll"; Name = "API DLL" },
    @{ Path = "$apiPath\web.config"; Name = "API web.config" },
    @{ Path = "$apiPath\appsettings.json"; Name = "API appsettings" },
    @{ Path = "$webPath\index.html"; Name = "Web index.html" },
    @{ Path = "$webPath\web.config"; Name = "Web web.config" }
)

foreach ($file in $criticalFiles) {
    if (Test-Path $file.Path) {
        Write-Host "  ✓ $($file.Name) exists" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $($file.Name) NOT FOUND" -ForegroundColor Red
        $allHealthy = $false
    }
}

Write-Host ""

# Check 5: Event Log Errors (last 5 minutes)
Write-Host "[CHECK 5/5] Recent Event Log Errors" -ForegroundColor Yellow
try {
    $fiveMinutesAgo = (Get-Date).AddMinutes(-5)
    $errors = Get-EventLog -LogName Application -Source "IIS*" -EntryType Error -After $fiveMinutesAgo -Newest 10 -ErrorAction SilentlyContinue

    if ($errors.Count -eq 0) {
        Write-Host "  ✓ No recent IIS errors" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ Found $($errors.Count) recent IIS error(s)" -ForegroundColor Yellow
        foreach ($error in $errors | Select-Object -First 3) {
            Write-Host "    - $($error.TimeGenerated): $($error.Message.Substring(0, [Math]::Min(100, $error.Message.Length)))" -ForegroundColor Gray
        }

        # Don't fail deployment for warnings, just alert
    }
} catch {
    Write-Host "  ℹ Cannot check event logs: $_" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Health Check Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($allHealthy) {
    Write-Host "✓ ALL CHECKS PASSED" -ForegroundColor Green
    Write-Host ""
    Write-Host "Deployment is healthy and ready for use!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green

    # Set Azure DevOps pipeline variable
    Write-Host "##vso[task.setvariable variable=HealthCheckPassed;isOutput=true]true"

    exit 0
} else {
    Write-Host "✗ HEALTH CHECK FAILED" -ForegroundColor Red
    Write-Host ""
    Write-Host "⚠ Some checks did not pass!" -ForegroundColor Yellow
    Write-Host "Review errors above and consider rollback." -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Red

    # Set Azure DevOps pipeline variable
    Write-Host "##vso[task.setvariable variable=HealthCheckPassed;isOutput=true]false"

    exit 1
}
