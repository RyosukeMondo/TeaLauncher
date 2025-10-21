# TeaLauncher

[![Release Build](https://github.com/RyosukeMondo/TeaLauncher/actions/workflows/release.yml/badge.svg)](https://github.com/RyosukeMondo/TeaLauncher/actions/workflows/release.yml)

Modern Windows Command Launcher built with Avalonia UI and .NET 8.

**Version 2.0** - Complete rewrite with modern UI framework and YAML configuration.

## Features

- 🚀 **Global Hotkey**: Press `Ctrl+Space` to activate from anywhere
- ⚡ **Fast**: Sub-100ms response time with hardware-accelerated UI
- 📝 **YAML Configuration**: Modern, human-readable configuration format
- 🎨 **Modern UI**: Avalonia-based interface with blur effects
- 🔄 **Auto-completion**: Tab-based command completion
- 🪟 **Windows 10/11**: Native Windows integration

## Installation

### Download Latest Release

Download pre-built binaries from the [latest release](https://github.com/RyosukeMondo/TeaLauncher/releases/latest).

#### Windows

1. Download `TeaLauncher-{version}-win-x64.exe` from the latest release
2. Place the executable in a folder of your choice
3. Create a `commands.yaml` file in the same folder (see [Configuration](#configuration))
4. Double-click `TeaLauncher-{version}-win-x64.exe` to run
5. Press `Ctrl+Space` to activate the launcher

#### Linux

1. Download `TeaLauncher-{version}-linux-x64` from the latest release
2. Make it executable:
   ```bash
   chmod +x TeaLauncher-{version}-linux-x64
   ```
3. Create a `commands.yaml` file in the same folder (see [Configuration](#configuration))
4. Run the executable:
   ```bash
   ./TeaLauncher-{version}-linux-x64
   ```
5. Press `Ctrl+Space` to activate the launcher

## Quick Start

### Configuration

Create or edit `commands.yaml`:

```yaml
commands:
  - name: google
    linkto: https://google.com
    description: Google search

  - name: github
    linkto: https://github.com
    description: GitHub

  - name: notepad
    linkto: C:\Windows\System32\notepad.exe
    description: Text editor
```

## Building from Source

### Requirements

- .NET 8 SDK
- Works on Linux, macOS, or Windows (cross-compilation supported)

### Build Commands

```bash
# Build for Windows (from any OS)
dotnet build TeaLauncher.Avalonia -c Release -r win-x64

# Create single-file executable
dotnet publish TeaLauncher.Avalonia -c Release -r win-x64 \
  --self-contained true -p:PublishSingleFile=true

# Run tests
dotnet test TeaLauncher.Avalonia.Tests
```

For comprehensive build instructions and troubleshooting, see [BUILD.md](BUILD.md).

For migration from v1.x, see [docs/MIGRATION.md](docs/MIGRATION.md).

## Usage

1. **Activate**: Press `Ctrl+Space` from anywhere
2. **Type**: Start typing a command name
3. **Auto-complete**: Press `Tab` to complete
4. **Execute**: Press `Enter` to run
5. **Cancel**: Press `Esc` to clear or hide

### Special Commands

- `!version` - Show version information
- `!reload` - Reload configuration file
- `!exit` - Exit TeaLauncher

## Architecture

- **TeaLauncher.Avalonia**: Modern Avalonia UI application (.NET 8)
- **CommandLauncher**: Shared core library (CommandManager, AutoCompleteMachine)

### Technology Stack

- **UI Framework**: Avalonia 11.2
- **Configuration**: YamlDotNet 16.3
- **Runtime**: .NET 8
- **Platform**: Windows 10/11 (x64)

## License

GNU General Public License v2 (GPL-2.0)

See [LICENSE](LICENSE) for full license text.

## Releases

### For Maintainers: Creating a Release

Releases are automated via GitHub Actions when a version tag is pushed:

1. **Create a version tag** following semantic versioning:
   ```bash
   git tag v2.0.0           # Production release
   git tag v2.1.0-beta      # Pre-release (beta)
   git tag v2.1.0-rc1       # Pre-release (release candidate)
   ```

2. **Push the tag** to trigger the release workflow:
   ```bash
   git push origin v2.0.0
   ```

3. **Monitor the workflow** at [Actions → Release Build](https://github.com/RyosukeMondo/TeaLauncher/actions/workflows/release.yml)

The workflow will:
- Run all quality checks (tests, coverage ≥60%, code metrics, formatting)
- Build Windows and Linux binaries
- Create a GitHub Release with both platform executables
- Auto-generate release notes from commit history

For troubleshooting workflow issues, see [TESTING.md](TESTING.md).

## Credits

Original version by Toshiyuki Hirooka <toshi.hirooka@gmail.com>

v2.0 Avalonia migration - Complete UI modernization and YAML configuration
