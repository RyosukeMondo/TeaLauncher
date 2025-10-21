# Code Coverage Gap Analysis
**Date:** 2025-10-21
**Current Coverage:** 61.39% (784/1277 lines covered)
**Target Coverage:** ≥80%
**Gap:** 18.61% (~238 additional lines needed)

## Executive Summary

This analysis identifies specific uncovered lines in critical services and provides a prioritized list of test scenarios to achieve ≥80% code coverage. The analysis focuses on four critical services identified in the pre-commit-compliance spec.

## Current Coverage by Service

| Service | Coverage | Lines Covered | Priority | Impact |
|---------|----------|---------------|----------|--------|
| **AvaloniaDialogService** | 4.34% | 4/96 | **1 - CRITICAL** | +12-15% overall |
| **CommandExecutorService** | 81.15% | 106/138 | **2 - HIGH** | +2-3% overall |
| **YamlConfigLoaderService** | 86.88% | 53/61 | **3 - MEDIUM** | +0.5-1% overall |
| **AutoCompleterService** | 100% | 36/36 | 4 - LOW | Fully covered ✓ |

---

## Priority 1: AvaloniaDialogService (CRITICAL)
**File:** `TeaLauncher.Avalonia/Infrastructure/UI/AvaloniaDialogService.cs`
**Current Coverage:** 4.34% (4/96 lines)
**Target Coverage:** ≥85%
**Impact:** HIGH - 96 uncovered lines, +12-15% to overall coverage

### Uncovered Methods

#### 1. CreateDialogWindow(string title) - 0% (0/11 lines)
**Uncovered Lines:** 133-143

**Test Scenarios:**
- Verify Window properties: Width=400, Height=200
- Verify Title is set correctly
- Verify CanResize=false
- Verify SizeToContent=SizeToContent.WidthAndHeight
- Verify WindowStartupLocation=WindowStartupLocation.CenterOwner

---

#### 2. ConfigureDialogContent(string message, DialogType dialogType) - 0% (0/20 lines)
**Uncovered Lines:** 152-176

**Test Scenarios:**
- **DialogType.Message:** Verify StackPanel with default TextBlock styling
- **DialogType.Error:** Verify Red Foreground and FontWeight.Bold applied
- **DialogType.Confirm:** Verify TextBlock text alignment
- Verify StackPanel Margin and spacing
- Verify TextBlock TextWrapping and MaxWidth

---

#### 3. CreateDialogButtons(DialogType dialogType, Window dialog, ref DialogResult result) - 0% (0/16 lines)
**Uncovered Lines:** 186-204

**Test Scenarios:**
- **DialogType.Message/Error:** Verify AddOkButton is called (single OK button)
- **DialogType.Confirm:** Verify AddConfirmButtons is called (Yes/No buttons)
- Verify button panel HorizontalAlignment=HorizontalAlignment.Center

---

#### 4. AddConfirmButtons(StackPanel buttonPanel, Window dialog, DialogResult result) - 0% (0/26 lines)
**Uncovered Lines:** 213-234

**Test Scenarios:**
- Verify Yes button properties: Content="Yes", Width=100, Margin
- Verify No button properties: Content="No", Width=100, Margin
- **Yes Button Click:** Verify result.Value=true and dialog.Close() is called
- **No Button Click:** Verify result.Value=false and dialog.Close() is called
- Verify both buttons are added to StackPanel

---

#### 5. AddOkButton(StackPanel buttonPanel, Window dialog) - 0% (0/13 lines)
**Uncovered Lines:** 248-261

**Test Scenarios:**
- Verify OK button properties: Content="OK", Width=100, IsDefault=true
- **OK Button Click:** Verify dialog.Close() is called
- Verify button is added to StackPanel

---

#### 6. GetActiveWindow() - 66.66% (4/6 lines)
**Uncovered Lines:** 272-273

**Test Scenarios:**
- Test case when no active window exists (null handling)

---

#### 7. ShowDialogInternalAsync - 0% (0/10 lines)
**Uncovered Lines:** 111-125

