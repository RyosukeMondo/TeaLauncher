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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using TeaLauncher.Avalonia.Application.Orchestration;
using TeaLauncher.Avalonia.Application.Exceptions;
using TeaLauncher.Avalonia.Configuration;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Domain.Models;

namespace TeaLauncher.Avalonia.Tests.Application.Orchestration;

/// <summary>
/// Unit tests for the ApplicationOrchestrator class.
/// Tests initialization, configuration reloading, and special command handling.
/// </summary>
[TestFixture]
public class ApplicationOrchestratorTests
{
    private IConfigurationLoader _mockConfigLoader;
    private ICommandRegistry _mockCommandRegistry;
    private ICommandExecutor _mockCommandExecutor;
    private IHotkeyManager _mockHotkeyManager;
    private ApplicationOrchestrator _orchestrator;

    [SetUp]
    public void SetUp()
    {
        _mockConfigLoader = Substitute.For<IConfigurationLoader>();
        _mockCommandRegistry = Substitute.For<ICommandRegistry>();
        _mockCommandExecutor = Substitute.For<ICommandExecutor>();
        _mockHotkeyManager = Substitute.For<IHotkeyManager>();

        _orchestrator = new ApplicationOrchestrator(
            _mockConfigLoader,
            _mockCommandRegistry,
            _mockCommandExecutor,
            _mockHotkeyManager);
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithNullConfigLoader_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var act = () => new ApplicationOrchestrator(
            null!,
            _mockCommandRegistry,
            _mockCommandExecutor,
            _mockHotkeyManager);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configurationLoader");
    }

