# PowerShell Test Script for Task 13 - Application Startup
# Run this on Windows after copying the executable and my.conf

param(
    [Parameter(Mandatory=$false)]
    [string]$ExePath = ".\CommandLauncher.exe",

    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "resource\conf\my.conf"
)

Write-Host "=== TeaLauncher Startup Test (Task 13) ===" -ForegroundColor Cyan
Write-Host ""

# Check if executable exists
if (-not (Test-Path $ExePath)) {
    Write-Host "ERROR: CommandLauncher.exe not found at: $ExePath" -ForegroundColor Red
    Write-Host "Please copy the executable to this directory first." -ForegroundColor Yellow
    exit 1
}

# Check if config exists
if (-not (Test-Path $ConfigPath)) {
    Write-Host "ERROR: Configuration file not found at: $ConfigPath" -ForegroundColor Red
    Write-Host "Please ensure resource\conf\my.conf is in the correct location." -ForegroundColor Yellow
    exit 1
}

Write-Host "Executable: $ExePath" -ForegroundColor Green
Write-Host "Config: $ConfigPath" -ForegroundColor Green
Write-Host ""

# Get file size
$fileSize = (Get-Item $ExePath).Length / 1MB
Write-Host "Executable Size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan

if ($fileSize -gt 50) {
    Write-Host "Build Type: Self-Contained (includes .NET runtime)" -ForegroundColor Cyan
} else {
    Write-Host "Build Type: Framework-Dependent (requires .NET 8 Desktop Runtime)" -ForegroundColor Cyan

    # Check for .NET 8 Desktop Runtime
    Write-Host ""
    Write-Host "Checking for .NET 8 Desktop Runtime..." -ForegroundColor Yellow
    $runtimes = & dotnet --list-runtimes 2>$null | Select-String "Microsoft.WindowsDesktop.App 8"

    if ($runtimes) {
        Write-Host "✓ .NET 8 Desktop Runtime found:" -ForegroundColor Green
        $runtimes | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
    } else {
        Write-Host "✗ .NET 8 Desktop Runtime NOT found!" -ForegroundColor Red
        Write-Host "  Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        Write-Host "  Install 'Desktop Runtime' to run framework-dependent builds." -ForegroundColor Yellow
        Write-Host ""
        $continue = Read-Host "Continue anyway? (y/n)"
        if ($continue -ne 'y') {
            exit 1
        }
    }
}

Write-Host ""
Write-Host "Starting application and measuring startup time..." -ForegroundColor Cyan
Write-Host "Watch for the system tray icon (TeaLauncher) to appear!" -ForegroundColor Yellow
Write-Host ""

# Measure startup time
$startTime = Measure-Command {
    $process = Start-Process -FilePath $ExePath -ArgumentList $ConfigPath -PassThru
    Start-Sleep -Milliseconds 500  # Give it time to initialize
}

Write-Host "=== Test Results ===" -ForegroundColor Cyan
Write-Host "Startup Time: $([math]::Round($startTime.TotalMilliseconds, 0)) ms" -ForegroundColor $(if ($startTime.TotalMilliseconds -le 300) { "Green" } else { "Yellow" })
Write-Host "Target: ≤ 300 ms" -ForegroundColor Gray

if ($startTime.TotalMilliseconds -le 300) {
    Write-Host "✓ PASS: Startup time meets requirement" -ForegroundColor Green
} else {
    Write-Host "⚠ WARNING: Startup time exceeds target (but may still be acceptable)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Manual Verification Required ===" -ForegroundColor Cyan
Write-Host "Please check the following:" -ForegroundColor White
Write-Host "  [ ] System tray icon appeared (TeaLauncher icon in notification area)"
Write-Host "  [ ] No error dialogs were shown"
Write-Host "  [ ] Application feels responsive"
Write-Host "  [ ] No console errors visible"
Write-Host ""
Write-Host "To close the application:" -ForegroundColor Yellow
Write-Host "  1. Right-click the system tray icon"
Write-Host "  2. Select 'Exit' or type !exit"
Write-Host ""
Write-Host "Press any key to finish test..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
