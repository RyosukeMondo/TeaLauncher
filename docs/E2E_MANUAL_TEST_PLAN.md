# End-to-End Manual Test Plan for TeaLauncher Avalonia

This document provides a comprehensive manual testing checklist for validating TeaLauncher on Windows 10 and Windows 11. These tests must be performed on actual Windows installations as they involve UI interactions, global hotkeys, and Windows-specific APIs.

## Test Environment Requirements

- **Operating Systems**: Windows 10 (version 1809+) and Windows 11
- **Prerequisites**:
  - TeaLauncher.exe (built with `scripts/build-windows.sh`)
  - Sample `commands.yaml` file
  - Administrator privileges (for some tests)

## Pre-Test Setup

1. Build the application:
   ```bash
   cd /path/to/TeaLauncher
   ./scripts/build-windows.sh
   ```

2. Copy to Windows test machine:
   - `TeaLauncher.Avalonia/bin/Release/net8.0-windows/win-x64/publish/TeaLauncher.exe`
   - `TeaLauncher.Avalonia/commands.yaml`

3. Create test `commands.yaml`:
   ```yaml
   commands:
     - name: google
       linkto: https://google.com
       description: Google search engine

     - name: github
       linkto: https://github.com

     - name: notepad
       linkto: C:\Windows\System32\notepad.exe
       description: Windows Notepad

     - name: calc
       linkto: calc.exe
       arguments: /standard

     - name: reload
       linkto: '!reload'
       description: Reload configuration

     - name: exit
       linkto: '!exit'
       description: Exit application

     - name: version
       linkto: '!version'
       description: Show version info
   ```

---

## Test Suite

### 1. Application Startup and Initialization

**Requirement Coverage**: 6.2, 6.4

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 1.1 | Double-click `TeaLauncher.exe` | Application starts without error, no window visible, process appears in Task Manager | ☐ Pass ☐ Fail |
| 1.2 | Check startup time | Application ready within 300ms (check logs or use profiler) | ☐ Pass ☐ Fail |
| 1.3 | Verify memory usage | Memory usage ≤ 15MB when idle (check Task Manager) | ☐ Pass ☐ Fail |
| 1.4 | Missing `commands.yaml` | Error dialog appears with clear message about missing file | ☐ Pass ☐ Fail |
| 1.5 | Start from command line with custom config | `TeaLauncher.exe custom.yaml` loads custom configuration | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 2. Global Hotkey Registration (Ctrl+Space)

**Requirement Coverage**: 2.1, 2.2, 2.3, 2.4, 2.5, 7.1, 7.2, 7.3, 7.4, 7.5

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 2.1 | Press Ctrl+Space from desktop | Launcher window appears centered, focused, topmost | ☐ Pass ☐ Fail |
| 2.2 | Press Ctrl+Space from another app (Chrome, notepad) | Launcher window appears over the active application | ☐ Pass ☐ Fail |
| 2.3 | Window visible but not focused, press Ctrl+Space | Window becomes focused (not hidden) | ☐ Pass ☐ Fail |
| 2.4 | Window visible and focused, press Ctrl+Space | Window hides | ☐ Pass ☐ Fail |
| 2.5 | Hotkey response time | Window appears within 100ms of keypress (subjective: feels instant) | ☐ Pass ☐ Fail |
| 2.6 | Start app, close app, verify cleanup | No error on exit, hotkey unregistered (test by running again) | ☐ Pass ☐ Fail |
| 2.7 | Run two instances (if allowed) | Second instance either blocks or uses different hotkey ID | ☐ Pass ☐ Fail |

**Hotkey Conflict Testing**:

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 2.8 | Pre-register Ctrl+Space with AutoHotkey or another tool | TeaLauncher tries alternative hotkey IDs, shows warning if all fail | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 3. Window Appearance and UI

