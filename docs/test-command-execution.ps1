# Interactive Test Script for Task 15: Command Execution Testing
# This script guides Windows testers through command execution verification
# for TeaLauncher .NET 8 migration (Requirements 5.6, 6.1, 6.2, 6.3)

# Test configuration
$testTitle = "Task 15: Command Execution Testing"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$resultFile = "test-results-task15_$timestamp.txt"

# Initialize results
$testResults = @()
$totalTests = 10
$passedTests = 0
$failedTests = 0

# Colors for output
$colorPass = "Green"
$colorFail = "Red"
$colorInfo = "Cyan"
$colorWarning = "Yellow"

# Banner
Clear-Host
Write-Host "=" * 80 -ForegroundColor $colorInfo
Write-Host " TeaLauncher .NET 8 Migration - $testTitle" -ForegroundColor $colorInfo
Write-Host "=" * 80 -ForegroundColor $colorInfo
Write-Host ""

# Prerequisites check
Write-Host "PREREQUISITES CHECK" -ForegroundColor $colorWarning
Write-Host "-" * 80
Write-Host ""
Write-Host "Before starting, ensure:" -ForegroundColor $colorInfo
Write-Host "  1. TeaLauncher.exe is running"
Write-Host "  2. System tray icon is visible"
Write-Host "  3. Configuration file (resource/conf/my.conf) is loaded"
Write-Host "  4. Tasks 13 and 14 are completed (startup and UI verified)"
Write-Host ""

$ready = Read-Host "Are all prerequisites met? (y/n)"
if ($ready -ne 'y') {
    Write-Host "Prerequisites not met. Please complete Tasks 13 and 14 first." -ForegroundColor $colorFail
    exit 1
}

Write-Host ""
Write-Host "Starting command execution tests..." -ForegroundColor $colorInfo
Write-Host ""

# Helper function to record test result
function Record-TestResult {
    param(
        [string]$TestName,
        [string]$Description,
        [bool]$Passed,
        [string]$Notes = ""
    )

    $result = @{
        Name = $TestName
        Description = $Description
        Passed = $Passed
        Notes = $Notes
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }

    $script:testResults += $result

    if ($Passed) {
        $script:passedTests++
        Write-Host "  [PASS] $TestName" -ForegroundColor $colorPass
    } else {
        $script:failedTests++
        Write-Host "  [FAIL] $TestName" -ForegroundColor $colorFail
    }

    if ($Notes) {
        Write-Host "         Notes: $Notes" -ForegroundColor Gray
    }
}

# Test Case 1: HTTP URL Execution
Write-Host "Test 1: HTTP URL Command Execution" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [reader] linkto = http://reader.livedoor.com/reader/"
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: reader"
Write-Host "  3. Press Enter"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "Did the default browser open with the URL http://reader.livedoor.com/reader/?" -ForegroundColor $colorWarning
$result1 = Read-Host "(y/n)"
$notes1 = ""
if ($result1 -ne 'y') {
    $notes1 = Read-Host "What happened instead?"
}
Record-TestResult "Test 1: HTTP URL" "Process.Start opens HTTP URL in browser" ($result1 -eq 'y') $notes1
Write-Host ""

# Test Case 2: HTTPS URL Execution
Write-Host "Test 2: HTTPS URL Command Execution" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [mail] linkto = https://mail.google.com/?hl=ja"
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: mail"
Write-Host "  3. Press Enter"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "Did the default browser open with the URL https://mail.google.com/?hl=ja?" -ForegroundColor $colorWarning
$result2 = Read-Host "(y/n)"
$notes2 = ""
if ($result2 -ne 'y') {
    $notes2 = Read-Host "What happened instead?"
}
Record-TestResult "Test 2: HTTPS URL" "Process.Start opens HTTPS URL in browser" ($result2 -eq 'y') $notes2
Write-Host ""

# Test Case 3: Absolute File Path (Optional - may not exist)
Write-Host "Test 3: Application Execution (Absolute Path)" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [vim] linkto = c:\tools\vim\gvim.exe"
Write-Host ""
Write-Host "NOTE: This path may not exist on your system. That's OK!" -ForegroundColor $colorWarning
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: vim"
Write-Host "  3. Press Enter"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "What happened?" -ForegroundColor $colorWarning
Write-Host "  a) gvim.exe launched (if path exists)"
Write-Host "  b) System showed 'file not found' error"
Write-Host "  c) TeaLauncher crashed or showed error"
$result3 = Read-Host "Enter choice (a/b/c)"
$passed3 = ($result3 -eq 'a') -or ($result3 -eq 'b')
$notes3 = "Path behavior: " + $result3
Record-TestResult "Test 3: Absolute Path" "Process.Start handles absolute paths correctly" $passed3 $notes3
Write-Host ""

# Test Case 4: System Command (Notepad)
Write-Host "Test 4: System Command Execution" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [notepad] linkto = notepad"
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: notepad"
Write-Host "  3. Press Enter"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "Did Notepad application launch?" -ForegroundColor $colorWarning
$result4 = Read-Host "(y/n)"
$notes4 = ""
if ($result4 -ne 'y') {
    $notes4 = Read-Host "What happened instead?"
}
Record-TestResult "Test 4: System Command" "Process.Start resolves commands from PATH" ($result4 -eq 'y') $notes4
Write-Host ""

