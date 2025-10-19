# Manual Test Plan - Task 16: Configuration File Parsing Testing

## Test Overview

**Task**: Manual testing on Windows - Configuration file parsing
**Requirements**: 6.1, 6.2, 8.3
**Purpose**: Confirm ConfigLoader works with Dictionary<string, Dictionary<string, string>> on .NET 8
**Prerequisite**: Tasks 13-15 completed (application startup, UI, and command execution verified)

## Test Environment Requirements

### Hardware
- Windows 10 version 1607+ or Windows 11
- Real hardware (not virtualized if possible)
- Write access to application directory

### Software
- .NET 8 Desktop Runtime installed (for framework-dependent build)
- Text editor (Notepad, VS Code, or similar)
- Command prompt or PowerShell

### Application State
- TeaLauncher.exe must be running
- System tray icon visible
- Configuration file: resource/conf/my.conf
- ConfigLoader.cs using Dictionary (not Hashtable)

## Test Cases

### Test Case 1: Add New Simple URL Command
**Requirement**: 6.1, 6.2, 8.3

**Initial State**:
- TeaLauncher running with default my.conf

**Steps**:
1. Press Ctrl+Space to verify current commands work
2. Type: `test` and press Tab
3. Verify: No command named "test_url" exists yet
4. Press Escape to hide window
5. Open resource/conf/my.conf in text editor
6. Add new section at end of file:
   ```ini
   [test_url]
   linkto = https://example.com
   ```
7. Save file (do NOT close editor yet)
8. In TeaLauncher, press Ctrl+Space
9. Type: `!reload` and press Enter
10. Wait for reload to complete (should be instant)
11. Press Ctrl+Space again
12. Type: `test` and press Tab

**Expected Result**:
- After reload, auto-completion shows "test_url" as an option
- No error dialogs or exceptions during reload
- Typing full "test_url" and pressing Enter opens https://example.com in browser
- Dictionary-based ConfigLoader parses new section correctly

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 2: Add Command with Special Characters
**Requirement**: 6.1, 6.2, 8.3

**Configuration Addition**:
```ini
[test_special]
linkto = https://example.com/search?q=dotnet+8&lang=en
```

**Steps**:
1. While my.conf is still open in editor, add the above section
2. Save file
3. In TeaLauncher, press Ctrl+Space
4. Type: `!reload` and press Enter
5. Press Ctrl+Space
6. Type: `test_sp` and press Tab

**Expected Result**:
- Command "test_special" appears in auto-completion
- URL with query parameters and special characters (+, &, =) parsed correctly
- No encoding issues or parsing errors
- Executing command opens correct URL with parameters

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 3: Add Application Command with Arguments
**Requirement**: 6.1, 6.2, 8.3

**Configuration Addition**:
```ini
[test_notepad]
linkto = notepad.exe resource\conf\my.conf
```

**Steps**:
1. Add the above section to my.conf
2. Save file
3. Execute !reload in TeaLauncher
4. Type: `test_note` and press Tab to verify auto-completion
5. Press Enter to execute command

**Expected Result**:
- Command "test_notepad" appears in auto-completion
- Executing command opens Notepad with my.conf file loaded
- Arguments (file path) passed correctly to Process.Start
- Dictionary correctly stores command with spaces in linkto value

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 4: Add Multiple Commands in Batch
**Requirement**: 6.1, 6.2, 8.3

**Configuration Addition**:
```ini
[test_google]
linkto = https://google.com

[test_github]
linkto = https://github.com

[test_cmd]
linkto = cmd.exe /k echo Dictionary Test
```

**Steps**:
1. Add all three sections to my.conf
2. Save file
3. Execute !reload
4. Test each command:
   - Type `test_goo` + Tab → verify "test_google" appears
   - Type `test_git` + Tab → verify "test_github" appears
   - Type `test_cm` + Tab → verify "test_cmd" appears
5. Execute test_cmd to verify arguments work

**Expected Result**:
- All three commands load successfully in single reload
- Dictionary handles multiple new entries without collision
- Auto-completion works for all new commands
- Command execution works for all three
- CMD window opens with "Dictionary Test" message

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 5: Modify Existing Command
**Requirement**: 6.1, 6.2, 8.3

**Configuration Modification**:
Change existing [test_url] from:
```ini
[test_url]
linkto = https://example.com
```
to:
```ini
[test_url]
linkto = https://microsoft.com
```

**Steps**:
1. Modify the [test_url] section's linkto value
2. Save my.conf
3. Execute !reload
4. Type `test_url` and press Enter

**Expected Result**:
- Dictionary correctly updates existing key value
- New URL (microsoft.com) opens instead of old URL
- No duplicate entries created
- Reload overwrites old value cleanly

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 6: Delete Command and Reload
**Requirement**: 6.1, 6.2, 8.3

**Configuration Modification**:
Delete the [test_cmd] section entirely from my.conf

**Steps**:
1. Remove entire [test_cmd] section (3 lines)
2. Save my.conf
3. Execute !reload
4. Type `test_cm` and press Tab

**Expected Result**:
- Command "test_cmd" no longer appears in auto-completion
- Dictionary correctly removes deleted section
- No orphaned entries remain
- Other test commands still work

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 7: Configuration Parse Error Handling
**Requirement**: 6.1, 6.2, 8.3

