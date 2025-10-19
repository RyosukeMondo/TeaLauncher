# PowerShell Test Script for Task 14 - Hotkey and UI Interaction
# Run this on Windows while TeaLauncher is running

Write-Host "=== TeaLauncher Hotkey and UI Interaction Test (Task 14) ===" -ForegroundColor Cyan
Write-Host ""

# Check if TeaLauncher is running
$teaLauncher = Get-Process -Name "CommandLauncher" -ErrorAction SilentlyContinue

if (-not $teaLauncher) {
    Write-Host "ERROR: TeaLauncher (CommandLauncher.exe) is not running!" -ForegroundColor Red
    Write-Host "Please start TeaLauncher first (see Task 13 test)." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Start with: .\CommandLauncher.exe resource\conf\my.conf" -ForegroundColor Gray
    exit 1
}

Write-Host "✓ TeaLauncher is running (Process ID: $($teaLauncher.Id))" -ForegroundColor Green
Write-Host ""

# Display test instructions
Write-Host "=== INTERACTIVE TEST PROCEDURE ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will guide you through manual interaction tests." -ForegroundColor White
Write-Host "You will need to press keys and observe TeaLauncher behavior." -ForegroundColor White
Write-Host ""

# Test 1: Hotkey trigger
Write-Host "--- Test 1: Global Hotkey (Ctrl+Space) ---" -ForegroundColor Yellow
Write-Host ""
Write-Host "ACTION: Press Ctrl+Space on your keyboard" -ForegroundColor White
Write-Host ""
Write-Host "Expected behavior:" -ForegroundColor Gray
Write-Host "  - TeaLauncher input window appears" -ForegroundColor Gray
Write-Host "  - Window shows quickly (< 100ms)" -ForegroundColor Gray
Write-Host "  - Textbox has keyboard focus" -ForegroundColor Gray
Write-Host ""

$test1 = Read-Host "Did the window appear correctly? (y/n)"
if ($test1 -eq 'y') {
    Write-Host "✓ Test 1 PASSED" -ForegroundColor Green
    $test1Pass = $true
} else {
    Write-Host "✗ Test 1 FAILED" -ForegroundColor Red
    $test1Pass = $false
    $issue1 = Read-Host "Describe the issue"
}

Write-Host ""

# Test 2: Focus check
Write-Host "--- Test 2: Textbox Focus ---" -ForegroundColor Yellow
Write-Host ""
Write-Host "VERIFY: Is the textbox ready for input?" -ForegroundColor White
Write-Host "  - Cursor should be blinking in the textbox" -ForegroundColor Gray
Write-Host "  - You should be able to type immediately without clicking" -ForegroundColor Gray
Write-Host ""

$test2 = Read-Host "Does the textbox have focus? (y/n)"
if ($test2 -eq 'y') {
    Write-Host "✓ Test 2 PASSED" -ForegroundColor Green
    $test2Pass = $true
} else {
    Write-Host "✗ Test 2 FAILED" -ForegroundColor Red
    $test2Pass = $false
    $issue2 = Read-Host "Describe the issue"
}

Write-Host ""

# Test 3: Auto-completion
Write-Host "--- Test 3: Auto-completion (Tab Key) ---" -ForegroundColor Yellow
Write-Host ""
Write-Host "ACTION: In the TeaLauncher window:" -ForegroundColor White
Write-Host "  1. Type a single letter that matches multiple commands (e.g., 'g')" -ForegroundColor Gray
Write-Host "  2. Press the Tab key" -ForegroundColor Gray
Write-Host ""
Write-Host "Expected behavior:" -ForegroundColor Gray
Write-Host "  - Auto-completion suggestions appear" -ForegroundColor Gray
Write-Host "  - Only commands starting with your letter are shown" -ForegroundColor Gray
Write-Host "  - Pressing Tab again cycles through matches" -ForegroundColor Gray
Write-Host ""

$test3 = Read-Host "Did auto-completion work correctly? (y/n)"
if ($test3 -eq 'y') {
    Write-Host "✓ Test 3 PASSED" -ForegroundColor Green
    $test3Pass = $true
    $prefix = Read-Host "What prefix did you test? (e.g., 'g')"
    $matches = Read-Host "How many matches appeared? (or list them)"
} else {
    Write-Host "✗ Test 3 FAILED" -ForegroundColor Red
    $test3Pass = $false
    $issue3 = Read-Host "Describe the issue"
}

