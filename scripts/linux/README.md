# TeaLauncher Linux Build Scripts

Helper scripts for building TeaLauncher on Linux for Windows deployment.

## Setup Script

### setup.sh

Automatically installs .NET 8 SDK and configures your environment for cross-compilation.

**Usage:**
```bash
./scripts/linux/setup.sh
```

**What it does:**
- Detects your Linux distribution (Ubuntu, Debian, Fedora, Arch)
- Installs required system dependencies using your package manager (requires sudo)
- Downloads and installs .NET 8 SDK to `~/.dotnet` (no sudo required)
- Configures PATH in `~/.bashrc`
- Restores project dependencies

**After running:**
```bash
source ~/.bashrc  # Or restart your terminal
```

## Build Scripts

### build-windows.sh

Cross-compiles TeaLauncher for Windows from Linux.

**Usage:**
```bash
./scripts/linux/build-windows.sh [options]
```

**Options:**
- `-c, --config <config>` - Build configuration: Debug or Release (default: Debug)
- `-r, --runtime <runtime>` - Target runtime: win-x64 or win-arm64 (default: win-x64)
- `--clean` - Clean before building
- `-h, --help` - Show help message

**Examples:**
```bash
# Debug build for Windows x64
./scripts/linux/build-windows.sh

# Release build
./scripts/linux/build-windows.sh -c Release

# Clean release build
./scripts/linux/build-windows.sh -c Release --clean

# Build for Windows ARM64
./scripts/linux/build-windows.sh -r win-arm64
```

**Output location:**
```
CommandLauncher/bin/<config>/net8.0-windows/<runtime>/CommandLauncher.exe
```

### publish-windows.sh

Creates distributable packages for Windows deployment.

**Usage:**
```bash
./scripts/linux/publish-windows.sh [options]
```

**Options:**
- `-r, --runtime <runtime>` - Target runtime: win-x64 or win-arm64 (default: win-x64)
- `-t, --type <type>` - Deployment type:
  - `framework-dependent` (default, ~1-5MB, requires .NET runtime)
  - `self-contained` (large ~60-80MB, no runtime required)
- `-h, --help` - Show help message

**Examples:**
```bash
# Framework-dependent build (small, requires .NET 8 on target)
./scripts/linux/publish-windows.sh

# Self-contained build (large, standalone)
./scripts/linux/publish-windows.sh -t self-contained

# Framework-dependent for ARM64
./scripts/linux/publish-windows.sh -r win-arm64
```

**Output location:**
```
CommandLauncher/bin/Release/net8.0-windows/<runtime>/publish/
```

The script offers to create a zip package for easy distribution.

## Typical Workflow

1. **First time setup:**
   ```bash
   ./scripts/linux/setup.sh
   source ~/.bashrc
   ```

2. **Development builds:**
   ```bash
   ./scripts/linux/build-windows.sh
   ```

3. **Release for distribution:**
   ```bash
   ./scripts/linux/publish-windows.sh -t self-contained
   ```

4. **Transfer to Windows and test:**
   ```bash
   scp -r CommandLauncher/bin/Release/net8.0-windows/win-x64/publish/ user@windows-pc:C:/TeaLauncher/
   ```

## Requirements

- Linux distribution: Ubuntu, Debian, Fedora, Arch Linux (or compatible)
- Internet connection for downloading .NET SDK
- `wget` (usually pre-installed)
- `sudo` access for installing system dependencies
- ~2GB disk space for .NET SDK and build artifacts

## Troubleshooting

**Script permission denied:**
```bash
chmod +x ./scripts/linux/*.sh
```

**.NET command not found after setup:**
```bash
source ~/.bashrc
# Or restart your terminal
```

**Build fails with "SDK not found":**
```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
dotnet --version
```

For more detailed troubleshooting, see BUILD.md.