**Test Scenarios:**
- Integration test: Verify CreateDialogWindow → ConfigureDialogContent → CreateDialogButtons flow
- Verify dialog.ShowDialog() is called
- Verify result.Value is returned correctly

---

## Priority 2: CommandExecutorService (HIGH)
**File:** `TeaLauncher.Avalonia/Application/Services/CommandExecutorService.cs`
**Current Coverage:** 81.15% (106/138 lines)
**Target Coverage:** ≥90%
**Impact:** MEDIUM - 27 uncovered lines, +2-3% to overall coverage

### Uncovered Code Paths

#### 1. ResolveCommandTarget(string commandInput) - 80% (28/35 lines)
**Uncovered Lines:** 84, 86-88, 117-119

**Test Scenarios:**
- **Special Command Detection (lines 84, 86-88):**
  - Test `!reload` command: Verify special command path is taken
  - Test `!version` command: Verify special command path is taken
  - Test `!exit` command: Verify special command path is taken

- **Registry Lookup Failure (lines 117-119):**
  - Test command not found in registry
  - Verify exception is thrown with proper error message

---

#### 2. LaunchProcess(ProcessStartInfo startInfo, string commandInput) - 38.46% (5/13 lines)
**Uncovered Lines:** 158-166

**Test Scenarios:**
- **Exception Handling:**
  - Mock Process.Start() to throw Win32Exception (file not found)
  - Mock Process.Start() to throw InvalidOperationException (invalid executable)
  - Mock Process.Start() to throw generic Exception
  - Verify ProcessStartException is thrown with original exception details
  - Verify error message includes commandInput

---

#### 3. IsPath(string input) - 64.70% (11/17 lines)
**Uncovered Lines:** 209-210, 215, 217, 219, 225

**Test Scenarios:**
- Test invalid path characters (e.g., `path<>:invalid`)
- Test null/empty input paths
- Test paths with only whitespace
- Test relative paths vs absolute paths
- Test paths with drive letters on non-Windows systems

---

#### 4. IsSpecialCommand(string input) - 77.77% (7/9 lines)
**Uncovered Lines:** 239-240

**Test Scenarios:**
- Test non-special command inputs (e.g., `normal-command`, `help`, `list`)
- Test empty string input
- Test commands without `!` prefix

---

#### 5. Split(string input) - 93.93% (31/33 lines)
**Uncovered Lines:** 261-262

**Test Scenarios:**
- Test empty string input: Verify returns empty array
- Test null input: Verify null/exception handling

---

#### 6. GetExecution(string input) - 90% (9/10 lines)
**Uncovered Lines:** 184

**Test Scenarios:**
- Test registry miss (command not found): Verify fallback behavior or exception

---

## Priority 3: YamlConfigLoaderService (MEDIUM)
**File:** `TeaLauncher.Avalonia/Infrastructure/Configuration/YamlConfigLoaderService.cs`
**Current Coverage:** 86.88% (53/61 lines)
**Target Coverage:** ≥95%
**Impact:** LOW - 8 uncovered lines, +0.5-1% to overall coverage

### Uncovered Code Paths

#### LoadConfiguration(string filePath) - 78.37% (29/37 lines)
**Uncovered Lines:** 104-105, 107, 114-115, 117-119

**Test Scenarios:**

1. **Malformed YAML (lines 104-105):**
   - Create test YAML file with invalid syntax (e.g., incorrect indentation, invalid characters)
   - Verify YamlException is caught and wrapped in ConfigurationException
   - Verify error message is user-friendly

2. **File Not Found (line 107):**
   - Test LoadConfiguration with non-existent file path
   - Verify FileNotFoundException is handled gracefully
   - Verify ConfigurationException is thrown with descriptive message

3. **Empty Configuration File (lines 114-115):**
   - Create empty YAML file (0 bytes)
   - Verify null/empty configuration is handled
   - Verify appropriate exception or default configuration

