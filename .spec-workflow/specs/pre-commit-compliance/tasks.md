# Tasks Document

## Phase 1: Code Formatting (Quick Win)

- [x] 1. Apply code formatting to resolve whitespace violations
  - Files:
    - TestCommandLauncher/Test_AutoCompleteMachine.cs (13 violations)
    - TestCommandLauncher/Test_ConfigLoader.cs (15 violations)
  - Run `dotnet format` on entire solution
  - Verify all 267 tests pass after formatting
  - Commit formatting changes independently
  - _Leverage: .editorconfig, dotnet format CLI_
  - _Requirements: 3_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Code Quality Engineer with expertise in .NET code formatting standards | Task: Apply dotnet format to fix all 28 whitespace violations in TestCommandLauncher test files following requirement 3, using .editorconfig formatting rules. Run `dotnet format` command, verify no formatting violations remain with `dotnet format --verify-no-changes`, and ensure all 267 tests still pass. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Do not modify .editorconfig rules, do not change any code logic or test assertions, ensure tests remain functionally identical | _Leverage: .editorconfig for formatting rules, existing test suite for validation | _Requirements: Requirement 3 (Code Formatting Compliance) | Success: `dotnet format --verify-no-changes` returns zero violations, all 267 tests pass, no functional changes to test logic_

## Phase 2: Method Refactoring

- [x] 2. Refactor CommandExecutorService.ExecuteAsync method
  - File: TeaLauncher.Avalonia/Application/Services/CommandExecutorService.cs
  - Extract helper methods from 68-line ExecuteAsync:
    - `ResolveCommandTarget(string commandInput)` - handles command resolution and argument combination
    - `BuildProcessStartInfo(string filename, string args)` - creates ProcessStartInfo configuration
    - `LaunchProcess(ProcessStartInfo startInfo)` - executes process and handles exceptions
  - Keep each extracted method ≤50 lines
  - Maintain existing public API (no changes to ExecuteAsync signature)
  - Run unit tests after each extraction to verify no regression
  - _Leverage: Existing helper methods (IsPath, GetExecution, GetArguments, IsSpecialCommand)_
  - _Requirements: 2_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# Refactoring Specialist with expertise in SOLID principles and code maintainability | Task: Refactor CommandExecutorService.ExecuteAsync (currently 68 lines) following requirement 2 by extracting three private helper methods: ResolveCommandTarget, BuildProcessStartInfo, and LaunchProcess. Each helper must be ≤50 lines and have a single responsibility. Preserve exact functionality - all existing unit tests must pass. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Do not change public API or method signature, do not alter any business logic or behavior, maintain existing exception handling patterns, do not modify ICommandExecutor interface | _Leverage: Existing helper methods (IsPath, GetExecution, GetArguments, IsSpecialCommand) in CommandExecutorService.cs, existing unit tests in TeaLauncher.Avalonia.Tests/Application/Services/CommandExecutorServiceTests.cs | _Requirements: Requirement 2 (Code Metrics Compliance) | Success: MetricsChecker reports zero violations for ExecuteAsync method, all 267 existing tests pass, extracted methods are well-named and ≤50 lines each, no performance regression_

- [ ] 3. Refactor AvaloniaDialogService.ShowDialogInternalAsync method
  - File: TeaLauncher.Avalonia/Infrastructure/UI/AvaloniaDialogService.cs
  - Extract helper methods from 81-line ShowDialogInternalAsync:
    - `CreateDialogWindow(string title)` - creates base Window with properties
    - `ConfigureDialogContent(string message, DialogType dialogType)` - builds message StackPanel with styling
    - `CreateDialogButtons(DialogType dialogType, Window dialog, ref bool result)` - creates button panel with event handlers
  - Keep each extracted method ≤50 lines
  - Maintain existing private API (no changes to ShowDialogInternalAsync behavior)
  - Run unit and E2E tests after refactoring to verify UI behavior unchanged
  - _Leverage: Existing DialogType enum, Avalonia.Controls components_
  - _Requirements: 2_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI Developer with expertise in MVVM patterns and UI component refactoring | Task: Refactor AvaloniaDialogService.ShowDialogInternalAsync (currently 81 lines) following requirement 2 by extracting three private helper methods: CreateDialogWindow, ConfigureDialogContent, and CreateDialogButtons. Each helper must be ≤50 lines and encapsulate one UI composition concern. Preserve exact UI behavior - all dialog rendering and event handling must work identically. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Do not change dialog appearance or behavior, do not alter event handler logic, maintain existing ref parameter pattern for result tracking, do not modify public API | _Leverage: Existing DialogType enum, Avalonia.Controls (Window, StackPanel, TextBlock, Button), existing UI tests in TeaLauncher.Avalonia.Tests/Infrastructure/UI/AvaloniaDialogServiceTests.cs | _Requirements: Requirement 2 (Code Metrics Compliance) | Success: MetricsChecker reports zero violations for ShowDialogInternalAsync method, all 267 tests pass, E2E tests confirm dialogs render correctly, extracted methods are well-organized and ≤50 lines each_

