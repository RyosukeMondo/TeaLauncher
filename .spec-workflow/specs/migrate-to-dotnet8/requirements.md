# Requirements Document

## Introduction

This specification defines the migration of TeaLauncher from .NET Framework 3.5 to .NET 8 LTS. The migration modernizes the codebase to leverage current technology standards, improved performance, and active platform support while maintaining TeaLauncher's core simplicity and speed.

**Purpose**: Transition TeaLauncher to a modern, actively-supported platform that enables future enhancements without legacy framework constraints.

**Value to Users**:
- Faster startup and response times through .NET 8 performance improvements
- Smaller memory footprint with modern runtime optimizations
- Self-contained deployment option (no separate .NET installation required)
- Foundation for future cross-platform support
- Continued security updates and platform support

**Scope**: Complete migration of the codebase to .NET 8, dropping .NET Framework 3.5 entirely (no backwards compatibility).

## Alignment with Product Vision

This migration directly supports the Future Vision outlined in product.md:

- **Cross-Platform Support**: .NET 8 is the foundation for future Linux/macOS ports
- **Modern Architecture**: Enables plugin systems, fuzzy matching, and advanced features
- **Performance**: Aligns with success metrics (< 500ms startup, < 100ms hotkey response, < 20MB memory)
- **Community Growth**: Modern codebase attracts contributors familiar with current .NET practices

Maintains Product Principles:
- **Simplicity First**: Migration maintains minimal UI and single-purpose design
- **Fast Feedback**: .NET 8 performance improvements enhance responsiveness
- **Configuration as Code**: No changes to .conf file format (user-facing stability)

## Requirements

### Requirement 1: Platform Migration

**User Story:** As a Windows user, I want TeaLauncher to run on .NET 8, so that I benefit from modern platform performance and security updates.

#### Acceptance Criteria

1. WHEN the application starts THEN the system SHALL run on .NET 8 runtime (not .NET Framework)
2. WHEN the application is deployed THEN the system SHALL support Windows 10 version 1607+ and Windows 11
3. WHEN the application starts THEN the system SHALL NOT require separate .NET Framework installation
4. IF the user has Windows 7/8/XP THEN the application SHALL display a clear error message stating minimum Windows 10 requirement

### Requirement 2: Project Structure Modernization

**User Story:** As a developer, I want the project to use modern SDK-style project files, so that I can leverage current build tools and development workflows.

#### Acceptance Criteria

1. WHEN the project is built THEN the system SHALL use SDK-style .csproj format (Microsoft.NET.Sdk.WindowsDesktop)
2. WHEN the solution is opened THEN the system SHALL be compatible with Visual Studio 2022, VS Code, and Rider
3. WHEN dependencies are managed THEN the system SHALL use PackageReference instead of packages.config
4. WHEN assembly metadata is defined THEN the system SHALL use .csproj properties instead of AssemblyInfo.cs files
5. IF the developer runs `dotnet build` THEN the project SHALL compile successfully via CLI

### Requirement 3: Dependency Modernization

**User Story:** As a developer, I want all third-party dependencies updated to .NET 8 compatible versions, so that I can maintain and test the code with current tools.

#### Acceptance Criteria

1. WHEN tests are executed THEN the system SHALL use NUnit 4.x (latest stable)
2. WHEN the project is analyzed THEN the system SHALL NOT reference .NET Framework assemblies
3. WHEN building the project THEN the system SHALL resolve all package dependencies from NuGet
4. IF there are test projects THEN they SHALL use .NET 8 target framework

### Requirement 4: Minimal Code Changes

**User Story:** As a developer, I want minimal code changes during migration, so that I can verify the migration is successful without introducing new bugs.

#### Acceptance Criteria

1. WHEN configuration is stored THEN the system SHALL use Dictionary<string, Dictionary<string, string>> instead of Hashtable (standard .NET collection)
2. WHEN the existing code is migrated THEN the system SHALL maintain existing logic, algorithms, and control flow
3. WHEN P/Invoke signatures are reviewed THEN they SHALL remain compatible with .NET 8 interop requirements
4. IF compiler errors occur due to API changes THEN only the minimum necessary changes SHALL be made
5. WHEN the migration is complete THEN the code structure SHALL remain recognizable to developers familiar with the .NET Framework version

### Requirement 5: Windows Forms Compatibility

**User Story:** As a Windows user, I want the UI to work identically to the current version, so that my workflow is not disrupted.

#### Acceptance Criteria

