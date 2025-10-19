# Design Document

## Overview

This design document specifies the technical approach for migrating TeaLauncher from .NET Framework 3.5 to .NET 8 LTS. The migration strategy prioritizes minimal code changes while achieving full .NET 8 compatibility. The existing application architecture, component boundaries, and user-facing functionality remain unchanged.

**Migration Scope:**
- Convert legacy .csproj files to SDK-style format
- Update target framework from `net35` to `net8.0-windows`
- Migrate NUnit 2.5.2 to NUnit 4.x
- Replace Hashtable with Dictionary<string, Dictionary<string, string>>
- Verify P/Invoke interop compatibility
- Enable both framework-dependent and self-contained deployment

**Out of Scope:**
- Code refactoring or architectural changes
- Introduction of new libraries or frameworks
- UI/UX modifications
- Configuration file format changes

## Cross-Platform Development Workflow

**Development Platform:** Linux (primary development environment)

**Target Platform:** Windows (runtime execution environment)

### KISS Approach: Build on Linux, Test on Windows

**.NET 8 supports cross-compilation out-of-the-box** - you can develop on Linux and produce Windows executables without Windows-specific build tools.

**Development on Linux:**
```bash
# Install .NET 8 SDK on Linux
# Ubuntu/Debian:
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Build Windows executable from Linux
dotnet build -r win-x64

# Run unit tests on Linux (non-UI tests work)
dotnet test

# Publish Windows executable from Linux
dotnet publish -c Release -r win-x64 --self-contained
```

**What Works on Linux:**
- ✅ Edit all source code
- ✅ Build Windows executables (`dotnet build -r win-x64`)
- ✅ Run unit tests for AutoCompleteMachine and ConfigLoader (pure logic, no UI)
- ✅ Compile without errors
- ✅ Package Windows deployments

**What Requires Windows:**
- ❌ Run the application (Windows Forms + P/Invoke requires Windows)
- ❌ Test UI behavior (Form, TextBox, hotkey registration)
- ❌ Test P/Invoke hotkey functionality (user32.dll Windows-specific)
- ❌ Manual testing and verification

### Recommended Workflow

**Option 1: Develop on Linux, Test on Windows VM/Machine**
```bash
# On Linux: develop and build
dotnet build -r win-x64
dotnet test  # Run non-UI unit tests

# Transfer executable to Windows for testing
scp bin/Release/net8.0-windows/win-x64/CommandLauncher.exe windows-machine:~/

# On Windows: manual testing
CommandLauncher.exe resource/conf/my.conf
```

**Option 2: CI/CD Pipeline (GitHub Actions)**
```yaml
# .github/workflows/build.yml
- Linux job: Build and run unit tests
- Windows job: Build and run full integration tests
```

**Simplest Setup (KISS):**
1. Develop all code on Linux
2. Build Windows executable on Linux: `dotnet build -r win-x64`
3. Run non-UI tests on Linux: `dotnet test`
4. Transfer .exe to Windows for manual verification
5. Final testing on Windows before release

**No special Windows SDKs required** - .NET 8 SDK on Linux includes everything needed to compile Windows Forms applications.

## Steering Document Alignment

### Technical Standards (tech.md)

**Preserved from Current Architecture:**
- Event-driven Windows Forms architecture remains intact
- Interface-based dependency injection (ICommandManager*) unchanged
- Delegate-based event callbacks maintained
- Exception-driven validation patterns preserved
- Process.Start for command execution (no changes)

**Updated for .NET 8:**
- Build System: MSBuild 3.5 → .NET 8 SDK (`dotnet build`)
- Package Management: Assembly references → NuGet PackageReference
- Testing Framework: NUnit 2.5.2 → NUnit 4.2.2 (latest stable)
- Target Framework: `TargetFrameworkVersion v3.5` → `TargetFramework net8.0-windows`

**Platform Requirements:**
- Runtime: .NET 8 Desktop Runtime (includes Windows Forms support)
- Compatibility: Windows 10 1607+ and Windows 11
- Architecture: x64 and ARM64 (drop x86)

### Project Structure (structure.md)

**Directory Organization - No Changes:**
```
TeaLauncher/
├── CommandLauncher/              # Main application (unchanged)
├── TestCommandLauncher/          # Unit tests (unchanged)
├── resource/conf/                # Configuration files (unchanged)
├── CommandLauncher.sln           # Solution file (format updated)
├── README                        # Updated with .NET 8 requirements
└── LICENSE                       # Unchanged
```