- [ ] 4. Verify metrics compliance after refactoring
  - Run MetricsChecker to confirm zero method length violations
  - Run full test suite to ensure all 267 tests pass
  - Run performance tests to confirm no regression
  - _Leverage: tools/MetricsChecker, PerformanceTestBase_
  - _Requirements: 2_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Engineer with expertise in code quality validation and performance testing | Task: Verify that refactoring tasks 2-3 meet requirement 2 by running MetricsChecker to confirm zero method length violations, running full test suite (all 267 tests must pass), and validating no performance regression using PerformanceTestBase. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: This is a validation task only - do not make code changes unless tests fail, do not skip any verification steps | _Leverage: tools/MetricsChecker/MetricsChecker.csproj for metrics validation, TeaLauncher.Avalonia.Tests/Performance/PerformanceTestBase.cs for performance testing, existing test suite for regression detection | _Requirements: Requirement 2 (Code Metrics Compliance) | Success: `dotnet run --project tools/MetricsChecker/MetricsChecker.csproj` reports zero violations, all 267 tests pass with `dotnet test`, performance tests show ≤5% variance from baseline_

## Phase 3: Coverage Enhancement

- [ ] 5. Analyze coverage gaps and prioritize test areas
  - Analyze coverage report: coverage/**/coverage.cobertura.xml
  - Identify uncovered lines in critical services (current: 61%, target: ≥80%)
  - Create prioritized list of services needing coverage:
    1. CommandExecutorService (special commands, error paths)
    2. AutoCompleterService (edge cases)
    3. YamlConfigLoaderService (malformed YAML, missing files)
    4. AvaloniaDialogService (DialogType variations)
  - Document uncovered line numbers and scenarios
  - _Leverage: coverage/*/coverage.cobertura.xml, scripts/check-coverage.sh_
  - _Requirements: 1_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Test Coverage Analyst with expertise in code coverage analysis and gap identification | Task: Analyze current code coverage report (61%) following requirement 1, identify specific uncovered lines in critical services (CommandExecutorService, AutoCompleterService, YamlConfigLoaderService, AvaloniaDialogService), and create prioritized list of test scenarios needed to reach ≥80% coverage. Parse coverage.cobertura.xml to identify exact line numbers lacking coverage. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Do not write tests yet (analysis only), focus on critical services first, ensure analysis is data-driven from actual coverage report | _Leverage: coverage/**/coverage.cobertura.xml for line-by-line coverage data, scripts/check-coverage.sh for threshold validation, TESTING.md for existing test patterns | _Requirements: Requirement 1 (Code Coverage Compliance) | Success: Detailed analysis document created identifying specific uncovered lines, prioritized list of 10-15 test scenarios to implement, clear path to ≥80% coverage_

- [ ] 6. Add CommandExecutorService edge case tests
  - File: TeaLauncher.Avalonia.Tests/Application/Services/CommandExecutorServiceTests.cs
  - Add unit tests for uncovered scenarios:
    - Special command detection (!reload, !version, !exit)
    - Registry lookup failures (command not found)
    - Argument combination edge cases
    - ProcessStartInfo configuration variations
  - Use NSubstitute for mocking ICommandRegistry
  - Use FluentAssertions for test assertions
  - Target 90%+ coverage for CommandExecutorService
  - _Leverage: Existing test patterns in CommandExecutorServiceTests.cs, NSubstitute, FluentAssertions_
  - _Requirements: 1_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# Unit Test Developer with expertise in NUnit, NSubstitute mocking, and FluentAssertions | Task: Add comprehensive unit tests to CommandExecutorServiceTests.cs following requirement 1 to cover special command detection, registry lookup failures, and argument combination edge cases identified in task 5. Follow AAA pattern (Arrange-Act-Assert), use NSubstitute for ICommandRegistry mocking, and FluentAssertions for expressive assertions. Target 90%+ coverage for CommandExecutorService. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Follow existing test naming convention (MethodName_Scenario_ExpectedBehavior), do not test implementation details (only public API), ensure tests are deterministic (no flakiness), run each test 10 times to verify 100% pass rate | _Leverage: TeaLauncher.Avalonia.Tests/Application/Services/CommandExecutorServiceTests.cs for existing patterns, NSubstitute 5.1.0 for mocking, FluentAssertions 6.12.0 for assertions, task 5 analysis for specific scenarios | _Requirements: Requirement 1 (Code Coverage Compliance) | Success: 10-15 new tests added covering special commands and edge cases, all new tests pass 10 consecutive runs, CommandExecutorService coverage ≥90%, total test execution time increase ≤10%_

