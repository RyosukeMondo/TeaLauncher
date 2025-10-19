# Project Structure

## Directory Organization

```
TeaLauncher/
├── CommandLauncher/              # Main application source code
│   ├── Properties/               # Assembly metadata and resources
│   │   ├── AssemblyInfo.cs      # Version, copyright, assembly attributes
│   │   └── Resources.Designer.cs # Embedded resource definitions
│   ├── Program.cs               # Application entry point
│   ├── Form1.cs                 # MainWindow UI and orchestration
│   ├── Form1.Designer.cs        # Auto-generated UI designer code
│   ├── Form1.resx              # Form resources (layout, strings)
│   ├── CommandManager.cs        # Command registry and execution engine
│   ├── AutoCompleteMachine.cs   # Prefix-matching auto-completion logic
│   ├── ConfigLoader.cs          # Configuration file parser
│   ├── Hotkey.cs               # Global hotkey registration (Windows API)
│   ├── IMEController.cs         # Input Method Editor support
│   └── CommandLauncher.csproj   # MSBuild project configuration
│
├── TestCommandLauncher/          # Unit test project (NUnit)
│   ├── Properties/               # Test project assembly metadata
│   ├── Test_AutoCompleteMachine.cs  # AutoCompleteMachine test cases
│   ├── Test_ConfigLoader.cs     # ConfigLoader test cases
│   └── TestCommandLauncher.csproj   # Test project configuration
│
├── resource/                     # Runtime resources and examples
│   └── conf/                     # Configuration file templates
│       └── my.conf              # Default user configuration file
│
├── CommandLauncher.sln          # Visual Studio solution file
├── README                       # Project documentation
└── LICENSE                      # GNU GPL v2 license text
```

**Organizational Pattern**: **Flat structure within project directories**
- Simple project with minimal hierarchy - all source files in root of CommandLauncher/
- Separation by project type (application vs. tests)
- Resource isolation in dedicated `resource/` directory

## Naming Conventions

### Files
- **Classes/Components**: `PascalCase.cs` (e.g., `CommandManager.cs`, `AutoCompleteMachine.cs`)
- **Main Form**: `Form1.cs` with `Form1.Designer.cs` (Visual Studio WinForms convention)
- **Tests**: `Test_[ClassName].cs` (e.g., `Test_AutoCompleteMachine.cs`, `Test_ConfigLoader.cs`)
- **Designer Files**: `[FileName].Designer.cs` for auto-generated code
- **Resources**: `[FileName].resx` for embedded resources

### Code
- **Classes/Types**: `PascalCase` (e.g., `CommandManager`, `ConfigLoader`, `HotKey`)
- **Interfaces**: `I[PascalCase]` prefix (e.g., `ICommandManagerInitializer`, `ICommandManagerFinalizer`)
- **Functions/Methods**: `PascalCase` (e.g., `RegisterCommand()`, `AutoCompleteWord()`, `LoadConfigFile()`)
- **Constants**: `UPPER_SNAKE_CASE` (e.g., `WM_HOTKEY`, `MOD_KEY`)
- **Variables**:
  - Public/protected: `PascalCase` (e.g., `HotKeyPressed` event)
  - Private fields: `m_[PascalCase]` prefix (e.g., `m_CommandManager`, `m_Hotkey`, `m_ConfigFileName`)
  - Local variables: `lowercase` or `snake_case` (e.g., `filename`, `conf_filename`, `is_begin_quotation`)
- **Test Methods**: `Test[Description]` (e.g., `TestRegistration()`, `TestAutoComplete()`)
- **Enums**: `PascalCase` for type, `UPPER_CASE` for values (e.g., `MOD_KEY.CONTROL`, `MOD_KEY.ALT`)

### Namespace
- **Single namespace**: `CommandLauncher` for all application code
- **Test namespace**: `CommandLauncher.Tests` (though some test files omit `.Tests`)

## Import Patterns

