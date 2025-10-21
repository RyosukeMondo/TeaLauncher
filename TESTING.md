# Testing Guide for TeaLauncher

This document describes the testing strategy, patterns, and practices for TeaLauncher. Following these guidelines ensures consistent, maintainable, and effective tests.

## Overview

TeaLauncher uses a comprehensive testing strategy with four levels of testing:

1. **Unit Tests**: Test individual components in isolation with mocked dependencies
2. **Integration Tests**: Test service interactions with real implementations
3. **End-to-End Tests**: Test complete user workflows using Avalonia headless mode
4. **Performance Tests**: Validate performance requirements for critical operations

Our testing approach emphasizes:
- **Clean Architecture**: Tests follow the same layered structure as the application
- **Testability**: All services use dependency injection for easy mocking
- **Coverage Goals**: ≥80% overall coverage, ≥90% for Domain and Application layers
- **Quality Gates**: Automated pre-commit hooks and CI/CD checks enforce quality standards
- **Performance Validation**: Automated tests ensure sub-100ms command execution and sub-50ms autocomplete

## Test Categories

### Unit Tests

**When to use**: Testing individual service methods, algorithms, and business logic in isolation.

**Characteristics**:
- Use NSubstitute to mock all dependencies
- Fast execution (entire suite completes in ≤5 seconds)
- Focus on single responsibility per test
- Test both success and failure paths

**Location**: `TeaLauncher.Avalonia.Tests/Application/Services/`, `TeaLauncher.Avalonia.Tests/Infrastructure/`

**Example**:
```csharp
[Test]
public void RegisterCommand_WithValidCommand_ShouldAddToRegistry()
{
    // Arrange
    var mockAutoCompleter = Substitute.For<IAutoCompleter>();
    var service = new CommandRegistryService(mockAutoCompleter);
    var command = new Command("google", "https://www.google.com");

    // Act
    service.RegisterCommand(command);

    // Assert
    service.HasCommand("google").Should().BeTrue();
    service.GetAllCommands().Should().ContainSingle()
        .Which.Should().Be(command);
}
```

### Integration Tests

**When to use**: Testing how services work together with real implementations and real I/O.

**Characteristics**:
- Use real service implementations (not mocks)
- Use real dependency injection container
- Test with real file I/O using temporary files
- Mock only platform-specific APIs (hotkeys, IME) for Linux compatibility
- Complete in ≤15 seconds

**Location**: `TeaLauncher.Avalonia.Tests/Integration/`

**Example**:
```csharp
[Test]
public async Task LoadConfig_RegisterCommands_ExecuteCommand_ShouldSucceed()
{
    // Arrange
    var serviceProvider = TestServiceProvider.CreateWithRealServices();
    var configLoader = serviceProvider.GetRequiredService<IConfigurationLoader>();
    var registry = serviceProvider.GetRequiredService<ICommandRegistry>();

    // Act - Load configuration and register commands
    var config = await configLoader.LoadConfigurationAsync(configPath);
    foreach (var commandEntry in config.Commands)
    {
        var command = new Command(
            commandEntry.Name,
            commandEntry.LinkTo,
            commandEntry.Description,
            commandEntry.Arguments
        );
        registry.RegisterCommand(command);
    }

    // Assert - Verify commands are registered
    registry.GetAllCommands().Should().HaveCount(5);
}
```

### End-to-End Tests

**When to use**: Testing complete user workflows from UI input to command execution.

**Characteristics**:
- Use Avalonia.Headless for UI testing without display server
- Simulate real user interactions (keyboard input, mouse clicks)
- Test complete application lifecycle
- Run on Linux CI without Windows dependencies
- Complete in ≤30 seconds

**Location**: `TeaLauncher.Avalonia.Tests/EndToEnd/`

**Example**:
```csharp
[AvaloniaTest]
public void AutoCompletion_TabKey_CompletesCommand()
{
    // Arrange
    var window = new MainWindow(testConfigPath);
    window.Show();
    var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");

    // Act - Type partial command and press Tab
    commandBox.Text = "test";
    var tabKeyEventArgs = new KeyEventArgs
    {
        Key = Key.Tab,
        RoutedEvent = InputElement.KeyDownEvent
    };
    commandBox.RaiseEvent(tabKeyEventArgs);

    // Assert - Text should be auto-completed
    commandBox.Text.Should().StartWith("test");
}
```

### Performance Tests

**When to use**: Validating that critical operations meet performance requirements.

**Characteristics**:
- Inherit from `PerformanceTestBase` for timing utilities
- Use high-resolution `Stopwatch` for accurate measurements
- Include JIT warmup to avoid measurement bias
- Test against specific thresholds (100ms execution, 50ms autocomplete, 200ms config load)
- Run in Release mode for accurate performance data

**Location**: `TeaLauncher.Avalonia.Tests/Performance/`

**Performance Requirements**:
- Command execution: ≤100ms from input to process start
- Autocomplete: ≤50ms with 1000 words
- Configuration loading: ≤200ms for 100 commands

