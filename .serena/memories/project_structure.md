# TeaLauncher Project Structure

## Root Directory
```
TeaLauncher/
├── .github/workflows/     # CI/CD GitHub Actions workflows
├── .husky/                # Git hooks configuration (Husky.Net)
├── .spec-workflow/        # Specification-driven development documents
├── CommandLauncher/       # Legacy shared library (linked files)
├── docs/                  # Documentation
├── scripts/               # Build and quality check scripts
├── tools/                 # Custom development tools
│   └── MetricsChecker/    # Code metrics analyzer
├── TeaLauncher.Avalonia/  # Main application project
└── TeaLauncher.Avalonia.Tests/  # Test project
```

## TeaLauncher.Avalonia (Main Project)
```
TeaLauncher.Avalonia/
├── Domain/
│   ├── Interfaces/        # Business logic contracts
│   │   ├── ICommandRegistry.cs
│   │   ├── ICommandExecutor.cs
│   │   ├── IAutoCompleter.cs
│   │   ├── IConfigurationLoader.cs
│   │   ├── IHotkeyManager.cs
│   │   └── IIMEController.cs
│   └── Models/            # Domain models
│       └── Command.cs     # Command record type
│
├── Application/
│   ├── Services/          # Business logic implementations
│   │   ├── CommandRegistryService.cs
│   │   ├── CommandExecutorService.cs
│   │   └── AutoCompleterService.cs
│   └── Orchestration/     # Workflow coordination
│       └── ApplicationOrchestrator.cs
│
├── Infrastructure/
│   ├── Configuration/     # Configuration loading
│   │   └── YamlConfigLoaderService.cs
│   └── Platform/          # Platform-specific code
│       ├── WindowsHotkeyService.cs
│       └── WindowsIMEControllerService.cs
│
├── Configuration/         # Legacy configuration models
│   ├── CommandConfig.cs
│   └── CommandEntry.cs
│
├── Views/                 # Avalonia UI views
│   ├── MainWindow.axaml
│   └── MainWindow.axaml.cs
│
├── Assets/                # Images, icons, resources
├── App.axaml              # Application XAML
├── App.axaml.cs           # Application code-behind
├── Program.cs             # Application entry point
├── ServiceConfiguration.cs # DI container configuration
└── commands.yaml          # Default command configuration
```

## TeaLauncher.Avalonia.Tests (Test Project)
```
TeaLauncher.Avalonia.Tests/
├── Application/
│   ├── Services/          # Unit tests for services
│   │   ├── CommandRegistryServiceTests.cs
│   │   ├── CommandExecutorServiceTests.cs
│   │   └── AutoCompleterServiceTests.cs
│   └── Orchestration/     # Unit tests for orchestration
│       └── ApplicationOrchestratorTests.cs
│
├── Infrastructure/
│   └── Configuration/     # Infrastructure tests
│       └── YamlConfigLoaderServiceTests.cs
│
├── Integration/           # Integration tests
│   ├── Fixtures/          # Test YAML files
│   │   ├── test-commands.yaml
│   │   └── invalid-commands.yaml
│   ├── CommandWorkflowTests.cs
│   └── ConfigurationIntegrationTests.cs
│
├── EndToEnd/              # End-to-end tests
│   ├── Fixtures/          # E2E test data
│   │   └── e2e-test-config.yaml
│   ├── ApplicationLifecycleTests.cs
│   ├── UserWorkflowTests.cs
│   └── SpecialCommandsTests.cs
│
├── Utilities/             # Test helpers
│   ├── TestServiceProvider.cs  # DI container helpers
│   ├── MockFactory.cs          # Mock creation utilities
│   └── TestFixtures.cs         # Reusable test data
│
├── Configuration/         # Legacy tests
├── Views/                 # UI component tests
├── GlobalUsings.cs        # Global using directives
└── TestBase.cs            # Common test setup
```

## Key Files and Their Purposes

### Main Application
- **Program.cs**: Entry point, sets up DI container
- **App.axaml.cs**: Avalonia application initialization
- **ServiceConfiguration.cs**: Registers all services in DI container
- **MainWindow.axaml.cs**: Main UI window with command input

### Domain Layer
- **Command.cs**: Immutable command model (record type)
- **Interfaces/**: Contracts for all business operations

### Application Layer
- **CommandRegistryService.cs**: Manages command registration and lookup
- **CommandExecutorService.cs**: Handles command execution via Process.Start
- **AutoCompleterService.cs**: Provides auto-completion logic
- **ApplicationOrchestrator.cs**: Coordinates initialization, config reload, special commands

### Infrastructure Layer
- **YamlConfigLoaderService.cs**: Loads commands from YAML files
- **WindowsHotkeyService.cs**: Registers global hotkeys (Windows P/Invoke)
- **WindowsIMEControllerService.cs**: Controls IME state (Windows P/Invoke)

### Quality Assurance
- **.husky/**: Pre-commit hooks configuration
- **scripts/check-coverage.sh**: Coverage threshold validation
- **scripts/check-metrics.sh**: Code metrics validation
- **tools/MetricsChecker/**: Roslyn-based metrics analyzer
- **.github/workflows/ci.yml**: CI/CD pipeline

### Documentation
- **README.md**: Project overview and quick start
- **TESTING.md**: Comprehensive testing guide
- **BUILD.md**: Build instructions
- **QUALITY_GATES_VALIDATION.md**: Quality gates documentation
- **.spec-workflow/specs/**: Feature specifications and design documents

## Important Patterns

### Linked Files from Legacy Project
The following files are linked from `CommandLauncher/` for backward compatibility:
- `CommandManager.cs` (linked to Core/)
- `AutoCompleteMachine.cs` (linked to Core/)

These are gradually being replaced by the new Application layer services.

### Test Mirroring
Test files mirror the source structure:
- Source: `Application/Services/FooService.cs`
- Tests: `Application/Services/FooServiceTests.cs`

### Configuration Files
- **Main app**: `TeaLauncher.Avalonia/commands.yaml`
- **Unit tests**: Mock configuration in memory
- **Integration tests**: `Integration/Fixtures/test-commands.yaml`
- **E2E tests**: `EndToEnd/Fixtures/e2e-test-config.yaml`
