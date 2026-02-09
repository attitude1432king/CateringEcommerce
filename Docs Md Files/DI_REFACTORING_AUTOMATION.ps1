# ================================================================
# DEPENDENCY INJECTION REFACTORING AUTOMATION SCRIPT
# PowerShell script to help automate controller refactoring
# ================================================================

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "DI REFACTORING AUTOMATION SCRIPT" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$sourceControllerPath = "D:\Pankaj\Project\CateringEcommerce\CateringEcommerce.API\Controllers"
$refactoredPath = "D:\Pankaj\Project\CateringEcommerce\REFACTORED_CONTROLLERS"
$backupPath = "D:\Pankaj\Project\CateringEcommerce\ORIGINAL_CONTROLLERS_BACKUP"

# Function to analyze controller for DI violations
function Analyze-Controller {
    param (
        [string]$filePath
    )

    $content = Get-Content $filePath -Raw
    $violations = @()

    # Check for IConfiguration dependency
    if ($content -match 'public\s+\w+Controller\s*\(\s*IConfiguration\s+config') {
        $violations += "IConfiguration dependency"
    }

    # Check for GetConnectionString
    if ($content -match 'GetConnectionString\s*\(') {
        $violations += "GetConnectionString call"
    }

    # Check for manual repository instantiation
    if ($content -match 'new\s+\w+Repository\s*\(') {
        $violations += "Manual repository instantiation"
    }

    # Check for manual SqlDatabaseManager
    if ($content -match 'new\s+SqlDatabaseManager\s*\(') {
        $violations += "Manual SqlDatabaseManager instantiation"
    }

    return $violations
}

