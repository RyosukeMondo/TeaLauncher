# Product Overview

## Product Purpose
TeaLauncher is a lightweight, keyboard-driven command launcher for Windows that provides instant access to URLs, files, and custom commands through a simple hotkey interface. It solves the problem of slow navigation and context switching by enabling users to launch resources immediately without leaving their current workflow.

## Target Users
Primary users are Windows power users and developers who:
- Need rapid access to frequently-used URLs, files, and applications
- Prefer keyboard-driven workflows over mouse navigation
- Want to minimize disruption when switching between tasks
- Value productivity tools that are simple, fast, and unobtrusive

Pain points addressed:
- Time wasted navigating through menus and file explorers
- Context switching overhead when opening browsers or file managers
- Need for quick access to custom commands and scripts
- Repetitive tasks that could be automated with simple shortcuts

## Key Features

1. **Global Hotkey Activation**: Press Ctrl+Space from anywhere to invoke the launcher input window
2. **Smart Command System**: Register custom commands mapped to URLs, file paths, or executables via simple configuration files
3. **Auto-completion**: Tab-based command completion for faster input and discovery of available commands
4. **Special Commands**: Built-in system commands (!version, !reload, !exit) for application management
5. **Flexible Execution**: Support for direct URLs (http://, https://, ftp://) and Windows file paths (X:\)
6. **Argument Passing**: Pass additional arguments to registered commands at runtime
7. **Live Configuration Reload**: Update commands without restarting the application

## Business Objectives
- Provide a free, open-source productivity tool for the Windows community
- Minimize user friction with zero-configuration defaults (conf/my.conf)
- Maintain a small footprint with minimal dependencies (.NET Framework 3.5+)
- Foster community adoption through GNU GPL v2 licensing

## Success Metrics
- **Startup Time**: < 500ms application launch time
- **Response Time**: < 100ms from hotkey press to window display
- **Memory Footprint**: < 20MB RAM usage during idle
- **User Adoption**: GitHub stars and fork activity
- **Reliability**: Zero crashes during normal operation

## Product Principles

1. **Simplicity First**: Single-purpose tool with minimal UI - show input, execute command, hide interface
2. **Keyboard-Centric**: All interactions optimized for keyboard use; mouse interaction is secondary
3. **Unobtrusive**: Runs silently in the background until needed; no persistent UI elements
4. **Configuration as Code**: Human-readable .conf files that users can version control and share
5. **Fast Feedback**: Immediate visual feedback for auto-completion and command execution

## Monitoring & Visibility (if applicable)
- **Dashboard Type**: Not applicable - command-line oriented tool
- **Real-time Updates**: Not applicable
- **Key Metrics Displayed**: Error messages shown via MessageBox dialog when commands fail
- **Sharing Capabilities**: Configuration files (.conf) can be shared and version controlled

## Future Vision
TeaLauncher aims to remain a focused, lightweight launcher while potentially exploring:

### Potential Enhancements
- **Cross-Platform Support**: .NET Core port for Linux and macOS compatibility
- **Plugin Architecture**: Extensibility through dynamic command providers
- **Fuzzy Matching**: Improve auto-completion with fuzzy search algorithms
- **Command History**: Recently-used commands for faster repeat execution
- **Theming**: Customizable UI appearance for better visual integration
- **Command Aliases**: Multiple names for the same command
- **Variable Substitution**: Environment variable and placeholder support in commands
- **Analytics**: Optional usage statistics to understand command patterns (privacy-preserving)
- **Collaboration**: Community repository for sharing useful command configurations