4. **Invalid Command Structure (lines 117-119):**
   - Create YAML with missing required fields (e.g., no `name` or `path`)
   - Create YAML with wrong data types (e.g., `commands: string` instead of `commands: []`)
   - Verify validation fails with descriptive error message

---

## Priority 4: AutoCompleterService (LOW)
**File:** `TeaLauncher.Avalonia/Application/Services/AutoCompleterService.cs`
**Current Coverage:** 100% (36/36 lines)
**Target Coverage:** 100% (maintain)
**Impact:** NONE - Already fully covered ✓

**Action:** No additional tests needed. Maintain existing coverage.

---

## Coverage Improvement Projection

| Priority | Service | Lines to Cover | Projected Coverage Gain |
|----------|---------|----------------|------------------------|
| 1 | AvaloniaDialogService | 92 / 96 | +12-15% |
| 2 | CommandExecutorService | 24 / 27 | +2-3% |
| 3 | YamlConfigLoaderService | 7 / 8 | +0.5-1% |
| **Total** | **All Services** | **~123 lines** | **+15-19%** |

**Projected Overall Coverage:** 76-80% (borderline for ≥80% threshold)

---

## Recommendations

### Phase 1: Priority 1 Tests (Target: 70-75% overall coverage)
1. Implement all AvaloniaDialogService tests (Task 8)
2. Focus on headless UI testing with Avalonia.Headless
3. Verify DialogType variations, button behaviors, and styling

### Phase 2: Priority 2 Tests (Target: 78-82% overall coverage)
1. Implement CommandExecutorService edge case tests (Task 6)
2. Focus on special commands, registry failures, and exception handling
3. Use NSubstitute for ICommandRegistry mocking

### Phase 3: Priority 3 Tests (Target: 80-83% overall coverage)
1. Implement YamlConfigLoaderService error path tests (Task 7)
2. Create malformed YAML fixtures for testing
3. Test file I/O error scenarios

### Phase 4: Re-assessment (if coverage < 80%)
1. Run coverage analysis again after Phases 1-3
2. Identify remaining gaps in other services
3. Add targeted tests to reach exactly ≥80%

---

## Test Implementation Strategy

### Task 6: CommandExecutorService Tests
**File:** `TeaLauncher.Avalonia.Tests/Application/Services/CommandExecutorServiceTests.cs`
**Estimated Tests:** 10-12 new test methods
**Estimated Coverage Gain:** +2-3% overall

### Task 7: AutoCompleterService + YamlConfigLoaderService Tests
**Files:**
- `TeaLauncher.Avalonia.Tests/Application/Services/AutoCompleterServiceTests.cs` (no changes needed)
- `TeaLauncher.Avalonia.Tests/Infrastructure/Configuration/YamlConfigLoaderServiceTests.cs`

**Estimated Tests:** 4-5 new test methods (YamlConfigLoaderService only)
**Estimated Coverage Gain:** +0.5-1% overall

### Task 8: AvaloniaDialogService Tests
**File:** `TeaLauncher.Avalonia.Tests/Infrastructure/UI/AvaloniaDialogServiceTests.cs`
**Estimated Tests:** 8-10 new test methods
**Estimated Coverage Gain:** +12-15% overall

---

## Success Criteria

- [ ] All prioritized test scenarios implemented
- [ ] Code coverage ≥80% (verified with `scripts/check-coverage.sh`)
- [ ] All tests pass 10 consecutive runs (0% flakiness)
- [ ] Test execution time ≤850ms
- [ ] Coverage balanced across all critical services
- [ ] No coverage regressions in currently covered code

---

## Appendix: Raw Coverage Data

**Overall Project Metrics:**
- Line Rate: 61.39%
- Branch Rate: 53.24%
- Lines Covered: 784 / 1277
- Branches Covered: 205 / 385

**Coverage Report Location:**
`coverage/94160349-8ce0-48a7-a093-08e44bb1e6d3/coverage.cobertura.xml`

**Generated:** 2025-10-21
**Tool:** Coverlet (XPlat Code Coverage)