**Naming Conventions - Preserved:**
- PascalCase for classes, methods, properties
- m_ prefix for private fields
- I prefix for interfaces
- Test_ prefix for test classes

**File Organization - Maintained:**
- One primary class per file
- Partial classes (MainWindow in Form1.cs + Form1.Designer.cs)
- Designer files remain auto-generated
- License headers preserved

## Code Reuse Analysis

### Components Remaining Unchanged (Logic Preserved)

**100% Code Reuse (Migration Only):**

1. **AutoCompleteMachine.cs**
   - Purpose: Prefix-matching auto-completion
   - Changes: None (pure logic, no framework dependencies)
   - Migration: Verify List<T> compatibility (.NET 8 compatible)

2. **Hotkey.cs**
   - Purpose: Global hotkey registration via P/Invoke
   - Changes: Verify P/Invoke signatures for .NET 8 interop
   - Migration: Test RegisterHotKey/UnregisterHotKey on .NET 8

3. **IMEController.cs**
   - Purpose: Input Method Editor support
   - Changes: None expected (Windows Forms component)
   - Migration: Verify functionality on .NET 8

4. **CommandManager.cs**
   - Purpose: Command execution and management
   - Changes: Minimal (see Data Models section for Hashtable → Dictionary)
   - Migration: Update collection types only

5. **MainWindow (Form1.cs)**
   - Purpose: UI orchestration
   - Changes: None (Windows Forms compatible with .NET 8)
   - Migration: Verify event handlers and form lifecycle

### Components Requiring Minimal Changes

**ConfigLoader.cs:**
- **Current Implementation**: Uses `Hashtable` for configuration storage
- **Required Change**: Replace `Hashtable m_Conf` with `Dictionary<string, Dictionary<string, string>>`
- **Rationale**: Dictionary is the standard .NET collection, Hashtable is legacy
- **Migration Impact**: Minimal - same API surface, just stronger typing
- **Code Change Estimate**: ~10 lines (type declarations and casts)

### Integration Points

**Windows API (P/Invoke):**
- **Component**: Hotkey.cs
- **Integration**: user32.dll via DllImport
- **Migration Verification**: Ensure IntPtr, MOD_KEY enum, and Windows message handling work on .NET 8
- **Testing**: Manual hotkey testing on Windows 10/11

**Windows Forms:**
- **Components**: Form1.cs, Form1.Designer.cs
- **Integration**: System.Windows.Forms from .NET 8 Desktop Runtime
- **Migration Verification**: TextBox, Form, NotifyIcon, MessageBox compatibility
- **Testing**: UI behavior testing (show/hide, keyboard events, system tray)

**File System:**
- **Component**: ConfigLoader.cs
- **Integration**: StreamReader, File I/O
- **Migration Verification**: Path handling remains Windows-compatible
- **Testing**: Load existing .conf files from resource/conf/

## Architecture

### SDK-Style Project Structure

**Old Format (.NET Framework 3.5):**
```xml
<Project ToolsVersion="3.5" ...>
  <PropertyGroup>
    <TargetFrameworkVersion>v3.5</TargetFrameworkVersion>
    <AssemblyName>CommandLauncher</AssemblyName>
    <OutputType>WinExe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Windows.Forms" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
</Project>
```

**New Format (.NET 8 SDK-Style):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <AssemblyName>CommandLauncher</AssemblyName>
    <RootNamespace>CommandLauncher</RootNamespace>
    <ApplicationIcon />
    <StartupObject>CommandLauncher.Program</StartupObject>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Platforms>x64;ARM64</Platforms>
  </PropertyGroup>
</Project>
```

**Key Changes:**
- `Sdk="Microsoft.NET.Sdk"` attribute (enables SDK-style features)
- `TargetFramework` replaces `TargetFrameworkVersion`
- `UseWindowsForms=true` enables Windows Forms support
- No explicit `<Compile>` items (auto-discovered by SDK)
- No AssemblyInfo.cs (metadata in .csproj)
- `TreatWarningsAsErrors=true` maintained from original

### Assembly Metadata Migration

**Old Approach (AssemblyInfo.cs):**
```csharp
[assembly: AssemblyTitle("CommandLauncher")]
[assembly: AssemblyDescription("TeaLauncher - Simple command launcher")]
[assembly: AssemblyCompany("Toshiyuki Hirooka")]
[assembly: AssemblyProduct("TeaLauncher")]
[assembly: AssemblyCopyright("Copyright © 2010-2025")]
[assembly: AssemblyVersion("1.0.0.0")]
```

**New Approach (.csproj Properties):**
```xml
<PropertyGroup>
  <AssemblyTitle>CommandLauncher</AssemblyTitle>
  <Description>TeaLauncher - Simple command launcher</Description>
  <Company>Toshiyuki Hirooka</Company>
  <Product>TeaLauncher</Product>
  <Copyright>Copyright © 2010-2025</Copyright>
  <Version>2.0.0</Version>
  <FileVersion>2.0.0.0</FileVersion>
