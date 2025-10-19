= TeaLauncher Build and Deployment Guide =

This document provides comprehensive instructions for building, testing, and
deploying TeaLauncher with .NET 8.


== Quick Start ==

For Linux developers (cross-compiling to Windows):

1. Initial setup:
   ./scripts/linux/setup.sh

2. Build:
   ./scripts/linux/build-windows.sh -c Release

3. Create release package:
   ./scripts/linux/publish-windows.sh -t self-contained

For detailed instructions, see sections below.


== Prerequisites ==

=== .NET 8 SDK ===

Required for building the application.

Linux installation:
  wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
  chmod +x ./dotnet-install.sh
  ./dotnet-install.sh --channel 8.0

  Add to PATH (add to ~/.bashrc or ~/.profile):
  export PATH="$HOME/.dotnet:$PATH"
  export DOTNET_ROOT="$HOME/.dotnet"

Windows installation:
  Download from: https://dotnet.microsoft.com/download/dotnet/8.0
  Run installer: dotnet-sdk-8.0.x-win-x64.exe
  Verify installation: dotnet --version

=== .NET 8 Desktop Runtime (Windows only) ===

Required for running framework-dependent builds on Windows.

Windows installation:
  Download from: https://dotnet.microsoft.com/download/dotnet/8.0
  Select "Desktop Runtime" installer
  Run: windowsdesktop-runtime-8.0.x-win-x64.exe


== Linux Development Workflow ==

=== Initial Setup ===

==== Automated Setup (Recommended) ====

Use the provided setup script to automatically install .NET 8 SDK and dependencies:

  ./scripts/linux/setup.sh

This script will:
  - Detect your Linux distribution
  - Install required system dependencies (with sudo)
  - Install .NET 8 SDK to ~/.dotnet (no sudo)
  - Configure PATH in ~/.bashrc
  - Restore project dependencies

After running, either restart your terminal or run:
  source ~/.bashrc

==== Manual Setup ====

1. Clone repository:
   git clone https://github.com/YOUR_USERNAME/TeaLauncher.git
   cd TeaLauncher

2. Verify .NET 8 SDK:
   dotnet --version
   # Should show 8.0.x

3. Restore dependencies:
   dotnet restore

=== Building ===

==== Using Build Scripts (Recommended) ====

Quick build for Windows:
  ./scripts/linux/build-windows.sh

Build release version:
  ./scripts/linux/build-windows.sh -c Release

Clean release build:
  ./scripts/linux/build-windows.sh -c Release --clean

Build for Windows ARM64:
  ./scripts/linux/build-windows.sh -r win-arm64

Show all options:
  ./scripts/linux/build-windows.sh --help

==== Manual Build Commands ====

Build for Windows x64 (cross-compile from Linux):
  dotnet build -r win-x64

Build output location:
  CommandLauncher/bin/Debug/net8.0-windows/win-x64/CommandLauncher.exe

Build in Release mode:
  dotnet build -c Release -r win-x64

Clean build:
  dotnet clean
  dotnet build -r win-x64

=== Testing ===

Run all unit tests:
  dotnet test

Run tests with verbose output:
  dotnet test -v detailed

Note: UI tests and P/Invoke tests will be skipped on Linux as they require
Windows Forms and Windows API.

=== Publishing ===

==== Using Publish Script (Recommended) ====

Framework-dependent build (small, requires .NET runtime):
  ./scripts/linux/publish-windows.sh

Self-contained build (large, no runtime required):
  ./scripts/linux/publish-windows.sh -t self-contained

Build for Windows ARM64:
  ./scripts/linux/publish-windows.sh -r win-arm64

The script will:
  - Build the release version
  - Show output location
  - List files to distribute
  - Optionally create a zip package

==== Manual Publish Commands ====

See "Deployment Options" section below for detailed publish commands.


== Windows Testing Workflow ==

=== Transfer Files from Linux ===

1. Build on Linux:
   dotnet build -c Release -r win-x64