**Configuration with Intentional Error**:
Add malformed section:
```ini
[test_broken
linkto = https://example.com
```
(Note: Missing closing bracket in section name)

**Steps**:
1. Add the malformed section to my.conf
2. Save file
3. Execute !reload
4. Observe application behavior

**Expected Result**:
- ConfigLoader detects parsing error
- Application shows error dialog or logs error
- Application remains running (does not crash)
- Other commands still function
- Dictionary-based parser has same error handling as Hashtable version

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 8: Section with No linkto Key
**Requirement**: 6.1, 6.2, 8.3

**Configuration Addition**:
```ini
[test_no_linkto]
description = This section has no linkto key
```

**Steps**:
1. Add section without linkto key
2. Save my.conf
3. Execute !reload
4. Type `test_no` and press Tab

**Expected Result**:
- Section without linkto key is either ignored or handled gracefully
- No exceptions thrown during parsing
- Application remains stable
- Dictionary correctly handles sections with different key structures

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 9: Empty Section
**Requirement**: 6.1, 6.2, 8.3

**Configuration Addition**:
```ini
[test_empty]

```

**Steps**:
1. Add empty section (just section name, no keys)
2. Save my.conf
3. Execute !reload
4. Type `test_emp` and press Tab

**Expected Result**:
- Empty section handled gracefully
- No exceptions during parsing
- Command may appear in auto-completion but have empty/null linkto value
- Attempting to execute shows appropriate error or does nothing

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

### Test Case 10: Verify Dictionary Type Safety
**Requirement**: 8.3

**Technical Verification**:
This test verifies the Dictionary implementation provides type safety improvements over Hashtable.

**Steps**:
1. Review ConfigLoader.cs code to confirm:
   - Field declaration: `Dictionary<string, Dictionary<string, string>> m_Conf`
   - GetConfig returns: `Dictionary<string, string>`
   - No casts like (Hashtable) or (string) present
   - ParseConfig uses: `m_Conf[section] = new Dictionary<string, string>()`
2. After previous tests, execute several commands to verify runtime stability
3. No casting exceptions can occur (compile-time type safety)

**Expected Result**:
- Code review confirms Dictionary types throughout
- All previous tests passed without casting errors
- Type safety prevents runtime casting exceptions
- Modern collection API (ContainsKey, TryGetValue) available

**Actual Result**: [ ] PASS [ ] FAIL

**Notes**: _______________________________________________________________

---

## Regression Verification

After all new test cases, verify original commands still work:

**Original Commands Test**:
- [ ] [reader] → Opens Livedoor Reader
- [ ] [mail] → Opens Gmail
- [ ] [notepad] → Opens Notepad
- [ ] [cmd] → Opens Command Prompt
- [ ] [reload_config] → Executes !reload without errors
- [ ] [version] → Shows version dialog
- [ ] [exit] → Exits application cleanly

**Result**: [ ] ALL PASS [ ] SOME FAILED

**Failed Commands**: _______________________________________________________________

---

## Test Completion Checklist

- [ ] All 10 test cases executed
- [ ] All test cases PASSED
- [ ] Regression tests PASSED
- [ ] No crashes or exceptions observed
- [ ] Dictionary-based ConfigLoader works identically to Hashtable version
- [ ] Configuration file parsing behavior unchanged
- [ ] Auto-completion works with reloaded configuration
- [ ] !reload command functions correctly
- [ ] Type safety improvements verified (no casts in code)

## Test Environment Details

**Date**: _______________
**Tester**: _______________
**OS**: Windows 10 / Windows 11 (circle one)
**OS Build**: _______________
**.NET Version**: _______________
**Build Type**: Framework-dependent / Self-contained (circle one)
**TeaLauncher Version**: _______________

## Test Summary

**Total Test Cases**: 10 core + 7 regression = 17 tests
**Passed**: _____ / 17
**Failed**: _____ / 17
**Pass Rate**: _____% (Target: 100%)

## Notes and Observations

_______________________________________________________________________________
_______________________________________________________________________________
_______________________________________________________________________________
_______________________________________________________________________________

## Sign-off

**Test Status**: [ ] PASSED [ ] FAILED [ ] BLOCKED

**Tester Signature**: _______________________ **Date**: _______________

**Notes**: If any test failed, document details above and retest after fixes.

---

## Appendix: Configuration File Reference

**Location**: resource/conf/my.conf

**Format**:
```ini
[section_name]
linkto = <url_or_command>
```

**Section Name Rules**:
- Enclosed in square brackets
- Used as command name for auto-completion
- Must be unique within file

**linkto Value Rules**:
- Can be HTTP/HTTPS URL
- Can be file path (absolute or relative)
- Can be system command (must be in PATH)
- Can be special command (!reload, !version, !exit)
- Can include arguments after command

**ConfigLoader Behavior**:
- Parses INI-style configuration file
- Stores in Dictionary<string, Dictionary<string, string>>
- Section name → outer Dictionary key
- "linkto" → inner Dictionary key
- Command value → inner Dictionary value
- Case-sensitive section names
- Reload clears and rebuilds entire Dictionary
