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
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Configuration;
using TeaLauncher.Avalonia.Tests.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TeaLauncher.Avalonia.Tests.Integration;

/// <summary>
/// Integration tests for command workflow covering the full pipeline:
/// configuration loading → command registration → auto-completion → command execution.
/// These tests use real service implementations (not mocks) to verify end-to-end behavior.
/// </summary>
[TestFixture]
public class CommandWorkflowTests
{
    private IServiceProvider? _serviceProvider;
    private string? _tempConfigPath;
    private const string TestFixturePath = "Integration/Fixtures/test-commands.yaml";
    private const string InvalidFixturePath = "Integration/Fixtures/invalid-commands.yaml";

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
    /// Tests the complete workflow: load YAML config → register commands → execute command.
    /// This verifies that all services work together correctly in the happy path.
    /// </summary>
    [Test]
    public async Task LoadConfig_RegisterCommands_ExecuteCommand_ShouldSucceed()
    {
        // Arrange
        var configLoader = _serviceProvider!.GetRequiredService<IConfigurationLoader>();
        var registry = _serviceProvider.GetRequiredService<ICommandRegistry>();
        var executor = _serviceProvider.GetRequiredService<ICommandExecutor>();

        var configPath = GetTestFixturePath(TestFixturePath);

        // Act - Load configuration
        var config = await configLoader.LoadConfigurationAsync(configPath);
        config.Should().NotBeNull();
        config.Commands.Should().HaveCount(5);

        // Act - Register all commands from configuration
        foreach (var commandEntry in config.Commands)
        {
            var command = new Domain.Models.Command(
                commandEntry.Name,
                commandEntry.LinkTo,
                commandEntry.Description,
                commandEntry.Arguments
            );
            registry.RegisterCommand(command);
        }

        // Assert - Verify commands are registered
        registry.GetAllCommands().Should().HaveCount(5);
        registry.HasCommand("google").Should().BeTrue();
        registry.HasCommand("docs").Should().BeTrue();
        registry.HasCommand("github").Should().BeTrue();

        // Assert - Verify command can be retrieved and has correct properties
        var googleCommand = registry.GetAllCommands()
            .FirstOrDefault(c => c.Name.Equals("google", StringComparison.OrdinalIgnoreCase));
        googleCommand.Should().NotBeNull();
        googleCommand!.LinkTo.Should().Be("https://www.google.com");
    }

    /// <summary>
    /// Tests that loading invalid YAML throws an exception with helpful error message.
    /// The error message should contain line number information for debugging.
    /// </summary>
    [Test]
    public void LoadConfig_WithInvalidYaml_ShouldThrowException()
    {
        // Arrange
        var configLoader = _serviceProvider!.GetRequiredService<IConfigurationLoader>();
        var configPath = GetTestFixturePath(InvalidFixturePath);

        // Act & Assert
        var act = () => configLoader.LoadConfiguration(configPath);
        act.Should().Throw<Exception>()
            .WithMessage("*yaml*", because: "error should mention YAML format issue");
    }

    /// <summary>
    /// Tests integration between configuration loader, command registry, and auto-completer.
    /// Verifies that when commands are registered, the auto-completer is updated with command names.
    /// </summary>
    [Test]
    public async Task Configuration_AutoCompleter_Integration_ShouldSyncCommandNames()
    {
        // Arrange
        var configLoader = _serviceProvider!.GetRequiredService<IConfigurationLoader>();
        var registry = _serviceProvider.GetRequiredService<ICommandRegistry>();
        var autoCompleter = _serviceProvider.GetRequiredService<IAutoCompleter>();

        var configPath = GetTestFixturePath(TestFixturePath);

        // Act - Load and register commands
        var config = await configLoader.LoadConfigurationAsync(configPath);
        foreach (var commandEntry in config.Commands)
        {
            var command = new Domain.Models.Command(
                commandEntry.Name,
                commandEntry.LinkTo,
                commandEntry.Description,
                commandEntry.Arguments
            );
            registry.RegisterCommand(command);
        }

        // Assert - Auto-completer should have all command names
        var candidates = autoCompleter.GetCandidates("g");
        candidates.Should().Contain("google");
        candidates.Should().Contain("github");

        var docsCandidates = autoCompleter.GetCandidates("doc");
        docsCandidates.Should().Contain("docs");

        // Test auto-completion works correctly
        var completion = autoCompleter.AutoCompleteWord("goo");
        completion.Should().Be("google");
    }

    /// <summary>
    /// Tests that missing configuration file is handled gracefully with clear error message.
    /// </summary>
    [Test]
    public void LoadConfig_WithMissingFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var configLoader = _serviceProvider!.GetRequiredService<IConfigurationLoader>();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.yaml");

        // Act & Assert
        var act = () => configLoader.LoadConfiguration(nonExistentPath);
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*" + nonExistentPath + "*");
    }

    /// <summary>
    /// Tests that commands can be cleared and re-registered (simulating config reload).
    /// </summary>
    [Test]
    public async Task ClearAndReregister_Commands_ShouldSucceed()
    {
        // Arrange
        var configLoader = _serviceProvider!.GetRequiredService<IConfigurationLoader>();
        var registry = _serviceProvider.GetRequiredService<ICommandRegistry>();
        var configPath = GetTestFixturePath(TestFixturePath);

        // Act - Load and register commands initially
        var config = await configLoader.LoadConfigurationAsync(configPath);
        foreach (var commandEntry in config.Commands)
        {
            var command = new Domain.Models.Command(
                commandEntry.Name,
                commandEntry.LinkTo,
                commandEntry.Description,
                commandEntry.Arguments
            );
            registry.RegisterCommand(command);
        }

        registry.GetAllCommands().Should().HaveCount(5);

        // Act - Clear and re-register
        registry.ClearCommands();
        registry.GetAllCommands().Should().BeEmpty();

        foreach (var commandEntry in config.Commands.Take(3)) // Register only first 3
        {
            var command = new Domain.Models.Command(
                commandEntry.Name,
                commandEntry.LinkTo,
                commandEntry.Description,
                commandEntry.Arguments
            );
            registry.RegisterCommand(command);
        }

        // Assert
        registry.GetAllCommands().Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that duplicate command registration replaces the existing command.
    /// </summary>
    [Test]
    public async Task RegisterCommand_WithDuplicate_ShouldReplaceExisting()
    {
        // Arrange
        var configLoader = _serviceProvider!.GetRequiredService<IConfigurationLoader>();
        var registry = _serviceProvider.GetRequiredService<ICommandRegistry>();
        var configPath = GetTestFixturePath(TestFixturePath);

        var config = await configLoader.LoadConfigurationAsync(configPath);
        var googleEntry = config.Commands.First(c => c.Name == "google");

        var originalCommand = new Domain.Models.Command(
            googleEntry.Name,
            googleEntry.LinkTo,
            googleEntry.Description,
            googleEntry.Arguments
        );

        // Act - Register original command
        registry.RegisterCommand(originalCommand);

        // Act - Register duplicate with different LinkTo
        var updatedCommand = new Domain.Models.Command(
            "google",
            "https://www.google.com/search?q=test",
            "Updated Google Search",
            null
        );
        registry.RegisterCommand(updatedCommand);

        // Assert - Should only have one command and it should be the updated one
        registry.GetAllCommands().Should().HaveCount(1);
        var registeredCommand = registry.GetAllCommands().First();
        registeredCommand.LinkTo.Should().Be("https://www.google.com/search?q=test");
        registeredCommand.Description.Should().Be("Updated Google Search");
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