**Example**:
```csharp
[TestFixture]
public class AutoCompletePerformanceTests : PerformanceTestBase
{
    private IAutoCompleter _autoCompleter;

    [Test]
    public void AutoComplete_With1000Words_Within50ms()
    {
        // Arrange
        _autoCompleter.UpdateWordList(EdgeCaseTestFixtures.LargeWordList);
        const string prefix = "word";
        const int maxAllowedMs = 50;

        // Act - TimeOperation handles warmup and measurement
        var result = TimeOperation(
            "AutoComplete with 1000 words",
            () => _autoCompleter.AutoCompleteWord(prefix),
            maxAllowedMs);

        // Assert - AssertDuration provides detailed failure message
        AssertDuration(result);
    }
}
```

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Category
```bash
# Unit tests only
dotnet test --filter FullyQualifiedName~Application.Services

# Integration tests only
dotnet test --filter FullyQualifiedName~Integration

# End-to-End tests only
dotnet test --filter FullyQualifiedName~EndToEnd

# Performance tests only
dotnet test --filter FullyQualifiedName~Performance
```

### Run Tests with Coverage
```bash
# Generate coverage data
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate HTML coverage report
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:./coverage/**/coverage.cobertura.xml -targetdir:./coverage-report -reporttypes:Html

# Open coverage report (Linux)
xdg-open ./coverage-report/index.html

# Open coverage report (Windows)
start ./coverage-report/index.html
```

### Run Tests in Watch Mode
```bash
dotnet watch test
```

## Testing Patterns

### AAA Pattern (Arrange-Act-Assert)

All tests follow the AAA pattern for clarity and consistency:

```csharp
[Test]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and mocks
    var mockDependency = Substitute.For<IDependency>();
    var service = new ServiceUnderTest(mockDependency);

    // Act - Execute the method being tested
    var result = service.DoSomething();

    // Assert - Verify the expected outcome
    result.Should().Be(expectedValue);
}
```

### Test Naming Convention

Tests use the pattern: `[MethodName]_[Scenario]_[ExpectedResult]`

Examples:
- `RegisterCommand_WithValidCommand_ShouldAddToRegistry`
- `ExecuteAsync_WithMissingCommand_ShouldThrowException`
- `AutoCompleteWord_WithNoMatch_ShouldReturnEmptyString`

### Mocking with NSubstitute

```csharp
// Create a mock
var mockRegistry = Substitute.For<ICommandRegistry>();

// Configure mock behavior
mockRegistry.HasCommand("google").Returns(true);
mockRegistry.GetAllCommands().Returns(new List<Command> { command });

// Verify method was called
mockRegistry.Received(1).RegisterCommand(Arg.Any<Command>());

// Verify method was NOT called
mockRegistry.DidNotReceive().ClearCommands();
```

### Assertions with FluentAssertions

```csharp
// Value assertions
result.Should().Be(expectedValue);
result.Should().NotBeNull();
result.Should().BeOfType<Command>();

// Collection assertions
commands.Should().HaveCount(5);
commands.Should().Contain(command);
commands.Should().BeEmpty();
commands.Should().ContainSingle().Which.Name.Should().Be("google");

// String assertions
text.Should().StartWith("test");
text.Should().Contain("command");
text.Should().BeNullOrEmpty();

// Exception assertions
Action act = () => service.DoSomething();
act.Should().Throw<ArgumentNullException>()
    .WithParameterName("parameter");
```

### Testing Dialogs in E2E Tests

TeaLauncher uses the `IDialogService` abstraction to enable testable dialog interactions without requiring a display server.

**Why We Need MockDialogService**:
- E2E tests run in Avalonia.Headless mode (no display server)
- Real `AvaloniaDialogService` cannot show dialogs in headless environments
- `MockDialogService` records dialog calls for verification instead of displaying them

**Using MockDialogService in Tests**:

```csharp
[Test]
public async Task CommandExecution_WithError_ShowsErrorDialog()
{
    // Arrange
    var mockDialogService = MockFactory.CreateMockDialogService();
    var serviceProvider = TestServiceProvider.CreateWithMocks();
    var window = new MainWindow(testConfigPath, mockDialogService);

    // Act - trigger error condition
    await window.ExecuteInvalidCommand();

    // Assert - verify error dialog was shown
    mockDialogService.VerifyErrorShown("Error", "Command execution failed");

    // Alternative: inspect all dialog calls
    var dialogCalls = mockDialogService.GetDialogCalls();
    dialogCalls.Should().ContainSingle()
        .Which.DialogType.Should().Be("Error");
}
```

**MockDialogService API**:
- `ShowMessageAsync(title, message)` - Records message dialog
- `ShowConfirmAsync(title, message)` - Records confirm dialog, returns pre-configured response
- `ShowErrorAsync(title, message)` - Records error dialog
- `SetConfirmResponse(bool)` - Configure response for confirm dialogs
- `VerifyMessageShown(title, message)` - Assert message was shown
- `VerifyConfirmShown(title)` - Assert confirmation was requested
- `VerifyErrorShown(title, message)` - Assert error was shown
- `GetDialogCalls()` - Get all recorded dialog calls
- `ClearDialogHistory()` - Reset dialog history between tests

**Important**: Even `TestServiceProvider.CreateWithRealServices()` uses `MockDialogService` for `IDialogService` because headless mode cannot display real dialogs. This is documented in the code with comments explaining the limitation.