# Function to backup original controller
function Backup-Controller {
    param (
        [string]$sourceFile
    )

    $fileName = Split-Path $sourceFile -Leaf
    $relativePath = $sourceFile.Replace($sourceControllerPath, "").TrimStart("\")
    $backupFile = Join-Path $backupPath $relativePath
    $backupDir = Split-Path $backupFile -Parent

    if (-not (Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    }

    Copy-Item $sourceFile $backupFile -Force
    Write-Host "  [BACKUP] $fileName backed up" -ForegroundColor Yellow
}

# Function to apply refactored controller
function Apply-RefactoredController {
    param (
        [string]$refactoredFile,
        [string]$targetFile
    )

    $fileName = Split-Path $targetFile -Leaf

    # Backup original
    Backup-Controller -sourceFile $targetFile

    # Copy refactored version
    Copy-Item $refactoredFile $targetFile -Force
    Write-Host "  [APPLIED] $fileName refactored" -ForegroundColor Green
}

# Function to scan all controllers
function Scan-AllControllers {
    Write-Host ""
    Write-Host "Scanning controllers for DI violations..." -ForegroundColor Cyan
    Write-Host ""

    $controllers = Get-ChildItem -Path $sourceControllerPath -Filter "*Controller.cs" -Recurse
    $violationReport = @()

    foreach ($controller in $controllers) {
        $violations = Analyze-Controller -filePath $controller.FullName

        if ($violations.Count -gt 0) {
            $relativePath = $controller.FullName.Replace($sourceControllerPath, "").TrimStart("\")
            $violationReport += [PSCustomObject]@{
                Controller = $relativePath
                Violations = $violations -join ", "
                ViolationCount = $violations.Count
                Status = "Pending"
            }
        }
    }

    # Display report
    $violationReport | Format-Table -AutoSize

    Write-Host ""
    Write-Host "SUMMARY:" -ForegroundColor Yellow
    Write-Host "  Total Controllers Scanned: $($controllers.Count)" -ForegroundColor White
    Write-Host "  Controllers with Violations: $($violationReport.Count)" -ForegroundColor Red
    Write-Host "  Clean Controllers: $($controllers.Count - $violationReport.Count)" -ForegroundColor Green
    Write-Host ""

    # Export report
    $reportPath = "D:\Pankaj\Project\CateringEcommerce\DI_VIOLATION_REPORT.csv"
    $violationReport | Export-Csv -Path $reportPath -NoTypeInformation
    Write-Host "Report exported to: $reportPath" -ForegroundColor Cyan
}

# Function to apply all refactored controllers
function Apply-AllRefactoredControllers {
    Write-Host ""
    Write-Host "Applying all refactored controllers..." -ForegroundColor Cyan
    Write-Host ""

    if (-not (Test-Path $refactoredPath)) {
        Write-Host "ERROR: Refactored controllers directory not found: $refactoredPath" -ForegroundColor Red
        return
    }

    $refactoredControllers = Get-ChildItem -Path $refactoredPath -Filter "*Controller.cs" -Recurse
    $appliedCount = 0

    foreach ($refactored in $refactoredControllers) {
        $relativePath = $refactored.FullName.Replace($refactoredPath, "").TrimStart("\")
        $targetFile = Join-Path $sourceControllerPath $relativePath

        if (Test-Path $targetFile) {
            Apply-RefactoredController -refactoredFile $refactored.FullName -targetFile $targetFile
            $appliedCount++
        }
        else {
            Write-Host "  [SKIP] Target not found: $relativePath" -ForegroundColor Yellow
        }
    }

    Write-Host ""
    Write-Host "SUMMARY:" -ForegroundColor Yellow
    Write-Host "  Refactored Controllers Applied: $appliedCount" -ForegroundColor Green
    Write-Host "  Backup Location: $backupPath" -ForegroundColor Cyan
    Write-Host ""
}

# Function to rollback to original controllers
function Rollback-Controllers {
    Write-Host ""
    Write-Host "Rolling back to original controllers..." -ForegroundColor Yellow
    Write-Host ""

    if (-not (Test-Path $backupPath)) {
        Write-Host "ERROR: Backup directory not found: $backupPath" -ForegroundColor Red
        return
    }

    $backupControllers = Get-ChildItem -Path $backupPath -Filter "*Controller.cs" -Recurse
    $rolledBackCount = 0

    foreach ($backup in $backupControllers) {
        $relativePath = $backup.FullName.Replace($backupPath, "").TrimStart("\")
        $targetFile = Join-Path $sourceControllerPath $relativePath

        if (Test-Path $targetFile) {
            Copy-Item $backup.FullName $targetFile -Force
            Write-Host "  [RESTORED] $($backup.Name)" -ForegroundColor Green
            $rolledBackCount++
        }
    }

    Write-Host ""
    Write-Host "Rolled back $rolledBackCount controllers" -ForegroundColor Green
    Write-Host ""
}

# Function to verify Program.cs registrations
function Verify-ProgramRegistrations {
    Write-Host ""
    Write-Host "Verifying Program.cs DI registrations..." -ForegroundColor Cyan
    Write-Host ""

    $programPath = "D:\Pankaj\Project\CateringEcommerce\CateringEcommerce.API\Program.cs"

    if (-not (Test-Path $programPath)) {
        Write-Host "ERROR: Program.cs not found" -ForegroundColor Red
        return
    }

    $content = Get-Content $programPath -Raw

    $requiredRegistrations = @(
        "ITokenService",
        "IAdminAuthRepository",
        "IAdminCateringRepository",
        "IAdminDashboardRepository",
        "IAdminEarningsRepository",
        "IOwnerEarningsRepository",
        "ICartRepository"
    )

    $missingRegistrations = @()

    foreach ($registration in $requiredRegistrations) {
        if ($content -notmatch "AddScoped<$registration") {
            $missingRegistrations += $registration
        }
    }

    if ($missingRegistrations.Count -eq 0) {
        Write-Host "  ✅ All required registrations found" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠️ Missing registrations:" -ForegroundColor Yellow
        foreach ($missing in $missingRegistrations) {
            Write-Host "    - $missing" -ForegroundColor Red
        }
    }

    Write-Host ""
}

# Function to generate refactoring template for a controller
function Generate-RefactoringTemplate {
    param (
        [string]$controllerPath
    )

    Write-Host ""
    Write-Host "Analyzing controller: $controllerPath" -ForegroundColor Cyan
    Write-Host ""

    $content = Get-Content $controllerPath -Raw

    # Extract repository usages
    $repositories = [regex]::Matches($content, 'new\s+(\w+Repository)\s*\(') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique

    # Extract service usages
    $services = [regex]::Matches($content, 'new\s+(\w+Service)\s*\(') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique

    Write-Host "DETECTED DEPENDENCIES:" -ForegroundColor Yellow
    Write-Host ""

    if ($repositories.Count -gt 0) {
        Write-Host "Repositories:" -ForegroundColor Cyan
        foreach ($repo in $repositories) {
            $interfaceName = "I" + $repo
            Write-Host "  - $repo -> $interfaceName" -ForegroundColor White
        }
    }

    if ($services.Count -gt 0) {
        Write-Host "Services:" -ForegroundColor Cyan
        foreach ($service in $services) {
            $interfaceName = "I" + $service
            Write-Host "  - $service -> $interfaceName" -ForegroundColor White
        }
    }

    Write-Host ""
    Write-Host "REFACTORING TEMPLATE:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "// Constructor before:" -ForegroundColor Red
    Write-Host "public MyController(IConfiguration config)" -ForegroundColor Red
    Write-Host ""
    Write-Host "// Constructor after:" -ForegroundColor Green

    $constructorParams = @()
    foreach ($repo in $repositories) {
        $interfaceName = "I" + $repo
        $paramName = $repo.Substring(0,1).ToLower() + $repo.Substring(1)
        $constructorParams += "    $interfaceName $paramName"
    }
    foreach ($service in $services) {
        $interfaceName = "I" + $service
        $paramName = $service.Substring(0,1).ToLower() + $service.Substring(1)
        $constructorParams += "    $interfaceName $paramName"
    }
    $constructorParams += "    ILogger<MyController> logger"

    Write-Host "public MyController(" -ForegroundColor Green
    Write-Host ($constructorParams -join ",`n") -ForegroundColor Green
    Write-Host ")" -ForegroundColor Green
    Write-Host ""
}

# Main Menu
function Show-Menu {
    Write-Host ""
    Write-Host "===================================================" -ForegroundColor Cyan
    Write-Host "SELECT AN OPTION:" -ForegroundColor Cyan
    Write-Host "===================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Scan All Controllers (Generate Violation Report)" -ForegroundColor White
    Write-Host "2. Apply All Refactored Controllers" -ForegroundColor White
    Write-Host "3. Rollback to Original Controllers" -ForegroundColor White
    Write-Host "4. Verify Program.cs DI Registrations" -ForegroundColor White
    Write-Host "5. Generate Refactoring Template for Single Controller" -ForegroundColor White
    Write-Host "6. Exit" -ForegroundColor White
    Write-Host ""
    $choice = Read-Host "Enter your choice (1-6)"

    switch ($choice) {
        "1" { Scan-AllControllers }
        "2" { Apply-AllRefactoredControllers }
        "3" { Rollback-Controllers }
        "4" { Verify-ProgramRegistrations }
        "5" {
            $controllerPath = Read-Host "Enter full path to controller"
            Generate-RefactoringTemplate -controllerPath $controllerPath
        }
        "6" {
            Write-Host ""
            Write-Host "Exiting..." -ForegroundColor Cyan
            Write-Host ""
            exit
        }
        default {
            Write-Host "Invalid choice. Please try again." -ForegroundColor Red
        }
    }

    Show-Menu
}

# Start the script
Show-Menu