**Requirement Coverage**: 1.1, 1.7

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 3.1 | Window size | Window is 500x80 pixels | ☐ Pass ☐ Fail |
| 3.2 | Window position | Window appears centered on screen | ☐ Pass ☐ Fail |
| 3.3 | Window decorations | Window has no title bar or borders (borderless) | ☐ Pass ☐ Fail |
| 3.4 | Window topmost | Window stays on top of all other windows | ☐ Pass ☐ Fail |
| 3.5 | Window taskbar | Window does not appear in taskbar | ☐ Pass ☐ Fail |
| 3.6 | Background blur effect | Window has translucent background with blur (on Windows 10+) | ☐ Pass ☐ Fail |
| 3.7 | Input box focus | Input box is focused when window appears | ☐ Pass ☐ Fail |
| 3.8 | Input box watermark | Placeholder text visible when input is empty | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 4. Auto-Completion Behavior

**Requirement Coverage**: 1.2, 5.3

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 4.1 | Type "g" | Dropdown shows "google", "github" (prefix match) | ☐ Pass ☐ Fail |
| 4.2 | Type "go" | Dropdown shows only "google" | ☐ Pass ☐ Fail |
| 4.3 | Type "xyz" (no match) | Dropdown is empty or hidden | ☐ Pass ☐ Fail |
| 4.4 | Type "n" | Dropdown shows "notepad" | ☐ Pass ☐ Fail |
| 4.5 | Auto-completion latency | Dropdown appears within 10ms (feels instant) | ☐ Pass ☐ Fail |
| 4.6 | Clear input, dropdown | Dropdown disappears when input cleared | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 5. Tab Completion

**Requirement Coverage**: 1.3

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 5.1 | Type "g", press Tab | Input completes to longest common prefix ("g" if "google"/"github", or full name if unique) | ☐ Pass ☐ Fail |
| 5.2 | Type "go", press Tab | Input completes to "google" (unique match) | ☐ Pass ☐ Fail |
| 5.3 | Type "x" (no match), press Tab | Nothing happens or input unchanged | ☐ Pass ☐ Fail |
| 5.4 | Empty input, press Tab | Nothing happens | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 6. Command Execution (Enter Key)

**Requirement Coverage**: 1.4, 5.1, 5.4, 5.5

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 6.1 | Type "google", press Enter | Google.com opens in default browser, window hides | ☐ Pass ☐ Fail |
| 6.2 | Type "notepad", press Enter | Notepad opens, window hides | ☐ Pass ☐ Fail |
| 6.3 | Type "calc", press Enter | Calculator opens with `/standard` argument | ☐ Pass ☐ Fail |
| 6.4 | Type invalid command "xyz", press Enter | Error dialog displays exception message | ☐ Pass ☐ Fail |
| 6.5 | Empty input, press Enter | Nothing happens OR window hides (implementation choice) | ☐ Pass ☐ Fail |
| 6.6 | Command execution time | Process starts immediately (< 50ms perceived delay) | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 7. Escape Key Behavior

**Requirement Coverage**: 1.5

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 7.1 | Type "google", press Escape | Input clears, window stays visible | ☐ Pass ☐ Fail |
| 7.2 | Press Escape again (input empty) | Window hides | ☐ Pass ☐ Fail |
| 7.3 | Show window, press Escape immediately | Window hides (input already empty) | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 8. Special Commands

**Requirement Coverage**: 5.2

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 8.1 | Type "reload", press Enter | Configuration reloads, success message or silent reload | ☐ Pass ☐ Fail |
| 8.2 | Type "exit", press Enter | Application exits cleanly | ☐ Pass ☐ Fail |
| 8.3 | Type "version", press Enter | Version information displays in dialog | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 9. Configuration Reload (!reload)