    [Test]
    public void Constructor_WithNullCommandRegistry_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var act = () => new ApplicationOrchestrator(
            _mockConfigLoader,
            null!,
            _mockCommandExecutor,
            _mockHotkeyManager);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("commandRegistry");
    }

    [Test]
    public void Constructor_WithNullCommandExecutor_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var act = () => new ApplicationOrchestrator(
            _mockConfigLoader,
            _mockCommandRegistry,
            null!,
            _mockHotkeyManager);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("commandExecutor");
    }

    [Test]
    public void Constructor_WithNullHotkeyManager_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var act = () => new ApplicationOrchestrator(
            _mockConfigLoader,
            _mockCommandRegistry,
            _mockCommandExecutor,
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("hotkeyManager");
    }

    [Test]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Arrange, Act & Assert
        _orchestrator.Should().NotBeNull();
    }

    #endregion

    #region InitializeAsync Tests

    [Test]
    public async Task InitializeAsync_WithValidConfig_ShouldLoadAndRegisterCommands()
    {
        // Arrange
        var configPath = "test-config.yaml";
        var commandEntries = new List<CommandEntry>
        {
            new CommandEntry { Name = "google", LinkTo = "https://google.com", Description = "Search", Arguments = null },
            new CommandEntry { Name = "docs", LinkTo = "https://docs.com", Description = "Documentation", Arguments = null }
        };
        var config = new CommandsConfig
        {
            Commands = commandEntries
        };

        _mockConfigLoader.LoadConfigurationAsync(configPath)
            .Returns(Task.FromResult(config));

        // Act
        await _orchestrator.InitializeAsync(configPath);

        // Assert
        await _mockConfigLoader.Received(1).LoadConfigurationAsync(configPath);
        _mockCommandRegistry.Received(2).RegisterCommand(Arg.Any<Command>());
        _mockCommandRegistry.Received(1).RegisterCommand(
            Arg.Is<Command>(c => c.Name == "google" && c.LinkTo == "https://google.com"));
        _mockCommandRegistry.Received(1).RegisterCommand(
            Arg.Is<Command>(c => c.Name == "docs" && c.LinkTo == "https://docs.com"));
    }

    [Test]
    public async Task InitializeAsync_WithEmptyConfig_ShouldNotRegisterAnyCommands()
    {
        // Arrange
        var configPath = "test-config.yaml";
        var config = new CommandsConfig
        {
            Commands = new List<CommandEntry>()
        };

        _mockConfigLoader.LoadConfigurationAsync(configPath)
            .Returns(Task.FromResult(config));

        // Act
        await _orchestrator.InitializeAsync(configPath);

        // Assert
        _mockCommandRegistry.DidNotReceiveWithAnyArgs().RegisterCommand(default!);
    }

    [Test]
    public void InitializeAsync_WithNullConfigPath_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = async () => await _orchestrator.InitializeAsync(null!);

        act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("configPath");
    }

    [Test]
    public void InitializeAsync_WithEmptyConfigPath_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = async () => await _orchestrator.InitializeAsync("");

        act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("configPath");
    }

    [Test]
    public void InitializeAsync_WhenConfigLoadFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configPath = "invalid-config.yaml";
        _mockConfigLoader.LoadConfigurationAsync(configPath)
            .Returns<Task<CommandsConfig>>(x => throw new InvalidOperationException("Config file not found"));

        // Act
        var act = async () => await _orchestrator.InitializeAsync(configPath);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to initialize application from configuration file*");
    }

    #endregion

    #region ReloadConfigurationAsync Tests

    [Test]
    public async Task ReloadConfigurationAsync_AfterInitialize_ShouldClearAndReloadCommands()
    {
        // Arrange
        var configPath = "test-config.yaml";
        var initialCommandEntries = new List<CommandEntry>
        {
            new CommandEntry { Name = "google", LinkTo = "https://google.com", Description = "Search", Arguments = null }
        };
        var reloadedCommandEntries = new List<CommandEntry>
        {
            new CommandEntry { Name = "google", LinkTo = "https://google.com", Description = "Search", Arguments = null },
            new CommandEntry { Name = "github", LinkTo = "https://github.com", Description = "GitHub", Arguments = null }
        };

        var initialConfig = new CommandsConfig { Commands = initialCommandEntries };
        var reloadedConfig = new CommandsConfig { Commands = reloadedCommandEntries };

        _mockConfigLoader.LoadConfigurationAsync(configPath)
            .Returns(Task.FromResult(initialConfig), Task.FromResult(reloadedConfig));

        await _orchestrator.InitializeAsync(configPath);

        // Act
        await _orchestrator.ReloadConfigurationAsync();

        // Assert
        _mockCommandRegistry.Received(1).ClearCommands();
        _mockCommandRegistry.Received(2).RegisterCommand(Arg.Is<Command>(c => c.Name == "google")); // Once in init, once in reload
        _mockCommandRegistry.Received(1).RegisterCommand(Arg.Is<Command>(c => c.Name == "github")); // Only in reload
    }

    [Test]
    public void ReloadConfigurationAsync_BeforeInitialize_ShouldThrowInvalidOperationException()
    {
        // Arrange, Act & Assert
        var act = async () => await _orchestrator.ReloadConfigurationAsync();

        act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot reload configuration: no configuration file path is set*");
    }

    [Test]
    public async Task ReloadConfigurationAsync_WhenConfigLoadFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configPath = "test-config.yaml";
        var initialConfig = new CommandsConfig { Commands = new List<CommandEntry>() };

        _mockConfigLoader.LoadConfigurationAsync(configPath)
            .Returns(Task.FromResult(initialConfig));

        await _orchestrator.InitializeAsync(configPath);

        _mockConfigLoader.LoadConfigurationAsync(configPath)
            .Returns<Task<CommandsConfig>>(x => throw new InvalidOperationException("Config file corrupted"));

        // Act
        var act = async () => await _orchestrator.ReloadConfigurationAsync();

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to reload configuration from*");
    }

    #endregion

    #region HandleSpecialCommandAsync Tests

    [Test]
    public async Task HandleSpecialCommandAsync_WithReloadCommand_ShouldReloadConfiguration()
    {
        // Arrange
        var configPath = "test-config.yaml";
        var config = new CommandsConfig { Commands = new List<CommandEntry>() };

        _mockConfigLoader.LoadConfigurationAsync(configPath)
            .Returns(Task.FromResult(config));

        await _orchestrator.InitializeAsync(configPath);

        // Act
        var result = await _orchestrator.HandleSpecialCommandAsync("!reload");

        // Assert
        result.Should().Be("Configuration reloaded successfully");
        _mockCommandRegistry.Received(1).ClearCommands();
    }

    [Test]
    public async Task HandleSpecialCommandAsync_WithVersionCommand_ShouldReturnVersion()
    {
        // Arrange & Act
        var result = await _orchestrator.HandleSpecialCommandAsync("!version");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("TeaLauncher Version");
    }

    [Test]
    public void HandleSpecialCommandAsync_WithExitCommand_ShouldThrowExitApplicationException()
    {
        // Arrange, Act & Assert
        var act = async () => await _orchestrator.HandleSpecialCommandAsync("!exit");

        act.Should().ThrowAsync<ExitApplicationException>()
            .WithMessage("Application exit requested via !exit command");
    }

    [Test]
    public void HandleSpecialCommandAsync_WithUnknownCommand_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = async () => await _orchestrator.HandleSpecialCommandAsync("!unknown");

        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Unknown special command: '!unknown'*");
    }

    [Test]
    public void HandleSpecialCommandAsync_WithNullCommand_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = async () => await _orchestrator.HandleSpecialCommandAsync(null!);

        act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("command");
    }

    [Test]
    public void HandleSpecialCommandAsync_WithEmptyCommand_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = async () => await _orchestrator.HandleSpecialCommandAsync("");

        act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("command");
    }

    [Test]
    public void HandleSpecialCommandAsync_WithNonSpecialCommand_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var act = async () => await _orchestrator.HandleSpecialCommandAsync("regular-command");

        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Not a special command: 'regular-command'*");
    }

    [Test]
    public async Task HandleSpecialCommandAsync_WithUppercaseReload_ShouldWork()
    {
        // Arrange
        var configPath = "test-config.yaml";
        var config = new CommandsConfig { Commands = new List<CommandEntry>() };

        _mockConfigLoader.LoadConfigurationAsync(configPath)
            .Returns(Task.FromResult(config));

        await _orchestrator.InitializeAsync(configPath);

        // Act
        var result = await _orchestrator.HandleSpecialCommandAsync("!RELOAD");

        // Assert
        result.Should().Be("Configuration reloaded successfully");
    }

    #endregion

    #region GetVersion Tests

    [Test]
    public void GetVersion_ShouldReturnVersionString()
    {
        // Arrange & Act
        var version = _orchestrator.GetVersion();

        // Assert
        version.Should().NotBeNullOrEmpty();
        version.Should().Contain("TeaLauncher Version");
    }

    [Test]
    public void GetVersion_ShouldReturnConsistentValue()
    {
        // Arrange
        var version1 = _orchestrator.GetVersion();
        var version2 = _orchestrator.GetVersion();

        // Act & Assert
        version1.Should().Be(version2);
    }

    #endregion
}
