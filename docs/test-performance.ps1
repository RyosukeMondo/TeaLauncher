# TeaLauncher Performance Testing Script
# Task 17: Performance and Memory Verification
# Measures startup time, hotkey response, and memory footprint

param(
    [Parameter(Mandatory=$false)]
    [string]$ExecutablePath = ".\CommandLauncher.exe",

    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "resource\conf\my.conf",

    [Parameter(Mandatory=$false)]
    [int]$RunCount = 3
)

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "TeaLauncher .NET 8 Performance Testing" -ForegroundColor Cyan
Write-Host "Task 17: Performance and Memory Verification" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Verify executable exists
if (-not (Test-Path $ExecutablePath)) {
    Write-Host "ERROR: CommandLauncher.exe not found at: $ExecutablePath" -ForegroundColor Red
    Write-Host "Please specify the correct path with -ExecutablePath parameter" -ForegroundColor Yellow
    exit 1
}

# Verify config exists
if (-not (Test-Path $ConfigPath)) {
    Write-Host "ERROR: Configuration file not found at: $ConfigPath" -ForegroundColor Red
    Write-Host "Please specify the correct path with -ConfigPath parameter" -ForegroundColor Yellow
    exit 1
}

Write-Host "Test Configuration:" -ForegroundColor Green
Write-Host "  Executable: $ExecutablePath"
Write-Host "  Config: $ConfigPath"
Write-Host "  Runs per test: $RunCount"
Write-Host ""

# Test 1: Startup Time Measurement
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Test 1: Startup Time Measurement" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Target: ≤ 300ms" -ForegroundColor Yellow
Write-Host ""

$startupTimes = @()

for ($i = 1; $i -le $RunCount; $i++) {
    Write-Host "Run $i/$RunCount..." -NoNewline

    # Kill any existing instances
    Get-Process CommandLauncher -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Milliseconds 500

    # Measure startup time
    $measure = Measure-Command {
        $process = Start-Process -FilePath $ExecutablePath -ArgumentList $ConfigPath -PassThru -WindowStyle Hidden

        # Wait for process to initialize (tray icon appears)
        # We approximate this by waiting for the process to stabilize
        Start-Sleep -Milliseconds 100

        # Verify process is running
        if ($process.HasExited) {
            Write-Host " FAILED (process exited)" -ForegroundColor Red
            exit 1
        }
    }

    $timeMs = [math]::Round($measure.TotalMilliseconds, 2)
    $startupTimes += $timeMs

    Write-Host " $timeMs ms" -ForegroundColor $(if ($timeMs -le 300) { "Green" } else { "Red" })

    # Clean up
    Get-Process CommandLauncher -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}

$avgStartup = [math]::Round(($startupTimes | Measure-Object -Average).Average, 2)
$minStartup = [math]::Round(($startupTimes | Measure-Object -Minimum).Minimum, 2)
$maxStartup = [math]::Round(($startupTimes | Measure-Object -Maximum).Maximum, 2)

Write-Host ""
Write-Host "Startup Time Results:" -ForegroundColor Green
Write-Host "  Average: $avgStartup ms" -ForegroundColor $(if ($avgStartup -le 300) { "Green" } else { "Red" })
Write-Host "  Minimum: $minStartup ms"
Write-Host "  Maximum: $maxStartup ms"
Write-Host "  Status: " -NoNewline
if ($avgStartup -le 300) {
    Write-Host "PASS ✓" -ForegroundColor Green
} else {
    Write-Host "FAIL ✗ (exceeds 300ms target)" -ForegroundColor Red
}
Write-Host ""

# Test 2: Memory Footprint Measurement
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Test 2: Memory Footprint Measurement" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Target: ≤ 20MB after 60 seconds idle" -ForegroundColor Yellow
Write-Host ""

# Kill any existing instances
Get-Process CommandLauncher -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

# Launch application
Write-Host "Launching TeaLauncher..." -NoNewline
$process = Start-Process -FilePath $ExecutablePath -ArgumentList $ConfigPath -PassThru -WindowStyle Hidden
Start-Sleep -Milliseconds 500

if ($process.HasExited) {
    Write-Host " FAILED (process exited)" -ForegroundColor Red
    exit 1
}
Write-Host " OK" -ForegroundColor Green

# Wait 60 seconds for memory to stabilize
Write-Host "Waiting 60 seconds for memory to stabilize..." -NoNewline
for ($i = 1; $i -le 60; $i++) {
    Start-Sleep -Seconds 1
    if ($i % 10 -eq 0) {
        Write-Host " $i" -NoNewline
    }
}
Write-Host " Done" -ForegroundColor Green

# Measure memory
$proc = Get-Process CommandLauncher -ErrorAction SilentlyContinue
if ($null -eq $proc) {
    Write-Host "ERROR: Process not found after 60 seconds" -ForegroundColor Red
    exit 1
}

$privateMemoryMB = [math]::Round($proc.PrivateMemorySize64 / 1MB, 2)
$workingSetMB = [math]::Round($proc.WorkingSet64 / 1MB, 2)

