# Changelog

All notable changes to TeaLauncher will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2025-10-21

### Added
- **First-time initialization mode**: New welcome window appears on first run
  - Detects if config file exists in current directory
  - Generates sample `commands.yaml` configuration file automatically
  - User-friendly setup wizard with clear instructions
  - Visual display of config file location
- **Keyboard shortcut selection**: Users can now choose their preferred hotkey
  - Option to select between Ctrl+Space or Alt+Space
  - Preference saved to `.tealauncher-settings.json`
  - Setting persisted across application restarts
- **Settings persistence**: New settings service for user preferences
  - JSON-based configuration storage
  - Automatic loading on application startup
- **Improved user experience**:
  - Clear explanation of background operation
  - Instructions on how to activate the launcher
  - Sample commands in generated config file

### Changed
- Application startup flow now includes initialization check
- Hotkey registration now uses saved user preference instead of hardcoded value
- Main window initialization deferred until after setup completes

### Fixed
- User confusion about "nothing happens" when running the executable
  - First-run window now explains the background operation model
  - Clear instructions on how to activate with keyboard shortcut

## [2.0.0] - 2025-10-21

### Added
- Complete rewrite with modern Avalonia UI framework
- .NET 8 support with Windows-specific optimizations
- Clean Architecture implementation with Domain, Application, Infrastructure layers
- Dependency Injection for better testability and maintainability
- Comprehensive test suite (Unit, Integration, E2E tests)
- YAML-based configuration with YamlDotNet
- Global hotkey support (Ctrl+Space) for activation
- IME control for Japanese/Asian language input
- Auto-completion with Tab key
- Special commands: `!reload`, `!version`, `!exit`
- Blur effects and modern Windows 11-style UI
- Single-file deployment support
- CI/CD with GitHub Actions
- Code quality gates (coverage, metrics, formatting)
- Pre-commit hooks with Husky.Net

### Changed
- Migration from Windows Forms to Avalonia UI
- Configuration format changed to YAML
- Improved performance with hardware-accelerated rendering
- Better keyboard handling and IME support

### Technical Details
- Target Framework: .NET 8 (net8.0-windows)
- UI Framework: Avalonia 11.2.2
- Configuration: YamlDotNet 16.3.0
- Testing: NUnit 4.x with NSubstitute and FluentAssertions
- Architecture: Clean Architecture with DI
- Deployment: Single-file, self-contained executable

[2.1.0]: https://github.com/yourusername/TeaLauncher/compare/v2.0.0...v2.1.0
[2.0.0]: https://github.com/yourusername/TeaLauncher/releases/tag/v2.0.0
