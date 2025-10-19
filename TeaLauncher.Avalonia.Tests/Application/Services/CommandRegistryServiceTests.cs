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
using NSubstitute;
using FluentAssertions;
using TeaLauncher.Avalonia.Application.Services;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Domain.Models;
using TeaLauncher.Avalonia.Tests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaLauncher.Avalonia.Tests.Application.Services;

/// <summary>
/// Unit tests for CommandRegistryService.
/// Tests command registration, removal, lookup, and auto-completer synchronization.
/// </summary>
[TestFixture]
public class CommandRegistryServiceTests
{
    private IAutoCompleter _mockAutoCompleter = null!;
    private CommandRegistryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockAutoCompleter = Substitute.For<IAutoCompleter>();
        _service = new CommandRegistryService(_mockAutoCompleter);
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithNullAutoCompleter_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new CommandRegistryService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("autoCompleter");
    }

    [Test]
    public void Constructor_WithValidAutoCompleter_ShouldSucceed()
    {
        // Arrange & Act
        var service = new CommandRegistryService(_mockAutoCompleter);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region RegisterCommand Tests

    [Test]
    public void RegisterCommand_WithValidCommand_ShouldAddToRegistry()
    {
        // Arrange
        var command = new Command("google", "https://www.google.com");

        // Act
        _service.RegisterCommand(command);

        // Assert
        _service.HasCommand("google").Should().BeTrue();
        _service.GetAllCommands().Should().ContainSingle()
            .Which.Should().Be(command);
    }

    [Test]
    public void RegisterCommand_WithValidCommand_ShouldUpdateAutoCompleter()
    {
        // Arrange
        var command = new Command("google", "https://www.google.com");

        // Act
        _service.RegisterCommand(command);

        // Assert
        _mockAutoCompleter.Received(1).UpdateWordList(
            Arg.Is<IEnumerable<string>>(words => words.SequenceEqual(new[] { "google" })));
    }

    [Test]
    public void RegisterCommand_WithMultipleCommands_ShouldAddAllToRegistry()
    {
        // Arrange
        var command1 = new Command("google", "https://www.google.com");
        var command2 = new Command("github", "https://www.github.com");
        var command3 = new Command("notepad", "notepad.exe");

        // Act
        _service.RegisterCommand(command1);
        _service.RegisterCommand(command2);
        _service.RegisterCommand(command3);

        // Assert
        _service.GetAllCommands().Should().HaveCount(3);
        _service.HasCommand("google").Should().BeTrue();
        _service.HasCommand("github").Should().BeTrue();
        _service.HasCommand("notepad").Should().BeTrue();
    }

    [Test]
    public void RegisterCommand_WithDuplicateName_ShouldReplaceExistingCommand()
    {
        // Arrange
        var command1 = new Command("google", "https://www.google.com");
        var command2 = new Command("google", "https://www.google.co.uk");

        // Act
        _service.RegisterCommand(command1);
        _service.RegisterCommand(command2);

        // Assert
        _service.GetAllCommands().Should().ContainSingle();
        var registeredCommand = _service.GetAllCommands().First();
        registeredCommand.LinkTo.Should().Be("https://www.google.co.uk");
    }

    [Test]
    public void RegisterCommand_WithDuplicateNameDifferentCase_ShouldReplaceExistingCommand()
    {
        // Arrange
        var command1 = new Command("google", "https://www.google.com");
        var command2 = new Command("GOOGLE", "https://www.google.co.uk");

        // Act
        _service.RegisterCommand(command1);
        _service.RegisterCommand(command2);

        // Assert
        _service.GetAllCommands().Should().ContainSingle();
        var registeredCommand = _service.GetAllCommands().First();
        registeredCommand.LinkTo.Should().Be("https://www.google.co.uk");
    }

    [Test]
    public void RegisterCommand_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Action act = () => _service.RegisterCommand(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("command");
    }

    [Test]
    public void RegisterCommand_WithNullCommandName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new Command(null!, "https://www.google.com");

        // Act
        Action act = () => _service.RegisterCommand(command);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("command")
            .WithMessage("*Command name cannot be null or whitespace*");
    }

    [Test]
    public void RegisterCommand_WithEmptyCommandName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new Command("", "https://www.google.com");

        // Act
        Action act = () => _service.RegisterCommand(command);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("command")
            .WithMessage("*Command name cannot be null or whitespace*");
    }

    [Test]
    public void RegisterCommand_WithWhitespaceCommandName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new Command("   ", "https://www.google.com");

        // Act
        Action act = () => _service.RegisterCommand(command);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("command")
            .WithMessage("*Command name cannot be null or whitespace*");
    }

    #endregion

    #region RemoveCommand Tests

    [Test]
    public void RemoveCommand_WithExistingCommand_ShouldRemoveFromRegistry()
    {
        // Arrange
        var command = new Command("google", "https://www.google.com");
        _service.RegisterCommand(command);

        // Act
        var result = _service.RemoveCommand("google");

        // Assert
        result.Should().BeTrue();
        _service.HasCommand("google").Should().BeFalse();
        _service.GetAllCommands().Should().BeEmpty();
    }

    [Test]
    public void RemoveCommand_WithExistingCommand_ShouldUpdateAutoCompleter()
    {
        // Arrange
        var command = new Command("google", "https://www.google.com");
        _service.RegisterCommand(command);
        _mockAutoCompleter.ClearReceivedCalls();

        // Act
        _service.RemoveCommand("google");

        // Assert
        _mockAutoCompleter.Received(1).UpdateWordList(
            Arg.Is<IEnumerable<string>>(words => !words.Any()));
    }

    [Test]
    public void RemoveCommand_WithNonExistingCommand_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.RemoveCommand("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void RemoveCommand_WithNonExistingCommand_ShouldNotUpdateAutoCompleter()
    {
        // Arrange
        var command = new Command("google", "https://www.google.com");
        _service.RegisterCommand(command);
        _mockAutoCompleter.ClearReceivedCalls();

        // Act
        _service.RemoveCommand("nonexistent");

        // Assert
        _mockAutoCompleter.DidNotReceive().UpdateWordList(Arg.Any<IEnumerable<string>>());
    }

    [Test]
    public void RemoveCommand_WithDifferentCase_ShouldRemoveCommand()
    {
        // Arrange
        var command = new Command("google", "https://www.google.com");
        _service.RegisterCommand(command);

        // Act
        var result = _service.RemoveCommand("GOOGLE");

        // Assert
        result.Should().BeTrue();
        _service.HasCommand("google").Should().BeFalse();
    }

    [Test]
    public void RemoveCommand_WithNullCommandName_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.RemoveCommand(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void RemoveCommand_WithEmptyCommandName_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.RemoveCommand("");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void RemoveCommand_WithWhitespaceCommandName_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.RemoveCommand("   ");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ClearCommands Tests

    [Test]
    public void ClearCommands_WithMultipleCommands_ShouldRemoveAll()
    {
        // Arrange
        _service.RegisterCommand(new Command("google", "https://www.google.com"));
        _service.RegisterCommand(new Command("github", "https://www.github.com"));
        _service.RegisterCommand(new Command("notepad", "notepad.exe"));

        // Act
        _service.ClearCommands();

        // Assert
        _service.GetAllCommands().Should().BeEmpty();
    }

    [Test]
    public void ClearCommands_WithMultipleCommands_ShouldClearAutoCompleter()
    {
        // Arrange
        _service.RegisterCommand(new Command("google", "https://www.google.com"));
        _service.RegisterCommand(new Command("github", "https://www.github.com"));
        _mockAutoCompleter.ClearReceivedCalls();

        // Act
        _service.ClearCommands();

        // Assert
        _mockAutoCompleter.Received(1).UpdateWordList(
            Arg.Is<IEnumerable<string>>(words => !words.Any()));
    }

    [Test]
    public void ClearCommands_WithEmptyRegistry_ShouldNotThrowException()
    {
        // Arrange & Act
        Action act = () => _service.ClearCommands();

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region HasCommand Tests

    [Test]
    public void HasCommand_WithExistingCommand_ShouldReturnTrue()
    {
        // Arrange
        var command = new Command("google", "https://www.google.com");
        _service.RegisterCommand(command);

        // Act
        var result = _service.HasCommand("google");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void HasCommand_WithNonExistingCommand_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.HasCommand("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasCommand_WithDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        var command = new Command("google", "https://www.google.com");
        _service.RegisterCommand(command);

        // Act
        var result = _service.HasCommand("GOOGLE");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void HasCommand_WithNullCommandName_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.HasCommand(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasCommand_WithEmptyCommandName_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.HasCommand("");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasCommand_WithWhitespaceCommandName_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.HasCommand("   ");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAllCommands Tests

    [Test]
    public void GetAllCommands_WithEmptyRegistry_ShouldReturnEmptyList()
    {
        // Arrange & Act
        var commands = _service.GetAllCommands();

        // Assert
        commands.Should().NotBeNull();
        commands.Should().BeEmpty();
    }

    [Test]
    public void GetAllCommands_WithMultipleCommands_ShouldReturnAllCommands()
    {
        // Arrange
        var command1 = new Command("google", "https://www.google.com");
        var command2 = new Command("github", "https://www.github.com");
        var command3 = new Command("notepad", "notepad.exe");

        _service.RegisterCommand(command1);
        _service.RegisterCommand(command2);
        _service.RegisterCommand(command3);

        // Act
        var commands = _service.GetAllCommands();

        // Assert
        commands.Should().HaveCount(3);
        commands.Should().Contain(command1);
        commands.Should().Contain(command2);
        commands.Should().Contain(command3);
    }

    [Test]
    public void GetAllCommands_ShouldReturnReadOnlyList()
    {
        // Arrange
        _service.RegisterCommand(new Command("google", "https://www.google.com"));

        // Act
        var commands = _service.GetAllCommands();

        // Assert
        commands.Should().BeAssignableTo<IReadOnlyList<Command>>();
    }

    #endregion

    #region Integration Tests

    [Test]
    public void RegisterAndRemoveMultipleCommands_ShouldMaintainCorrectState()
    {
        // Arrange
        var command1 = new Command("google", "https://www.google.com");
        var command2 = new Command("github", "https://www.github.com");
        var command3 = new Command("notepad", "notepad.exe");

        // Act
        _service.RegisterCommand(command1);
        _service.RegisterCommand(command2);
        _service.RegisterCommand(command3);
        _service.RemoveCommand("github");

        // Assert
        _service.GetAllCommands().Should().HaveCount(2);
        _service.HasCommand("google").Should().BeTrue();
        _service.HasCommand("github").Should().BeFalse();
        _service.HasCommand("notepad").Should().BeTrue();
    }

    [Test]
    public void AutoCompleterSynchronization_ShouldUpdateOnEveryChange()
    {
        // Arrange
        var command1 = new Command("google", "https://www.google.com");
        var command2 = new Command("github", "https://www.github.com");

        // Act & Assert
        _service.RegisterCommand(command1);
        _mockAutoCompleter.Received(1).UpdateWordList(
            Arg.Is<IEnumerable<string>>(words => words.SequenceEqual(new[] { "google" })));

        _service.RegisterCommand(command2);
        _mockAutoCompleter.Received(1).UpdateWordList(
            Arg.Is<IEnumerable<string>>(words =>
                words.Count() == 2 && words.Contains("google") && words.Contains("github")));

        _service.RemoveCommand("google");
        _mockAutoCompleter.Received(1).UpdateWordList(
            Arg.Is<IEnumerable<string>>(words => words.SequenceEqual(new[] { "github" })));

        _service.ClearCommands();
        _mockAutoCompleter.Received(1).UpdateWordList(
            Arg.Is<IEnumerable<string>>(words => !words.Any()));
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that RegisterCommand stores unicode command names correctly.
    /// </summary>
    [Test]
    public void RegisterCommand_WithUnicodeNames_StoresCorrectly()
    {
        // Arrange
        var unicodeName = EdgeCaseTestFixtures.UnicodeCommandNames[0]; // 搜索
        var command = new Command(unicodeName, "https://www.google.com");

        // Act
        _service.RegisterCommand(command);

        // Assert
        _service.HasCommand(unicodeName).Should().BeTrue();
        _service.GetAllCommands().Should().ContainSingle()
            .Which.Name.Should().Be(unicodeName);
    }

    /// <summary>
    /// Tests that HasCommand finds unicode commands correctly (case-insensitive where applicable).
    /// </summary>
    [Test]
    public void HasCommand_WithUnicodeLookup_FindsCorrectly()
    {
        // Arrange
        var unicodeName = EdgeCaseTestFixtures.UnicodeCommandNames[11]; // café
        var command = new Command(unicodeName, "https://www.example.com");
        _service.RegisterCommand(command);

        // Act
        var result = _service.HasCommand("café");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetAllCommands returns empty list when registry is empty.
    /// This verifies the edge case of an empty registry.
    /// </summary>
    [Test]
    public void GetAllCommands_EmptyRegistry_ReturnsEmpty()
    {
        // Arrange - registry is already empty from SetUp

        // Act
        var commands = _service.GetAllCommands();

        // Assert
        commands.Should().NotBeNull();
        commands.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that RegisterCommand handles emoji in command names correctly.
    /// </summary>
    [Test]
    public void RegisterCommand_WithEmojiInName_StoresCorrectly()
    {
        // Arrange
        var emojiCommand = EdgeCaseTestFixtures.UnicodeCommandNames[9]; // 🔍search
        var command = new Command(emojiCommand, "https://www.google.com");

        // Act
        _service.RegisterCommand(command);

        // Assert
        _service.HasCommand(emojiCommand).Should().BeTrue();
        _service.GetAllCommands().Should().ContainSingle()
            .Which.Name.Should().Be(emojiCommand);
    }

    /// <summary>
    /// Tests that RegisterCommand handles mixed-script command names correctly.
    /// </summary>
    [Test]
    public void RegisterCommand_WithMixedScriptName_StoresCorrectly()
    {
        // Arrange
        var mixedScript = EdgeCaseTestFixtures.UnicodeCommandNames[15]; // search検索
        var command = new Command(mixedScript, "https://www.example.com");

        // Act
        _service.RegisterCommand(command);

        // Assert
        _service.HasCommand(mixedScript).Should().BeTrue();
        _service.GetAllCommands().Should().ContainSingle()
            .Which.Name.Should().Be(mixedScript);
    }

    #endregion
}