**Requirement Coverage**: 8.1, 8.2, 8.3, 8.5

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 9.1 | Edit `commands.yaml` (add "gitlab" command), execute !reload | "gitlab" appears in auto-completion | ☐ Pass ☐ Fail |
| 9.2 | Execute !reload with valid config | All commands updated, auto-completion refreshed | ☐ Pass ☐ Fail |
| 9.3 | Delete `commands.yaml`, execute !reload | Error dialog with file path | ☐ Pass ☐ Fail |
| 9.4 | Add syntax error to `commands.yaml`, execute !reload | Error dialog with line/column information | ☐ Pass ☐ Fail |
| 9.5 | Add duplicate command names, execute !reload | Error dialog about duplicates (if validated) | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 10. IME Control (Japanese Windows or IME Installed)

**Requirement Coverage**: 3.1, 3.2, 3.3

*Skip if IME not available*

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 10.1 | Enable Japanese IME, press Ctrl+Space | Window appears, IME is in alphanumeric mode (off) | ☐ Pass ☐ Fail |
| 10.2 | IME active before showing window | Window shows, IME resets (off-on-off sequence) | ☐ Pass ☐ Fail |
| 10.3 | Hide window | IME state unchanged from when window was visible | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 11. Error Handling and Robustness

**Requirement Coverage**: 8.2, 8.3, 5.4

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 11.1 | Invalid YAML syntax in config | Clear error message with line number | ☐ Pass ☐ Fail |
| 11.2 | Missing required field (linkto) | Validation error listing missing fields | ☐ Pass ☐ Fail |
| 11.3 | Unknown fields in YAML | Ignored gracefully, no error | ☐ Pass ☐ Fail |
| 11.4 | Execute non-existent executable | Error dialog with exception details | ☐ Pass ☐ Fail |
| 11.5 | Very large config (1000+ commands) | Loads in < 200ms, auto-completion still responsive | ☐ Pass ☐ Fail |
| 11.6 | Unicode/emoji in commands | Displays and executes correctly | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 12. Cross-Compilation Validation

**Requirement Coverage**: 6.1, 6.2, 6.3, 6.4, 6.5

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 12.1 | Build on Linux with `scripts/build-windows.sh` | Build succeeds without errors | ☐ Pass ☐ Fail |
| 12.2 | Run produced exe on Windows 10 | Application runs successfully | ☐ Pass ☐ Fail |
| 12.3 | Run produced exe on Windows 11 | Application runs successfully | ☐ Pass ☐ Fail |
| 12.4 | Check exe dependencies | All .NET 8 runtime dependencies included (self-contained) | ☐ Pass ☐ Fail |
| 12.5 | Verify single-file deployment | Only TeaLauncher.exe needed (no DLL files) | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 13. Performance Testing

**Requirement Coverage**: Performance NFRs

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 13.1 | Startup time (cold start) | ≤ 300ms to ready state | ☐ Pass ☐ Fail |
| 13.2 | Hotkey response time | ≤ 100ms from Ctrl+Space to window visible | ☐ Pass ☐ Fail |
| 13.3 | Memory usage (idle) | ≤ 15MB RAM | ☐ Pass ☐ Fail |
| 13.4 | Auto-completion latency | ≤ 10ms for 1000 commands | ☐ Pass ☐ Fail |
| 13.5 | Config load time (100 commands) | ≤ 200ms | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 14. Compatibility Testing

**Requirement Coverage**: Compatibility NFRs

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 14.1 | Windows 10 version 1809 | All features work | ☐ Pass ☐ Fail |
| 14.2 | Windows 10 version 21H2 | All features work | ☐ Pass ☐ Fail |
| 14.3 | Windows 11 version 21H2 | All features work | ☐ Pass ☐ Fail |
| 14.4 | Windows 11 version 22H2 | All features work | ☐ Pass ☐ Fail |
| 14.5 | Windows 11 version 23H2 | All features work | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

### 15. Resource Cleanup and Exit

**Requirement Coverage**: Reliability NFRs