### Import Order
1. **System namespaces** (alphabetical by subsystem):
   ```csharp
   using System;
   using System.Collections;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Data;
   using System.Diagnostics;
   using System.Drawing;
   using System.IO;
   using System.Linq;
   using System.Runtime.InteropServices;
   using System.Text;
   using System.Threading;
   using System.Windows.Forms;
   ```

2. **Third-party libraries**:
   ```csharp
   using NUnit.Framework;  // Test projects only
   ```

3. **Internal project references**:
   ```csharp
   using CommandLauncher;  // In test projects
   ```

**No relative imports** - C# uses namespace-based imports only.

### Module/Package Organization
- **Flat namespace structure**: All classes in single `CommandLauncher` namespace
- **No sub-namespaces**: No layering like `.UI`, `.Core`, `.Utils`
- **Assembly references**: System assemblies + NUnit for tests (no external NuGet packages)

## Code Structure Patterns

### File Organization
Every source file follows this strict pattern:

```csharp
1. GNU GPL v2 license header (18 lines, block comment)
2. Blank line
3. Attribution comment (if third-party code, e.g., Hotkey.cs)
4. Using statements (System first, then third-party, then project)
5. Blank line
6. Namespace declaration
7. Class/interface/enum definitions
```

### Class Organization
Standard order within class definitions:

```csharp
1. Nested types (enums, nested classes)
2. Private fields (m_ prefixed)
3. Events (public event EventHandler)
4. Constructor(s)
5. Public methods
6. Private/protected helper methods
7. Interface implementations (grouped together)
```

**Example from CommandManager**:
```csharp
class CommandManager : AutoCompleteMachine
{
    // 1. Private fields
    private List<Command> m_Commands = new List<Command>();
    ICommandManagerInitializer m_Initializer;
    ICommandManagerFinalizer m_Finalizer;
    ICommandManagerDialogShower m_DialogShower;

    // 2. Constructor
    public CommandManager(...) { }

    // 3. Public methods
    public void RegisterCommand(Command command) { }
    public void RemoveCommand(string command) { }
    public void Run(string command) { }

    // 4. Private helpers
    private bool IsPath(string str) { }
    private bool IsSpecialCommand(string str) { }
    private void RunSpecialCommand(string cmd) { }
    private List<string> Split(string str) { }
}
```

### Function/Method Organization
```
1. Input validation and guard clauses
2. Variable declarations
3. Core logic
4. Exception handling (try/catch/finally)
5. Return statement
```

**Pattern**: Defensive validation first, happy path after
- Use exceptions for invalid state (e.g., `ConfigLoaderNotExistsSectionException`)
- Delegate-based callbacks for filtering (e.g., `m_Commands.Find(delegate(Command cmd) { ... })`)

### File Organization Principles
- **One primary class per file** (except nested types like `HotKeyForm` inside `HotKey`)
- **Designer files separate**: UI code auto-generated in `.Designer.cs` files
- **Partial classes**: `public partial class MainWindow` split between `Form1.cs` and `Form1.Designer.cs`
- **Related functionality grouped**: Command struct defined in same file as CommandManager

## Code Organization Principles

1. **Single Responsibility**: Each class has one clear purpose:
   - `CommandManager`: Command registry and execution
   - `AutoCompleteMachine`: Prefix matching and completion
   - `ConfigLoader`: File parsing only
   - `MainWindow`: UI orchestration and event handling

2. **Modularity**: Components communicate through well-defined interfaces:
   - `ICommandManagerInitializer`: Reload configuration
   - `ICommandManagerFinalizer`: Exit application
   - `ICommandManagerDialogShower`: Display messages

3. **Inheritance for Composition**: `CommandManager` extends `AutoCompleteMachine` to reuse completion logic

4. **Testability**: Core logic (AutoCompleteMachine, ConfigLoader) isolated from UI for unit testing

## Module Boundaries

