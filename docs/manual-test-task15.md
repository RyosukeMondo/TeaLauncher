# Manual Test Plan - Task 15: Command Execution Testing

## Test Overview

**Task**: Manual testing on Windows - Command execution
**Requirements**: 5.6, 6.1, 6.2, 6.3
**Purpose**: Verify Process.Start and command execution work correctly on .NET 8
**Prerequisite**: Task 13 and 14 completed (application startup and UI interaction verified)

## Test Environment Requirements

### Hardware
- Windows 10 version 1607+ or Windows 11
- Real hardware (not virtualized if possible)
- Internet connection (for URL testing)

### Software
- .NET 8 Desktop Runtime installed (for framework-dependent build)
- Default web browser configured
- Text editor (Notepad) available
- Command prompt or PowerShell

### Application State
- TeaLauncher.exe must be running
- System tray icon visible
- Configuration file loaded: resource/conf/my.conf

## Test Cases

### Test Case 1: URL Command Execution (HTTP)
**Requirement**: 5.6, 6.1

**Configuration**:
```ini
[reader]
linkto = http://reader.livedoor.com/reader/
```

**Steps**:
1. Press Ctrl+Space to show input window
2. Type: `reader`
3. Press Enter

**Expected Result**:
- Default web browser opens
- URL http://reader.livedoor.com/reader/ is loaded
- Input window hides automatically after execution
- No errors or exceptions

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 2: URL Command Execution (HTTPS)
**Requirement**: 5.6, 6.1

**Configuration**:
```ini
[mail]
linkto = https://mail.google.com/?hl=ja
```

**Steps**:
1. Press Ctrl+Space to show input window
2. Type: `mail`
3. Press Enter

**Expected Result**:
- Default web browser opens
- URL https://mail.google.com/?hl=ja is loaded
- SSL/HTTPS protocol handled correctly
- Input window hides automatically

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 3: Application Execution (File Path - Absolute)
**Requirement**: 5.6, 6.1

**Configuration**:
```ini
[vim]
linkto = c:\tools\vim\gvim.exe
```

**Steps**:
1. Press Ctrl+Space to show input window
2. Type: `vim`
3. Press Enter

**Expected Result**:
- If gvim.exe exists at c:\tools\vim\gvim.exe, application launches
- If path does not exist, system shows "file not found" error
- TeaLauncher remains running (does not crash)
- Input window hides after execution attempt

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 4: Application Execution (Command Name - System Path)
**Requirement**: 5.6, 6.1

**Configuration**:
```ini
[notepad]
linkto = notepad
```

**Steps**:
1. Press Ctrl+Space to show input window
2. Type: `notepad`
3. Press Enter

**Expected Result**:
- Notepad application launches
- System resolves 'notepad' from PATH environment variable
- Process.Start correctly uses shell execution
- Input window hides after launch

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 5: Command Prompt Execution
**Requirement**: 5.6, 6.1

**Configuration**:
```ini
[cmd]
linkto = cmd.exe
```

**Steps**:
1. Press Ctrl+Space to show input window
2. Type: `cmd`
3. Press Enter

**Expected Result**:
- Command Prompt window opens
- cmd.exe process starts successfully
- TeaLauncher remains running in background
- Input window hides after execution

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 6: Special Command - !version
**Requirement**: 6.2, 6.3

**Configuration**:
```ini
[version]
linkto = !version
```

**Steps**:
1. Press Ctrl+Space to show input window
2. Type: `version`
3. Press Enter

**Expected Result**:
- Version dialog/message box appears
- Shows application version information
- Dialog can be closed with OK button
- Input window behavior appropriate for modal dialog

**Actual Result**: [ ] PASS [ ] FAIL

**Version Displayed**: _______________________________________________________________

**Notes**: _______________________________________________________________

---

### Test Case 7: Special Command - !reload
**Requirement**: 6.2, 6.3

**Configuration**:
```ini
[reload_config]
linkto = !reload
```

**Steps**:
1. Press Ctrl+Space to show input window
2. Type: `reload_config`
3. Press Enter

**Expected Result**:
- Configuration reloads from conf/my.conf file
- No errors or exceptions displayed
- New configuration becomes active immediately
- Input window hides after reload
- Can verify reload by testing a command after modifying my.conf

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 8: Special Command - !exit
**Requirement**: 6.2, 6.3

**Configuration**:
```ini
[exit]
linkto = !exit
```

**Steps**:
1. Press Ctrl+Space to show input window
2. Type: `exit`
3. Press Enter

**Expected Result**:
- Application exits gracefully
- System tray icon disappears
- All windows close
- No error messages or crash dialogs
- Process terminates cleanly (check Task Manager)

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 9: Application with Arguments
**Requirement**: 5.6, 6.1

