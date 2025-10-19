# Interactive Configuration File Parsing Test Script
# Task 16: Manual testing on Windows - Configuration file parsing
# Requirements: 6.1, 6.2, 8.3
# Purpose: Guide tester through ConfigLoader Dictionary verification

param(
    [string]$ConfigPath = "resource\conf\my.conf",
    [string]$OutputFile = "test-results-config-parsing-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
)

# Test results collection
$script:TestResults = @()
$script:TestCount = 0
$script:PassCount = 0
$script:FailCount = 0

# Color codes for output
function Write-TestHeader {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-TestStep {
    param([string]$Message)
    Write-Host "[STEP] $Message" -ForegroundColor Yellow
}

function Write-TestPass {
    param([string]$Message)
    Write-Host "[PASS] $Message" -ForegroundColor Green
}

function Write-TestFail {
    param([string]$Message)
    Write-Host "[FAIL] $Message" -ForegroundColor Red
}

function Write-TestInfo {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor White
}

function Wait-ForKeyPress {
    param([string]$Message = "Press any key to continue...")
    Write-Host "`n$Message" -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

function Record-TestResult {
    param(
        [string]$TestName,
        [string]$Result,
        [string]$Notes = ""
    )

    $script:TestCount++
    $resultObj = [PSCustomObject]@{
        TestNumber = $script:TestCount
        TestName = $TestName
        Result = $Result
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Notes = $Notes
    }

    $script:TestResults += $resultObj

    if ($Result -eq "PASS") {
        $script:PassCount++
        Write-TestPass "$TestName - PASSED"
    } else {
        $script:FailCount++
        Write-TestFail "$TestName - FAILED"
    }
}

function Get-TestResult {
    param([string]$Prompt)

    do {
        $response = Read-Host "$Prompt (P=Pass, F=Fail, S=Skip)"
        $response = $response.ToUpper()
    } while ($response -notin @('P', 'F', 'S'))

    switch ($response) {
        'P' { return 'PASS' }
        'F' { return 'FAIL' }
        'S' { return 'SKIP' }
    }
}

function Backup-ConfigFile {
    if (Test-Path $ConfigPath) {
        $backupPath = "$ConfigPath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        Copy-Item $ConfigPath $backupPath
        Write-TestInfo "Configuration backed up to: $backupPath"
        return $backupPath
    } else {
        Write-TestFail "Configuration file not found: $ConfigPath"
        return $null
    }
}

function Restore-ConfigFile {
    param([string]$BackupPath)

    if ($BackupPath -and (Test-Path $BackupPath)) {
        Copy-Item $BackupPath $ConfigPath -Force
        Write-TestInfo "Configuration restored from backup"
    }
}

# Main test execution
Write-Host @"
╔════════════════════════════════════════════════════════════════╗
║  Configuration File Parsing Test - Task 16                     ║
║  Testing Dictionary-based ConfigLoader on .NET 8               ║
╚════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

Write-TestInfo "Test Requirements: 6.1, 6.2, 8.3"
Write-TestInfo "Purpose: Verify Dictionary<string, Dictionary<string, string>> works correctly"
Write-TestInfo "Output will be saved to: $OutputFile"
Write-Host ""

# System information
Write-TestHeader "System Information"
$osInfo = Get-CimInstance Win32_OperatingSystem
$dotnetVersion = (dotnet --version 2>$null)
Write-Host "OS: $($osInfo.Caption) Build $($osInfo.BuildNumber)"
Write-Host "PowerShell: $($PSVersionTable.PSVersion)"
Write-Host ".NET SDK: $dotnetVersion"
Write-Host "Config Path: $ConfigPath"

Wait-ForKeyPress

# Prerequisite checks
Write-TestHeader "Prerequisite Checks"

Write-TestStep "Checking if configuration file exists"
if (Test-Path $ConfigPath) {
    Write-TestPass "Configuration file found: $ConfigPath"
} else {
    Write-TestFail "Configuration file not found: $ConfigPath"
    Write-Host "Please ensure the application directory structure is correct."
    exit 1
}

Write-TestStep "Checking if TeaLauncher is running"
$teaProcess = Get-Process CommandLauncher -ErrorAction SilentlyContinue
if ($teaProcess) {
    Write-TestPass "TeaLauncher is running (PID: $($teaProcess.Id))"
} else {
    Write-TestFail "TeaLauncher is not running"
    Write-Host "Please start CommandLauncher.exe before running this test."
    $startNow = Read-Host "Start TeaLauncher now? (Y/N)"
    if ($startNow -eq 'Y') {
        Write-Host "Please start TeaLauncher manually and press any key when ready..."
        Wait-ForKeyPress
    } else {
        exit 1
    }
}

Wait-ForKeyPress

# Backup configuration
Write-TestHeader "Configuration Backup"
$backupPath = Backup-ConfigFile
if (-not $backupPath) {
    exit 1
}

Wait-ForKeyPress

# Test Case 1: Add New Simple URL Command
Write-TestHeader "Test Case 1: Add New Simple URL Command"
Write-TestStep "This test verifies adding a new URL command and reloading configuration"

Write-Host @"

Instructions:
1. Press Ctrl+Space to show TeaLauncher input window
2. Type: test (then press Tab)
3. Verify: No command 'test_url' exists yet
4. Press Escape to hide window
5. Open $ConfigPath in Notepad:
"@

Write-Host "   notepad $ConfigPath" -ForegroundColor Green

Write-Host @"

6. Add the following at the end of the file:

   [test_url]
   linkto = https://example.com

7. Save the file (Ctrl+S) but keep Notepad open
8. In TeaLauncher, press Ctrl+Space
9. Type: !reload (then press Enter)
10. Press Ctrl+Space again
11. Type: test (then press Tab)
12. Verify: 'test_url' now appears in auto-completion
13. Type full 'test_url' and press Enter
14. Verify: Browser opens https://example.com

"@

Wait-ForKeyPress "Press any key when you've completed the steps above..."

$result = Get-TestResult "Did the new command load and execute correctly?"
$notes = Read-Host "Notes (optional, press Enter to skip)"
Record-TestResult "Add New Simple URL Command" $result $notes

Wait-ForKeyPress

# Test Case 2: Add Command with Special Characters
Write-TestHeader "Test Case 2: Add Command with Special Characters"
Write-TestStep "Testing URL with query parameters and special characters"

Write-Host @"

Instructions:
1. In Notepad (with my.conf still open), add:

   [test_special]
   linkto = https://example.com/search?q=dotnet+8&lang=en

2. Save the file (Ctrl+S)
3. In TeaLauncher, press Ctrl+Space
4. Type: !reload (then press Enter)
5. Press Ctrl+Space
6. Type: test_sp (then press Tab)
7. Verify: 'test_special' appears
8. Press Enter to execute
9. Verify: Browser opens with full URL including ?q=dotnet+8&lang=en

"@

Wait-ForKeyPress "Press any key when you've completed the steps above..."

$result = Get-TestResult "Did the command with special characters work correctly?"
$notes = Read-Host "Notes (optional)"
Record-TestResult "Add Command with Special Characters" $result $notes

Wait-ForKeyPress

# Test Case 3: Add Application Command with Arguments
Write-TestHeader "Test Case 3: Add Application Command with Arguments"
Write-TestStep "Testing command execution with arguments"

Write-Host @"

Instructions:
1. In Notepad (with my.conf still open), add:

   [test_notepad]
   linkto = notepad.exe resource\conf\my.conf

2. Save the file (Ctrl+S)
3. In TeaLauncher: !reload
4. Type: test_note (press Tab)
5. Verify: 'test_notepad' appears
6. Press Enter
7. Verify: Notepad opens with my.conf file loaded
8. Close the Notepad window that just opened

"@

Wait-ForKeyPress "Press any key when you've completed the steps above..."

$result = Get-TestResult "Did the command with arguments execute correctly?"
$notes = Read-Host "Notes (optional)"
Record-TestResult "Add Application Command with Arguments" $result $notes

Wait-ForKeyPress

# Test Case 4: Add Multiple Commands in Batch
Write-TestHeader "Test Case 4: Add Multiple Commands in Batch"
Write-TestStep "Testing multiple new commands in single reload"

Write-Host @"

Instructions:
1. In Notepad, add these three sections:

   [test_google]
   linkto = https://google.com

   [test_github]
   linkto = https://github.com

   [test_cmd]
   linkto = cmd.exe /k echo Dictionary Test

2. Save the file (Ctrl+S)
3. In TeaLauncher: !reload
4. Test each command:
   - Type: test_goo + Tab → Should show 'test_google'
   - Type: test_git + Tab → Should show 'test_github'
   - Type: test_cm + Tab → Should show 'test_cmd'
5. Execute test_cmd (type full name and Enter)
6. Verify: CMD window opens with "Dictionary Test" message
7. Close the CMD window

"@

Wait-ForKeyPress "Press any key when you've completed the steps above..."

$result = Get-TestResult "Did all three commands load and work correctly?"
$notes = Read-Host "Notes (optional)"
Record-TestResult "Add Multiple Commands in Batch" $result $notes

Wait-ForKeyPress

# Test Case 5: Modify Existing Command
Write-TestHeader "Test Case 5: Modify Existing Command"
Write-TestStep "Testing Dictionary value update for existing key"

Write-Host @"

Instructions:
1. In Notepad, find the [test_url] section
2. Change:
   FROM: linkto = https://example.com
   TO:   linkto = https://microsoft.com

3. Save the file (Ctrl+S)
4. In TeaLauncher: !reload
5. Type: test_url (then Enter)
6. Verify: Browser opens https://microsoft.com (NOT example.com)

"@

Wait-ForKeyPress "Press any key when you've completed the steps above..."

$result = Get-TestResult "Did the modified command open the new URL?"
$notes = Read-Host "Notes (optional)"
Record-TestResult "Modify Existing Command" $result $notes

Wait-ForKeyPress

# Test Case 6: Delete Command and Reload
Write-TestHeader "Test Case 6: Delete Command and Reload"
Write-TestStep "Testing Dictionary key removal"

Write-Host @"

Instructions:
1. In Notepad, find and DELETE the entire [test_cmd] section:

   [test_cmd]
   linkto = cmd.exe /k echo Dictionary Test

   (Delete all 3 lines)

2. Save the file (Ctrl+S)
3. In TeaLauncher: !reload
4. Type: test_cm (press Tab)
5. Verify: 'test_cmd' does NOT appear
6. Type: test_goo (press Tab)
7. Verify: 'test_google' still works (not affected by deletion)

"@

Wait-ForKeyPress "Press any key when you've completed the steps above..."

$result = Get-TestResult "Was the command removed and others unaffected?"
$notes = Read-Host "Notes (optional)"
Record-TestResult "Delete Command and Reload" $result $notes

Wait-ForKeyPress

# Test Case 7: Configuration Parse Error Handling
Write-TestHeader "Test Case 7: Configuration Parse Error Handling"
Write-TestStep "Testing error handling with malformed configuration"

Write-Host @"

Instructions:
1. In Notepad, add this INTENTIONALLY BROKEN section:

   [test_broken
   linkto = https://example.com

   (Note: Missing closing bracket on section name)

2. Save the file (Ctrl+S)
3. In TeaLauncher: !reload
4. Observe: Application should show error OR log error
5. Verify: Application does NOT crash
6. Verify: Other commands still work
7. Fix the error: Change [test_broken to [test_broken]
8. Save and reload again
9. Verify: Application recovers, no errors

"@

Wait-ForKeyPress "Press any key when you've completed the steps above..."

$result = Get-TestResult "Did error handling work correctly (no crash, recovery after fix)?"
$notes = Read-Host "Notes (optional)"
Record-TestResult "Configuration Parse Error Handling" $result $notes

Wait-ForKeyPress

# Test Case 8: Regression - Original Commands Still Work
Write-TestHeader "Test Case 8: Regression Verification"
Write-TestStep "Verifying original commands unaffected by changes"

Write-Host @"

Instructions:
Test each original command:

1. Type: reader (press Enter)
   Verify: Opens http://reader.livedoor.com/reader/

2. Type: mail (press Enter)
   Verify: Opens Gmail

3. Type: notepad (press Enter)
   Verify: Opens Notepad application

4. Type: !version (press Enter)
   Verify: Shows version dialog

5. Type: !reload (press Enter)
   Verify: Reloads without error

"@

Wait-ForKeyPress "Press any key when you've tested all original commands..."

$result = Get-TestResult "Do all original commands still work correctly?"
$notes = Read-Host "Notes (optional)"
Record-TestResult "Regression - Original Commands" $result $notes

Wait-ForKeyPress

# Test Case 9: Dictionary Type Safety Verification
Write-TestHeader "Test Case 9: Dictionary Type Safety Verification"
Write-TestStep "Confirming no casting errors occurred during testing"

Write-Host @"

Technical Verification:

Throughout all previous tests, verify:
- No InvalidCastException errors occurred
- No type conversion errors in TeaLauncher
- All commands executed without type-related exceptions
- Dictionary provides compile-time type safety

This confirms that Dictionary<string, Dictionary<string, string>>
is working correctly, unlike the old Hashtable which required runtime casts.

"@

Wait-ForKeyPress "Press any key to continue..."

$result = Get-TestResult "Were there zero casting/type errors during all tests?"
$notes = Read-Host "Notes (optional)"
Record-TestResult "Dictionary Type Safety" $result $notes

Wait-ForKeyPress

# Cleanup
Write-TestHeader "Test Cleanup"

Write-Host @"

Cleanup Steps:
1. Close Notepad if still open
2. In TeaLauncher, type: !exit (press Enter)
3. Verify: Application closes cleanly

"@

Wait-ForKeyPress "Press any key after closing TeaLauncher..."

$restoreConfig = Read-Host "Restore original configuration? (Y/N)"
if ($restoreConfig -eq 'Y') {
    Restore-ConfigFile $backupPath
    Write-TestInfo "Configuration restored. Backup kept at: $backupPath"
} else {
    Write-TestInfo "Configuration not restored. Backup available at: $backupPath"
}

# Generate summary
Write-TestHeader "Test Summary"

$passRate = if ($script:TestCount -gt 0) {
    [math]::Round(($script:PassCount / $script:TestCount) * 100, 2)
} else {
    0
}

Write-Host "Total Tests: $script:TestCount"
Write-Host "Passed: $script:PassCount" -ForegroundColor Green
Write-Host "Failed: $script:FailCount" -ForegroundColor $(if ($script:FailCount -gt 0) { "Red" } else { "Gray" })
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -eq 100) { "Green" } else { "Yellow" })

# Save results to file
$output = @"
Configuration File Parsing Test Results
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Task: 16 - Manual testing on Windows - Configuration file parsing
Requirements: 6.1, 6.2, 8.3

System Information:
- OS: $($osInfo.Caption) Build $($osInfo.BuildNumber)
- PowerShell: $($PSVersionTable.PSVersion)
- .NET SDK: $dotnetVersion
- Config Path: $ConfigPath

Test Summary:
- Total Tests: $script:TestCount
- Passed: $script:PassCount
- Failed: $script:FailCount
- Pass Rate: $passRate%

Detailed Results:
$(foreach ($result in $script:TestResults) {
    "[$($result.TestNumber)] $($result.TestName): $($result.Result)"
    if ($result.Notes) {
        "    Notes: $($result.Notes)"
    }
    ""
})

Backup Configuration: $backupPath

Test Status: $(if ($passRate -eq 100) { "PASSED" } else { "FAILED" })
"@

$output | Out-File -FilePath $OutputFile -Encoding UTF8
Write-TestInfo "Results saved to: $OutputFile"

Write-Host "`n"
if ($passRate -eq 100) {
    Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║   ALL TESTS PASSED - TASK 16 COMPLETE ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Green
} else {
    Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║   SOME TESTS FAILED - REVIEW REQUIRED  ║" -ForegroundColor Red
    Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Red
}

Write-Host "`nPress any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
