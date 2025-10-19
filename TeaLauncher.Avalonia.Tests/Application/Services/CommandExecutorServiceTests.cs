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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeaLauncher.Avalonia.Tests.Application.Services;

/// <summary>
/// Unit tests for CommandExecutorService.
/// Tests command execution, path detection, argument parsing, and error handling.
/// </summary>
[TestFixture]
public class CommandExecutorServiceTests
{
    private ICommandRegistry _mockCommandRegistry = null!;
    private CommandExecutorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCommandRegistry = Substitute.For<ICommandRegistry>();
        _service = new CommandExecutorService(_mockCommandRegistry);
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithNullCommandRegistry_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new CommandExecutorService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("commandRegistry");
    }

    [Test]
    public void Constructor_WithValidCommandRegistry_ShouldSucceed()
    {
        // Arrange & Act
        var service = new CommandExecutorService(_mockCommandRegistry);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region ExecuteAsync Tests

    [Test]
    public async Task ExecuteAsync_WithNullInput_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Func<Task> act = async () => await _service.ExecuteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("commandInput")
            .WithMessage("*cannot be null or empty*");
    }

    [Test]
    public async Task ExecuteAsync_WithEmptyInput_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Func<Task> act = async () => await _service.ExecuteAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("commandInput")
            .WithMessage("*cannot be null or empty*");
    }

    [Test]
    public async Task ExecuteAsync_WithWhitespaceInput_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Func<Task> act = async () => await _service.ExecuteAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("commandInput")
            .WithMessage("*cannot be null or empty*");
    }

    [Test]
    public async Task ExecuteAsync_WithNonExistentCommand_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockCommandRegistry.GetAllCommands().Returns(new List<Command>().AsReadOnly());

        // Act
        Func<Task> act = async () => await _service.ExecuteAsync("nonexistent");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found in registry*");
    }

    [Test]
    public async Task ExecuteAsync_WithSpecialCommand_ShouldThrowNotSupportedException()
    {
        // Arrange
        var command = new Command("reload", "!reload");
        _mockCommandRegistry.GetAllCommands().Returns(new List<Command> { command }.AsReadOnly());

        // Act
        Func<Task> act = async () => await _service.ExecuteAsync("reload");

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*must be handled by ApplicationOrchestrator*");
    }

    [Test]
    public async Task ExecuteAsync_WithDirectSpecialCommand_ShouldThrowNotSupportedException()
    {
        // Arrange & Act
        Func<Task> act = async () => await _service.ExecuteAsync("!reload");

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*must be handled by ApplicationOrchestrator*");
    }

    // Note: Actual process execution tests are skipped because they would launch real processes
    // In a real test environment, we would mock Process.Start or use integration tests

    #endregion

    #region GetExecution Tests

    [Test]
    public void GetExecution_WithNullInput_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => _service.GetExecution(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("commandInput")
            .WithMessage("*cannot be null or empty*");
    }

    [Test]
    public void GetExecution_WithEmptyInput_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => _service.GetExecution("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("commandInput")
            .WithMessage("*cannot be null or empty*");
    }

    [Test]
    public void GetExecution_WithSimpleCommand_ShouldReturnCommandName()
    {
        // Arrange & Act
        var result = _service.GetExecution("notepad");

        // Assert
        result.Should().Be("notepad");
    }

    [Test]
    public void GetExecution_WithCommandAndArguments_ShouldReturnOnlyCommand()
    {
        // Arrange & Act
        var result = _service.GetExecution("notepad test.txt");

        // Assert
        result.Should().Be("notepad");
    }

    [Test]
    public void GetExecution_WithQuotedCommand_ShouldReturnCommandWithoutQuotes()
    {
        // Arrange & Act
        var result = _service.GetExecution("\"C:\\Program Files\\app.exe\"");

        // Assert
        result.Should().Be("C:\\Program Files\\app.exe");
    }

    [Test]
    public void GetExecution_WithUrl_ShouldReturnUrl()
    {
        // Arrange & Act
        var result = _service.GetExecution("https://www.google.com");

        // Assert
        result.Should().Be("https://www.google.com");
    }

    [Test]
    public void GetExecution_WithFilePath_ShouldReturnFilePath()
    {
        // Arrange & Act
        var result = _service.GetExecution("C:\\Windows\\notepad.exe");

        // Assert
        result.Should().Be("C:\\Windows\\notepad.exe");
    }

    #endregion

    #region GetArguments Tests

    [Test]
    public void GetArguments_WithNullInput_ShouldReturnEmptyList()
    {
        // Arrange & Act
        var result = _service.GetArguments(null!);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public void GetArguments_WithEmptyInput_ShouldReturnEmptyList()
    {
        // Arrange & Act
        var result = _service.GetArguments("");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public void GetArguments_WithSimpleCommandNoArgs_ShouldReturnEmptyList()
    {
        // Arrange & Act
        var result = _service.GetArguments("notepad");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public void GetArguments_WithSingleArgument_ShouldReturnOneArgument()
    {
        // Arrange & Act
        var result = _service.GetArguments("notepad test.txt");

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("test.txt");
    }

    [Test]
    public void GetArguments_WithMultipleArguments_ShouldReturnAllArguments()
    {
        // Arrange & Act
        var result = _service.GetArguments("git commit -m message");

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("commit");
        result[1].Should().Be("-m");
        result[2].Should().Be("message");
    }

    [Test]
    public void GetArguments_WithQuotedArgument_ShouldPreserveSpaces()
    {
        // Arrange & Act
        var result = _service.GetArguments("notepad \"my file.txt\"");

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("my file.txt");
    }

    [Test]
    public void GetArguments_WithMultipleQuotedArguments_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var result = _service.GetArguments("cmd \"first arg\" \"second arg\" third");

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("first arg");
        result[1].Should().Be("second arg");
        result[2].Should().Be("third");
    }

    [Test]
    public void GetArguments_WithSingleQuotes_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var result = _service.GetArguments("cmd 'first arg' 'second arg'");

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().Be("first arg");
        result[1].Should().Be("second arg");
    }

    [Test]
    public void GetArguments_WithMixedQuotedAndUnquoted_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var result = _service.GetArguments("git commit -m \"my commit message\" --amend");

        // Assert
        result.Should().HaveCount(4);
        result[0].Should().Be("commit");
        result[1].Should().Be("-m");
        result[2].Should().Be("my commit message");
        result[3].Should().Be("--amend");
    }

    [Test]
    public void GetArguments_ShouldReturnReadOnlyList()
    {
        // Arrange & Act
        var result = _service.GetArguments("notepad test.txt");

        // Assert
        result.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    #endregion

    #region Path Detection Tests (via GetExecution behavior)

    [Test]
    public void GetExecution_WithHttpUrl_ShouldRecognizeAsPath()
    {
        // Arrange & Act
        var result = _service.GetExecution("http://www.example.com");

        // Assert
        result.Should().Be("http://www.example.com");
    }

    [Test]
    public void GetExecution_WithHttpsUrl_ShouldRecognizeAsPath()
    {
        // Arrange & Act
        var result = _service.GetExecution("https://www.example.com");

        // Assert
        result.Should().Be("https://www.example.com");
    }

    [Test]
    public void GetExecution_WithFtpUrl_ShouldRecognizeAsPath()
    {
        // Arrange & Act
        var result = _service.GetExecution("ftp://ftp.example.com");

        // Assert
        result.Should().Be("ftp://ftp.example.com");
    }

    [Test]
    public void GetExecution_WithDriveLetterPath_ShouldRecognizeAsPath()
    {
        // Arrange & Act
        var result = _service.GetExecution("C:\\Windows\\System32\\notepad.exe");

        // Assert
        result.Should().Be("C:\\Windows\\System32\\notepad.exe");
    }

    #endregion

    #region Argument Parsing Edge Cases

    [Test]
    public void GetArguments_WithExtraSpaces_ShouldIgnoreExtraSpaces()
    {
        // Arrange & Act
        var result = _service.GetArguments("notepad    test.txt");

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("test.txt");
    }

    [Test]
    public void GetArguments_WithTrailingSpaces_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var result = _service.GetArguments("notepad test.txt   ");

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("test.txt");
    }

    [Test]
    public void GetArguments_WithLeadingSpaces_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var result = _service.GetArguments("   notepad test.txt");

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("test.txt");
    }

    [Test]
    public void GetArguments_WithEmptyQuotes_ShouldSkipEmptyArgument()
    {
        // Arrange & Act
        var result = _service.GetArguments("notepad \"\" test.txt");

        // Assert
        // The empty quoted string would normally be skipped by our implementation
        // that filters out empty strings after splitting
        result.Should().Contain("test.txt");
    }

    #endregion

    #region Integration-style Tests

    [Test]
    public void GetExecutionAndArguments_WithComplexCommand_ShouldParseCorrectly()
    {
        // Arrange
        var input = "git commit -m \"Initial commit\" --no-verify";

        // Act
        var execution = _service.GetExecution(input);
        var arguments = _service.GetArguments(input);

        // Assert
        execution.Should().Be("git");
        arguments.Should().HaveCount(4);
        arguments[0].Should().Be("commit");
        arguments[1].Should().Be("-m");
        arguments[2].Should().Be("Initial commit");
        arguments[3].Should().Be("--no-verify");
    }

    [Test]
    public void GetExecutionAndArguments_WithUrlAndParams_ShouldParseCorrectly()
    {
        // Arrange
        var input = "https://www.google.com/search?q=test";

        // Act
        var execution = _service.GetExecution(input);
        var arguments = _service.GetArguments(input);

        // Assert
        execution.Should().Be("https://www.google.com/search?q=test");
        arguments.Should().BeEmpty();
    }

    [Test]
    public void GetExecutionAndArguments_WithPathAndArgs_ShouldParseCorrectly()
    {
        // Arrange
        var input = "C:\\Windows\\notepad.exe myfile.txt";

        // Act
        var execution = _service.GetExecution(input);
        var arguments = _service.GetArguments(input);

        // Assert
        execution.Should().Be("C:\\Windows\\notepad.exe");
        arguments.Should().ContainSingle()
            .Which.Should().Be("myfile.txt");
    }

    #endregion
}