### Performance Testing Guidelines

Performance tests validate that TeaLauncher meets its performance requirements using automated, repeatable measurements.

**Performance Requirements**:
- **Command execution**: ≤100ms from user input to process start
- **Autocomplete**: ≤50ms with 1000 words in the word list
- **Configuration loading**: ≤200ms for configuration with 100 commands
- **Hotkey response**: ≤100ms (not testable in headless mode)

**Creating Performance Tests**:

1. **Inherit from PerformanceTestBase**:
```csharp
[TestFixture]
public class MyPerformanceTests : PerformanceTestBase
{
    // Test implementation
}
```

2. **Use TimeOperation or TimeOperationAsync**:
```csharp
[Test]
public async Task ConfigLoad_100Commands_Within200ms()
{
    // Arrange
    var configLoader = _serviceProvider.GetRequiredService<IConfigurationLoader>();
    var configPath = CreateTempConfigWith100Commands();
    const int maxAllowedMs = 200;

    // Act - TimeOperationAsync handles warmup and measurement
    var result = await TimeOperationAsync(
        "Config loading with 100 commands",
        async () => await configLoader.LoadConfigurationAsync(configPath),
        maxAllowedMs);

    // Assert - provides detailed failure message with actual vs expected
    AssertDuration(result);
}
```

**PerformanceTestBase Features**:
- **Automatic JIT warmup**: Runs operation once before measurement to eliminate JIT compilation overhead
- **High-resolution timing**: Uses `Stopwatch` for accurate measurements
- **Detailed failure messages**: Shows actual duration, max allowed, and percentage of limit used
- **Async support**: Both synchronous (`TimeOperation`) and asynchronous (`TimeOperationAsync`) methods

**Best Practices**:
- Run performance tests in **Release mode** for accurate measurements
- Mock external dependencies (Process.Start, file I/O) to isolate performance of the code under test
- Use realistic test data (e.g., `EdgeCaseTestFixtures.LargeWordList` for autocomplete tests)
- Document known performance variability in comments
- If tests are flaky due to system load, run multiple iterations and take average

**Example Performance Test**:
```csharp
[Test]
public void AutoComplete_With1000Words_Within50ms()
{
    // Arrange
    _autoCompleter.UpdateWordList(EdgeCaseTestFixtures.LargeWordList);
    const string prefix = "word";
    const int maxAllowedMs = 50;

    // Act
    var result = TimeOperation(
        "AutoComplete with 1000 words (AutoCompleteWord)",
        () => _autoCompleter.AutoCompleteWord(prefix),
        maxAllowedMs);

    // Assert - fails with message like:
    // "AutoComplete with 1000 words took 75.23ms but should complete
    //  within 50ms (150.5% of limit used)"
    AssertDuration(result);
}
```

### Edge Case Test Patterns

Edge case tests ensure TeaLauncher handles uncommon scenarios correctly, including unicode input, special characters, large datasets, and malformed configuration.

**EdgeCaseTestFixtures Utility**:

TeaLauncher provides centralized edge case test data in `EdgeCaseTestFixtures.cs`:

```csharp
// Unicode command names (Chinese, Japanese, Arabic, Emoji, etc.)
var unicodeCommands = EdgeCaseTestFixtures.UnicodeCommandNames;
// Contains: "搜索", "検索", "поиск", "🔍search", etc.

// Special characters that may cause parsing issues
var specialArgs = EdgeCaseTestFixtures.SpecialCharacterArguments;
// Contains: quotes, backslashes, pipes, unicode filenames, etc.

// 1000 words for autocomplete performance testing
var largeWordList = EdgeCaseTestFixtures.LargeWordList;

// Malformed YAML samples for error testing
var malformedYaml = EdgeCaseTestFixtures.MalformedYamlSamples;
// Contains: missing colons, wrong indentation, invalid escapes, etc.
```

**Testing Unicode Support**:
```csharp
[Test]
public void RegisterCommand_WithUnicodeNames_StoresCorrectly()
{
    // Arrange
    var registry = new CommandRegistryService(_mockAutoCompleter);

    // Act - test with various unicode scripts
    foreach (var unicodeName in EdgeCaseTestFixtures.UnicodeCommandNames)
    {
        var command = new Command(unicodeName, "https://example.com");
        registry.RegisterCommand(command);
    }

    // Assert
    registry.GetAllCommands().Should().HaveCount(
        EdgeCaseTestFixtures.UnicodeCommandNames.Count);
}
```

**Testing Special Character Handling**:
```csharp
[Test]
public void ExecuteAsync_WithQuotedArguments_ParsesCorrectly()
{
    // Arrange
    var executor = new CommandExecutorService();
    var testArg = "arg with \"nested quotes\" inside";

    // Act
    var result = await executor.ExecuteAsync("command", testArg);

    // Assert - verify argument was parsed correctly, quotes preserved
    result.Arguments.Should().Contain(testArg);
}
```

**Testing Large Dataset Performance**:
```csharp
[Test]
public void AutoComplete_With1000Words_ReturnsCorrectly()
{
    // Arrange
    _autoCompleter.UpdateWordList(EdgeCaseTestFixtures.LargeWordList);

    // Act
    var result = _autoCompleter.GetCandidates("word");

    // Assert - should handle large dataset efficiently
    result.Should().NotBeEmpty();
    result.Should().AllSatisfy(word => word.Should().StartWith("word"));
}
```

