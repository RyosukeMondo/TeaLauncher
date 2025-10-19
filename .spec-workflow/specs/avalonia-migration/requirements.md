# Requirements Document

## Introduction

This specification defines the complete modernization of TeaLauncher from Windows Forms (.NET Framework 3.5) to Avalonia UI (.NET 8), including both UI framework migration and configuration format modernization. The migration targets Windows deployment only, built using cross-platform tooling on Linux.

**Migration Scope**:
- Windows-only implementation (no cross-platform runtime support)
- Modern UI with Avalonia framework
- Modern YAML configuration format
- No backward compatibility required

**Build Environment**: Linux host building Windows binaries (cross-compilation)
**Timeline**: Planned for Q1 2025

This modernization enables TeaLauncher to leverage .NET 8 performance improvements, modern C# language features, and human-friendly YAML configuration format.

## Alignment with Product Vision

This migration directly supports TeaLauncher's product principles:

1. **Simplicity First**: Avalonia maintains the same minimalist single-input UI while providing better rendering and modern visual appearance
2. **Fast Feedback**: Avalonia's hardware-accelerated rendering ensures sub-100ms hotkey response times
3. **Configuration as Code**: Modernized YAML configuration format with human-readable syntax, comments support, and excellent IDE tooling
4. **Unobtrusive**: System tray integration and global hotkey functionality preserved

**Success Metrics Impact**:
- Maintains **< 100ms** hotkey response time (hardware acceleration improves rendering)
- Reduces **memory footprint** from potential 20MB to ~15MB (modern .NET runtime efficiency)
- Improves **startup time** to < 300ms (AOT compilation opportunities in .NET 8)

## Requirements

### Requirement 1: UI Framework Migration

**User Story:** As a TeaLauncher user, I want the application to continue working exactly as before, so that my workflow remains uninterrupted after the update.

#### Acceptance Criteria

1. WHEN user presses Ctrl+Space (or Alt+Space in debug) THEN the launcher window SHALL appear with identical visual behavior to the current Windows Forms version
2. WHEN user types into the command box THEN auto-completion SHALL display matching commands with the same prefix-matching behavior
3. WHEN user presses Tab THEN the command SHALL auto-complete using the longest common prefix
4. WHEN user presses Enter THEN the command SHALL execute and the window SHALL hide
5. WHEN user presses Escape THEN the command box SHALL clear (if text present) OR the window SHALL hide (if empty)
6. IF the window is visible but not active THEN pressing Ctrl+Space SHALL activate the window without hiding it
7. WHEN the window displays THEN it SHALL show a translucent background with blur effect and remain topmost

### Requirement 2: Windows-Specific Platform Features

**User Story:** As a TeaLauncher user, I want global hotkey functionality to continue working on Windows, so that I can instantly activate the launcher from any application.

#### Acceptance Criteria

1. WHEN the application starts THEN it SHALL register Ctrl+Space as a global system hotkey on Windows
2. IF the hotkey is already registered by another application THEN the system SHALL try alternative IDs (0x0000 to 0xBFFF range)
3. WHEN the registered hotkey is pressed anywhere in Windows THEN the application SHALL receive the WM_HOTKEY message and show the launcher window
4. WHEN the application exits THEN it SHALL unregister the global hotkey
5. IF running in DEBUG configuration THEN the hotkey SHALL be Alt+Space instead of Ctrl+Space

### Requirement 3: IME Support (Windows-Only)

**User Story:** As a Japanese TeaLauncher user, I want the Input Method Editor (IME) to be properly controlled when the launcher appears, so that I can type commands in alphanumeric mode without manual IME switching.

#### Acceptance Criteria

1. WHEN the launcher window shows THEN it SHALL turn IME off, on, then off again (reset sequence)
2. IF running on Windows THEN IME control SHALL use Windows Imm32.dll API calls
3. WHEN the window is hidden THEN IME state SHALL be left unchanged (no IME control needed)

### Requirement 4: YAML Configuration Format

**User Story:** As a TeaLauncher user, I want to use YAML configuration format, so that I can benefit from human-readable syntax, inline comments, and excellent readability for managing my commands.

#### Acceptance Criteria

1. WHEN the application starts THEN it SHALL load commands from a `commands.yaml` file using YamlDotNet library
2. WHEN the configuration file is missing THEN the system SHALL display an error with the expected file path
3. WHEN configuration has invalid YAML syntax THEN the system SHALL display a clear error message with line number and details
4. WHEN configuration lacks required fields THEN the system SHALL display validation errors listing missing fields
5. WHEN configuration contains unknown fields THEN the system SHALL ignore them gracefully
6. WHEN command entries include optional properties THEN the system SHALL use sensible defaults
7. WHEN the YAML is valid THEN the system SHALL deserialize it into strongly-typed configuration objects
8. WHEN YAML contains comments THEN they SHALL be preserved and ignored during parsing

### Requirement 5: Business Logic Preservation

**User Story:** As a TeaLauncher user, I want command execution to work the same way as before, so that my existing workflows remain functional.

#### Acceptance Criteria

