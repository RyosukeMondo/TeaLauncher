# TeaLauncher Migration Guide

## Overview

This guide helps you migrate from the legacy TeaLauncher (Windows Forms + .NET Framework 3.5 + INI configuration) to the modern TeaLauncher.Avalonia (Avalonia UI + .NET 8 + YAML configuration).

### What's Changed

**UI Framework:**
- **Old:** Windows Forms (.NET Framework 3.5)
- **New:** Avalonia UI (.NET 8)
- **Impact:** Modern, hardware-accelerated rendering with better visual effects

**Configuration Format:**
- **Old:** INI-style `.conf` files (`[section]` and `key=value`)
- **New:** YAML format with `.yaml` files
- **Impact:** More readable, supports comments, better IDE tooling

**Runtime:**
- **Old:** .NET Framework 3.5 (Windows-only, legacy)
- **New:** .NET 8 (modern, performant, cross-compilable)
- **Impact:** Better performance, smaller memory footprint, faster startup

### What Stays the Same

- **Functionality:** All features work identically
- **Hotkey:** Ctrl+Space (Alt+Space in debug builds)
- **Commands:** URLs, executables, file paths, special commands
- **Behavior:** Auto-completion, command execution, window activation

### Migration Benefits

- **Performance:** ~50% faster startup (≤300ms vs ≤500ms)
- **Memory:** ~25% lower memory usage (≤15MB vs ≤20MB)
- **Configuration:** Human-readable YAML with inline comments
- **Modern UI:** Hardware-accelerated rendering with blur effects
- **Build Environment:** Cross-compile from Linux to Windows

---

## Configuration Format Migration

### Old Format (.conf) vs New Format (.yaml)

#### Old INI Format (my.conf)

```ini
[reader]
linkto = http://reader.livedoor.com/reader/

[mail]
linkto = https://mail.google.com/?hl=ja

[vim]
linkto = c:\tools\vim\gvim.exe

[notepad]
linkto = notepad

[cmd]
linkto = cmd.exe

[reload_config]
linkto = !reload

[edit_config]
linkto = notepad conf/my.conf

[version]
linkto = !version

[exit]
linkto = !exit
```

#### New YAML Format (commands.yaml)

```yaml
# TeaLauncher Commands Configuration
# This YAML file defines the commands available in TeaLauncher.
# Each command has a name (keyword) and a linkto (target URL/executable/path).
# Optional fields: description and arguments

commands:
  # Web URLs - Open websites in default browser
  - name: reader
    linkto: http://reader.livedoor.com/reader/
    description: Open Livedoor Reader

  - name: mail
    linkto: https://mail.google.com/?hl=ja
    description: Open Gmail

  # Applications - Launch executables
  - name: vim
    linkto: c:\tools\vim\gvim.exe
    description: Open GVim text editor

  - name: notepad
    linkto: notepad
    description: Open Windows Notepad

  - name: cmd
    linkto: cmd.exe
    description: Open Command Prompt

  # File paths - Open specific files or folders
  - name: edit_config
    linkto: notepad
    arguments: commands.yaml
    description: Edit this configuration file

  # Special commands - Built-in TeaLauncher commands
  - name: reload_config
    linkto: "!reload"
    description: Reload commands.yaml without restarting

  - name: version
    linkto: "!version"
    description: Show TeaLauncher version information

  - name: exit
    linkto: "!exit"
    description: Exit TeaLauncher application
```

### Key Differences

| Feature | Old (.conf) | New (.yaml) |
|---------|-------------|-------------|
| **Format** | INI sections | YAML list of objects |
| **Command Name** | Section header `[name]` | Property `name: value` |
| **Command Target** | `linkto = value` | `linkto: value` |
| **Comments** | Not supported | `# Comment text` |
| **Descriptions** | Not supported | `description: text` (optional) |
| **Arguments** | Included in linkto | Separate `arguments:` field (optional) |
| **Indentation** | Not significant | 2-space indentation required |
| **Quotes** | Not required | Required for special chars (`:`, `!`, etc.) |

