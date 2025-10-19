# TeaLauncher Project Overview

## Purpose
TeaLauncher is a modern Windows command launcher built with Avalonia UI and .NET 8. It provides a fast, keyboard-driven interface for launching applications, opening URLs, and executing commands from anywhere in the Windows environment.

## Key Features
- **Global Hotkey**: Activate with `Ctrl+Space` from anywhere
- **Fast Performance**: Sub-100ms response time with hardware-accelerated UI
- **YAML Configuration**: Human-readable configuration format for commands
- **Modern UI**: Avalonia-based interface with blur effects
- **Auto-completion**: Tab-based command completion
- **Windows Integration**: Native Windows 10/11 integration

## Architecture
The project follows Clean Architecture principles with:
- **Domain Layer**: Interfaces and models defining business contracts
- **Application Layer**: Services implementing business logic
- **Infrastructure Layer**: Platform-specific implementations (Windows hotkeys, IME, YAML configuration)
- **Presentation Layer**: Avalonia UI (MainWindow, Views)

## Version
Current version: 2.0.0 - Complete rewrite with modern UI framework

## Target Platform
- Windows 10/11 (x64)
- Developed on Linux using cross-compilation
- Tests run on both Linux and Windows via CI/CD