# Test Case 5: Command Prompt
Write-Host "Test 5: Command Prompt Execution" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [cmd] linkto = cmd.exe"
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: cmd"
Write-Host "  3. Press Enter"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "Did Command Prompt window open?" -ForegroundColor $colorWarning
$result5 = Read-Host "(y/n)"
$notes5 = ""
if ($result5 -ne 'y') {
    $notes5 = Read-Host "What happened instead?"
}
# Close the cmd window for next tests
Write-Host "Please close the Command Prompt window before continuing." -ForegroundColor $colorWarning
Read-Host "Press Enter when cmd.exe is closed"
Record-TestResult "Test 5: cmd.exe" "Process.Start launches Command Prompt" ($result5 -eq 'y') $notes5
Write-Host ""

# Test Case 6: Special Command - !version
Write-Host "Test 6: Special Command - !version" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [version] linkto = !version"
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: version"
Write-Host "  3. Press Enter"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "Did a version dialog/message appear?" -ForegroundColor $colorWarning
$result6 = Read-Host "(y/n)"
$versionInfo = ""
if ($result6 -eq 'y') {
    $versionInfo = Read-Host "What version was displayed?"
}
$notes6 = if ($versionInfo) { "Version: $versionInfo" } else { "" }
Record-TestResult "Test 6: !version" "Special command shows version information" ($result6 -eq 'y') $notes6
Write-Host ""

# Test Case 7: Special Command - !reload
Write-Host "Test 7: Special Command - !reload" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [reload_config] linkto = !reload"
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: reload_config"
Write-Host "  3. Press Enter"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "Did the configuration reload without errors?" -ForegroundColor $colorWarning
Write-Host "(Input window should hide, no error dialogs)" -ForegroundColor Gray
$result7 = Read-Host "(y/n)"
$notes7 = ""
if ($result7 -ne 'y') {
    $notes7 = Read-Host "What error occurred?"
}
Record-TestResult "Test 7: !reload" "Special command reloads configuration" ($result7 -eq 'y') $notes7
Write-Host ""

# Test Case 8: Command with Arguments
Write-Host "Test 8: Command with Arguments" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [edit_config] linkto = notepad conf/my.conf"
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: edit_config"
Write-Host "  3. Press Enter"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "Did Notepad open with conf/my.conf loaded?" -ForegroundColor $colorWarning
$result8 = Read-Host "(y/n)"
$notes8 = ""
if ($result8 -ne 'y') {
    $notes8 = Read-Host "What happened instead?"
}
# Close notepad for next tests
Write-Host "Please close Notepad before continuing." -ForegroundColor $colorWarning
Read-Host "Press Enter when Notepad is closed"
Record-TestResult "Test 8: Arguments" "Process.Start passes arguments correctly" ($result8 -eq 'y') $notes8
Write-Host ""

# Test Case 9: Execution Performance
Write-Host "Test 9: Command Execution Performance" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type any command (e.g., notepad)"
Write-Host "  3. Press Enter"
Write-Host "  4. Observe the response time"
Write-Host ""
Read-Host "Press Enter when ready to proceed"
Write-Host ""
Write-Host "Did the command execute immediately (< 100ms perceived)?" -ForegroundColor $colorWarning
Write-Host "(Input window should hide instantly, command should start quickly)" -ForegroundColor Gray
$result9 = Read-Host "(y/n)"
$notes9 = ""
if ($result9 -ne 'y') {
    $notes9 = Read-Host "How long did it take (estimate)?"
}
# Close any opened application
Write-Host "Please close any opened applications before continuing." -ForegroundColor $colorWarning
Read-Host "Press Enter when ready"
Record-TestResult "Test 9: Performance" "Command execution is fast (< 100ms)" ($result9 -eq 'y') $notes9
Write-Host ""

# Test Case 10: Special Command - !exit
Write-Host "Test 10: Special Command - !exit (FINAL TEST)" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Configuration: [exit] linkto = !exit"
Write-Host ""
Write-Host "WARNING: This will close TeaLauncher!" -ForegroundColor $colorWarning
Write-Host ""
Write-Host "Steps:"
Write-Host "  1. Press Ctrl+Space to show input window"
Write-Host "  2. Type: exit"
Write-Host "  3. Press Enter"
Write-Host "  4. Observe that TeaLauncher exits gracefully"
Write-Host ""
Read-Host "Press Enter when ready to proceed with final test"
Write-Host ""
Write-Host "After executing the !exit command:" -ForegroundColor $colorWarning
Write-Host ""
$result10_exit = Read-Host "Did the application exit gracefully? (y/n)"
$result10_tray = Read-Host "Did the system tray icon disappear? (y/n)"
$result10_error = Read-Host "Were there any error messages or crashes? (y/n)"
$passed10 = ($result10_exit -eq 'y') -and ($result10_tray -eq 'y') -and ($result10_error -eq 'n')
$notes10 = "Exit: $result10_exit, Tray: $result10_tray, Errors: $result10_error"
Record-TestResult "Test 10: !exit" "Special command exits application gracefully" $passed10 $notes10
Write-Host ""

