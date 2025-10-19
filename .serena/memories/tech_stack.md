# TeaLauncher Technology Stack

## Core Framework
- **.NET 8** (`net8.0-windows` target framework)
- **C# 12** with nullable reference types enabled
- **Windows Runtime Identifier**: `win-x64`

## UI Framework
- **Avalonia 11.2.2**: Cross-platform MVVM UI framework
- **Avalonia.Desktop**: Desktop platform support
- **Avalonia.Themes.Fluent**: Modern fluent design theme
- **Avalonia.Fonts.Inter**: Inter font family
- **Avalonia.Headless**: Headless mode for E2E testing

## Dependencies
- **YamlDotNet 16.3.0**: YAML configuration parsing
- **Microsoft.Extensions.DependencyInjection 8.0.1**: Dependency injection container

## Testing Framework
- **NUnit 4.x**: Testing framework
- **NSubstitute 5.1.0**: Mocking framework
- **FluentAssertions 6.12.0**: Fluent assertion library
- **Avalonia.Headless.NUnit**: Integration for headless E2E tests
- **Coverlet**: Code coverage collection (XPlat Code Coverage)

## Quality Tools
- **Husky.Net**: Git hooks for pre-commit verification
- **MetricsChecker**: Custom Roslyn-based code metrics analyzer
- **dotnet format**: Code formatting verification
- **reportgenerator**: HTML coverage report generation

## Build & CI/CD
- **GitHub Actions**: CI/CD pipeline
- **Multi-platform builds**: Linux and Windows
- **Cross-compilation**: Build Windows binaries from Linux

## Code Analysis
- **Microsoft.CodeAnalysis.CSharp**: Roslyn compiler API for metrics checking
- **TreatWarningsAsErrors**: Enabled for strict compilation