Write-Host ""
Write-Host "Memory Footprint Results:" -ForegroundColor Green
Write-Host "  Private Working Set: $privateMemoryMB MB" -ForegroundColor $(if ($privateMemoryMB -le 20) { "Green" } else { "Red" })
Write-Host "  Working Set: $workingSetMB MB"
Write-Host "  Status: " -NoNewline
if ($privateMemoryMB -le 20) {
    Write-Host "PASS ✓" -ForegroundColor Green
} else {
    Write-Host "FAIL ✗ (exceeds 20MB target)" -ForegroundColor Red
}
Write-Host ""

# Clean up
Get-Process CommandLauncher -ErrorAction SilentlyContinue | Stop-Process -Force

# Test 3: Hotkey Response Time (Manual)
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Test 3: Hotkey Response Time (Manual)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Target: < 100ms (should feel instant)" -ForegroundColor Yellow
Write-Host ""
Write-Host "NOTE: Hotkey response time requires manual testing." -ForegroundColor Yellow
Write-Host "Automated testing of keyboard input and window visibility is complex." -ForegroundColor Yellow
Write-Host ""
Write-Host "Manual Test Procedure:" -ForegroundColor Cyan
Write-Host "  1. Launch TeaLauncher manually: .\CommandLauncher.exe $ConfigPath"
Write-Host "  2. Press Ctrl+Space to trigger the input window"
Write-Host "  3. Verify the window appears INSTANTLY (< 100ms is imperceptible)"
Write-Host "  4. If there is ANY noticeable delay, the target is not met"
Write-Host "  5. Press Escape to hide the window"
Write-Host "  6. Repeat several times to confirm consistency"
Write-Host ""
Write-Host "Expected Result: Window should appear instantly with no perceptible delay"
Write-Host ""

# Summary Report
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Performance Test Summary" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

$allPassed = $true

Write-Host "Test Results:" -ForegroundColor Green
Write-Host ""

Write-Host "  [1] Startup Time: " -NoNewline
if ($avgStartup -le 300) {
    Write-Host "PASS ✓ ($avgStartup ms)" -ForegroundColor Green
} else {
    Write-Host "FAIL ✗ ($avgStartup ms > 300ms)" -ForegroundColor Red
    $allPassed = $false
}

Write-Host "  [2] Memory Footprint: " -NoNewline
if ($privateMemoryMB -le 20) {
    Write-Host "PASS ✓ ($privateMemoryMB MB)" -ForegroundColor Green
} else {
    Write-Host "FAIL ✗ ($privateMemoryMB MB > 20MB)" -ForegroundColor Red
    $allPassed = $false
}

Write-Host "  [3] Hotkey Response: " -NoNewline
Write-Host "MANUAL TEST REQUIRED" -ForegroundColor Yellow

Write-Host ""
Write-Host "Overall Status: " -NoNewline
if ($allPassed) {
    Write-Host "PASS ✓ (automated tests)" -ForegroundColor Green
} else {
    Write-Host "FAIL ✗ (one or more automated tests failed)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Test Report" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "Executable: $ExecutablePath"
Write-Host "Config: $ConfigPath"
Write-Host ""
Write-Host "Performance Metrics:" -ForegroundColor Green
Write-Host "  Startup Time (avg): $avgStartup ms (target: ≤ 300ms)"
Write-Host "  Startup Time (min): $minStartup ms"
Write-Host "  Startup Time (max): $maxStartup ms"
Write-Host "  Memory (Private):   $privateMemoryMB MB (target: ≤ 20MB)"
Write-Host "  Memory (Working):   $workingSetMB MB"
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  - Complete manual hotkey response testing"
Write-Host "  - Document results in manual-test-task17.md"
Write-Host "  - If any tests failed, investigate performance issues"
Write-Host "  - Test both framework-dependent and self-contained builds"
Write-Host ""

# Export results to file
$resultsFile = "test-results-task17-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
$resultsPath = Join-Path (Get-Location) $resultsFile

$report = @"
TeaLauncher .NET 8 Performance Test Results
Task 17: Performance and Memory Verification

Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Executable: $ExecutablePath
Config: $ConfigPath
Windows Version: $([System.Environment]::OSVersion.VersionString)
.NET Runtime: $($PSVersionTable.PSVersion)

=== Test Results ===

Startup Time (target: ≤ 300ms):
  Run 1: $($startupTimes[0]) ms
  Run 2: $($startupTimes[1]) ms
  Run 3: $($startupTimes[2]) ms
  Average: $avgStartup ms
  Minimum: $minStartup ms
  Maximum: $maxStartup ms
  Status: $(if ($avgStartup -le 300) { "PASS" } else { "FAIL" })

Memory Footprint (target: ≤ 20MB after 60s idle):
  Private Working Set: $privateMemoryMB MB
  Working Set: $workingSetMB MB
  Status: $(if ($privateMemoryMB -le 20) { "PASS" } else { "FAIL" })

Hotkey Response (target: < 100ms):
  Status: MANUAL TEST REQUIRED

=== Overall Status ===

Automated Tests: $(if ($allPassed) { "PASS" } else { "FAIL" })
Manual Test Required: Hotkey Response Time

"@

$report | Out-File -FilePath $resultsPath -Encoding UTF8
Write-Host "Results saved to: $resultsPath" -ForegroundColor Green
Write-Host ""

exit $(if ($allPassed) { 0 } else { 1 })