# Test Summary
Write-Host ""
Write-Host "=" * 80 -ForegroundColor $colorInfo
Write-Host " TEST SUMMARY" -ForegroundColor $colorInfo
Write-Host "=" * 80 -ForegroundColor $colorInfo
Write-Host ""
Write-Host "Total Tests: $totalTests" -ForegroundColor $colorInfo
Write-Host "Passed:      $passedTests" -ForegroundColor $colorPass
Write-Host "Failed:      $failedTests" -ForegroundColor $(if ($failedTests -eq 0) { $colorPass } else { $colorFail })
Write-Host ""

if ($passedTests -eq $totalTests) {
    Write-Host "SUCCESS: All command execution tests passed!" -ForegroundColor $colorPass
    Write-Host "Process.Start and command execution work correctly on .NET 8." -ForegroundColor $colorPass
} else {
    Write-Host "ATTENTION: Some tests failed. Please review the results." -ForegroundColor $colorWarning
}

Write-Host ""
Write-Host "Detailed Results:" -ForegroundColor $colorInfo
Write-Host "-" * 80

foreach ($result in $testResults) {
    $status = if ($result.Passed) { "[PASS]" } else { "[FAIL]" }
    $statusColor = if ($result.Passed) { $colorPass } else { $colorFail }

    Write-Host "$status $($result.Name)" -ForegroundColor $statusColor
    Write-Host "       Description: $($result.Description)" -ForegroundColor Gray
    if ($result.Notes) {
        Write-Host "       Notes: $($result.Notes)" -ForegroundColor Gray
    }
    Write-Host "       Time: $($result.Timestamp)" -ForegroundColor Gray
    Write-Host ""
}

# System Information
Write-Host ""
Write-Host "System Information:" -ForegroundColor $colorInfo
Write-Host "-" * 80
Write-Host "Windows Version: $([System.Environment]::OSVersion.VersionString)"
Write-Host ".NET Runtime: $(dotnet --version 2>$null)"
Write-Host "Test Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host ""

# Save results to file
Write-Host "Saving results to: $resultFile" -ForegroundColor $colorInfo

$output = @"
================================================================================
TeaLauncher .NET 8 Migration - Task 15: Command Execution Testing
================================================================================

Test Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Windows Version: $([System.Environment]::OSVersion.VersionString)
.NET Runtime: $(dotnet --version 2>$null)

================================================================================
TEST SUMMARY
================================================================================

Total Tests: $totalTests
Passed:      $passedTests
Failed:      $failedTests

Status: $(if ($passedTests -eq $totalTests) { "ALL TESTS PASSED" } else { "SOME TESTS FAILED" })

================================================================================
DETAILED RESULTS
================================================================================

"@

foreach ($result in $testResults) {
    $status = if ($result.Passed) { "PASS" } else { "FAIL" }
    $output += @"
[$status] $($result.Name)
    Description: $($result.Description)
    Notes: $($result.Notes)
    Timestamp: $($result.Timestamp)

"@
}

$output += @"
================================================================================
REQUIREMENTS COVERAGE
================================================================================

Requirement 5.6 (Process.Start compatibility): $(if ($testResults[0..4] | Where-Object { -not $_.Passed }) { "FAIL" } else { "PASS" })
Requirement 6.1 (Configuration loading):        $(if ($testResults[0..7] | Where-Object { -not $_.Passed }) { "FAIL" } else { "PASS" })
Requirement 6.2 (Configuration compatibility):  $(if ($testResults[5..7] | Where-Object { -not $_.Passed }) { "FAIL" } else { "PASS" })
Requirement 6.3 (Special commands):             $(if ($testResults[5,6,9] | Where-Object { -not $_.Passed }) { "FAIL" } else { "PASS" })

================================================================================
NOTES
================================================================================

Command execution testing verifies that TeaLauncher can:
- Launch URLs in the default browser (HTTP/HTTPS)
- Execute system commands and applications
- Pass arguments to launched processes
- Handle special commands (!version, !reload, !exit)
- Maintain compatibility with existing configuration files

All functionality uses Process.Start which must work identically on .NET 8
as it did on .NET Framework 3.5.

================================================================================
END OF REPORT
================================================================================
"@

$output | Out-File -FilePath $resultFile -Encoding UTF8

Write-Host "Results saved successfully!" -ForegroundColor $colorPass
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor $colorInfo
Write-Host "  1. Review the detailed test plan: docs/manual-test-task15.md"
Write-Host "  2. Document any issues found"
Write-Host "  3. If all tests passed, mark Task 15 as complete"
Write-Host "  4. Proceed to Task 16: Configuration file parsing testing"
Write-Host ""
Write-Host "Thank you for testing TeaLauncher .NET 8 migration!" -ForegroundColor $colorInfo
Write-Host ""

# Pause before exit
Read-Host "Press Enter to exit"