| Test ID | Test Case | Expected Result | Status |
|---------|-----------|-----------------|--------|
| 15.1 | Execute !exit command | Application exits cleanly, no errors | ☐ Pass ☐ Fail |
| 15.2 | Close via Task Manager | Application terminates, hotkey unregistered | ☐ Pass ☐ Fail |
| 15.3 | Check hotkey after exit | Ctrl+Space no longer registered (verify by running another app) | ☐ Pass ☐ Fail |
| 15.4 | Check process after exit | No TeaLauncher.exe in Task Manager | ☐ Pass ☐ Fail |
| 15.5 | Restart application after exit | Application starts successfully again | ☐ Pass ☐ Fail |

**Notes**: ___________________________________________

---

## Test Execution Summary

**Test Date**: ___________
**Tester Name**: ___________
**Windows Version**: ___________
**Build Version**: ___________

**Overall Results**:
- Total Tests: ___________
- Passed: ___________
- Failed: ___________
- Skipped: ___________

**Critical Issues Found**:
1. ___________________________________________
2. ___________________________________________
3. ___________________________________________

**Sign-off**: ___________

---

## Automated Test Coverage

The following requirements are covered by automated tests in `TeaLauncher.Avalonia.Tests`:

- **Configuration Tests**: `YamlConfigLoaderTests.cs` (Requirements 4.1-4.8)
- **Integration Tests**: `CommandExecutionTests.cs` (Requirements 5.1-5.5, 8.1-8.3)
- **End-to-End Validation**: `EndToEndValidationTests.cs` (All YAML and configuration requirements)
- **Component Tests**: `WindowsHotkeyTests.cs`, `WindowsIMEControllerTests.cs`, `MainWindowTests.cs`

**Note**: UI interaction tests (hotkey, window display, IME) cannot be fully automated and require manual validation on Windows.

---

## Troubleshooting Common Issues

### Issue: Application won't start
- **Check**: .NET 8 runtime installed (for non-self-contained builds)
- **Check**: commands.yaml exists in same directory as exe
- **Check**: Windows version is 1809+

### Issue: Hotkey not working
- **Check**: Another application using Ctrl+Space (try AutoHotkey detection)
- **Check**: Run as administrator (some hotkeys require elevation)
- **Check**: Event Viewer for errors

### Issue: Window doesn't appear
- **Check**: Window might be on another monitor (check all screens)
- **Check**: Resolution/scaling settings
- **Check**: Application logs (if available)

### Issue: Blur effect not working
- **Check**: Windows 10 version 1809+ required
- **Check**: Transparency effects enabled in Windows settings
- **Check**: DWM (Desktop Window Manager) running

### Issue: Commands not executing
- **Check**: commands.yaml syntax is valid
- **Check**: Paths to executables are correct
- **Check**: Permissions to execute commands

---

## Appendix: Example Test Commands YAML

```yaml
commands:
  # Web URLs
  - name: google
    linkto: https://google.com
    description: Google search engine

  - name: github
    linkto: https://github.com
    description: GitHub website

  - name: stackoverflow
    linkto: https://stackoverflow.com

  # Windows executables
  - name: notepad
    linkto: C:\Windows\System32\notepad.exe
    description: Windows Notepad

  - name: calc
    linkto: calc.exe
    description: Windows Calculator

  - name: paint
    linkto: mspaint.exe

  # Executables with arguments
  - name: notepad-test
    linkto: C:\Windows\System32\notepad.exe
    arguments: C:\test.txt

  # Special commands
  - name: reload
    linkto: '!reload'
    description: Reload configuration

  - name: exit
    linkto: '!exit'
    description: Exit application

  - name: version
    linkto: '!version'
    description: Show version info

  # Test cases
  - name: unicode-test
    linkto: https://example.com
    description: Unicode テスト 🚀

  - name: long-description
    linkto: https://example.com
    description: This is a very long description to test how the UI handles longer text that might overflow or need truncation in the display
```