Write-Host ""

# Test 4: Escape key
Write-Host "--- Test 4: Hide Window (Escape Key) ---" -ForegroundColor Yellow
Write-Host ""
Write-Host "ACTION: Press the Escape key" -ForegroundColor White
Write-Host ""
Write-Host "Expected behavior:" -ForegroundColor Gray
Write-Host "  - Window disappears immediately" -ForegroundColor Gray
Write-Host "  - System tray icon remains visible" -ForegroundColor Gray
Write-Host "  - TeaLauncher continues running" -ForegroundColor Gray
Write-Host ""

$test4 = Read-Host "Did the window hide correctly? (y/n)"
if ($test4 -eq 'y') {
    Write-Host "✓ Test 4 PASSED" -ForegroundColor Green
    $test4Pass = $true
} else {
    Write-Host "✗ Test 4 FAILED" -ForegroundColor Red
    $test4Pass = $false
    $issue4 = Read-Host "Describe the issue"
}

Write-Host ""

# Test 5: Re-trigger hotkey
Write-Host "--- Test 5: Hotkey Re-triggering ---" -ForegroundColor Yellow
Write-Host ""
Write-Host "ACTION: Press Ctrl+Space again to show the window" -ForegroundColor White
Write-Host ""
Write-Host "Expected behavior:" -ForegroundColor Gray
Write-Host "  - Window appears again" -ForegroundColor Gray
Write-Host "  - Same fast response as before" -ForegroundColor Gray
Write-Host "  - Textbox has focus" -ForegroundColor Gray
Write-Host ""

$test5 = Read-Host "Did the window re-appear correctly? (y/n)"
if ($test5 -eq 'y') {
    Write-Host "✓ Test 5 PASSED" -ForegroundColor Green
    $test5Pass = $true
} else {
    Write-Host "✗ Test 5 FAILED" -ForegroundColor Red
    $test5Pass = $false
    $issue5 = Read-Host "Describe the issue"
}

Write-Host ""

# Test 6: Repeated show/hide cycles
Write-Host "--- Test 6: Repeated Show/Hide Cycles ---" -ForegroundColor Yellow
Write-Host ""
Write-Host "ACTION: Repeat the following cycle 5 times:" -ForegroundColor White
Write-Host "  1. Press Ctrl+Space (show window)" -ForegroundColor Gray
Write-Host "  2. Press Escape (hide window)" -ForegroundColor Gray
Write-Host ""
Write-Host "Verify each cycle works consistently with no degradation." -ForegroundColor Gray
Write-Host ""

Read-Host "Press Enter when ready to start cycling test" | Out-Null

Write-Host "Perform 5 cycles now..." -ForegroundColor Yellow
Write-Host ""

Start-Sleep -Seconds 10  # Give time for manual testing

$test6 = Read-Host "Did all 5 cycles work correctly? (y/n)"
if ($test6 -eq 'y') {
    Write-Host "✓ Test 6 PASSED" -ForegroundColor Green
    $test6Pass = $true
} else {
    Write-Host "✗ Test 6 FAILED" -ForegroundColor Red
    $test6Pass = $false
    $issue6 = Read-Host "Describe the issue (which cycle failed?)"
}

Write-Host ""
Write-Host "=== TEST SUMMARY ===" -ForegroundColor Cyan
Write-Host ""

$totalTests = 6
$passedTests = 0

if ($test1Pass) { $passedTests++ }
if ($test2Pass) { $passedTests++ }
if ($test3Pass) { $passedTests++ }
if ($test4Pass) { $passedTests++ }
if ($test5Pass) { $passedTests++ }
if ($test6Pass) { $passedTests++ }

Write-Host "Tests Passed: $passedTests / $totalTests" -ForegroundColor $(if ($passedTests -eq $totalTests) { "Green" } else { "Yellow" })
Write-Host ""

