# Technology Stack

## Project Type
Windows Desktop Application - A system tray-resident utility providing global hotkey-activated command launching functionality.

## Core Technologies

### Primary Language(s)
- **Language**: C# (.NET Framework 3.5)
- **Runtime/Compiler**: .NET Framework 3.5+ (requires installation on target systems)
- **Language-specific tools**: MSBuild 3.5, Visual Studio 2008/2010 project format

### Key Dependencies/Libraries
- **System.Windows.Forms**: Primary UI framework for input window and system tray integration
- **System.Diagnostics**: Process launching and application execution
- **System.Collections**: Generic collections (List<T>) and legacy Hashtable for configuration storage
- **System.IO**: File I/O for configuration file reading
- **NUnit 2.5.2**: Unit testing framework for AutoCompleteMachine and ConfigLoader components

### Application Architecture
**Event-driven Windows Forms architecture** with the following components:

**Core Components:**
- **MainWindow (Form1)**: Primary UI - displays input textbox, handles keyboard events, manages window visibility
- **CommandManager**: Command registry and execution engine, inherits AutoCompleteMachine for completion features
- **ConfigLoader**: Custom INI-style configuration file parser with Hashtable-based storage
- **AutoCompleteMachine**: Prefix-matching auto-completion engine using List-based word storage
- **Hotkey**: Global hotkey registration using Windows API interop (Ctrl+Space)
- **IMEController**: Input Method Editor handling for international text input (specifically Google IME support)

**Architectural Patterns:**
- Interface-based dependency injection (ICommandManagerInitializer, ICommandManagerFinalizer, ICommandManagerDialogShower)
- Delegate-based event callbacks for component communication
- Exception-driven validation (ConfigLoaderException hierarchy)

### Data Storage (if applicable)
- **Primary storage**: File-based configuration (.conf files) with custom INI-like format
- **Caching**: In-memory Hashtable storage for parsed configuration (section -> key/value pairs)
- **Data formats**:
  - Custom text format: `[section_name]` followed by `key = value` pairs
  - Configuration reloaded on-demand via `!reload` special command

### External Integrations (if applicable)
- **APIs**: Windows Shell API for process launching (Process.Start)
- **Protocols**: Supports http://, https://, ftp:// URL schemes for web launching
- **Authentication**: None - local system access only
- **Windows API Interop**: Global hotkey registration via P/Invoke (user32.dll)

### Monitoring & Dashboard Technologies (if applicable)
- **Dashboard Framework**: Not applicable - minimal UI (single input textbox)
- **Real-time Communication**: Not applicable
- **Visualization Libraries**: Not applicable
- **State Management**: Configuration state stored in Hashtable, command list in List<Command>

## Development Environment

### Build & Development Tools
- **Build System**: MSBuild 3.5 (Microsoft.CSharp.targets)
- **Package Management**: None - standard library only, NUnit added via assembly reference
- **Development workflow**: Visual Studio 2008/2010 IDE, compile-run-test cycle
- **Project Structure**: Solution file (.sln) with two projects:
  - CommandLauncher.csproj (main executable)
  - TestCommandLauncher.csproj (unit tests)

### Code Quality Tools
- **Static Analysis**: Warnings treated as errors in both Debug and Release configurations
- **Formatting**: None specified - follows default C# conventions
- **Testing Framework**:
  - NUnit 2.5.2 for unit testing
  - Test projects: Test_AutoCompleteMachine, Test_ConfigLoader
  - Test runner: nunit.exe (configured in .csproj StartProgram)
- **Documentation**: Code comments in Japanese with XML documentation headers

### Version Control & Collaboration
- **VCS**: Git (evidenced by .git directory)
- **Branching Strategy**: Not specified - appears to be simple trunk-based development
- **Code Review Process**: Not specified
- **License**: GNU General Public License v2

### Dashboard Development (if applicable)
Not applicable - no dashboard component.

## Deployment & Distribution (if applicable)
- **Target Platform(s)**: Windows desktop (x86/AnyCPU) - requires .NET Framework 3.5 SP1 or higher
- **Distribution Method**:
  - Direct executable download (TeaLauncher.exe + conf/ directory)
  - ClickOnce deployment support configured in .csproj
- **Installation Requirements**:
  - Windows operating system (XP or later)
  - .NET Framework 3.5 SP1 (bootstrapper configured)
  - Windows Installer 3.1 (for ClickOnce deployment)
- **Update Mechanism**: Manual - users download new versions explicitly

## Technical Requirements & Constraints