2. Transfer to Windows:
   - Copy entire bin/Release/net8.0-windows/win-x64/ directory
   - Or use scp, rsync, USB drive, network share

   Example with scp:
   scp -r CommandLauncher/bin/Release/net8.0-windows/win-x64/ user@windows-pc:C:/TeaLauncher/

3. Copy configuration:
   - Also transfer resource/conf/my.conf to Windows

=== Running Tests on Windows ===

1. Ensure .NET 8 Desktop Runtime is installed
   (not needed for self-contained builds)

2. Open Command Prompt or PowerShell

3. Navigate to executable directory:
   cd C:\TeaLauncher\win-x64

4. Run application:
   CommandLauncher.exe resource\conf\my.conf

5. Test functionality:
   - Verify system tray icon appears
   - Press Ctrl+Space to show input window
   - Type partial command and press Tab for auto-completion
   - Press Enter to execute command
   - Press Escape to hide window
   - Test special commands: !version, !reload, !exit

=== Performance Testing ===

Measure startup time:
  PowerShell: Measure-Command { .\CommandLauncher.exe resource\conf\my.conf }
  Target: ≤ 300ms

Measure memory usage:
  1. Launch application
  2. Wait 60 seconds with no activity
  3. Open Task Manager
  4. Find CommandLauncher.exe process
  5. Check "Memory (Private Working Set)"
  Target: ≤ 20MB

Measure hotkey response:
  - Use stopwatch to measure Ctrl+Space to window visible
  Target: < 100ms


== Deployment Options ==

TeaLauncher supports two deployment models with different tradeoffs.

=== Framework-Dependent Deployment ===

Characteristics:
  - Small size (~1-5 MB)
  - Requires .NET 8 Desktop Runtime on target Windows machine
  - Fast startup
  - Shared runtime across applications
  - Recommended for most users

Build command:
  dotnet publish -c Release -r win-x64 -f net8.0-windows CommandLauncher/CommandLauncher.csproj

Output location:
  CommandLauncher/bin/Release/net8.0-windows/win-x64/publish/

Files to distribute:
  - CommandLauncher.exe
  - CommandLauncher.dll
  - CommandLauncher.deps.json
  - CommandLauncher.runtimeconfig.json
  - resource/conf/my.conf (configuration)

User requirements:
  - Windows 10 version 1607+ or Windows 11
  - .NET 8 Desktop Runtime installed

Installation instructions for users:
  1. Install .NET 8 Desktop Runtime from:
     https://dotnet.microsoft.com/download/dotnet/8.0
  2. Extract TeaLauncher files to a directory (e.g., C:\TeaLauncher)
  3. Run CommandLauncher.exe with configuration file

=== Self-Contained Deployment ===

Characteristics:
  - Large size (~60-80 MB)
  - No runtime installation required
  - Single executable file
  - Complete isolation from other .NET applications
  - Recommended for portable/USB deployment

Build command:
  dotnet publish -c Release -r win-x64 -f net8.0-windows --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true CommandLauncher/CommandLauncher.csproj

Output location:
  CommandLauncher/bin/Release/net8.0-windows/win-x64/publish/

Files to distribute:
  - CommandLauncher.exe (single file, includes runtime)
  - resource/conf/my.conf (configuration)

User requirements:
  - Windows 10 version 1607+ or Windows 11
  - No .NET installation required

Installation instructions for users:
  1. Extract TeaLauncher files to a directory (e.g., C:\TeaLauncher)
  2. Run CommandLauncher.exe with configuration file
  3. No additional software installation needed

=== Deployment Comparison Table ===

Feature                    | Framework-Dependent | Self-Contained
---------------------------|---------------------|------------------
Executable Size            | ~1-5 MB             | ~60-80 MB
Runtime Required           | Yes (.NET 8)        | No
Single File                | No (4+ files)       | Yes (1 .exe)
Startup Time               | Fast                | Fast
Update .NET Runtime        | Separate update     | Rebuild app
Recommended For            | Standard deployment | Portable/USB use
Build Command Complexity   | Simple              | More flags needed


== Common Build Tasks ==