**Testing Malformed Input**:
```csharp
[Test]
public void LoadConfiguration_MalformedYaml_ProvidesLineNumber()
{
    // Arrange
    var configLoader = new YamlConfigLoaderService();

    // Act & Assert - test each malformed YAML sample
    foreach (var (description, yaml) in EdgeCaseTestFixtures.MalformedYamlSamples)
    {
        var tempFile = WriteYamlToTempFile(yaml);

        Func<Task> act = async () => await configLoader.LoadConfigurationAsync(tempFile);

        act.Should().ThrowAsync<YamlException>()
            .WithMessage($"*{description}*",
                because: $"malformed YAML ({description}) should throw with helpful error");
    }
}
```

**Edge Case Categories**:
- **Unicode**: Command names and arguments in various scripts (CJK, Arabic, Cyrillic, Emoji)
- **Special Characters**: Quotes, backslashes, pipes, shell operators, whitespace
- **Large Datasets**: 1000+ words for autocomplete stress testing
- **Malformed Input**: Invalid YAML syntax, structural errors, encoding issues
- **Empty/Whitespace**: Empty strings, spaces-only, various whitespace characters
- **Case Sensitivity**: Uppercase, lowercase, mixed case variations

## Coverage Thresholds

### Overall Coverage Requirements
- **Line Coverage**: ≥80%
- **Branch Coverage**: ≥70%

### Layer-Specific Requirements
- **Domain Layer**: ≥90% (interfaces and models)
- **Application Layer**: ≥90% (services and orchestration)
- **Infrastructure Layer**: ≥80% (platform-specific code may have lower coverage on Linux)

### Checking Coverage

The coverage check script validates these thresholds:

```bash
./scripts/check-coverage.sh
```

If coverage is below threshold, the script will fail with:
```
❌ COVERAGE CHECK FAILED
Coverage 75% is below threshold 80%

To fix:
1. Add more unit tests to increase coverage
2. Run 'dotnet test --collect:"XPlat Code Coverage"' to generate coverage report
3. Check coverage report for uncovered lines
```

## Writing Tests

### Adding New Tests

1. **Create test file** in the appropriate directory matching the source structure:
   - Unit tests for `Application/Services/FooService.cs` → `Application/Services/FooServiceTests.cs`
   - Integration tests → `Integration/FooIntegrationTests.cs`
   - E2E tests → `EndToEnd/FooWorkflowTests.cs`

2. **Add XML documentation** to the test class:
```csharp
/// <summary>
/// Unit tests for FooService.
/// Tests foo creation, bar processing, and baz validation.
/// </summary>
[TestFixture]
public class FooServiceTests
{
    // ...
}
```

3. **Use test utilities** for common setup:
```csharp
// Create mock instances
var mockRegistry = MockFactory.CreateMockCommandRegistry(TestFixtures.SampleCommands);
var mockDialogService = MockFactory.CreateMockDialogService();

// Create service provider with mocks
var serviceProvider = TestServiceProvider.CreateWithMocks();

// Create service provider with real services
var serviceProvider = TestServiceProvider.CreateWithRealServices();

// Use edge case test data
var unicodeCommands = EdgeCaseTestFixtures.UnicodeCommandNames;
var largeWordList = EdgeCaseTestFixtures.LargeWordList;
```

4. **Follow AAA pattern** and use descriptive test names

5. **Add comments for complex scenarios**:
```csharp
[Test]
public void ParseCommandLine_WithEscapedQuotes_ShouldHandleCorrectly()
{
    // This test verifies that escaped quotes within quoted strings
    // are properly handled: command "arg with \"escaped\" quotes"
    // should be parsed as ["command", "arg with \"escaped\" quotes"]

    // Arrange
    var input = "command \"arg with \\\"escaped\\\" quotes\"";

    // Act
    var result = parser.ParseCommandLine(input);

    // Assert
    result.Should().HaveCount(2);
    result[1].Should().Be("arg with \"escaped\" quotes");
}
```

### Test Organization

Test files mirror the source structure:

```
TeaLauncher.Avalonia/
  Application/
    Services/
      CommandRegistryService.cs
  Domain/
    Interfaces/
      ICommandRegistry.cs

TeaLauncher.Avalonia.Tests/
  Application/
    Services/
      CommandRegistryServiceTests.cs   # Unit tests
  Integration/
    CommandWorkflowTests.cs            # Integration tests
  EndToEnd/
    UserWorkflowTests.cs               # E2E tests
  Performance/
    AutoCompletePerformanceTests.cs    # Performance tests
    CommandExecutionPerformanceTests.cs
    ConfigLoadPerformanceTests.cs
    PerformanceTestBase.cs             # Base class for performance tests
    PerformanceResult.cs               # Performance result data
  Utilities/
    TestServiceProvider.cs             # Test helpers
    MockFactory.cs
    TestFixtures.cs
    EdgeCaseTestFixtures.cs            # Edge case test data
    MockDialogService.cs               # Mock dialog service for E2E tests
```

## Pre-commit Workflow

