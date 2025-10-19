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

## Pre-commit Verification

Pre-commit hooks using Husky.Net automatically run quality checks before each commit.

### What Runs on Pre-commit

1. **Build**: Compile main project and tests
2. **Test**: Run all tests
3. **Coverage**: Generate coverage and verify ≥80% threshold
4. **Metrics**: Check code metrics (file ≤500 lines, method ≤50 lines, complexity ≤15)
5. **Format**: Verify code formatting

### Running Checks Manually

```bash
# Run all pre-commit checks
dotnet husky run

# Run individual checks
dotnet build --configuration Release --no-restore
dotnet test --no-build --verbosity minimal
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
./scripts/check-coverage.sh
dotnet run --project tools/MetricsChecker/MetricsChecker.csproj -- TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj
dotnet format --verify-no-changes
```

### Bypassing Hooks (Emergency Only)

⚠️ **Use only when absolutely necessary** (e.g., work-in-progress commit on a feature branch):

```bash
git commit --no-verify -m "WIP: Feature in progress"
```

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