### Core vs UI Separation
- **Core Modules** (no UI dependencies):
  - `AutoCompleteMachine.cs`: Pure logic, List-based storage
  - `ConfigLoader.cs`: File I/O only, Hashtable-based parsing
  - `CommandManager.cs`: Command execution via Process.Start

- **UI Modules** (Windows Forms dependent):
  - `Form1.cs` (MainWindow): TextBox input, event handlers, window visibility
  - `Hotkey.cs`: Wraps hidden Form for Windows message loop

- **Interface Boundary**: `ICommandManager*` interfaces prevent tight coupling

### Platform-Specific vs Cross-Platform
- **Windows-Specific Code**:
  - `Hotkey.cs`: P/Invoke to user32.dll (RegisterHotKey, UnregisterHotKey)
  - `IMEController.cs`: Google IME integration
  - File path handling: Backslash separators, drive letters (X:\)

- **Potentially Portable**:
  - `AutoCompleteMachine.cs`: Pure string logic
  - `ConfigLoader.cs`: StreamReader-based (portable with path adjustments)
  - `CommandManager.cs`: Uses System.Diagnostics.Process (mostly portable)

### Public API vs Internal
- **No explicit public API**: All classes in default internal access
- **Implicit API**: Classes and public methods constitute the API
- **Implementation Details**: Private methods (m_ prefixed fields, helper methods)

### Dependencies Direction
```
MainWindow (UI)
    ↓ implements interfaces
    ↓ creates instances
CommandManager
    ↓ inherits
AutoCompleteMachine
    ↓ uses
ConfigLoader (standalone)

Hotkey (standalone, platform-specific)
IMEController (standalone, platform-specific)
```

**Dependency Rule**: UI depends on core, core never depends on UI (enforced via interfaces)

## Code Size Guidelines

Current project follows these natural limits:

- **File size**:
  - Most files: 100-300 lines
  - Largest: `CommandManager.cs` (~290 lines), `Form1.cs` (~200 lines visible)
  - Target: < 500 lines per file

- **Function/Method size**:
  - Typical: 10-30 lines
  - Complex logic (e.g., `Split()` method): ~40 lines
  - Target: < 50 lines per method (excluding tests)

- **Class complexity**:
  - Single responsibility classes: < 10 public methods
  - Interface implementations: 3-5 methods per interface
  - Target: < 15 public methods per class

- **Nesting depth**:
  - Current: Max 3-4 levels (try/catch with foreach/if)
  - Target: ≤ 4 levels of indentation

## Dashboard/Monitoring Structure (if applicable)
**Not applicable** - TeaLauncher has no dashboard component. It's a minimalist UI application with a single input textbox.

## Documentation Standards

### Current Practices
- **License headers**: GNU GPL v2 header in every source file (mandatory)
- **XML documentation**: Sparse - primarily for public classes like `HotKey`
  ```csharp
  /// <summary>
  /// グローバルホットキーを登録するクラス。
  /// 使用後は必ずDisposeすること。
  /// </summary>
  ```
- **Inline comments**: Primarily in Japanese (e.g., `// 登録チェック`, `// コマンド管理`)
- **Attribution comments**: Third-party code sources cited (e.g., Hotkey.cs from http://www.k4.dion.ne.jp/~anis7742/codevault/)

### Expected Standards
- **Public classes**: Should have XML summary tags (currently inconsistent)
- **Complex logic**: Inline comments explaining "why" (currently mixed Japanese/English)
- **Interfaces**: Document contract expectations
- **README**: High-level usage instructions (exists at project root)
- **Test documentation**: Test method names are self-documenting (e.g., `TestAutoComplete_ReturnsCommonPrefix`)

### Language Conventions
- **Code**: C# (English keywords, PascalCase identifiers)
- **Comments**: Mixed Japanese and English (historical codebase)
- **User-facing**: English (README, LICENSE)
- **Future guideline**: Prefer English for new comments to support international contributors