### Migration Steps

1. **Create new YAML file:**
   ```bash
   # Create commands.yaml in the same directory as TeaLauncher.exe
   notepad commands.yaml
   ```

2. **Convert each INI section to YAML:**
   - Change `[command_name]` to `- name: command_name`
   - Change `linkto = target` to `linkto: target`
   - Add proper indentation (2 spaces)
   - Optionally add `description:` field

3. **Handle special cases:**
   - Quote special commands: `linkto: "!reload"`
   - Use `arguments:` for command-line arguments
   - Add `description:` for documentation

4. **Validate YAML syntax:**
   - Use online YAML validator (yamllint.com)
   - Or use VS Code with YAML extension
   - Launch TeaLauncher - it will show clear error messages for syntax errors

---

## YAML Syntax Guide

### Basic Structure

```yaml
commands:
  - name: command1
    linkto: target1
    description: Optional description
    arguments: Optional arguments

  - name: command2
    linkto: target2
```

### Required Fields

- `name` - Command keyword (what you type in TeaLauncher)
- `linkto` - Target URL, executable path, or special command

### Optional Fields

- `description` - Human-readable description (for documentation)
- `arguments` - Command-line arguments (passed to executable)

### Important Rules

1. **Indentation:** Use 2 spaces (not tabs)
2. **Quotes:** Use quotes for strings with special characters (`:`, `!`, `#`, etc.)
3. **Lists:** Each command starts with `- name:`
4. **Root:** All commands must be under `commands:` key

### Examples

#### Opening URLs

```yaml
commands:
  - name: google
    linkto: https://www.google.com/
    description: Open Google search

  - name: github
    linkto: https://github.com/
```

#### Launching Applications

```yaml
commands:
  - name: notepad
    linkto: notepad
    description: Open Windows Notepad

  - name: explorer
    linkto: explorer.exe
    description: Open Windows Explorer

  - name: vscode
    linkto: C:\Program Files\Microsoft VS Code\Code.exe
```

#### Commands with Arguments

```yaml
commands:
  - name: edit_hosts
    linkto: notepad
    arguments: C:\Windows\System32\drivers\etc\hosts
    description: Edit Windows hosts file

  - name: ping_google
    linkto: cmd.exe
    arguments: /k ping google.com
    description: Ping Google DNS
```

#### Special Commands

```yaml
commands:
  - name: reload
    linkto: "!reload"
    description: Reload configuration without restart

  - name: version
    linkto: "!version"
    description: Show TeaLauncher version

  - name: exit
    linkto: "!exit"
    description: Exit application
```

**Note:** Special commands starting with `!` must be quoted.

---

## Building from Source

### Prerequisites

- **Linux:** .NET 8 SDK
- **Target:** Windows 10 version 1809+ or Windows 11

### Building on Linux (Cross-Compilation)

#### Automated Build (Recommended)

```bash
# Navigate to TeaLauncher directory
cd TeaLauncher

# Run the build script
./scripts/build-windows.sh
```

The script will:
- Build TeaLauncher.Avalonia for Windows (win-x64)
- Create single-file executable
- Output to `dist/TeaLauncher-win-x64/`
- Include TeaLauncher.exe and commands.yaml

#### Manual Build

```bash
# Build Release configuration
dotnet publish TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:DebugType=None \
  -p:DebugSymbols=false

# Output location
cd TeaLauncher.Avalonia/bin/Release/net8.0-windows/win-x64/publish/
```

### Build Options

**Self-Contained (Recommended):**
- Includes .NET runtime
- Single executable file
- Size: ~60-80 MB
- No runtime installation required on Windows

**Framework-Dependent:**
- Requires .NET 8 Desktop Runtime on Windows
- Smaller size: ~1-5 MB
- Faster builds

To build framework-dependent, use:
```bash
dotnet publish TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj \
  -c Release \
  -r win-x64
```

---

## Deployment Instructions

### For End Users (Windows)

#### Option 1: Self-Contained Build (Recommended)