=== Clean Build ===

Remove all build artifacts:
  dotnet clean
  rm -rf CommandLauncher/bin CommandLauncher/obj
  rm -rf TestCommandLauncher/bin TestCommandLauncher/obj

Full rebuild:
  dotnet clean
  dotnet restore
  dotnet build -r win-x64

=== Build Both Projects Separately ===

Build main application:
  dotnet build CommandLauncher/CommandLauncher.csproj -r win-x64

Build test project:
  dotnet build TestCommandLauncher/TestCommandLauncher.csproj

=== Run Specific Test ===

Run specific test class:
  dotnet test --filter "FullyQualifiedName~Test_ConfigLoader"

Run specific test method:
  dotnet test --filter "FullyQualifiedName~Test_ConfigLoader.Test_Default"

=== Build for ARM64 Windows ===

Build for ARM64:
  dotnet build -r win-arm64

Note: ARM64 support is configured in .csproj but not extensively tested.


== Troubleshooting ==

=== Build Errors ===

"SDK not found":
  - Verify .NET 8 SDK installed: dotnet --version
  - Check PATH includes .NET SDK location
  - Restart terminal after installation

"Project targets .NET Framework":
  - Ensure .csproj uses <TargetFramework>net8.0-windows</TargetFramework>
  - Verify using SDK-style project format (Sdk="Microsoft.NET.Sdk")

"Windows Forms not found":
  - Check .csproj has <UseWindowsForms>true</UseWindowsForms>
  - Verify targeting net8.0-windows (not net8.0)

=== Runtime Errors on Windows ===

"Application failed to start":
  - Framework-dependent: Install .NET 8 Desktop Runtime
  - Self-contained: Verify Windows 10 version 1607+ or Windows 11
  - Check configuration file path is correct

"Hotkey not working":
  - Another application may be using Ctrl+Space
  - Try changing hotkey in configuration
  - Run as administrator if needed (usually not required)

"Commands not executing":
  - Verify my.conf syntax: [section] and linkto=value
  - Check file paths use Windows format (backslashes or forward slashes)
  - Test with !version command to verify app is responding

=== Test Failures ===

Tests fail on Linux:
  - UI tests are expected to fail/skip on Linux
  - Focus on ConfigLoader and AutoCompleteMachine tests passing

Tests fail on Windows:
  - Check .NET 8 Desktop Runtime installed
  - Verify test project references correct NUnit version (4.2.2+)
  - Run with verbose output: dotnet test -v detailed


== Development Best Practices ==

=== Build on Linux ===

1. Use Release configuration for final builds:
   dotnet build -c Release -r win-x64

2. Run tests before publishing:
   dotnet test

3. Verify no warnings (TreatWarningsAsErrors=true):
   dotnet build -r win-x64 -warnaserror

=== Test on Windows ===

1. Test both deployment types:
   - Framework-dependent build
   - Self-contained single-file build

2. Test on multiple Windows versions:
   - Windows 10 version 1607+
   - Windows 11

3. Verify performance targets:
   - Startup ≤ 300ms
   - Hotkey response < 100ms
   - Memory ≤ 20MB idle

=== Version Updates ===

When updating version:
1. Edit CommandLauncher/CommandLauncher.csproj:
   <Version>1.0.X</Version>
   <FileVersion>1.0.X.0</FileVersion>
   <AssemblyVersion>1.0.X.0</AssemblyVersion>

2. Rebuild:
   dotnet clean
   dotnet build -c Release -r win-x64

3. Verify version:
   Run application, execute !version command


== Next Steps ==

After successful .NET 8 migration:

1. Cross-platform support:
   - Linux native version (no Wine required)
   - macOS support
   - X11/Wayland hotkey integration

2. Modern features:
   - JSON configuration format (keep INI for compatibility)
   - Command history and favorites
   - Plugin system for custom commands

3. CI/CD integration:
   - GitHub Actions for automated builds
   - Automated testing on Windows VMs
   - Release artifact publishing

For cross-platform support, see future spec: migrate-to-cross-platform
