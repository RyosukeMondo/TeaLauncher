# Manual Test Plan - Task 14: Hotkey and UI Interaction Testing

## Test Information
- **Task**: Manual testing on Windows - Hotkey and UI interaction
- **Requirements**: 5.1-5.5 (Windows Forms, hotkey, UI responsiveness)
- **Test Date**: 2025-10-19
- **Tester**: [To be filled by tester]

## Prerequisites

### Completion of Task 13
- Task 13 (Application Startup) must be completed successfully
- TeaLauncher.exe must be running with system tray icon visible
- Configuration file (my.conf) must be loaded

### Windows Machine Requirements
- Windows 10 (version 1607 or later) OR Windows 11
- Real hardware recommended (P/Invoke hotkey testing may not work reliably in VMs)
- Desktop environment with system tray/notification area
- Keyboard with working Ctrl, Space, Tab, and Escape keys

### Files Required
- `CommandLauncher.exe` (already running from Task 13)
- `resource\conf\my.conf` (already loaded)

## Test Overview

This test verifies three critical .NET 8 compatibility areas:
1. **P/Invoke functionality** - Windows API hotkey registration (RegisterHotKey/UnregisterHotKey)
2. **Windows Forms UI** - Input window display and focus management
3. **Auto-completion logic** - Tab key interaction and command matching

## Test Procedure

### Test 1: Global Hotkey Registration (Ctrl+Space)

**Purpose**: Verify P/Invoke RegisterHotKey API works on .NET 8

**Prerequisites**:
- TeaLauncher is running with system tray icon visible
- Input window is NOT currently visible
- Another application has focus (e.g., Notepad, browser, File Explorer)

**Steps**:
1. Open any application (e.g., Notepad)
2. Ensure the other application has keyboard focus
3. Press **Ctrl+Space** simultaneously

**Verify** (all must pass):
- [ ] Input window appears on screen
- [ ] Response time feels fast (subjectively < 100ms, ideally instant)
- [ ] Window appears at expected position (center or last position)
- [ ] Window size is appropriate (not minimized or maximized)
- [ ] Textbox is visible and ready for input
- [ ] No error dialogs appear

**Measure Response Time** (optional but recommended):
```powershell
# Use test-hotkey.ps1 script (if available)
# Or manually use a stopwatch/timer
# Target: < 100ms from keypress to window visible
```

**Expected Result**: Input window shows in < 100ms with no errors

---

### Test 2: UI Focus Management

**Purpose**: Verify Windows Forms focus behavior on .NET 8

**Prerequisites**:
- Input window is visible (from Test 1)

**Steps**:
1. Observe the input window textbox

**Verify** (all must pass):
- [ ] Textbox has keyboard focus (cursor blinking in textbox)
- [ ] Textbox is ready to receive input (no need to click)
- [ ] Window is in foreground (not behind other windows)
- [ ] Window title bar shows correct text (if applicable)

**Expected Result**: Textbox has focus automatically when window appears

---

### Test 3: Auto-completion - Prefix Matching

**Purpose**: Verify AutoCompleteMachine logic works with .NET 8

**Prerequisites**:
- Input window is visible with textbox focused
- `my.conf` contains commands (e.g., [github], [google], [gmail], etc.)

**Steps**:
1. Ensure textbox is empty
2. Type a partial command name that matches multiple commands
   - Example: Type "g" (should match: github, google, gmail, etc.)
3. Press **Tab** key

**Verify** (all must pass):
- [ ] Auto-completion activates on Tab keypress
- [ ] Matching commands are shown (dropdown or inline completion)
- [ ] Only commands starting with typed prefix are shown
- [ ] Commands are sorted/displayed logically
- [ ] No errors or exceptions occur

**Test Multiple Prefixes**:
- Type "gi" → Press Tab → Verify matches (e.g., github, gitlab if configured)
- Type "go" → Press Tab → Verify matches (e.g., google)
- Type "xyz" → Press Tab → Verify no matches or appropriate behavior

**Expected Result**: Tab key triggers auto-completion showing only matching commands

---

### Test 4: Auto-completion - Exact Match

**Purpose**: Verify auto-completion behavior with exact command match

**Prerequisites**:
- Input window is visible

**Steps**:
1. Clear the textbox (if needed)
2. Type a complete command name exactly (e.g., "github")
3. Press **Tab** key

**Verify** (all must pass):
- [ ] Auto-completion recognizes exact match
- [ ] Behavior is appropriate (completes or shows single match)
- [ ] No errors occur

**Expected Result**: Exact match is handled correctly

---

### Test 5: Escape Key - Hide Window

**Purpose**: Verify Escape key hides window correctly on .NET 8

**Prerequisites**:
- Input window is visible