**Requirements:**
- Windows 10 version 1809+ or Windows 11
- No .NET installation required

**Installation:**
1. Download TeaLauncher release package
2. Extract to desired location (e.g., `C:\TeaLauncher\`)
3. Create or copy `commands.yaml` configuration
4. Run `TeaLauncher.exe`

#### Option 2: Framework-Dependent Build

**Requirements:**
- Windows 10 version 1809+ or Windows 11
- .NET 8 Desktop Runtime installed

**Installation:**
1. Install .NET 8 Desktop Runtime:
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Select "Desktop Runtime" installer
   - Run: `windowsdesktop-runtime-8.0.x-win-x64.exe`

2. Download TeaLauncher framework-dependent build
3. Extract to desired location
4. Create or copy `commands.yaml` configuration
5. Run `TeaLauncher.exe`

### Creating Configuration File

Create `commands.yaml` in the same directory as `TeaLauncher.exe`:

```yaml
commands:
  - name: google
    linkto: https://www.google.com/
    description: Open Google search

  - name: notepad
    linkto: notepad
    description: Open Notepad

  - name: reload
    linkto: "!reload"
    description: Reload configuration

  - name: exit
    linkto: "!exit"
    description: Exit TeaLauncher
```

### First Run

1. Launch `TeaLauncher.exe`
2. TeaLauncher runs in system tray (look for tray icon)
3. Press **Ctrl+Space** to show the launcher window
4. Type command name and press **Enter**
5. Press **Escape** to hide window

---

## Usage Guide

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **Ctrl+Space** | Show/hide launcher window |
| **Tab** | Auto-complete command (longest common prefix) |
| **Enter** | Execute command and hide window |
| **Escape** | Clear input (if text present) or hide window (if empty) |

**Note:** In debug builds, the hotkey is **Alt+Space** instead.

### Special Commands

| Command | Description |
|---------|-------------|
| `!reload` | Reload commands.yaml without restarting application |
| `!version` | Show TeaLauncher version information |
| `!exit` | Exit TeaLauncher application |

### Auto-Completion

Type partial command name and press **Tab**:

```
Example:
  Type: "goo" + Tab
  Result: "google" (auto-completed)

Multiple matches:
  Type: "n" + Tab
  Result: "n" (shows dropdown with "notepad", "nano", etc.)
```

### Reloading Configuration

After editing `commands.yaml`:
1. Press **Ctrl+Space** to show launcher
2. Type `reload` (or whatever you named the reload command)
3. Press **Enter**
4. New commands are immediately available

---

## Troubleshooting

### Configuration Errors

#### Missing Configuration File

**Error:**
```
Could not find file 'C:\TeaLauncher\commands.yaml'
```

**Solution:**
1. Create `commands.yaml` in the same directory as `TeaLauncher.exe`
2. Add at minimum:
   ```yaml
   commands:
     - name: test
       linkto: notepad
   ```

#### Invalid YAML Syntax

**Error:**
```
(Line: 5, Col: 10, Idx: 42) - (Line: 5, Col: 11, Idx: 43): While scanning a simple key, could not find expected ':'
```

**Solution:**
1. Check line 5 in `commands.yaml`
2. Ensure proper indentation (2 spaces)
3. Verify colons after field names: `name:`, `linkto:`
4. Use online YAML validator: https://yamllint.com

#### Missing Required Fields

**Error:**
```
Required property 'name' not found in command entry.
```

**Solution:**
1. Each command must have both `name` and `linkto` fields
2. Check for typos: `name` not `Name`, `linkto` not `linkTo`
3. Ensure proper structure:
   ```yaml
   commands:
     - name: mycommand
       linkto: target
   ```

### Hotkey Issues

#### Hotkey Not Working

**Symptoms:** Ctrl+Space does not show TeaLauncher window

**Solutions:**
1. Check if another application is using Ctrl+Space
   - Common conflicts: IME switchers, other launchers
   - TeaLauncher will try alternative hotkey IDs automatically

2. Check TeaLauncher is running
   - Look for TeaLauncher icon in system tray
   - If not visible, launch `TeaLauncher.exe`

3. Try restarting TeaLauncher
   - Use `!exit` command or Task Manager
   - Launch `TeaLauncher.exe` again

4. Check Windows version
   - Requires Windows 10 version 1809+ or Windows 11
   - Older versions not supported

#### Hotkey Works But Window Doesn't Show

**Symptoms:** Hotkey registers but window doesn't appear

**Solutions:**
1. Check if window is off-screen
   - Disconnect external monitors
   - Restart TeaLauncher

2. Check for YAML configuration errors
   - TeaLauncher may fail to initialize with bad config
   - Fix `commands.yaml` syntax errors

### Command Execution Errors

#### Command Not Found

**Symptoms:** Typing command name shows no auto-completion suggestions

**Solutions:**
1. Check command is defined in `commands.yaml`
2. Use `!reload` to reload configuration
3. Verify YAML syntax is correct
4. Check for typos in command name

#### Command Doesn't Execute

**Symptoms:** Pressing Enter doesn't launch application/URL

**Solutions:**
1. Check `linkto` path is correct
   - For executables: use full path or ensure it's in PATH
   - For URLs: include `http://` or `https://`

2. Check file/application exists
   - Example: `C:\Program Files\MyApp\myapp.exe`

3. Test with simple command:
   ```yaml
   commands:
     - name: test
       linkto: notepad
   ```

### Runtime Errors

#### Application Won't Start

**Error:** "Application failed to start"

**Solutions:**

**For Self-Contained Builds:**
1. Verify Windows 10 version 1809+ or Windows 11
2. Check antivirus isn't blocking execution
3. Run as Administrator (usually not required)

**For Framework-Dependent Builds:**
1. Install .NET 8 Desktop Runtime:
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Select "Desktop Runtime" installer

2. Verify installation:
   ```cmd
   dotnet --version
   ```
   Should show `8.0.x` or higher

#### IME Issues (Japanese Users)

**Symptoms:** IME state not reset when launcher window shows

**Solutions:**
1. IME control requires Windows 10+ with Imm32.dll
2. Try manually switching IME before typing
3. Report issue if IME reset sequence (Off-On-Off) fails

### Build Issues (Developers)

#### Cross-Compilation Fails on Linux

**Error:** "SDK not found" or "Project targets .NET Framework"

**Solutions:**
1. Verify .NET 8 SDK installed:
   ```bash
   dotnet --version
   # Should show 8.0.x
   ```

2. Install .NET 8 SDK on Linux:
   ```bash
   wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
   chmod +x ./dotnet-install.sh
   ./dotnet-install.sh --channel 8.0

   # Add to PATH
   export PATH="$HOME/.dotnet:$PATH"
   export DOTNET_ROOT="$HOME/.dotnet"
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build -r win-x64
   ```

#### Build Succeeds But Executable Won't Run on Windows

**Solutions:**
1. Ensure build target is `win-x64`:
   ```bash
   dotnet publish -r win-x64 --self-contained
   ```

2. For framework-dependent builds, ensure .NET 8 Desktop Runtime installed on Windows

3. Check TeaLauncher.Avalonia.csproj has:
   ```xml
   <TargetFramework>net8.0-windows</TargetFramework>
   <RuntimeIdentifier>win-x64</RuntimeIdentifier>
   ```

---

## Performance Comparison

### Startup Time

| Version | Target | Measurement |
|---------|--------|-------------|
| **Old (Forms)** | ≤ 500ms | Process start to tray ready |
| **New (Avalonia)** | ≤ 300ms | Process start to tray ready |
| **Improvement** | **40% faster** | - |

### Memory Usage (Idle)

| Version | Target | Measurement |
|---------|--------|-------------|
| **Old (Forms)** | ≤ 20MB | Private working set |
| **New (Avalonia)** | ≤ 15MB | Private working set |
| **Improvement** | **25% less** | - |

### Hotkey Response

| Version | Target | Measurement |
|---------|--------|-------------|
| **Old (Forms)** | < 100ms | Hotkey press to window visible |
| **New (Avalonia)** | < 100ms | Hotkey press to window visible |
| **Improvement** | **Same (hardware-accelerated)** | - |

---

## FAQ

### Q: Can I use my old .conf file?

**A:** No, the new version requires YAML format. Use the conversion guide in this document to migrate your configuration.

### Q: What happened to the .conf format?

**A:** The INI-style .conf format has been replaced with YAML for better readability, comment support, and modern IDE tooling. There is no backward compatibility.

### Q: Do I need to install .NET runtime?

**A:**
- **Self-contained builds:** No, runtime is included
- **Framework-dependent builds:** Yes, install .NET 8 Desktop Runtime

### Q: Can I run TeaLauncher on Linux or macOS?

**A:** Not yet. The current version targets Windows only. Cross-platform support is planned for future releases.

### Q: Will my commands work the same way?

**A:** Yes! All command types work identically:
- URLs (http://, https://)
- Executables (notepad, cmd.exe, full paths)
- File paths
- Special commands (!reload, !exit, !version)

### Q: How do I add new commands without restarting?

**A:**
1. Edit `commands.yaml`
2. Press Ctrl+Space
3. Type `reload` (or your reload command name)
4. Press Enter
5. New commands are available immediately

### Q: Can I change the hotkey from Ctrl+Space?

**A:** In the current version, the hotkey is hardcoded to Ctrl+Space (Alt+Space in debug builds). Custom hotkey configuration is planned for future releases.

### Q: Where should I put commands.yaml?

**A:** In the same directory as `TeaLauncher.exe`. You can specify a different path as a command-line argument:
```cmd
TeaLauncher.exe C:\MyConfig\commands.yaml
```

### Q: How do I run TeaLauncher at Windows startup?

**A:**
1. Press `Win+R`
2. Type `shell:startup` and press Enter
3. Create shortcut to `TeaLauncher.exe`
4. Optionally add command-line arguments in shortcut properties

### Q: Can I use environment variables in commands?

**A:** Yes, Windows will expand environment variables in paths:
```yaml
commands:
  - name: user_profile
    linkto: explorer.exe
    arguments: "%USERPROFILE%"
```

---

## Migration Checklist

- [ ] Read this migration guide
- [ ] Review old .conf configuration
- [ ] Create new commands.yaml file
- [ ] Convert all commands from INI to YAML format
- [ ] Validate YAML syntax (yamllint.com or VS Code)
- [ ] Download/build TeaLauncher.Avalonia
- [ ] Place commands.yaml next to TeaLauncher.exe
- [ ] Launch TeaLauncher.exe
- [ ] Test hotkey (Ctrl+Space)
- [ ] Test each command
- [ ] Test auto-completion (Tab key)
- [ ] Test special commands (!reload, !version, !exit)
- [ ] Test configuration reload after editing
- [ ] Verify performance (startup time, memory usage)
- [ ] Set up autostart if desired
- [ ] Remove old TeaLauncher version

---

## Getting Help

### Documentation

- **BUILD.md** - Building TeaLauncher from source
- **README** - Project overview
- **This file (MIGRATION.md)** - Migration guide

### Reporting Issues

If you encounter problems:
1. Check this troubleshooting guide
2. Verify your commands.yaml syntax
3. Test with minimal configuration (1-2 commands)
4. Check Windows version compatibility
5. Report issues on GitHub with:
   - TeaLauncher version (!version command)
   - Windows version
   - commands.yaml content (sanitized)
   - Error messages
   - Steps to reproduce

---

## Summary

The migration from Windows Forms + .conf to Avalonia + YAML brings significant improvements in performance, maintainability, and user experience. The configuration format change requires one-time manual migration, but the YAML format is more readable and supports inline documentation through comments.

**Key Takeaways:**
- Convert INI sections to YAML list items
- Use 2-space indentation
- Quote special characters (especially `!` for special commands)
- Validate YAML syntax before launching
- Use `!reload` to test changes without restart

Welcome to modern TeaLauncher!