1. WHEN the hotkey (Ctrl+Space) is pressed THEN the input window SHALL appear within 100ms
2. WHEN commands are entered THEN auto-completion SHALL function identically to the current version
3. WHEN the application runs THEN it SHALL reside in the system tray as before
4. WHEN P/Invoke code executes THEN RegisterHotKey/UnregisterHotKey SHALL function correctly on .NET 8
5. IF the user presses Escape THEN the window SHALL hide immediately as before

### Requirement 6: Configuration Compatibility

**User Story:** As a user, I want my existing .conf files to work without modification, so that I can upgrade without reconfiguring my commands.

#### Acceptance Criteria

1. WHEN the application starts with an existing .conf file THEN the system SHALL load all commands correctly
2. WHEN configuration is parsed THEN the system SHALL support the same `[section]` and `key = value` format
3. WHEN special commands are executed (!reload, !exit, !version) THEN they SHALL function identically
4. IF a malformed .conf file is loaded THEN the system SHALL display the same error handling behavior

### Requirement 7: Deployment Options

**User Story:** As a user, I want flexible deployment options, so that I can run TeaLauncher with or without a separate .NET installation.

#### Acceptance Criteria

1. WHEN building for release THEN the system SHALL support framework-dependent deployment (smaller size)
2. WHEN building for release THEN the system SHALL support self-contained deployment (no runtime dependency)
3. WHEN deploying self-contained THEN the executable SHALL be single-file with all dependencies embedded
4. IF framework-dependent deployment is used THEN the system SHALL require .NET 8 Desktop Runtime
5. WHEN the user runs the executable THEN it SHALL display a clear error if the required runtime is missing

### Requirement 8: Testing and Quality Assurance

**User Story:** As a developer, I want comprehensive tests migrated to .NET 8, so that I can verify the migration is successful.

#### Acceptance Criteria

1. WHEN tests are executed THEN all existing unit tests SHALL pass on .NET 8
2. WHEN AutoCompleteMachine is tested THEN Test_AutoCompleteMachine SHALL verify prefix matching works identically
3. WHEN ConfigLoader is tested THEN Test_ConfigLoader SHALL verify configuration parsing works identically
4. WHEN the build runs THEN warnings SHALL be treated as errors (maintaining existing quality bar)
5. IF new code is added THEN it SHALL maintain or improve test coverage

## Non-Functional Requirements

### Code Architecture and Modularity
- **Preserve Existing Architecture**: No refactoring of class responsibilities or component boundaries
- **No New Dependencies**: Use only .NET 8 BCL (Base Class Library) + NUnit 4.x for testing
- **Interface Stability**: Existing ICommandManager* interfaces remain unchanged
- **Migration-Only Changes**: Code modifications limited to .NET 8 compatibility requirements

### Performance
- **Startup Time**: SHALL be ≤ 300ms (improvement over current 500ms target) due to .NET 8 optimizations
- **Hotkey Response**: SHALL remain < 100ms (maintained from current requirement)
- **Memory Footprint**: SHALL be ≤ 15MB during idle (improvement over current 20MB target)
- **Build Time**: SHALL compile in < 10 seconds on modern hardware
- **Package Restore**: SHALL complete in < 5 seconds with cached NuGet packages

### Security
- **Platform Security**: Leverage .NET 8 security improvements and monthly patches
- **Process Execution**: Maintain current security model (inherit user permissions via Process.Start)
- **Configuration Files**: Continue plain-text format (no change to threat model)
- **Code Analysis**: Enable .NET 8 code analyzers to detect potential issues at compile time

### Reliability
- **Runtime Stability**: Zero regressions in existing functionality
- **Error Handling**: Maintain existing exception-based error handling patterns
- **Hotkey Conflicts**: P/Invoke interop must remain stable across Windows updates
- **Configuration Reload**: !reload command must work without application restart

### Usability
- **Zero User Retraining**: UI and keyboard interactions remain identical
- **Configuration Migration**: Existing .conf files work without modification
- **Error Messages**: Improve error clarity for .NET 8 runtime requirements
- **Installation**: Provide clear instructions for framework-dependent vs self-contained deployment

### Maintainability
- **Minimal Changes**: Preserve existing code style and patterns to maintain familiarity
- **Tooling**: Compatible with Visual Studio 2022, VS Code with C# extension, Rider 2024+
- **Build Reproducibility**: Deterministic builds via .NET 8 SDK
- **Documentation**: Update README with .NET 8 installation requirements and build instructions

### Compatibility
- **Target OS**: Windows 10 version 1607+ (Anniversary Update, August 2016) and Windows 11
- **Architecture**: x64 and ARM64 (drop x86 support - KISS principle)
- **Runtime**: .NET 8 Desktop Runtime (Windows Desktop Apps pack includes Windows Forms)
- **Breaking Change**: Explicitly drop Windows XP/Vista/7/8 support (documented migration)
