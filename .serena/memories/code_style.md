# TeaLauncher Code Style and Conventions

## C# Conventions

### General
- **Language Version**: C# 12
- **Nullable Reference Types**: Enabled (`<Nullable>enable</Nullable>`)
- **Warnings as Errors**: Enabled (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)

### Naming Conventions
- **Classes**: PascalCase (e.g., `CommandRegistryService`, `ApplicationOrchestrator`)
- **Interfaces**: PascalCase with `I` prefix (e.g., `ICommandRegistry`, `IAutoCompleter`)
- **Methods**: PascalCase (e.g., `RegisterCommand`, `ExecuteAsync`)
- **Properties**: PascalCase (e.g., `IsRegistered`, `CommandName`)
- **Private Fields**: _camelCase with underscore prefix (e.g., `_commands`, `_autoCompleter`)
  - **Note**: Legacy code uses `m_` prefix (e.g., `m_Commands`), but new code uses `_` prefix
- **Parameters**: camelCase (e.g., `command`, `configPath`)
- **Local Variables**: camelCase (e.g., `result`, `executor`)

### File Organization
- **One class per file**: File name matches class name
- **Namespaces**: Follow directory structure
  - Domain: `TeaLauncher.Avalonia.Domain.Interfaces` / `.Models`
  - Application: `TeaLauncher.Avalonia.Application.Services` / `.Orchestration`
  - Infrastructure: `TeaLauncher.Avalonia.Infrastructure.Configuration` / `.Platform`

### Type Usage
- **Prefer records for models**: Use `record` for immutable data (e.g., `Command`)
- **Use `required` keyword**: For required properties in records/classes
- **Nullable annotations**: Use `?` for nullable reference types
- **Async methods**: Suffix with `Async` (e.g., `LoadConfigurationAsync`)
- **Return types**: Use `Task` for async, specific types for sync

### Code Metrics Thresholds
Enforced by MetricsChecker and pre-commit hooks:
- **File length**: ≤500 lines
- **Method length**: ≤50 lines (excluding blank lines and comments)
- **Cyclomatic complexity**: ≤15 per method

## Architecture Patterns

### Dependency Injection
- **Constructor injection only**: No property injection or service locator
- **Interface-based**: All services depend on interfaces, not concrete types
- **Lifetime management**:
  - Singleton: Stateful services (registries, autocompleter, platform services)
  - Transient: Per-operation services (orchestrator, windows)

### Clean Architecture Layers
```
Domain (interfaces + models)
    ↑
Application (services + orchestration)
    ↑
Infrastructure (platform-specific implementations)
    ↑
Presentation (Avalonia UI)
```

### Service Patterns
- **Single Responsibility**: Each service has one clear purpose
- **Interface Segregation**: Interfaces are small and focused
- **Dependency Inversion**: Depend on abstractions, not concretions

## Documentation

### XML Documentation
- **Required for**:
  - All public classes
  - All public interfaces
  - All public methods
  - Test classes (with summary of what they test)
  
### Inline Comments
- **Use for**:
  - Complex algorithms
  - Non-obvious business logic
  - Workarounds or platform-specific hacks
  - Edge cases in tests

### Example:
```csharp
/// <summary>
/// Service responsible for managing command registration and lookup.
/// Synchronizes with auto-completer when commands change.
/// </summary>
public class CommandRegistryService : ICommandRegistry
{
    private readonly IAutoCompleter _autoCompleter;
    private readonly List<Command> _commands;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRegistryService"/> class.
    /// </summary>
    /// <param name="autoCompleter">The auto-completer to synchronize with.</param>
    public CommandRegistryService(IAutoCompleter autoCompleter)
    {
        _autoCompleter = autoCompleter ?? throw new ArgumentNullException(nameof(autoCompleter));
        _commands = new List<Command>();
    }
}
```

## Testing Conventions

### Test Naming
- **Pattern**: `[MethodName]_[Scenario]_[ExpectedResult]`
- **Examples**:
  - `RegisterCommand_WithValidCommand_ShouldAddToRegistry`
  - `ExecuteAsync_WithMissingCommand_ShouldThrowException`
  - `AutoCompleteWord_WithNoMatch_ShouldReturnEmptyString`

### Test Structure (AAA Pattern)
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

### Test Organization
- Mirror source structure: `Application/Services/FooService.cs` → `Application/Services/FooServiceTests.cs`
- Test categories: Unit (`Application/`, `Domain/`), Integration (`Integration/`), E2E (`EndToEnd/`)

## YAML Configuration Style
- **Lowercase keys**: `commands:`, `name:`, `linkto:`
- **Indentation**: 2 spaces
- **Optional fields**: Use `description:`, `arguments:` when needed
- **Example**:
```yaml
commands:
  - name: google
    linkto: https://google.com
    description: Google search
```

## Git Commit Messages
- **Format**: Present tense, imperative mood
- **Examples**:
  - ✅ "Add CommandRegistryService tests"
  - ✅ "Fix auto-completion for special characters"
  - ✅ "Refactor executor service to use async/await"
  - ❌ "Added tests"
  - ❌ "WIP"
