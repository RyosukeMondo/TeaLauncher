# Tasks Document

- [x] 1. Create Avalonia project structure
  - File: TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj
  - Set up new .NET 8 Avalonia project with Windows targeting
  - Configure cross-compilation for Linux to Windows builds
  - Purpose: Establish foundation for Avalonia UI migration
  - _Leverage: CommandLauncher/CommandLauncher.csproj (for reference)_
  - _Requirements: 6.1, 6.2_
  - _Prompt: Role: .NET Build Engineer specializing in cross-platform compilation and project configuration | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create a new .NET 8 Avalonia project configured for Windows targeting (net8.0-windows) with cross-compilation support from Linux, following requirements 6.1 and 6.2. Configure NuGet packages for Avalonia 11.2.2 and YamlDotNet 16.3.0. Set up build properties for single-file executable output with win-x64 runtime identifier. | Restrictions: Do not modify existing CommandLauncher project, use separate project directory, ensure PublishSingleFile and SelfContained are enabled, do not include cross-platform abstractions | Success: Project compiles successfully on Linux with `dotnet build -r win-x64`, all Avalonia and YamlDotNet packages restore correctly, project file includes proper Windows targeting, output is configured for single-file deployment | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [x] 2. Create YAML configuration models
  - File: TeaLauncher.Avalonia/Configuration/CommandConfig.cs
  - Define strongly-typed C# records for YAML configuration structure
  - Add YamlDotNet attributes and validation
  - Purpose: Establish data models for modern YAML configuration
  - _Leverage: CommandLauncher/ConfigLoader.cs (for configuration concepts)_
  - _Requirements: 4.1, 4.7_
  - _Prompt: Role: C# Developer specializing in data modeling and serialization | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create strongly-typed C# record models (CommandsConfig, CommandEntry) for YAML configuration following requirements 4.1 and 4.7. Use YamlDotNet attributes ([YamlMember]) for property mapping and required keyword for mandatory fields. Include properties: Name, LinkTo, Description (optional), Arguments (optional). | Restrictions: Use record types not classes, apply required keyword for Name and LinkTo, use nullable reference types for optional fields, do not include business logic in models | Success: Models compile without errors, YamlDotNet can deserialize YAML to these models, required fields throw exceptions when missing, code uses modern C# features (records, required, nullable reference types) | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [x] 3. Implement YamlConfigLoader
  - File: TeaLauncher.Avalonia/Configuration/YamlConfigLoader.cs
  - Create YAML parser using YamlDotNet to load commands.yaml
  - Add validation and error handling with clear messages
  - Purpose: Replace custom INI parser with modern YAML loading
  - _Leverage: CommandLauncher/ConfigLoader.cs (for interface patterns), Configuration/CommandConfig.cs_
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 8.2, 8.3_
  - _Prompt: Role: Backend Developer with expertise in configuration parsing and error handling | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Implement YamlConfigLoader class that loads commands.yaml using YamlDotNet following requirements 4.1-4.4 and 8.2-8.3. Include methods LoadConfigFile(string filePath) returning CommandsConfig, with proper exception handling for FileNotFoundException, YamlException, and validation errors. Provide clear error messages with line numbers for syntax errors. | Restrictions: Do not catch all exceptions generically, provide specific error messages for each failure type, do not modify YamlDotNet behavior, ensure line number reporting works | Success: Successfully deserializes valid YAML files, throws meaningful exceptions for missing files with file paths, throws YamlException with line/column information for syntax errors, validates required fields and reports missing field names, ignores unknown fields gracefully | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [x] 4. Create unit tests for YamlConfigLoader
  - File: TeaLauncher.Avalonia.Tests/Configuration/YamlConfigLoaderTests.cs
  - Write comprehensive tests for YAML parsing and error scenarios
  - Test valid configuration, invalid syntax, missing fields
  - Purpose: Ensure configuration loading reliability
  - _Leverage: TestCommandLauncher/Test_ConfigLoader.cs (for test patterns)_
  - _Requirements: 4.3, 4.4, 8.3_
  - _Prompt: Role: QA Engineer with expertise in unit testing and NUnit framework | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create comprehensive NUnit tests for YamlConfigLoader covering requirements 4.3, 4.4, and 8.3. Test scenarios: valid YAML deserialization, missing file handling, invalid YAML syntax with line number checking, missing required fields validation, unknown fields being ignored. Use in-memory YAML strings and temporary test files. | Restrictions: Must test both success and failure paths, do not test YamlDotNet library itself, ensure test isolation with proper cleanup, use Assert statements not console output | Success: All test cases pass, test coverage includes happy path and all error scenarios, tests verify exception types and error messages, tests clean up temporary files, tests run independently and consistently | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [x] 5. Link existing business logic files
  - File: TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj (modify)
  - Add <Compile Include="..."> links to CommandManager.cs and AutoCompleteMachine.cs
  - Create Core/ directory structure for linked files
  - Purpose: Reuse existing business logic without duplication
  - _Leverage: CommandLauncher/CommandManager.cs, CommandLauncher/AutoCompleteMachine.cs_
  - _Requirements: 5.1, 5.2_
  - _Prompt: Role: .NET Build Engineer specializing in project file management | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Add file links in TeaLauncher.Avalonia.csproj to reuse CommandManager.cs and AutoCompleteMachine.cs from CommandLauncher project following requirements 5.1 and 5.2. Use <Compile Include="../CommandLauncher/CommandManager.cs" Link="Core/CommandManager.cs" /> pattern. | Restrictions: Do not copy files, use Link attribute only, do not modify source files in CommandLauncher, ensure relative paths work from Avalonia project location | Success: Project compiles with linked files visible in Core/ folder, CommandManager and AutoCompleteMachine types are available in Avalonia project, no duplication of code, changes to original files reflect in Avalonia project | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [x] 6. Create WindowsHotkey component
  - File: TeaLauncher.Avalonia/Platform/WindowsHotkey.cs
  - Implement global hotkey registration using Windows user32.dll P/Invoke
  - Hook into Avalonia window messages for WM_HOTKEY handling
  - Purpose: Enable Ctrl+Space global hotkey activation on Windows
  - _Leverage: CommandLauncher/Hotkey.cs (for P/Invoke signatures and logic)_
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 7.1, 7.2, 7.3, 7.4, 7.5_
  - _Prompt: Role: Windows Platform Developer with expertise in P/Invoke and native API integration | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create WindowsHotkey class that registers global hotkey using RegisterHotKey/UnregisterHotKey from user32.dll following requirements 2.1-2.5 and 7.1-7.5. Get HWND from Avalonia window using TryGetPlatformHandle().Handle. Implement ID range search (0x0000-0xBFFF). Hook WM_HOTKEY (0x0312) messages. Expose HotKeyPressed event. Implement IDisposable for cleanup. Support ModifierKeys and Key parameters. | Restrictions: Do not use Windows Forms, must work with Avalonia Window directly, must try alternative IDs if registration fails, must unregister on Dispose, do not create abstraction layers | Success: RegisterHotKey succeeds with valid HWND, hotkey press triggers HotKeyPressed event, alternative IDs tried if first fails, UnregisterHotKey called on Dispose, supports both Ctrl+Space and Alt+Space configurations, compiles with win-x64 target | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 7. Create WindowsIMEController component
  - File: TeaLauncher.Avalonia/Platform/WindowsIMEController.cs
  - Implement IME control using Windows Imm32.dll P/Invoke
  - Remove Form inheritance, use standalone class with HWND
  - Purpose: Control Japanese IME state for alphanumeric input
  - _Leverage: CommandLauncher/IMEController.cs (for P/Invoke signatures)_
  - _Requirements: 3.1, 3.2, 3.3_
  - _Prompt: Role: Windows Platform Developer with expertise in IME and P/Invoke | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create WindowsIMEController class that controls IME state using Imm32.dll following requirements 3.1-3.3. Use ImmGetContext, ImmSetOpenStatus, ImmReleaseContext P/Invoke. Constructor takes IntPtr windowHandle. Implement On() and Off() methods. Implement IDisposable. Remove Form inheritance from original implementation. | Restrictions: Do not inherit from Form, use IntPtr for window handle directly, must properly release IME context after operations, do not expose IME conversion status methods if not needed | Success: IME can be turned on and off programmatically, properly releases IME context resources, compiles with win-x64 target, works with Avalonia window handle, implements IDisposable correctly | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 8. Create MainWindow XAML
  - File: TeaLauncher.Avalonia/Views/MainWindow.axaml
  - Design Avalonia XAML UI with AutoCompleteBox for command input
  - Configure window properties: topmost, borderless, blur background
  - Purpose: Replace Windows Forms UI with modern Avalonia interface
  - _Leverage: CommandLauncher/Form1.Designer.cs (for UI layout reference)_
  - _Requirements: 1.1, 1.7_
  - _Prompt: Role: UI/UX Developer specializing in Avalonia XAML and desktop application design | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create MainWindow.axaml with Avalonia XAML following requirements 1.1 and 1.7. Use AutoCompleteBox for command input with watermark text. Configure Window properties: Topmost="True", SystemDecorations="None", WindowStartupLocation="CenterScreen", ShowInTaskbar="False", CanResize="False". Set Width="500" Height="80". Apply translucent background with blur effect using Background="#CC000000" and TransparencyLevelHint="Blur". Style AutoCompleteBox with modern Fluent theme. | Restrictions: Do not use Windows Forms controls, must use Avalonia controls only, ensure blur effect works on Windows 10+, keep UI minimal (single input box), do not add unnecessary visual elements | Success: Window displays with blur background effect, AutoCompleteBox is properly styled and functional, window is borderless and topmost, window starts centered on screen, sizing matches design specifications (500x80) | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 9. Implement MainWindow code-behind
  - File: TeaLauncher.Avalonia/Views/MainWindow.axaml.cs
  - Implement ICommandManager interfaces and event handling
  - Wire up keyboard events, hotkey, IME, and command execution
  - Purpose: Provide UI orchestration and business logic integration
  - _Leverage: CommandLauncher/Form1.cs (for event handling logic), Core/CommandManager.cs, Platform/WindowsHotkey.cs, Platform/WindowsIMEController.cs, Configuration/YamlConfigLoader.cs_
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 5.1, 5.2, 5.3, 5.4, 5.5, 7.1, 8.1, 8.5_
  - _Prompt: Role: Full-stack Developer with expertise in Avalonia, MVVM patterns, and desktop application development | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Implement MainWindow.axaml.cs code-behind following requirements 1.1-1.6, 5.1-5.5, 7.1, 8.1, 8.5. Implement ICommandManagerInitializer, ICommandManagerFinalizer, ICommandManagerDialogShower interfaces. Initialize CommandManager, YamlConfigLoader, WindowsHotkey, WindowsIMEController in constructor. Load configuration from commands.yaml. Handle KeyDown events for Tab (auto-complete), Enter (execute), Escape (clear/hide). Implement ShowWindow() with IME reset (Off-On-Off). Implement HideWindow() with input clearing. Wire HotKeyPressed event to show/hide/activate logic. Implement special commands (!reload, !exit, !version). Update AutoCompleteBox ItemsSource from CommandManager.GetCandidates(). | Restrictions: Do not use Windows Forms patterns, must use Avalonia event handling, ensure proper disposal of resources (hotkey, IME), handle all keyboard events correctly, provide error dialogs for failures | Success: Window shows/hides on Ctrl+Space, keyboard events work correctly (Tab completes, Enter executes, Escape clears/hides), auto-completion displays candidates in dropdown, commands execute via CommandManager, IME is reset on window show, configuration can be reloaded with !reload, error dialogs display for failures | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 10. Create App.axaml and App.axaml.cs
  - File: TeaLauncher.Avalonia/App.axaml, TeaLauncher.Avalonia/App.axaml.cs
  - Set up Avalonia application entry point and theme configuration
  - Load command-line arguments for config file path
  - Purpose: Initialize Avalonia application with Fluent theme
  - _Leverage: CommandLauncher/Program.cs (for entry point pattern)_
  - _Requirements: 5.1_
  - _Prompt: Role: .NET Application Developer specializing in Avalonia application lifecycle | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create App.axaml with Avalonia Application definition using FluentTheme (Dark variant) following requirement 5.1. Create App.axaml.cs with OnFrameworkInitializationCompleted override. Read command-line arguments from IClassicDesktopStyleApplicationLifetime.Args, default to "commands.yaml" if no args provided. Instantiate MainWindow with config file path. Set desktop.MainWindow. | Restrictions: Do not use Windows Forms Application class, must use Avalonia application lifecycle, ensure theme is properly loaded, handle missing command-line args with default | Success: Application starts successfully with Fluent Dark theme, MainWindow receives config file path from args or default, application initializes properly on Windows, theme is applied to all controls | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 11. Create Program.cs entry point
  - File: TeaLauncher.Avalonia/Program.cs
  - Implement Main method with Avalonia AppBuilder configuration
  - Configure platform detection and InterFont
  - Purpose: Bootstrap Avalonia application for Windows
  - _Leverage: CommandLauncher/Program.cs (for STAThread pattern)_
  - _Requirements: 6.2, 6.4_
  - _Prompt: Role: .NET Application Developer specializing in application bootstrapping | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create Program.cs with [STAThread] Main method following requirements 6.2 and 6.4. Use BuildAvaloniaApp().StartWithClassicDesktopLifetime(args). Configure AppBuilder with UsePlatformDetect(), WithInterFont(), LogToTrace(). Ensure proper Windows runtime initialization. | Restrictions: Must use [STAThread] attribute for Windows COM compatibility, do not hardcode platform detection, use InterFont for modern typography, ensure proper error logging | Success: Application starts on Windows successfully, Avalonia initializes with platform detection, InterFont loads correctly, classic desktop lifetime starts properly, logs are written to trace output | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 12. Create example commands.yaml
  - File: TeaLauncher.Avalonia/commands.yaml
  - Create example YAML configuration with common commands
  - Document YAML format with comments
  - Purpose: Provide user-friendly configuration template
  - _Leverage: resource/conf/my.conf (for example commands)_
  - _Requirements: 4.1, 4.8_
  - _Prompt: Role: Technical Writer specializing in configuration documentation | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create commands.yaml example file following requirements 4.1 and 4.8. Include 5-10 example commands covering URLs (google, github), file paths (notepad, explorer), and special commands (!reload, !exit, !version). Add YAML comments explaining format. Use proper YAML syntax with proper indentation. Include optional description fields demonstrating documentation. | Restrictions: Must be valid YAML syntax, use 2-space indentation, include comments for user guidance, demonstrate both required and optional fields, do not include sensitive data | Success: File parses successfully with YamlConfigLoader, examples cover common use cases (URLs, executables, special commands), comments are helpful and clear, YAML structure is easy to understand and modify, demonstrates optional fields like description | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 13. Write integration tests
  - File: TeaLauncher.Avalonia.Tests/Integration/CommandExecutionTests.cs
  - Create integration tests for end-to-end command execution flow
  - Test configuration loading, command registration, and execution
  - Purpose: Verify complete workflow from YAML to execution
  - _Leverage: Configuration/YamlConfigLoader.cs, Core/CommandManager.cs_
  - _Requirements: 4.1, 5.1, 5.2, 5.3, 8.1_
  - _Prompt: Role: QA Engineer with expertise in integration testing and .NET test frameworks | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create integration tests following requirements 4.1, 5.1-5.3, and 8.1. Test scenarios: load YAML config → register commands → verify HasCommand → execute command. Test !reload special command reloads configuration. Test command with arguments passes arguments correctly. Use in-memory YAML and mock Process.Start where possible. | Restrictions: Must test integration points not individual units, do not test UI in integration tests, mock external dependencies (Process.Start), ensure test isolation and cleanup | Success: Tests verify full workflow from YAML loading to command execution, configuration reload test confirms new commands are registered, tests pass consistently, integration points between components are validated | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 14. Configure build script for Linux cross-compilation
  - File: scripts/build-windows.sh
  - Create build script for compiling Windows binaries on Linux
  - Configure dotnet publish with proper runtime identifiers
  - Purpose: Automate cross-compilation build process
  - _Leverage: scripts/ directory (for build script patterns)_
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  - _Prompt: Role: DevOps Engineer with expertise in .NET build automation and cross-compilation | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create build-windows.sh script following requirements 6.1-6.5. Use `dotnet publish TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false`. Create output directory structure with TeaLauncher.exe, commands.yaml, and README.md. Make script executable with proper error handling. | Restrictions: Must work on Linux without Windows SDK, ensure single-file output, strip debug symbols for size, verify output directory exists before build, clean old artifacts | Success: Script runs successfully on Linux, produces single-file TeaLauncher.exe for Windows, includes all necessary runtime dependencies, output size is optimized (no debug symbols), script handles errors gracefully, creates proper output directory structure | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 15. Create migration documentation
  - File: docs/MIGRATION.md
  - Document migration process, configuration format changes, and setup
  - Provide YAML examples and troubleshooting guide
  - Purpose: Help users migrate from old .conf to new YAML format
  - _Leverage: BUILD.md (for documentation style)_
  - _Requirements: 4.1, 6.1, 8.1_
  - _Prompt: Role: Technical Writer specializing in software documentation and user guides | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Create MIGRATION.md documentation following requirements 4.1, 6.1, and 8.1. Include sections: Overview of changes, Configuration format comparison (old .conf vs YAML), YAML syntax guide with examples, Building from source on Linux, Deployment instructions, Troubleshooting common issues (missing file, invalid YAML, hotkey conflicts). Use markdown formatting with code blocks. | Restrictions: Must be user-friendly for non-technical users, provide clear examples for each section, include both Windows usage and Linux build instructions, do not assume prior YAML knowledge | Success: Documentation is clear and comprehensive, includes side-by-side .conf to YAML examples, build instructions work on Linux, troubleshooting covers common issues, code examples are properly formatted and tested | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_

