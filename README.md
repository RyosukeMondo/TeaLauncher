# TeaLauncher

Modern Windows Command Launcher built with Avalonia UI and .NET 8.

**Version 2.0** - Complete rewrite with modern UI framework and YAML configuration.

## Features

- 🚀 **Global Hotkey**: Press `Ctrl+Space` to activate from anywhere
- ⚡ **Fast**: Sub-100ms response time with hardware-accelerated UI
- 📝 **YAML Configuration**: Modern, human-readable configuration format
- 🎨 **Modern UI**: Avalonia-based interface with blur effects
- 🔄 **Auto-completion**: Tab-based command completion
- 🪟 **Windows 10/11**: Native Windows integration

## Quick Start

### Using Pre-built Binary

1. Download the latest release
2. Extract `TeaLauncher.exe` and `commands.yaml`
3. Edit `commands.yaml` to add your commands
4. Run `TeaLauncher.exe`
5. Press `Ctrl+Space` to activate

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

## Credits

Original version by Toshiyuki Hirooka <toshi.hirooka@gmail.com>

v2.0 Avalonia migration - Complete UI modernization and YAML configuration