</PropertyGroup>
```

**Action:** Delete Properties/AssemblyInfo.cs files after migration

### Deployment Architecture

**Framework-Dependent Deployment:**
```xml
<PropertyGroup>
  <SelfContained>false</SelfContained>
  <PublishSingleFile>false</PublishSingleFile>
</PropertyGroup>
```
- Requires .NET 8 Desktop Runtime installed
- Smaller executable (~200KB)
- Faster updates (shared runtime)
- Build command: `dotnet publish -c Release -r win-x64`

**Self-Contained Deployment:**
```xml
<PropertyGroup>
  <SelfContained>true</SelfContained>
  <PublishSingleFile>true</PublishSingleFile>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  <PublishTrimmed>false</PublishTrimmed>
</PropertyGroup>
```
- No runtime installation required
- Single executable with embedded runtime (~60MB)
- Build command: `dotnet publish -c Release -r win-x64 --self-contained`

**Note:** PublishTrimmed set to `false` to avoid trimming Windows Forms dependencies

## Components and Interfaces

### Component: ConfigLoader

**Purpose:** Parse and load .conf configuration files

**Current Implementation:**
```csharp
class ConfigLoader
{
    Hashtable m_Conf = new Hashtable();

    public Hashtable GetConfig(string section)
    {
        return (Hashtable)m_Conf[section];
    }
}
```

**Migrated Implementation:**
```csharp
class ConfigLoader
{
    Dictionary<string, Dictionary<string, string>> m_Conf = new();

    public Dictionary<string, string> GetConfig(string section)
    {
        return m_Conf[section];
    }
}
```

**Changes:**
- Replace `Hashtable` with `Dictionary<string, Dictionary<string, string>>`
- Remove casts (type-safe)
- Use target-typed `new()` expression (C# 9 feature available in .NET 8)

**Interfaces:** Public API unchanged (callers still use GetConfig, GetSections)

**Dependencies:** System.Collections.Generic (built-in)

**Reuses:** Existing file parsing logic unchanged

### Component: CommandManager

**Purpose:** Command registry and execution

**Current Implementation:**
```csharp
class CommandManager : AutoCompleteMachine
{
    private List<Command> m_Commands = new List<Command>();

    public void RegisterCommand(Command command) { ... }
    public void Run(string command) { ... }
}
```

**Migrated Implementation:**
```csharp
class CommandManager : AutoCompleteMachine
{
    private List<Command> m_Commands = new();

    public void RegisterCommand(Command command) { ... }
    public void Run(string command) { ... }
}
```

**Changes:**
- Target-typed `new()` for list initialization
- Logic unchanged

**Interfaces:** ICommandManagerInitializer, ICommandManagerFinalizer, ICommandManagerDialogShower (unchanged)

**Dependencies:** AutoCompleteMachine (inheritance), ConfigLoader (uses GetConfig)

**Reuses:** All existing command execution logic

### Component: AutoCompleteMachine

**Purpose:** Prefix-matching auto-completion engine

**Implementation:** No changes required

**Interfaces:** RegisterWord, RemoveWord, AutoCompleteWord, GetCandidates

**Dependencies:** System.Collections.Generic.List<T> (.NET 8 compatible)

**Reuses:** 100% existing implementation

### Component: Hotkey (P/Invoke)

**Purpose:** Global hotkey registration

**Current Implementation:**
```csharp
[DllImport("user32.dll")]
extern static int RegisterHotKey(IntPtr HWnd, int ID, MOD_KEY MOD_KEY, Keys KEY);