### Performance Requirements
- **Startup time**: < 500ms application initialization
- **Hotkey response**: < 100ms from Ctrl+Space to window display
- **Memory usage**: < 20MB RAM during operation
- **Auto-completion speed**: Instantaneous for < 1000 registered commands
- **Configuration load**: < 200ms for typical config files (< 100 commands)

### Compatibility Requirements
- **Platform Support**: Windows XP, Vista, 7, 8, 10, 11 (any x86/x64 architecture)
- **Dependency Versions**: .NET Framework 3.5+ (includes 3.5 SP1, 4.x compatibility)
- **Standards Compliance**:
  - Windows Forms standard controls
  - Standard .exe manifest for Windows compatibility
  - File path conventions (backslash separators, drive letters)

### Security & Compliance
- **Security Requirements**:
  - Process.Start security: inherits user permissions, no elevation
  - No network communication (except user-initiated URL launches)
  - Configuration files in plain text (no sensitive data encryption)
- **Compliance Standards**: None applicable (local desktop utility)
- **Threat Model**:
  - Configuration file injection: user controls config, accepts command execution risk
  - Global hotkey conflicts: Ctrl+Space may conflict with other applications
  - IME interaction vulnerabilities: handled via IMEController component

### Scalability & Reliability
- **Expected Load**: Single-user desktop application, hundreds of registered commands
- **Availability Requirements**: Always-on background process, system tray resident
- **Growth Projections**: Command list expected to remain < 1000 entries (List<T> performance)

## Technical Decisions & Rationale

### Decision Log

1. **.NET Framework 3.5 Target**:
   - **Rationale**: Maximum compatibility with Windows XP through Windows 11
   - **Trade-offs**: No access to modern C# language features, older APIs
   - **Alternatives considered**: .NET Framework 4.x (rejected due to Windows XP support requirement)

2. **Custom Configuration Parser vs. Standard Formats**:
   - **Rationale**: Lightweight custom parser (ConfigLoader) avoids external dependencies
   - **Trade-offs**: Manual parsing logic, limited format validation
   - **Alternatives considered**: XML ConfigurationManager (rejected - too heavyweight), JSON (not standard in .NET 3.5)

3. **Hashtable vs. Dictionary<TKey, TValue>**:
   - **Rationale**: Hashtable used for maximum .NET 2.0 compatibility
   - **Trade-offs**: Lost type safety, casting required on retrieval
   - **Justification**: Legacy codebase predates widespread Dictionary adoption

4. **Interface-based Dependency Injection**:
   - **Rationale**: CommandManager decoupled from MainWindow via interfaces (ICommandManagerInitializer, ICommandManagerFinalizer, ICommandManagerDialogShower)
   - **Trade-offs**: Additional interface definitions, but improves testability
   - **Alternatives considered**: Direct coupling (rejected - harder to test)

5. **Global Hotkey via Windows API Interop**:
   - **Rationale**: RegisterHotKey API provides system-wide keyboard hook
   - **Trade-offs**: Requires P/Invoke, potential hotkey conflicts with other apps
   - **Alternatives considered**: Keyboard hook DLL (rejected - more complex, unnecessary for single hotkey)

6. **Process.Start for Command Execution**:
   - **Rationale**: Standard .NET API handles URLs, file paths, and executables uniformly
   - **Trade-offs**: Limited control over process lifecycle, no output capture
   - **Justification**: Fire-and-forget launcher model doesn't need process management

## Known Limitations

- **Hard-coded Hotkey**: Ctrl+Space cannot be customized without code modification (no UI for hotkey configuration)
- **No Fuzzy Matching**: Auto-completion requires exact prefix matching (AutoCompleteMachine.GetCandidates uses StartsWith only)
- **Single Instance**: No enforcement - multiple instances can run simultaneously, causing hotkey conflicts
- **Path Detection Logic**: Uses hard-coded heuristics (IsPath method checks http://, https://, ftp://, X:\) - may miss edge cases like UNC paths (\\server\share)
- **Error Handling**: Configuration errors throw exceptions but limited user-friendly error messages
- **Japanese-Only Comments**: Code documentation primarily in Japanese, may limit international contributors
- **No Command History**: Executed commands not tracked or suggested based on frequency
- **No Variable Substitution**: Configuration values are static strings (no %USERPROFILE% expansion)
- **Legacy .NET Version**: .NET Framework 3.5 prevents use of modern async/await, LINQ improvements, nullable reference types
- **Test Coverage**: Unit tests exist for AutoCompleteMachine and ConfigLoader only - no integration tests for UI or hotkey handling