TeaLauncher uses Husky.Net pre-commit hooks to automatically enforce quality standards before every commit. This prevents non-compliant code from entering the repository and ensures all commits meet our quality gates.

### Overview

When you run `git commit`, Husky.Net automatically executes a series of quality checks:

1. **build** - Compile main project in Release configuration
2. **build-tests** - Compile test project
3. **test** - Run all tests (currently 305 tests)
4. **coverage** - Generate code coverage report
5. **coverage-check** - Verify coverage ≥60% threshold
6. **metrics** - Check code metrics (file ≤500 lines, method ≤50 lines, complexity ≤15)
7. **format-check** - Verify code formatting compliance

**Total execution time**: ~15-20 seconds for a typical commit

If any check fails, the commit is **blocked** and you must fix the issues before committing.

### Pre-commit Execution Flow

```
┌─────────────────────────────────────┐
│  Developer runs: git commit -m "..."│
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ Husky.Net pre-commit hook triggers │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ Task 1: Build (Release)             │
│ - Compiles TeaLauncher.Avalonia     │
│ - ~3 seconds                        │
└──────────────┬──────────────────────┘
               │ ✓ Success
               ▼
┌─────────────────────────────────────┐
│ Task 2: Build Tests                 │
│ - Compiles test project             │
│ - ~2 seconds                        │
└──────────────┬──────────────────────┘
               │ ✓ Success
               ▼
┌─────────────────────────────────────┐
│ Task 3: Test                        │
│ - Runs all 305 tests                │
│ - ~2 seconds                        │
└──────────────┬──────────────────────┘
               │ ✓ All tests passed
               ▼
┌─────────────────────────────────────┐
│ Task 4: Coverage                    │
│ - Generates coverage.cobertura.xml  │
│ - ~3 seconds                        │
└──────────────┬──────────────────────┘
               │ ✓ Coverage collected
               ▼
┌─────────────────────────────────────┐
│ Task 5: Coverage Check              │
│ - Validates coverage ≥60%           │
│ - Currently: 61%                    │
│ - ~10ms                             │
└──────────────┬──────────────────────┘
               │ ✓ Threshold met
               ▼
┌─────────────────────────────────────┐
│ Task 6: Metrics                     │
│ - Checks file/method length         │
│ - Checks cyclomatic complexity      │
│ - ~3 seconds                        │
└──────────────┬──────────────────────┘
               │ ✓ No violations
               ▼
┌─────────────────────────────────────┐
│ Task 7: Format Check                │
│ - Verifies dotnet format compliance │
│ - ~6 seconds                        │
└──────────────┬──────────────────────┘
               │ ✓ Format compliant
               ▼
┌─────────────────────────────────────┐
│   ✅ Commit succeeds                │
└─────────────────────────────────────┘
```

### Successful Commit Example

```bash
$ git add TeaLauncher.Avalonia/Application/Services/FooService.cs
$ git commit -m "Add FooService implementation"

[Husky] 🚀 Loading tasks ...
--------------------------------------------------
[Husky] ⚡ Preparing task 'build'
[Husky] ⌛ Executing task 'build' ...
MSBuild version 17.8.0+...
  TeaLauncher.Avalonia -> .../bin/Release/net8.0-windows/win-x64/TeaLauncher.Avalonia.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.15
[Husky]  ✔ Successfully executed in 3,440ms
--------------------------------------------------
[Husky] ⚡ Preparing task 'build-tests'
[Husky] ⌛ Executing task 'build-tests' ...
  TeaLauncher.Avalonia.Tests -> .../bin/Release/net8.0-windows/TeaLauncher.Avalonia.Tests.dll
[Husky]  ✔ Successfully executed in 1,827ms
--------------------------------------------------
[Husky] ⚡ Preparing task 'test'
[Husky] ⌛ Executing task 'test' ...
Passed!  - Failed:     0, Passed:   305, Skipped:     0, Total:   305, Duration: 810 ms
[Husky]  ✔ Successfully executed in 2,445ms
--------------------------------------------------
[Husky] ⚡ Preparing task 'coverage'
[Husky] ⌛ Executing task 'coverage' ...
Passed!  - Failed:     0, Passed:   305, Skipped:     0, Total:   305, Duration: 764 ms
Attachments:
  /home/user/repos/TeaLauncher/coverage/.../coverage.cobertura.xml
[Husky]  ✔ Successfully executed in 3,402ms
--------------------------------------------------
[Husky] ⚡ Preparing task 'coverage-check'
[Husky] ⌛ Executing task 'coverage-check' ...
Checking coverage report: ./coverage/.../coverage.cobertura.xml
Current coverage: 61%
Required threshold: 60%
✅ Coverage check passed (61% >= 60%)
[Husky]  ✔ Successfully executed in 12ms
--------------------------------------------------
[Husky] ⚡ Preparing task 'metrics'
[Husky] ⌛ Executing task 'metrics' ...
Analyzing project: TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj
Analyzed 24 file(s).
✓ All code metrics checks passed.
[Husky]  ✔ Successfully executed in 3,067ms
--------------------------------------------------
[Husky] ⚡ Preparing task 'format-check'
[Husky] ⌛ Executing task 'format-check' ...
[Husky]  ✔ Successfully executed in 6,206ms
--------------------------------------------------

[master a1b2c3d] Add FooService implementation
 1 file changed, 50 insertions(+)
```

