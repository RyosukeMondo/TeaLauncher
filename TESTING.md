# Testing Guide for TeaLauncher

This document describes the testing strategy, patterns, and practices for TeaLauncher. Following these guidelines ensures consistent, maintainable, and effective tests.

## Overview

TeaLauncher uses a comprehensive testing strategy with three levels of testing:

1. **Unit Tests**: Test individual components in isolation with mocked dependencies
2. **Integration Tests**: Test service interactions with real implementations
3. **End-to-End Tests**: Test complete user workflows using Avalonia headless mode

Our testing approach emphasizes:
- **Clean Architecture**: Tests follow the same layered structure as the application
- **Testability**: All services use dependency injection for easy mocking
- **Coverage Goals**: ≥80% overall coverage, ≥90% for Domain and Application layers
- **Quality Gates**: Automated pre-commit hooks and CI/CD checks enforce quality standards

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

// Create service provider with mocks
var serviceProvider = TestServiceProvider.CreateWithMocks();

// Create service provider with real services
var serviceProvider = TestServiceProvider.CreateWithRealServices();
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
  Utilities/
    TestServiceProvider.cs             # Test helpers
    MockFactory.cs
    TestFixtures.cs
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