**Configuration**:
```ini
[edit_config]
linkto = notepad conf/my.conf
```

**Steps**:
1. Ensure conf/my.conf exists in the application directory
2. Press Ctrl+Space to show input window
3. Type: `edit_config`
4. Press Enter

**Expected Result**:
- Notepad opens with conf/my.conf loaded
- Arguments are passed correctly to Process.Start
- File path is resolved relative to application directory
- Content of my.conf is displayed in Notepad

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 10: Command Execution Performance
**Requirement**: 5.6

**Steps**:
1. Press Ctrl+Space to show input window
2. Type a command name (any valid command)
3. Press Enter
4. Measure time from Enter press to:
   - Input window hiding
   - Target application/URL launching

**Expected Result**:
- Input window hides immediately (< 50ms subjectively)
- Command execution starts quickly (< 100ms to initiate)
- No noticeable delay in Process.Start
- Response feels instant to the user

**Actual Result**: [ ] PASS [ ] FAIL

**Measured Time (subjective)**: _______________________________________________________________

**Notes**: _______________________________________________________________

---

## Test Coverage Summary

### Process.Start Compatibility (Requirement 5.6)
- [ ] HTTP URLs (Process.Start with http:// protocol)
- [ ] HTTPS URLs (Process.Start with https:// protocol)
- [ ] Absolute file paths (Process.Start with full path)
- [ ] System commands (Process.Start with PATH resolution)
- [ ] Commands with arguments (Process.Start with argument parsing)

### Configuration Loading (Requirement 6.1)
- [ ] [section] syntax parsed correctly
- [ ] linkto = value syntax parsed correctly
- [ ] All commands from my.conf loaded successfully
- [ ] No parsing errors with existing configuration format

### Configuration Compatibility (Requirement 6.2)
- [ ] Existing .conf file works without modification
- [ ] Section names preserved
- [ ] Command targets preserved
- [ ] Dictionary<string, Dictionary<string, string>> works identically to Hashtable

### Special Commands (Requirement 6.3)
- [ ] !version command functions correctly
- [ ] !reload command reloads configuration
- [ ] !exit command exits application gracefully
- [ ] Special command prefix (!) recognized and handled

## Expected Behavior vs .NET Framework 3.5

The command execution on .NET 8 should be **identical** to .NET Framework 3.5:

| Feature | .NET Framework 3.5 | .NET 8 Expected |
|---------|-------------------|-----------------|
| Process.Start for URLs | Opens default browser | ✅ Opens default browser |
| Process.Start for files | Launches application | ✅ Launches application |
| Process.Start for system commands | Resolves from PATH | ✅ Resolves from PATH |
| Argument parsing | Space-separated args | ✅ Space-separated args |
| !version command | Shows version dialog | ✅ Shows version dialog |
| !reload command | Reloads configuration | ✅ Reloads configuration |
| !exit command | Exits gracefully | ✅ Exits gracefully |
| Execution performance | Immediate (< 100ms) | ✅ Immediate (< 100ms) |

## Known Considerations

### URL Handling
- Default browser must be configured in Windows
- Some URLs may require authentication (e.g., mail.google.com)
- Browser behavior is system-dependent (default browser settings)

### File Path Execution
- Absolute paths (c:\tools\vim\gvim.exe) may not exist on test machine
- Test what happens when path doesn't exist (should show OS error, not crash)
- Relative paths resolved from application working directory

### Process.Start Security
- .NET 8 may show security prompts for some executables
- Windows Defender or antivirus may scan launched processes
- User Account Control (UAC) may prompt for elevated applications

### Shell Execution
- Process.Start uses Windows shell for URL and system command resolution
- Shell execution required for commands like "notepad" without full path
- .NET 8 should maintain compatibility with UseShellExecute behavior

## Test Result Documentation

### Overall Test Status
- [ ] All tests passed
- [ ] Some tests failed (document below)
- [ ] Tests blocked by prerequisites

### Failed Tests (if any)
Test Case #: _______________
Failure Description: _______________________________________________________________
_______________________________________________________________

### Issues Found
1. _______________________________________________________________
2. _______________________________________________________________
3. _______________________________________________________________

### Notes and Observations
_______________________________________________________________
_______________________________________________________________
_______________________________________________________________

## Sign-Off

**Tester Name**: _______________________________________________________________

**Test Date**: _______________________________________________________________

**Environment**:
- Windows Version: _______________________________________________________________
- .NET 8 Runtime Version: _______________________________________________________________
- Build Type: [ ] Framework-dependent [ ] Self-contained

**Overall Assessment**:
- [ ] Ready for production - all tests passed
- [ ] Requires fixes - critical issues found
- [ ] Needs investigation - unexpected behavior observed

**Tester Signature**: _______________________________________________________________