### Running Checks Manually

You can run pre-commit checks manually before committing:

```bash
# Run all pre-commit checks (exactly what runs on commit)
dotnet husky run

# Run individual checks
dotnet build --configuration Release --no-restore
dotnet build --configuration Release TeaLauncher.Avalonia.Tests/TeaLauncher.Avalonia.Tests.csproj
dotnet test --no-build --verbosity minimal
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
./scripts/check-coverage.sh
dotnet run --project tools/MetricsChecker/MetricsChecker.csproj -- TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj
dotnet format --verify-no-changes
```

**Recommendation**: Run `dotnet husky run` before committing large changes to catch issues early.

### Bypassing Hooks (Emergency Use Only)

⚠️ **Use only when absolutely necessary** (e.g., work-in-progress commit on a feature branch):

```bash
git commit --no-verify -m "WIP: Feature in progress"
```

**When to use `--no-verify`**:
- Creating a WIP commit on a feature branch for backup purposes
- Committing partial work before switching branches urgently
- Emergency hotfix when CI is broken (very rare)

**When NOT to use `--no-verify`**:
- Regular commits on main/master branch
- Pull request commits
- "Final" commits before code review
- To avoid fixing quality issues (fix them instead!)

### Troubleshooting Pre-commit Failures

#### Build Failure

**Symptom**:
```
[Husky] ⌛ Executing task 'build' ...
error CS0103: The name 'Foo' does not exist in the current context
[Husky]  ✘ Task 'build' failed
```

**Solution**:
1. Fix compilation errors in your code
2. Verify build succeeds locally: `dotnet build --configuration Release`
3. Retry commit

#### Test Failure

**Symptom**:
```
[Husky] ⌛ Executing task 'test' ...
Failed!  - Failed:     3, Passed:   302, Skipped:     0, Total:   305
[Husky]  ✘ Task 'test' failed
```

**Solution**:
1. Run tests locally: `dotnet test`
2. Fix failing tests (see test output for details)
3. Verify all tests pass: `dotnet test`
4. Retry commit

**Common causes**:
- Broke existing functionality
- Forgot to update tests after changing implementation
- Test data fixtures need updating

#### Coverage Below Threshold

**Symptom**:
```
[Husky] ⌛ Executing task 'coverage-check' ...
Current coverage: 58%
Required threshold: 60%
❌ COVERAGE CHECK FAILED
Coverage 58% is below threshold 60%
[Husky]  ✘ Task 'coverage-check' failed
```

**Solution**:
1. Run coverage locally: `dotnet test --collect:"XPlat Code Coverage"`
2. Check which lines are uncovered: `./scripts/check-coverage.sh` (shows detailed report)
3. Add unit tests for uncovered code paths
4. Verify coverage: `./scripts/check-coverage.sh`
5. Retry commit

**Tips**:
- Focus on Application and Domain layers (highest impact)
- Test both success and error paths
- Don't write "coverage tests" just to hit lines - write meaningful tests
- If new code is truly untestable, discuss with team about refactoring

#### Metrics Violation

**Symptom**:
```
[Husky] ⌛ Executing task 'metrics' ...
❌ METHOD TOO LONG: ExecuteAsync (68 lines) in CommandExecutorService.cs
   Maximum allowed: 50 lines
   Suggestion: Extract helper methods or refactor logic
[Husky]  ✘ Task 'metrics' failed
```

**Solution**:
1. Refactor long methods by extracting helper methods
2. Break complex classes into smaller, focused classes
3. Verify compliance: `dotnet run --project tools/MetricsChecker/MetricsChecker.csproj -- TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj`
4. Retry commit

**Common violations**:
- **Method > 50 lines**: Extract helper methods, each with single responsibility
- **File > 500 lines**: Split into multiple files (e.g., separate nested classes)
- **Complexity > 15**: Simplify conditional logic, extract validation methods

#### Format Violation

**Symptom**:
```
[Husky] ⌛ Executing task 'format-check' ...
Formatting code files in workspace 'TeaLauncher.Avalonia.sln'.
  TeaLauncher.Avalonia/Application/Services/FooService.cs
[Husky]  ✘ Task 'format-check' failed
```

**Solution**:
1. Auto-fix formatting: `dotnet format`
2. Verify compliance: `dotnet format --verify-no-changes`
3. Re-stage formatted files: `git add -u`
4. Retry commit

**Common violations**:
- Incorrect indentation (tabs vs spaces)
- Missing/extra blank lines
- Line length exceeds limit
- Inconsistent brace placement

**Note**: Run `dotnet format` before committing to automatically fix formatting.

### Best Practices

1. **Run `dotnet format` before committing**
   ```bash
   dotnet format && git add -u && git commit -m "Your message"
   ```

2. **Run tests during development, not just on commit**
   ```bash
   dotnet watch test  # Auto-runs tests on file changes
   ```

3. **Check metrics periodically for long methods**
   ```bash
   dotnet run --project tools/MetricsChecker/MetricsChecker.csproj -- TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj
   ```

