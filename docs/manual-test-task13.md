# Manual Test Plan - Task 13: Application Startup Testing

## Test Information
- **Task**: Manual testing on Windows - Application startup
- **Requirements**: 5.1 (Windows Forms compatibility), 8.1 (Testing)
- **Test Date**: 2025-10-19
- **Tester**: [To be filled by tester]

## Prerequisites

### Windows Machine Requirements
- Windows 10 (version 1607 or later) OR Windows 11
- For framework-dependent build: .NET 8 Desktop Runtime installed
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
  - Install: "Desktop Runtime" (not just SDK or ASP.NET Core Runtime)

### Files to Transfer from Linux Build

1. **Self-Contained Build** (recommended for first test - no runtime needed):
   - Source: `CommandLauncher/bin/Release/net8.0-windows/win-x64/publish/CommandLauncher.exe`
   - Size: ~155 MB (includes .NET 8 runtime)
   - Destination: Any folder on Windows (e.g., `C:\TeaLauncher\`)

2. **Framework-Dependent Build** (requires .NET 8 Desktop Runtime):
   - Source: `CommandLauncher/bin/Release/net8.0-windows/win-x64/CommandLauncher.exe`
   - Size: ~9.2 MB
   - Also copy all DLL files from the same directory
   - Destination: Any folder on Windows (e.g., `C:\TeaLauncher-FD\`)

3. **Configuration File** (required for both):
   - Source: `resource/conf/my.conf`
   - Destination: `resource\conf\my.conf` (maintain directory structure)

## Test Procedure

### Test 1: Self-Contained Build Startup

1. **Setup**:
   - Copy self-contained `CommandLauncher.exe` to test folder
   - Create `resource\conf\` subdirectory
   - Copy `my.conf` to `resource\conf\my.conf`

2. **Execute**:
   ```cmd
   CommandLauncher.exe resource\conf\my.conf
   ```

3. **Verify** (all must pass):
   - [ ] Application starts without error dialogs
   - [ ] No console errors or exceptions shown
   - [ ] System tray icon appears in notification area (TeaLauncher icon)
   - [ ] Startup feels fast (subjectively instant, target: ≤ 300ms)
   - [ ] No crashes or hangs

4. **Measure Startup Time** (optional but recommended):
   ```powershell
   Measure-Command { Start-Process -FilePath "CommandLauncher.exe" -ArgumentList "resource\conf\my.conf" }
   ```
   - Expected: TotalMilliseconds ≤ 300

5. **Close Application**:
   - Right-click system tray icon → Exit
   - Verify application closes cleanly

### Test 2: Framework-Dependent Build Startup

1. **Verify .NET 8 Runtime**:
   ```cmd
   dotnet --list-runtimes
   ```
   - Expected: `Microsoft.WindowsDesktop.App 8.0.x` listed

2. **Setup**:
   - Copy framework-dependent `CommandLauncher.exe` and DLLs to test folder
   - Create `resource\conf\` subdirectory
   - Copy `my.conf` to `resource\conf\my.conf`

3. **Execute**:
   ```cmd
   CommandLauncher.exe resource\conf\my.conf
   ```

4. **Verify** (same as Test 1):
   - [ ] Application starts without error dialogs
   - [ ] System tray icon appears
   - [ ] Startup time ≤ 300ms (should be faster than self-contained)
   - [ ] No crashes

## Success Criteria

All of the following must be TRUE:

- ✅ Self-contained build starts successfully on Windows
- ✅ Framework-dependent build starts successfully (with .NET 8 Runtime)
- ✅ System tray icon visible in both cases
- ✅ Startup time ≤ 300ms (subjectively fast)
- ✅ No error messages or crashes
- ✅ Application can be closed cleanly from system tray

## Test Results

### Self-Contained Build
- Tested on: [Windows version]
- Start time: [measured time in ms]
- System tray icon: [YES/NO]
- Errors: [NONE or describe]
- Result: [PASS/FAIL]

### Framework-Dependent Build
- .NET Runtime version: [e.g., 8.0.11]
- Tested on: [Windows version]
- Start time: [measured time in ms]
- System tray icon: [YES/NO]
- Errors: [NONE or describe]
- Result: [PASS/FAIL]

## Notes
- Test on real Windows hardware if possible (not Wine or VM)
- Both builds should produce identical behavior
- Self-contained build is larger but has no dependencies
- Framework-dependent build is smaller but requires .NET 8 Desktop Runtime

## Issues Found
[Document any issues, errors, or unexpected behavior here]

## Tester Sign-off
- Name: ___________________
- Date: ___________________
- Overall Result: [PASS/FAIL]
