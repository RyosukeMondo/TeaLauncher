/*
 * TeaLauncher. Simple command launcher.
 * Copyright (C) Toshiyuki Hirooka <toshi.hirooka@gmail.com> http://wasabi.in/
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TeaLauncher.Avalonia.Application.Orchestration;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Tests.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TeaLauncher.Avalonia.Tests.Integration;

/// <summary>
/// Integration tests for configuration loading, reloading, and application orchestration.
/// Tests the complete initialization and configuration management workflows.
/// </summary>
[TestFixture]
public class ConfigurationIntegrationTests
{
    private IServiceProvider? _serviceProvider;
    private string? _tempConfigPath;
    private const string TestFixturePath = "Integration/Fixtures/test-commands.yaml";

    [SetUp]
    public void SetUp()
    {
        // Create service provider with real services for integration testing
        _serviceProvider = TestServiceProvider.CreateWithRealServices();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up temporary files
        if (_tempConfigPath != null && File.Exists(_tempConfigPath))
        {
            File.Delete(_tempConfigPath);
            _tempConfigPath = null;
        }

        // Dispose service provider
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Tests the complete initialization → modification → reload workflow.
    /// Verifies that configuration changes are properly applied when reloading.
    /// </summary>
    [Test]
    public async Task LoadAndReload_ShouldUpdateCommands()
    {
        // Arrange - Create temporary config file
        var initialYaml = @"commands:
  - name: google
    linkto: https://www.google.com
    description: Search Google
  - name: docs
    linkto: https://docs.microsoft.com
    description: Microsoft Docs
";
        _tempConfigPath = TestFixtures.CreateTempYamlFile(initialYaml);

        var orchestrator = _serviceProvider!.GetRequiredService<ApplicationOrchestrator>();
        var registry = _serviceProvider.GetRequiredService<ICommandRegistry>();

        // Act - Initialize with original config
        await orchestrator.InitializeAsync(_tempConfigPath);

        // Assert - Verify initial commands loaded
        registry.GetAllCommands().Should().HaveCount(2);
        registry.HasCommand("google").Should().BeTrue();
        registry.HasCommand("docs").Should().BeTrue();
        registry.HasCommand("github").Should().BeFalse();

        // Act - Update config file with new commands
        var updatedYaml = @"commands:
  - name: google
    linkto: https://www.google.com
    description: Search Google
  - name: github
    linkto: https://github.com
    description: GitHub
  - name: notepad
    linkto: notepad.exe
    description: Notepad
";
        File.WriteAllText(_tempConfigPath, updatedYaml);

        // Act - Reload configuration
        await orchestrator.ReloadConfigurationAsync();

        // Assert - Verify updated commands loaded
        registry.GetAllCommands().Should().HaveCount(3);
        registry.HasCommand("google").Should().BeTrue();
        registry.HasCommand("github").Should().BeTrue();
        registry.HasCommand("notepad").Should().BeTrue();
        registry.HasCommand("docs").Should().BeFalse(); // This command was removed
    }

    /// <summary>
    /// Tests that missing configuration file provides a helpful error message.
    /// </summary>
    [Test]
    public void MissingFile_ShouldProvideHelpfulError()
    {
        // Arrange
        var orchestrator = _serviceProvider!.GetRequiredService<ApplicationOrchestrator>();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.yaml");

        // Act & Assert
        Func<Task> act = async () => await orchestrator.InitializeAsync(nonExistentPath);

        act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*" + nonExistentPath + "*");
    }

    /// <summary>
    /// Tests that invalid YAML during initialization throws appropriate exception.
    /// </summary>
    [Test]
    public void Initialize_WithInvalidYaml_ShouldThrowException()
    {
        // Arrange
        var invalidYaml = @"commands:
  - name: test
    this is invalid
    linkto: https://example.com
";
        _tempConfigPath = TestFixtures.CreateTempYamlFile(invalidYaml);

        var orchestrator = _serviceProvider!.GetRequiredService<ApplicationOrchestrator>();

        // Act & Assert
        Func<Task> act = async () => await orchestrator.InitializeAsync(_tempConfigPath);

        act.Should().ThrowAsync<Exception>()
            .WithMessage("*yaml*");
    }

    /// <summary>
    /// Tests that reload handles file I/O errors gracefully.
    /// </summary>
    [Test]
    public async Task Reload_AfterFileDeleted_ShouldThrowException()
    {
        // Arrange - Create and initialize with temp config
        var initialYaml = @"commands:
  - name: test
    linkto: https://example.com
";
        _tempConfigPath = TestFixtures.CreateTempYamlFile(initialYaml);

        var orchestrator = _serviceProvider!.GetRequiredService<ApplicationOrchestrator>();

        await orchestrator.InitializeAsync(_tempConfigPath);

        // Act - Delete config file
        File.Delete(_tempConfigPath);
        var deletedPath = _tempConfigPath;
        _tempConfigPath = null; // Prevent cleanup attempt

        // Act & Assert - Reload should fail
        Func<Task> act = async () => await orchestrator.ReloadConfigurationAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*" + deletedPath + "*");
    }

    /// <summary>
    /// Tests that empty configuration file is handled correctly.
    /// </summary>
    [Test]
    public async Task Initialize_WithEmptyConfig_ShouldLoadZeroCommands()
    {
        // Arrange
        var emptyYaml = @"commands: []
";
        _tempConfigPath = TestFixtures.CreateTempYamlFile(emptyYaml);

        var orchestrator = _serviceProvider!.GetRequiredService<ApplicationOrchestrator>();
        var registry = _serviceProvider.GetRequiredService<ICommandRegistry>();

        // Act
        await orchestrator.InitializeAsync(_tempConfigPath);

        // Assert
        registry.GetAllCommands().Should().BeEmpty();
    }

    /// <summary>
    /// Tests special command handling through the orchestrator.
    /// </summary>
    [Test]
    public async Task HandleSpecialCommand_Reload_ShouldReloadConfiguration()
    {
        // Arrange - Initialize with config
        _tempConfigPath = TestFixtures.CreateTempYamlFile(@"commands:
  - name: test
    linkto: https://example.com
");

        var orchestrator = _serviceProvider!.GetRequiredService<ApplicationOrchestrator>();
        var registry = _serviceProvider.GetRequiredService<ICommandRegistry>();

        await orchestrator.InitializeAsync(_tempConfigPath);

        // Act - Update config and trigger reload via special command
        File.WriteAllText(_tempConfigPath, @"commands:
  - name: newcommand
    linkto: https://newexample.com
");

        var result = await orchestrator.HandleSpecialCommandAsync("!reload");

        // Assert
        result.Should().Contain("reload", because: "special command should acknowledge reload");
        registry.HasCommand("newcommand").Should().BeTrue();
        registry.HasCommand("test").Should().BeFalse();
    }

    /// <summary>
    /// Tests that version special command returns version information.
    /// </summary>
    [Test]
    public async Task HandleSpecialCommand_Version_ShouldReturnVersionInfo()
    {
        // Arrange
        var orchestrator = _serviceProvider!.GetRequiredService<ApplicationOrchestrator>();

        // Act
        var result = await orchestrator.HandleSpecialCommandAsync("!version");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().ContainAny("version", "Version", "TeaLauncher");
    }

    /// <summary>
    /// Tests that exit special command throws the expected exception.
    /// </summary>
    [Test]
    public void HandleSpecialCommand_Exit_ShouldThrowExitException()
    {
        // Arrange
        var orchestrator = _serviceProvider!.GetRequiredService<ApplicationOrchestrator>();

        // Act & Assert
        Func<Task> act = async () => await orchestrator.HandleSpecialCommandAsync("!exit");

        // The exit command should throw an exception to signal application exit
        act.Should().ThrowAsync<Exception>();
    }

    /// <summary>
    /// Helper method to get the full path to a test fixture file.
    /// </summary>
    private static string GetTestFixturePath(string relativePath)
    {
        var testProjectDir = TestContext.CurrentContext.TestDirectory;
        return Path.Combine(testProjectDir, relativePath);
    }
}