4. **Monitor coverage as you write code**
   ```bash
   dotnet test --collect:"XPlat Code Coverage" && ./scripts/check-coverage.sh
   ```

5. **Commit small, focused changes**
   - Smaller commits = faster pre-commit execution
   - Easier to debug if pre-commit fails
   - Better Git history

6. **Use descriptive commit messages**
   - Pre-commit checks take ~20 seconds, so make commits count
   - Follow pattern: `<type>: <description>` (e.g., "feat: Add FooService")

### Hook Configuration

The pre-commit hooks are configured in `.husky/task-runner.json`:

```json
{
  "tasks": [
    {
      "name": "build",
      "command": "dotnet",
      "args": ["build", "--configuration", "Release", "--no-restore"]
    },
    {
      "name": "build-tests",
      "command": "dotnet",
      "args": ["build", "--configuration", "Release", "TeaLauncher.Avalonia.Tests/TeaLauncher.Avalonia.Tests.csproj"]
    },
    {
      "name": "test",
      "command": "dotnet",
      "args": ["test", "--no-build", "--verbosity", "minimal"]
    },
    {
      "name": "coverage",
      "command": "dotnet",
      "args": ["test", "--collect:XPlat Code Coverage", "--results-directory", "./coverage"]
    },
    {
      "name": "coverage-check",
      "command": "bash",
      "args": ["scripts/check-coverage.sh"]
    },
    {
      "name": "metrics",
      "command": "dotnet",
      "args": ["run", "--project", "tools/MetricsChecker/MetricsChecker.csproj", "--", "TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj"]
    },
    {
      "name": "format-check",
      "command": "dotnet",
      "args": ["format", "--verify-no-changes"]
    }
  ]
}
```

To modify hook behavior, edit this file (requires team discussion).

### Performance Optimization Tips

If pre-commit hooks feel slow:

1. **Use incremental builds** (already enabled with `--no-restore`)
2. **Keep test suite fast** (currently ~800ms, target <1 second)
3. **Commit more frequently** (warm build cache)
4. **Use `git commit --amend`** for small fixes (only runs hooks once)

### Disabling Hooks Temporarily (Development)

If you need to disable hooks during development (e.g., experimenting with breaking changes):

```bash
# Disable Husky globally (will affect all commits)
dotnet husky disable

# Make your commits
git commit -m "Experimental changes"

# Re-enable Husky
dotnet husky enable
```

⚠️ **Remember to re-enable before pushing!**

## CI/CD Integration

### GitHub Actions Workflows

Two workflows enforce quality gates:

1. **`ci.yml`**: Runs on push to main branch
   - Multi-platform build (Linux and Windows)
   - All tests with coverage
   - Metrics validation
   - Format verification

2. **`pr-validation.yml`**: Runs on pull requests
   - Same checks as `ci.yml`
   - Uploads coverage report as artifact
   - Adds PR comment with coverage summary if below threshold
   - Blocks merge if any check fails

### Viewing CI Results

1. **Check workflow status**: Go to GitHub Actions tab
2. **Download coverage report**: Click on workflow run → Artifacts → `coverage-report.zip`
3. **View coverage summary**: Shown in PR comments if coverage is below threshold

### Branch Protection

Pull requests require:
- All CI checks passing
- Coverage ≥80%
- No code metrics violations
- No format violations
- At least one approval

## Troubleshooting

### Common Test Failures

**Problem**: Tests fail with "Process cannot access file"
**Solution**: Ensure `[TearDown]` properly cleans up temporary files:
```csharp
[TearDown]
public void TearDown()
{
    if (File.Exists(_tempFile))
    {
        File.Delete(_tempFile);
    }
}
```

**Problem**: Tests fail on Linux with "Platform not supported"
**Solution**: Mock platform-specific services (IHotkeyManager, IIMEController):
```csharp
var mockHotkeyManager = Substitute.For<IHotkeyManager>();
mockHotkeyManager.IsRegistered.Returns(true);
```

**Problem**: E2E tests fail with "No X11 display"
**Solution**: Tests use Avalonia.Headless which doesn't require display server. Ensure you're using `[AvaloniaTest]` attribute:
```csharp
[AvaloniaTest]
public void TestName()
{
    // Test code
}
```

**Problem**: Coverage below threshold
**Solution**:
1. Generate HTML report to see uncovered lines
2. Add tests for uncovered code paths
3. Focus on Application and Domain layers (highest coverage requirements)

**Problem**: Tests are flaky (pass sometimes, fail sometimes)
**Solution**:
- Remove timing dependencies (`Task.Delay`, `Thread.Sleep`)
- Use deterministic mocks, not real external services
- Clean up state properly in `[TearDown]`

### Performance Issues

**Problem**: Tests take too long
**Solution**:
- Unit tests should use mocks, not real I/O
- Integration tests should use temporary files, not full database
- E2E tests should mock external processes (Process.Start)
- Run tests in parallel (default in NUnit)

## Examples

### Example Unit Test

See: [CommandRegistryServiceTests.cs](TeaLauncher.Avalonia.Tests/Application/Services/CommandRegistryServiceTests.cs)

Key features:
- Uses NSubstitute for mocking IAutoCompleter
- Tests all public methods with success and failure paths
- Verifies auto-completer synchronization
- Clear AAA pattern with descriptive names