- [ ] 7. Add AutoCompleterService and YamlConfigLoaderService tests
  - Files:
    - TeaLauncher.Avalonia.Tests/Application/Services/AutoCompleterServiceTests.cs
    - TeaLauncher.Avalonia.Tests/Infrastructure/Configuration/YamlConfigLoaderServiceTests.cs
  - Add AutoCompleterService tests:
    - Empty input handling
    - No candidates scenario
    - Special character handling
    - Prefix matching edge cases
  - Add YamlConfigLoaderService tests:
    - Malformed YAML parsing errors
    - Missing file handling
    - Empty configuration files
    - Invalid command structure validation
  - Use existing test fixture patterns for YAML files
  - _Leverage: Integration/Fixtures/*.yaml for test data, existing service test patterns_
  - _Requirements: 1_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Integration Test Developer with expertise in YAML processing and auto-completion logic testing | Task: Add comprehensive unit tests for AutoCompleterService and YamlConfigLoaderService following requirement 1, covering empty inputs, no candidates, special characters, malformed YAML, missing files, and validation errors identified in task 5. Use existing Integration/Fixtures/*.yaml patterns for test data. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Follow AAA pattern and existing test naming conventions, use real YAML files in test fixtures (not inline strings), ensure tests are isolated and independent, validate error messages are user-friendly | _Leverage: TeaLauncher.Avalonia.Tests/Integration/Fixtures/ for YAML test data, existing AutoCompleterServiceTests.cs and YamlConfigLoaderServiceTests.cs patterns, FluentAssertions for exception assertions | _Requirements: Requirement 1 (Code Coverage Compliance) | Success: 8-10 new tests added (4-5 per service), AutoCompleterService coverage ≥85%, YamlConfigLoaderService coverage ≥85%, all tests pass 10 consecutive runs_

- [ ] 8. Add AvaloniaDialogService coverage tests
  - File: TeaLauncher.Avalonia.Tests/Infrastructure/UI/AvaloniaDialogServiceTests.cs
  - Add tests for DialogType variations:
    - DialogType.Message rendering
    - DialogType.Error styling (red foreground, bold)
    - DialogType.Confirm button behavior (Yes/No clicks)
  - Add button event handler tests
  - Use Avalonia.Headless for UI testing
  - _Leverage: Avalonia.Headless, Avalonia.Headless.NUnit_
  - _Requirements: 1_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI Test Developer with expertise in headless UI testing and event simulation | Task: Add comprehensive tests for AvaloniaDialogService following requirement 1, covering all DialogType variations (Message, Error, Confirm), error styling verification, and button click event handlers using Avalonia.Headless for headless UI testing. Verify window properties, text styling, and button panel configuration. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Use Avalonia.Headless (do not require real window manager), test dialog appearance and behavior (not just return values), ensure tests work in CI environment, follow existing UI test patterns | _Leverage: Avalonia.Headless 11.2.2 and Avalonia.Headless.NUnit 11.2.2 for headless testing, existing AvaloniaDialogServiceTests.cs patterns, refactored helper methods from task 3 | _Requirements: Requirement 1 (Code Coverage Compliance) | Success: 6-8 new tests added covering all DialogType values and button behaviors, AvaloniaDialogService coverage ≥85%, tests verify UI appearance (not just logic), all tests pass in headless mode_

- [ ] 9. Verify coverage threshold and adjust if needed
  - Run coverage collection: `dotnet test --collect:"XPlat Code Coverage"`
  - Run coverage check: `scripts/check-coverage.sh`
  - Verify ≥80% coverage threshold achieved
  - If below 80%, identify remaining gaps and add targeted tests
  - Run full test suite 10 times to ensure 0% flakiness
  - Measure total test execution time (must be <850ms, currently ~700ms)
  - _Leverage: scripts/check-coverage.sh, coverlet.collector_
  - _Requirements: 1_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Validation Engineer with expertise in coverage analysis and test stability | Task: Validate that tasks 6-8 achieved ≥80% code coverage following requirement 1 by running coverage collection, executing check-coverage.sh script, and analyzing results. If coverage is below 80%, identify specific remaining gaps and add targeted tests iteratively until threshold is met. Verify test stability by running suite 10 consecutive times (must achieve 100% pass rate). Measure and validate test execution time is <850ms. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Must achieve exactly ≥80% (not approximate), focus on meaningful coverage (not just hitting lines), ensure new tests are stable (zero flakiness), do not sacrifice test quality for coverage percentage | _Leverage: scripts/check-coverage.sh for threshold validation, coverage/**/coverage.cobertura.xml for gap analysis, dotnet test --collect:"XPlat Code Coverage" for measurement | _Requirements: Requirement 1 (Code Coverage Compliance) | Success: scripts/check-coverage.sh passes with ≥80% coverage, all tests pass 10 consecutive runs (0% flakiness), total test execution time ≤850ms, coverage report shows balanced coverage across services_