1. WHEN a command is executed THEN it SHALL use Process.Start with UseShellExecute=true for URLs, file paths, and executables
2. WHEN user runs special commands (!reload, !exit, !version) THEN the system SHALL perform the corresponding action
3. WHEN user types partial command text THEN the auto-completion engine SHALL return prefix-matched candidates
4. IF command execution fails THEN the system SHALL display an error dialog with the exception message
5. WHEN configuration defines command arguments THEN they SHALL be passed to the executed process

### Requirement 6: Cross-Compilation Build Support

**User Story:** As a TeaLauncher developer, I want to build Windows binaries from Linux, so that I can use my Linux development environment while targeting Windows users.

#### Acceptance Criteria

1. WHEN running `dotnet build` on Linux with `-r win-x64` THEN the project SHALL compile successfully without errors
2. WHEN running `dotnet publish` with `-r win-x64` THEN the output SHALL be a Windows executable that runs on Windows 10/11
3. IF building on Linux THEN Windows-specific P/Invoke code SHALL compile conditionally using #if WINDOWS directives
4. WHEN the build completes THEN the output SHALL include all necessary runtime dependencies for Windows deployment
5. IF the target is win-x64 THEN the build SHALL define the WINDOWS compilation symbol automatically

### Requirement 7: Hotkey Native Interop

**User Story:** As a developer, I want Windows hotkey registration to work correctly through Avalonia's platform abstraction, so that global hotkeys function reliably on Windows.

#### Acceptance Criteria

1. WHEN the Avalonia window initializes THEN it SHALL provide access to the native Windows HWND handle
2. WHEN RegisterHotKey is called with the HWND THEN Windows SHALL register the global hotkey successfully
3. WHEN WM_HOTKEY messages arrive THEN they SHALL be intercepted before Avalonia's message processing
4. IF RegisterHotKey fails for a specific ID THEN the system SHALL try incrementing IDs until registration succeeds or the range is exhausted
5. WHEN the window closes THEN UnregisterHotKey SHALL be called with the correct HWND and hotkey ID

### Requirement 8: Configuration Management

**User Story:** As a TeaLauncher user, I want to reload my configuration file while the application is running, so that I can add new commands without restarting.

#### Acceptance Criteria

1. WHEN user executes !reload command THEN the system SHALL re-parse the configuration file and update the command registry
2. WHEN configuration file is missing THEN the system SHALL display an error dialog with the file path
3. WHEN configuration has syntax errors THEN the system SHALL display the parsing error with line and column information
4. WHEN configuration has duplicate command names THEN the system SHALL reject the configuration with a clear error message
5. WHEN configuration is successfully reloaded THEN all auto-completion data SHALL be refreshed immediately

## Non-Functional Requirements

### Code Architecture and Modularity

- **Single Responsibility Principle**: Separate UI (Avalonia views), business logic (CommandManager), configuration (modern parsers), and Windows-specific services (hotkey, IME)
- **Modular Design**: Windows-specific code (hotkey, IME) SHALL be implemented directly without cross-platform abstraction layers
- **Dependency Management**: Reuse business logic (CommandManager, AutoCompleteMachine), replace ConfigLoader with modern configuration library
- **Clear Interfaces**: Direct implementation of Windows APIs without unnecessary interface abstractions

### Performance

- **Startup Time**: ≤ 300ms from process start to system tray ready state (improved from 500ms target)
- **Hotkey Response**: ≤ 100ms from Ctrl+Space to window visible and focused
- **Memory Usage**: ≤ 15MB RAM during idle state (improved from 20MB baseline)
- **Auto-completion Latency**: ≤ 10ms for prefix matching against 1000 registered commands
- **Configuration Load**: ≤ 200ms for typical config files (< 100 commands)

### Compatibility

- **Platform Support**: Windows 10 version 1809+ and Windows 11 only (Avalonia 11.x requirement)
- **Build Environment**: Must compile on Linux using .NET 8 SDK with win-x64 runtime identifier
- **.NET Target**: net8.0-windows for Windows-specific features (hotkey, IME)
- **Configuration Format**: YAML format only using YamlDotNet library - no backward compatibility with .conf files

### Security

- **Process Execution**: Commands SHALL execute with user privileges only (no elevation)
- **Configuration Parsing**: SHALL safely handle malformed YAML files with clear error messages using YamlDotNet validation
- **Global Hotkey**: SHALL handle hotkey conflicts gracefully (try alternative IDs)
- **P/Invoke Safety**: Windows API calls SHALL validate return codes and handle errors

### Reliability

- **Error Handling**: All exceptions SHALL be caught and displayed to users via error dialogs
- **Graceful Degradation**: If hotkey registration fails, SHALL notify user but allow manual window activation
- **Resource Cleanup**: SHALL properly dispose of global hotkey registration and IME resources on exit
- **Single Instance**: Current behavior (multiple instances allowed) SHALL be maintained

### Maintainability

- **Code Reuse**: 60%+ of existing business logic (CommandManager, AutoCompleteMachine) SHALL be reused, ConfigLoader replaced with YamlDotNet
- **Windows-First Design**: Code SHALL be optimized for Windows without cross-platform abstraction overhead
- **Testability**: Core business logic SHALL remain testable independently of UI framework
- **Documentation**: Migration guide SHALL document cross-compilation setup and YAML configuration schema with examples