**Steps**:
1. Press **Escape** key

**Verify** (all must pass):
- [ ] Input window disappears immediately (no animation delay)
- [ ] Window is hidden (not minimized to taskbar)
- [ ] System tray icon remains visible
- [ ] Application continues running
- [ ] No error dialogs appear

**Expected Result**: Window hides immediately when Escape is pressed

---

### Test 6: Hotkey Re-triggering

**Purpose**: Verify hotkey can show window again after hiding

**Prerequisites**:
- Input window is hidden (from Test 5)
- Another application has focus

**Steps**:
1. Press **Ctrl+Space** again

**Verify** (all must pass):
- [ ] Input window appears again (same as Test 1)
- [ ] Response time is still fast (< 100ms)
- [ ] Textbox is cleared (or retains previous text, depending on design)
- [ ] Focus is on textbox

**Repeat**:
- Show window (Ctrl+Space)
- Hide window (Escape)
- Show window (Ctrl+Space)
- Repeat 5 times to verify consistency

**Expected Result**: Window can be shown/hidden repeatedly without issues

---

### Test 7: Tab Key - Multiple Presses

**Purpose**: Verify Tab key cycling behavior

**Prerequisites**:
- Input window is visible

**Steps**:
1. Type a prefix that matches multiple commands (e.g., "g")
2. Press **Tab** multiple times in sequence

**Verify** (all must pass):
- [ ] Each Tab press cycles through matching commands
- [ ] Cycling behavior is logical (forward through list)
- [ ] No errors or hangs occur
- [ ] All matching commands are accessible

**Expected Result**: Tab key cycles through all matching auto-completions

---

## Success Criteria

All of the following must be TRUE:

### P/Invoke Hotkey (Requirement 5.4)
- ✅ Ctrl+Space shows input window reliably
- ✅ Hotkey works from any application (global registration)
- ✅ Hotkey response time < 100ms

### Windows Forms UI (Requirements 5.1, 5.2, 5.3)
- ✅ Input window displays correctly on .NET 8
- ✅ Textbox receives focus automatically
- ✅ Window shows/hides without errors

### Auto-completion (Requirement 5.5)
- ✅ Tab key triggers auto-completion
- ✅ Prefix matching works correctly
- ✅ Commands are filtered and displayed

### Window Hiding (Requirement 5.3)
- ✅ Escape key hides window immediately
- ✅ Window can be shown again with hotkey

### Reliability
- ✅ Show/hide cycle works repeatedly
- ✅ No memory leaks or performance degradation

## Test Results

### Test 1: Global Hotkey (Ctrl+Space)
- Response time: [measured time in ms]
- Window appeared: [YES/NO]
- Errors: [NONE or describe]
- Result: [PASS/FAIL]

### Test 2: UI Focus
- Textbox has focus: [YES/NO]
- Window in foreground: [YES/NO]
- Result: [PASS/FAIL]

### Test 3: Auto-completion Prefix Matching
- Tab key works: [YES/NO]
- Matching commands shown: [YES/NO]
- Prefix tested: [e.g., "g" → github, google, gmail]
- Result: [PASS/FAIL]

### Test 4: Auto-completion Exact Match
- Exact match behavior: [describe]
- Result: [PASS/FAIL]

### Test 5: Escape Key
- Window hides: [YES/NO]
- Hide time: [immediate/delayed]
- Result: [PASS/FAIL]

### Test 6: Hotkey Re-triggering
- Show/hide cycles: [number tested, e.g., 5]
- All cycles successful: [YES/NO]
- Result: [PASS/FAIL]

### Test 7: Tab Key Multiple Presses
- Cycles through commands: [YES/NO]
- Number of matches tested: [count]
- Result: [PASS/FAIL]

## Notes

### P/Invoke Testing
- Test on real Windows hardware (not Wine or VM) for accurate hotkey testing
- Windows API RegisterHotKey/UnregisterHotKey are kernel-level operations
- VM/Wine environments may not support global hotkey registration correctly

### Auto-completion Testing
- Commands available depend on `my.conf` configuration
- Default my.conf includes: github, google, gmail, etc.
- Add test commands to my.conf if needed for comprehensive testing

### Performance
- Hotkey response should feel instant (< 100ms)
- No noticeable lag between keypress and window appearance
- Window hide should be immediate on Escape

## Issues Found
[Document any issues, errors, or unexpected behavior here]

### Known Issues
- If hotkey doesn't work: Check Windows Accessibility settings (Sticky Keys, etc.)
- If window doesn't appear: Check display scaling or multi-monitor setup

## Tester Sign-off
- Name: ___________________
- Date: ___________________
- Overall Result: [PASS/FAIL]
- Comments: ___________________