### Example Integration Test

See: [CommandWorkflowTests.cs](TeaLauncher.Avalonia.Tests/Integration/CommandWorkflowTests.cs)

Key features:
- Uses real service implementations
- Tests complete workflow from config load to command execution
- Uses TestServiceProvider.CreateWithRealServices()
- Tests with real YAML files in Fixtures/
- Proper cleanup in [TearDown]

### Example End-to-End Test

See: [UserWorkflowTests.cs](TeaLauncher.Avalonia.Tests/EndToEnd/UserWorkflowTests.cs)

Key features:
- Uses Avalonia.Headless for UI testing
- Simulates real user interactions (keyboard events)
- Tests complete user journeys
- Uses [AvaloniaTest] attribute
- Runs on Linux without display server
- Uses MockDialogService for dialog verification

### Example Performance Test

See: [AutoCompletePerformanceTests.cs](TeaLauncher.Avalonia.Tests/Performance/AutoCompletePerformanceTests.cs)

Key features:
- Inherits from PerformanceTestBase
- Uses TimeOperation/TimeOperationAsync for measurements
- Includes JIT warmup to avoid measurement bias
- Tests against specific thresholds (50ms for autocomplete)
- Uses EdgeCaseTestFixtures.LargeWordList for realistic stress testing
- Provides detailed failure messages with actual vs expected duration

### Example Edge Case Test

See: [CommandRegistryServiceTests.cs](TeaLauncher.Avalonia.Tests/Application/Services/CommandRegistryServiceTests.cs)

Key features:
- Uses EdgeCaseTestFixtures for unicode and special character testing
- Tests boundary conditions and uncommon scenarios
- Validates error handling for malformed input
- Ensures robust handling of large datasets
- Tests case sensitivity with various character sets

## Test Utilities

### TestServiceProvider

Helper for creating service providers in tests:

```csharp
// Create with all dependencies as mocks
var serviceProvider = TestServiceProvider.CreateWithMocks();

// Create with real service implementations
var serviceProvider = TestServiceProvider.CreateWithRealServices();

// Create with custom configuration
var serviceProvider = TestServiceProvider.CreateCustom(services =>
{
    services.AddSingleton<ICommandRegistry>(mockRegistry);
    services.AddSingleton<ICommandExecutor, CommandExecutorService>();
});
```

### MockFactory

Helper for creating pre-configured mocks:

```csharp
// Create mock with test commands
var mockRegistry = MockFactory.CreateMockCommandRegistry(
    new Command("google", "https://google.com"),
    new Command("docs", "https://docs.com")
);

// Create mock auto-completer with word list
var mockAutoCompleter = MockFactory.CreateMockAutoCompleter("google", "github", "docs");

// Create mock configuration loader with test config
var mockConfigLoader = MockFactory.CreateMockConfigurationLoader(config);

// Create mock dialog service for testing UI interactions
var mockDialogService = MockFactory.CreateMockDialogService(defaultConfirmResponse: true);
```

### TestFixtures

Reusable test data:

```csharp
// Use sample commands
var commands = TestFixtures.SampleCommands;

// Use valid YAML config
var yaml = TestFixtures.SampleYamlConfig;

// Use invalid YAML (for error testing)
var invalidYaml = TestFixtures.InvalidYamlConfig;
```

### EdgeCaseTestFixtures

Centralized edge case test data for robust testing:

```csharp
// Unicode command names (20+ examples in various scripts)
var unicodeNames = EdgeCaseTestFixtures.UnicodeCommandNames;
// Contains: "搜索" (Chinese), "検索" (Japanese), "поиск" (Russian), "🔍search" (Emoji), etc.

// Special character arguments for parsing tests
var specialArgs = EdgeCaseTestFixtures.SpecialCharacterArguments;
// Contains: quotes, backslashes, pipes, unicode filenames, shell operators

// Large word list (1000 words) for performance testing
var largeList = EdgeCaseTestFixtures.LargeWordList;

// Malformed YAML samples with descriptions
foreach (var (description, yaml) in EdgeCaseTestFixtures.MalformedYamlSamples)
{
    // Test error handling for each malformed sample
}

// Empty and whitespace-only inputs
var emptyInputs = EdgeCaseTestFixtures.EmptyAndWhitespaceInputs;

// Case sensitivity test pairs
var casePairs = EdgeCaseTestFixtures.CaseSensitivityPairs;
```

## Additional Resources

- [NUnit Documentation](https://docs.nunit.org/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Avalonia.Headless Documentation](https://docs.avaloniaui.net/docs/concepts/headless/)
- [Design Document](.spec-workflow/specs/architecture-testability-improvements/design.md)

## Contributing

When contributing tests:

1. **Follow existing patterns**: Use AAA, descriptive names, FluentAssertions
2. **Maintain coverage**: Ensure new code has ≥90% coverage (Application/Domain)
3. **Add documentation**: XML comments on test classes, inline comments for complex scenarios
4. **Test edge cases**: Not just happy paths, but also error conditions and boundary cases
5. **Keep tests fast**: Mock dependencies, use temporary files, clean up properly
6. **Verify locally**: Run pre-commit checks before pushing

Questions? Check the design document or open an issue!