## Phase 4: Pre-Commit Integration

- [ ] 10. Verify Husky pre-commit hooks execute all quality checks
  - Run `dotnet husky run` manually to test hook execution
  - Verify all tasks execute in correct order:
    1. build (Release configuration)
    2. build-tests
    3. test
    4. coverage
    5. coverage-check
    6. metrics
    7. format-check
  - Ensure all checks pass with current codebase
  - Test failure scenarios to verify blocking behavior
  - _Leverage: .husky/task-runner.json, existing Husky configuration_
  - _Requirements: 4_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: DevOps Engineer with expertise in Git hooks and CI/CD automation | Task: Verify Husky.Net pre-commit hooks execute all quality checks following requirement 4 by running `dotnet husky run` and confirming all tasks (build, test, coverage, metrics, format-check) execute in sequence and pass. Test failure scenarios by intentionally introducing violations (e.g., add whitespace, exceed method line limit) to verify hooks correctly block commits with clear error messages. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Do not modify .husky/task-runner.json unless hooks are broken, do not bypass any quality checks, ensure error messages are actionable for developers | _Leverage: .husky/task-runner.json for hook configuration, scripts/check-coverage.sh and tools/MetricsChecker for quality gates, `dotnet husky run` command for manual execution | _Requirements: Requirement 4 (Pre-Commit Hook Integration) | Success: `dotnet husky run` executes all 7 tasks successfully, intentional violations correctly block execution with clear errors, all quality gates pass (format, metrics, coverage, tests), pre-commit workflow completes in <15 seconds_

- [ ] 11. Test end-to-end commit workflow and document
  - Test actual `git commit` workflow with hooks enabled
  - Create test commit with all quality checks passing
  - Test `git commit --no-verify` bypass for emergency scenarios
  - Verify hooks trigger automatically on commit
  - Update TESTING.md with pre-commit workflow documentation
  - _Leverage: Existing TESTING.md structure and patterns_
  - _Requirements: 4_
  - _Prompt: Implement the task for spec pre-commit-compliance, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical Writer and QA Engineer with expertise in developer workflows and documentation | Task: Validate end-to-end pre-commit workflow following requirement 4 by creating actual git commits (with quality checks passing), testing `git commit --no-verify` bypass, and documenting the complete workflow in TESTING.md. Include troubleshooting steps for common failures (coverage below threshold, formatting violations, metrics violations). Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Do not commit test changes to master (use temporary branch or amend), ensure documentation is clear for new contributors, include both success and failure scenarios in documentation | _Leverage: Existing TESTING.md structure at /home/rmondo/repos/TeaLauncher/TESTING.md, Husky hooks documentation, quality gate error messages for troubleshooting examples | _Requirements: Requirement 4 (Pre-Commit Hook Integration) | Success: `git commit` automatically triggers all pre-commit checks, hooks block commits on quality failures with actionable messages, `--no-verify` bypass works when needed, TESTING.md updated with comprehensive pre-commit workflow documentation including troubleshooting guide_

## Summary

**Total Tasks**: 11
**Estimated Effort**:
- Phase 1 (Formatting): 0.5 hours
- Phase 2 (Refactoring): 3 hours
- Phase 3 (Coverage): 6 hours
- Phase 4 (Integration): 1.5 hours

**Dependencies**:
- Task 2 depends on Task 1 (formatting should be clean before refactoring)
- Task 3 depends on Task 2 (complete CommandExecutor refactoring first)
- Task 4 depends on Tasks 2-3 (verification after all refactoring)
- Tasks 6-8 depend on Task 5 (analysis guides test creation)
- Task 9 depends on Tasks 6-8 (verification after all test additions)
- Task 10 depends on Tasks 1-9 (all quality gates must pass first)
- Task 11 depends on Task 10 (workflow testing after validation)

**Success Criteria**:
- All 11 tasks marked as completed [x]
- Code coverage ≥80% (currently 61%)
- Zero code metrics violations (currently 2)
- Zero formatting violations (currently 28)
- All 267+ tests passing
- Pre-commit hooks blocking non-compliant commits
- Documentation updated