Write-Host "Test 1 - Global Hotkey: $(if ($test1Pass) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($test1Pass) { "Green" } else { "Red" })
Write-Host "Test 2 - Textbox Focus: $(if ($test2Pass) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($test2Pass) { "Green" } else { "Red" })
Write-Host "Test 3 - Auto-completion: $(if ($test3Pass) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($test3Pass) { "Green" } else { "Red" })
Write-Host "Test 4 - Escape Key: $(if ($test4Pass) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($test4Pass) { "Green" } else { "Red" })
Write-Host "Test 5 - Re-triggering: $(if ($test5Pass) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($test5Pass) { "Green" } else { "Red" })
Write-Host "Test 6 - Repeated Cycles: $(if ($test6Pass) { '✓ PASS' } else { '✗ FAIL' })" -ForegroundColor $(if ($test6Pass) { "Green" } else { "Red" })

Write-Host ""

# Overall result
if ($passedTests -eq $totalTests) {
    Write-Host "=== OVERALL RESULT: ALL TESTS PASSED ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "✓ P/Invoke hotkey registration works on .NET 8" -ForegroundColor Green
    Write-Host "✓ Windows Forms UI displays correctly" -ForegroundColor Green
    Write-Host "✓ Auto-completion logic functions properly" -ForegroundColor Green
    Write-Host "✓ Window show/hide behavior is reliable" -ForegroundColor Green
    $overallResult = "PASS"
} else {
    Write-Host "=== OVERALL RESULT: SOME TESTS FAILED ===" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Review the failures above and check:" -ForegroundColor Yellow
    Write-Host "  - Windows version (should be Windows 10 1607+ or Windows 11)" -ForegroundColor Gray
    Write-Host "  - .NET 8 Desktop Runtime is installed correctly" -ForegroundColor Gray
    Write-Host "  - TeaLauncher has permission to register global hotkeys" -ForegroundColor Gray
    Write-Host "  - Configuration file (my.conf) is valid and loaded" -ForegroundColor Gray
    $overallResult = "FAIL"
}

Write-Host ""

# Check memory usage
Write-Host "=== Memory Usage Check ===" -ForegroundColor Cyan
$teaLauncher = Get-Process -Name "CommandLauncher" -ErrorAction SilentlyContinue
if ($teaLauncher) {
    $memoryMB = [math]::Round($teaLauncher.WorkingSet / 1MB, 2)
    Write-Host "Current Memory: $memoryMB MB" -ForegroundColor $(if ($memoryMB -le 20) { "Green" } else { "Yellow" })
    Write-Host "Target: ≤ 20 MB" -ForegroundColor Gray

    if ($memoryMB -le 20) {
        Write-Host "✓ Memory usage is within target" -ForegroundColor Green
    } else {
        Write-Host "⚠ Memory usage exceeds target (but may be acceptable)" -ForegroundColor Yellow
    }
}

Write-Host ""

# Save results
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$resultsFile = "test-results-task14-$timestamp.txt"

$results = @"
=== TeaLauncher Task 14 Test Results ===
Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Tester: $env:USERNAME
Computer: $env:COMPUTERNAME
OS: $(Get-WmiObject Win32_OperatingSystem | Select-Object -ExpandProperty Caption)

Overall Result: $overallResult
Tests Passed: $passedTests / $totalTests

--- Individual Test Results ---
Test 1 - Global Hotkey: $(if ($test1Pass) { 'PASS' } else { "FAIL - $issue1" })
Test 2 - Textbox Focus: $(if ($test2Pass) { 'PASS' } else { "FAIL - $issue2" })
Test 3 - Auto-completion: $(if ($test3Pass) { "PASS - Prefix: $prefix, Matches: $matches" } else { "FAIL - $issue3" })
Test 4 - Escape Key: $(if ($test4Pass) { 'PASS' } else { "FAIL - $issue4" })
Test 5 - Re-triggering: $(if ($test5Pass) { 'PASS' } else { "FAIL - $issue5" })
Test 6 - Repeated Cycles: $(if ($test6Pass) { 'PASS' } else { "FAIL - $issue6" })

Memory Usage: $memoryMB MB
"@

$results | Out-File -FilePath $resultsFile -Encoding UTF8

Write-Host "Results saved to: $resultsFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "Document these results in: docs/manual-test-task14.md" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to close..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
