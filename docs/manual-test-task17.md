# Task 17: Performance and Memory Verification

**Objective**: Verify that TeaLauncher on .NET 8 meets or exceeds performance targets.

## Performance Targets

- **Startup Time**: ≤ 300ms (from process start to system tray icon appears)
- **Hotkey Response Time**: < 100ms (from Ctrl+Space keypress to input window visible)
- **Memory Footprint**: ≤ 20MB after 60 seconds idle (ideally ≤ 15MB)

## Prerequisites

- Windows 10 (version 1607+) or Windows 11
- TeaLauncher built in Release configuration
- Both framework-dependent and self-contained builds available
- Real hardware (not VM) for accurate measurements
- Close unnecessary applications before testing

## Test Environment Setup

1. **Build Release configuration on Linux**:
   ```bash
   # Framework-dependent build
   dotnet publish -c Release -r win-x64 CommandLauncher/CommandLauncher.csproj

   # Self-contained build
   dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true CommandLauncher/CommandLauncher.csproj
   ```

2. **Transfer to Windows test machine**:
   - Copy published executables from Linux
   - Copy `resource/conf/my.conf` maintaining directory structure
   - Ensure .NET 8 Desktop Runtime installed (for framework-dependent build)

3. **Close background applications**:
   - Close unnecessary applications to reduce measurement noise
   - Disable Windows Defender real-time scanning temporarily (optional, for cleaner results)
   - Close web browsers, IDEs, etc.

## Automated Testing

Run the automated performance measurement script:

```powershell
.\test-performance.ps1
```

The script will:
1. Measure startup time (3 runs, averaged)
2. Measure hotkey response time (3 runs, averaged)
3. Measure memory footprint after 60 seconds idle
4. Generate a performance report

## Manual Testing Procedure

If you prefer manual measurements or the script fails:

### 1. Startup Time Measurement

**Method 1: PowerShell Measure-Command**
```powershell
# Framework-dependent build
Measure-Command { Start-Process "CommandLauncher.exe" -ArgumentList "resource\conf\my.conf" -PassThru } | Select-Object TotalMilliseconds

# Wait for system tray icon to appear, then repeat 2 more times
# Average the 3 measurements
```

**Method 2: Manual stopwatch**
1. Have a stopwatch ready (phone app or physical stopwatch)
2. Start stopwatch as you double-click CommandLauncher.exe
3. Stop stopwatch when system tray icon appears
4. Record time in milliseconds
5. Close application with !exit command
6. Repeat 2 more times and calculate average

**Target**: ≤ 300ms (average of 3 runs)

### 2. Hotkey Response Time Measurement

**Method 1: Manual stopwatch with video**
1. Launch CommandLauncher.exe with `resource\conf\my.conf`
2. Use phone camera to record screen in slow motion (if available)
3. Press Ctrl+Space
4. Review video frame-by-frame to measure time from keypress to window visible
5. Repeat 3 times and average

**Method 2: Manual estimation**
1. Launch CommandLauncher.exe
2. Press Ctrl+Space multiple times
3. Window should appear "instantly" (< 100ms is imperceptible to humans)
4. If there's any noticeable delay, it exceeds target

**Target**: < 100ms (should feel instant)

### 3. Memory Footprint Measurement

**Using Task Manager**:
1. Launch CommandLauncher.exe with `resource\conf\my.conf`
2. Open Task Manager (Ctrl+Shift+Esc)
3. Go to "Details" tab
4. Find "CommandLauncher.exe" process
5. Right-click columns header, select "Select columns"
6. Enable "Private Working Set" and "Working Set (Memory)"
7. Wait 60 seconds without any user interaction
8. Record "Private Working Set" value (in KB, convert to MB)
9. Optionally record "Working Set (Memory)" as well

**Using PowerShell**:
```powershell
# Launch application first, then run:
Start-Sleep -Seconds 60
Get-Process CommandLauncher | Select-Object ProcessName, @{Name="PrivateMemoryMB";Expression={[math]::Round($_.PrivateMemorySize64/1MB, 2)}}, @{Name="WorkingSetMB";Expression={[math]::Round($_.WorkingSet64/1MB, 2)}}
```

**Target**: Private Working Set ≤ 20MB (ideally ≤ 15MB)

## Test Matrix

Test both build types:

| Build Type | Startup Time | Hotkey Response | Memory (60s idle) | Notes |
|------------|--------------|-----------------|-------------------|-------|
| Framework-dependent | ___ ms | ___ ms | ___ MB | Requires .NET 8 Runtime |
| Self-contained | ___ ms | ___ ms | ___ MB | Standalone, larger file |

## Expected Results

### Baseline Comparison (.NET Framework 3.5)
- Startup time: ~200ms (typical)
- Hotkey response: ~50ms (typical)
- Memory idle: ~10-15MB (typical)

### .NET 8 Performance Goals
- Startup time should be ≤ 300ms (allowing some overhead for .NET 8 JIT)
- Hotkey response should be < 100ms (instant feel)
- Memory should be ≤ 20MB (comparable to .NET Framework)

### Pass Criteria
- All three metrics meet targets on both build types
- No performance regressions compared to .NET Framework baseline
- Application feels responsive and lightweight

## Test Results

### Framework-Dependent Build

**Startup Time** (3 runs):
- Run 1: ___ ms
- Run 2: ___ ms
- Run 3: ___ ms
- Average: ___ ms
- **Result**: [ ] PASS (≤ 300ms) [ ] FAIL

**Hotkey Response Time** (3 runs):
- Run 1: ___ ms
- Run 2: ___ ms
- Run 3: ___ ms
- Average: ___ ms
- **Result**: [ ] PASS (< 100ms) [ ] FAIL

**Memory Footprint** (after 60s idle):
- Private Working Set: ___ MB
- Working Set: ___ MB (optional)
- **Result**: [ ] PASS (≤ 20MB) [ ] FAIL

### Self-Contained Build

**Startup Time** (3 runs):
- Run 1: ___ ms
- Run 2: ___ ms
- Run 3: ___ ms
- Average: ___ ms
- **Result**: [ ] PASS (≤ 300ms) [ ] FAIL

**Hotkey Response Time** (3 runs):
- Run 1: ___ ms
- Run 2: ___ ms
- Run 3: ___ ms
- Average: ___ ms
- **Result**: [ ] PASS (< 100ms) [ ] FAIL

**Memory Footprint** (after 60s idle):
- Private Working Set: ___ MB
- Working Set: ___ MB (optional)
- **Result**: [ ] PASS (≤ 20MB) [ ] FAIL

## Overall Assessment

- [ ] All performance targets met
- [ ] Performance comparable to .NET Framework 3.5
- [ ] No performance regressions detected
- [ ] Application feels responsive and lightweight

**Notes**: _______________________________________________________________

**Tested by**: _________________ **Date**: _________________

**Windows Version**: _________________

**.NET 8 Runtime Version**: _________________