[DllImport("user32.dll")]
extern static int UnregisterHotKey(IntPtr HWnd, int ID);
```

**Migrated Implementation:** No changes required

**Verification Required:**
- Test RegisterHotKey on .NET 8 with Windows 10/11
- Verify IntPtr marshalling
- Ensure WM_HOTKEY message handling works

**Interfaces:** Public API unchanged

**Dependencies:** System.Runtime.InteropServices (built-in)

**Reuses:** 100% existing P/Invoke code

### Component: MainWindow (Form1)

**Purpose:** UI orchestration and event handling

**Implementation:** No changes required

**Interfaces:** ICommandManagerInitializer, ICommandManagerFinalizer, ICommandManagerDialogShower

**Dependencies:** System.Windows.Forms (.NET 8 Desktop Runtime)

**Reuses:** 100% existing UI logic

## Data Models

### Configuration Data Model

**Before (Hashtable):**
```csharp
// Nested Hashtable structure
Hashtable m_Conf = new Hashtable();
// m_Conf[section] -> Hashtable
// m_Conf[section][key] -> string value (requires casting)
```

**After (Dictionary):**
```csharp
// Typed Dictionary structure
Dictionary<string, Dictionary<string, string>> m_Conf = new();
// m_Conf[section] -> Dictionary<string, string>
// m_Conf[section][key] -> string value (type-safe)
```

**Benefits:**
- Type safety (no casts)
- IntelliSense support
- Null reference warnings (if nullable enabled)
- Standard .NET collection (widely understood)

**Migration Impact:**
- ConfigLoader.cs: ~5 type signature changes
- MainWindow initialization: ~2 cast removals

### Command Data Model

**Unchanged:**
```csharp
class Command
{
    public string command;
    public string execution;
}
```

**Reason:** Simple data class, no framework dependencies

### Test Data Models

**NUnit 2.5.2 Attributes:**
```csharp
[TestFixture]
public class Test_AutoCompleteMachine
{
    [Test]
    public void TestRegistration() { ... }
}
```

**NUnit 4.x Attributes:** Same syntax (backwards compatible)

**Action:** Update NUnit package reference only, code unchanged

## Error Handling

### Scenario 1: Missing .NET 8 Runtime (Framework-Dependent Deployment)

**Description:** User runs TeaLauncher.exe without .NET 8 Desktop Runtime installed

**Handling:**
- Windows displays built-in error dialog: "You must install .NET Desktop Runtime to run this application"
- Dialog provides download link to https://dotnet.microsoft.com/download/dotnet/8.0
- Application exits gracefully

**User Impact:** Clear actionable message with installation instructions

**Implementation:** No code changes required (.NET 8 runtime provides this)

### Scenario 2: Configuration File Parse Error

**Description:** Malformed .conf file (existing error handling)

**Handling:** Unchanged - ConfigLoader throws exceptions:
- ConfigLoaderNotExistsSectionException
- ConfigLoaderMultiKeyException
- ConfigLoaderNotKeyValueException
- ConfigLoaderSameSectionException

**User Impact:** MessageBox with error details (existing behavior)

**Implementation:** No changes (exception hierarchy preserved)

### Scenario 3: Hotkey Registration Failure

**Description:** Another application already registered Ctrl+Space

**Handling:** Existing logic - iterates hotkey IDs until successful registration

**User Impact:** Transparent (fallback behavior)

**Implementation:** No changes (existing Hotkey.cs logic)

### Scenario 4: Windows Version Incompatibility

**Description:** User attempts to run on Windows 7/8

**Handling:** .NET 8 runtime refuses to start, displays OS compatibility error

**User Impact:** Clear message stating Windows 10+ requirement

**Implementation:** No code changes (platform enforcement by .NET 8)

## Testing Strategy

### Migration Verification Tests

**Phase 1: Build Verification**
1. Clean build with .NET 8 SDK succeeds
2. No compiler warnings or errors
3. TreatWarningsAsErrors remains enabled
4. Output executable is WinExe type
5. Assembly metadata correct (version, copyright)

**Phase 2: Unit Test Verification**
1. All existing NUnit tests pass on .NET 8
2. Test_AutoCompleteMachine: All test methods pass
3. Test_ConfigLoader: All test methods pass
4. Test execution via `dotnet test` succeeds
5. Test execution via Visual Studio Test Explorer succeeds

**Phase 3: Functional Testing**
1. **Startup:**
   - Application starts in < 300ms
   - System tray icon appears
   - No error dialogs

2. **Hotkey:**
   - Ctrl+Space shows input window
   - Window appears in < 100ms
   - Focus moves to textbox

3. **Auto-completion:**
   - Tab key triggers completion
   - Prefix matching works correctly
   - Completion behavior identical to .NET Framework version

4. **Command Execution:**
   - URL commands open browser (http://, https://, ftp://)
   - File path commands open applications
   - Special commands work (!reload, !exit, !version)

5. **Configuration:**
   - Load existing resource/conf/my.conf
   - All commands register correctly
   - !reload command refreshes configuration

**Phase 4: P/Invoke Testing**
1. RegisterHotKey succeeds on Windows 10
2. RegisterHotKey succeeds on Windows 11
3. UnregisterHotKey on application exit
4. WM_HOTKEY message received and processed
5. No memory leaks or handle leaks

**Phase 5: Deployment Testing**

**Framework-Dependent:**
1. Build: `dotnet publish -c Release -r win-x64`
2. Executable size < 500KB
3. Runs on machine with .NET 8 Desktop Runtime
4. Displays error on machine without runtime

**Self-Contained:**
1. Build: `dotnet publish -c Release -r win-x64 --self-contained`
2. Single-file executable created
3. Executable size ~60-80MB
4. Runs on machine without .NET installation
5. No external dependencies required

### Regression Testing Checklist

- [ ] Application starts without errors
- [ ] Hotkey (Ctrl+Space) triggers input window
- [ ] Auto-completion functions correctly
- [ ] URL commands open in browser
- [ ] File path commands execute
- [ ] Special commands (!reload, !exit, !version) work
- [ ] Configuration file parsing unchanged
- [ ] System tray integration works
- [ ] Window hide/show behavior correct
- [ ] Memory usage < 20MB idle
- [ ] No crashes during normal operation
- [ ] All unit tests pass

### Performance Benchmarks

**Baseline (.NET Framework 3.5):**
- Startup time: ~500ms
- Hotkey response: ~80ms
- Memory: ~18MB idle

**Target (.NET 8):**
- Startup time: ≤ 300ms (expected improvement)
- Hotkey response: < 100ms (maintained)
- Memory: ≤ 15MB idle (expected improvement)

**Measurement Method:**
- Startup: Time from Process.Start to Main() completion
- Hotkey: Time from keypress to window visible
- Memory: Task Manager private bytes after 60s idle

## Migration Steps Summary

1. **Convert Main Project (CommandLauncher.csproj):**
   - Replace with SDK-style .csproj
   - Set TargetFramework to net8.0-windows
   - Enable UseWindowsForms
   - Move AssemblyInfo to .csproj properties

2. **Convert Test Project (TestCommandLauncher.csproj):**
   - Replace with SDK-style .csproj
   - Set TargetFramework to net8.0-windows
   - Add NUnit 4.2.2 NuGet package

3. **Update ConfigLoader.cs:**
   - Replace Hashtable with Dictionary<string, Dictionary<string, string>>
   - Remove casts
   - Verify GetConfig and GetSections methods

4. **Verify All Components:**
   - Build solution
   - Run all tests
   - Manual functional testing

5. **Configure Deployment:**
   - Set up framework-dependent publish profile
   - Set up self-contained publish profile
   - Test both deployment types

6. **Update Documentation:**
   - Update README with .NET 8 requirements
   - Add build instructions for .NET 8 SDK
   - Document deployment options

## Build Commands Reference

### On Linux (Cross-Compilation)

**Install .NET 8 SDK on Linux:**
```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Verify installation
dotnet --version  # Should show 8.0.x
```

**Development Build (produces Windows executable):**
```bash
dotnet build -r win-x64
```

**Run Tests (non-UI tests work on Linux):**
```bash
dotnet test
```

**Framework-Dependent Publish for Windows:**
```bash
dotnet publish -c Release -r win-x64
```

**Self-Contained Publish for Windows:**
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

**Clean:**
```bash
dotnet clean
```

### On Windows (Native Build)

**Development Build:**
```bash
dotnet build
```

**Run All Tests (including UI tests):**
```bash
dotnet test
```

**Publish:**
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

### Cross-Platform Development Notes

- **Linux builds produce Windows .exe files** when using `-r win-x64`
- **Unit tests for logic components** (AutoCompleteMachine, ConfigLoader) run fine on Linux
- **UI and P/Invoke tests** must run on Windows (require Windows Forms runtime and user32.dll)
- **No Windows-specific SDKs needed** on Linux - .NET 8 SDK handles everything