- [ ] 16. End-to-end testing and validation
  - Test complete application on Windows 10/11
  - Verify hotkey registration, command execution, and configuration reload
  - Fix any issues discovered during testing
  - Purpose: Ensure production readiness
  - _Leverage: All components_
  - _Requirements: All_
  - _Prompt: Role: QA Engineer with expertise in end-to-end testing and Windows desktop applications | Task: Implement the task for spec avalonia-migration, first run spec-workflow-guide to get the workflow guide then implement the task: Perform comprehensive end-to-end testing covering all requirements. Test on Windows 10 and Windows 11. Scenarios: (1) Press Ctrl+Space from desktop → window appears focused, (2) Type partial command → Tab completes, (3) Press Enter → command executes and window hides, (4) Press Escape → clears input or hides window, (5) Edit commands.yaml → Execute !reload → new commands available, (6) Execute !version → shows version info, (7) Execute !exit → application closes cleanly, (8) Test IME reset when window shows, (9) Test hotkey conflict handling, (10) Test invalid YAML error messages. Document any bugs found and fix them. | Restrictions: Must test on real Windows installation, cannot skip any requirement validation, must verify error messages are user-friendly, test both Debug and Release builds | Success: All manual test scenarios pass on Windows 10 and 11, hotkey works globally across all applications, auto-completion displays correct candidates, commands execute successfully, configuration reload works without restart, error messages are clear and helpful, no crashes or hangs during testing, resource cleanup works properly (hotkey unregistered on exit) | Instructions: Mark this task as in-progress [-] in .spec-workflow/specs/avalonia-migration/tasks.md before starting, then mark as completed [x] when done_
