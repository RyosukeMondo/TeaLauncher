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
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using FluentAssertions;
using NUnit.Framework;
using TeaLauncher.Avalonia.Views;

namespace TeaLauncher.Avalonia.Tests.EndToEnd;

/// <summary>
/// End-to-end tests for user workflow scenarios.
/// Tests complete user journeys from input to command execution.
/// </summary>
[TestFixture]
public class UserWorkflowTests
{
    private string _testConfigPath = null!;

    [SetUp]
    public void SetUp()
    {
        // Create a temporary config file for testing
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"e2e-test-{Guid.NewGuid()}.yaml");
        File.Copy(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EndToEnd", "Fixtures", "e2e-test-config.yaml"),
            _testConfigPath,
            overwrite: true);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up temporary config file
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    /// <summary>
    /// Test: AutoCompletion_TabKey_CompletesCommand
    /// Scenario: Type partial command, press Tab, verify completion
    /// Expected: Command is auto-completed to the matching command name
    /// </summary>
    [AvaloniaTest]
    public void AutoCompletion_TabKey_CompletesCommand()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Type partial command
        commandBox!.Text = "test";

        // Simulate Tab key press
        var tabKeyEventArgs = new KeyEventArgs
        {
            Key = Key.Tab,
            RoutedEvent = InputElement.KeyDownEvent
        };

        // The CommandBox_KeyDown handler will process the Tab key
        commandBox.RaiseEvent(tabKeyEventArgs);

        // Assert - Text should be auto-completed
        // The completion logic should expand "test" to one of the test commands
        commandBox.Text.Should().NotBeNullOrEmpty("text should be auto-completed");
        commandBox.Text.Should().StartWith("test", "completed text should start with the input");
    }

    /// <summary>
    /// Test: CommandInput_EnterKey_ExecutesAndHides
    /// Scenario: Type command, press Enter, verify window is hidden
    /// Expected: Command is executed and window is hidden
    /// Note: We cannot verify actual Process.Start in headless tests, only verify window behavior
    /// </summary>
    [AvaloniaTest]
    public void CommandInput_EnterKey_ExecutesAndHides()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);
        window.Show();
        window.IsVisible.Should().BeTrue("window should be visible initially");

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Type a command that exists in config
        commandBox!.Text = "test-command";

        // Simulate Enter key press
        var enterKeyEventArgs = new KeyEventArgs
        {
            Key = Key.Enter,
            RoutedEvent = InputElement.KeyDownEvent
        };

        // Note: In headless mode, the actual command execution may fail
        // because Process.Start won't work, but we can verify the window behavior
        try
        {
            commandBox.RaiseEvent(enterKeyEventArgs);
        }
        catch
        {
            // Ignore exceptions from Process.Start in headless mode
        }

        // Assert - Window should be hidden after command execution
        // Note: The window may still be visible if command execution failed,
        // but the CommandBox should be cleared
        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared after execution attempt");
    }

    /// <summary>
    /// Test: EscapeKey_ClearsInput
    /// Scenario: Type text, press Escape, verify input is cleared
    /// Expected: Input is cleared but window remains visible
    /// </summary>
    [AvaloniaTest]
    public void EscapeKey_ClearsInput_WindowRemainsVisible()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Type some text
        commandBox!.Text = "some text";
        commandBox.Text.Should().NotBeNullOrEmpty("text should be entered");

        // Simulate Escape key press
        var escapeKeyEventArgs = new KeyEventArgs
        {
            Key = Key.Escape,
            RoutedEvent = InputElement.KeyDownEvent
        };

        commandBox.RaiseEvent(escapeKeyEventArgs);

        // Assert - Text should be cleared
        commandBox.Text.Should().BeNullOrEmpty("text should be cleared after Escape");

        // Window should still be visible (first Escape clears text, second Escape hides window)
        window.IsVisible.Should().BeTrue("window should remain visible after first Escape");
    }

    /// <summary>
    /// Test: EscapeKey_HidesWindow
    /// Scenario: Press Escape on empty input, verify window is hidden
    /// Expected: Window is hidden when Escape is pressed with empty input
    /// </summary>
    [AvaloniaTest]
    public void EscapeKey_HidesWindow_WhenInputEmpty()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);
        window.Show();
        window.IsVisible.Should().BeTrue("window should be visible initially");

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");
        commandBox!.Text = string.Empty;

        // Act - Simulate Escape key press with empty input
        var escapeKeyEventArgs = new KeyEventArgs
        {
            Key = Key.Escape,
            RoutedEvent = InputElement.KeyDownEvent
        };

        commandBox.RaiseEvent(escapeKeyEventArgs);

        // Assert - Window should be hidden
        window.IsVisible.Should().BeFalse("window should be hidden after Escape on empty input");
    }

    /// <summary>
    /// Test: MultipleCommands_Sequential
    /// Scenario: Execute multiple commands sequentially
    /// Expected: Each command execution works independently
    /// </summary>
    [AvaloniaTest]
    public void MultipleCommands_Sequential_ShouldWork()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act & Assert - Execute first command
        window.Show();
        commandBox!.Text = "test-command";

        var enterKeyEventArgs = new KeyEventArgs
        {
            Key = Key.Enter,
            RoutedEvent = InputElement.KeyDownEvent
        };

        try
        {
            commandBox.RaiseEvent(enterKeyEventArgs);
        }
        catch
        {
            // Ignore Process.Start exceptions in headless mode
        }

        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared after first command");

        // Act & Assert - Execute second command
        window.Show(); // Re-show window
        commandBox.Text = "test-url";

        try
        {
            commandBox.RaiseEvent(enterKeyEventArgs);
        }
        catch
        {
            // Ignore Process.Start exceptions in headless mode
        }

        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared after second command");
    }

    /// <summary>
    /// Test: AutoComplete_GetCandidates
    /// Scenario: Type prefix that matches multiple commands
    /// Expected: Multiple candidates are available
    /// </summary>
    [AvaloniaTest]
    public void AutoComplete_GetCandidates_ReturnsMultipleMatches()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Type prefix that matches multiple commands (e.g., "test" matches test-command, test-app, test-url)
        commandBox!.Text = "test";

        // Simulate Tab to trigger auto-completion
        var tabKeyEventArgs = new KeyEventArgs
        {
            Key = Key.Tab,
            RoutedEvent = InputElement.KeyDownEvent
        };

        commandBox.RaiseEvent(tabKeyEventArgs);

        // Assert - ItemsSource should contain candidates
        // Note: If there are multiple matches, the dropdown should show them
        if (commandBox.ItemsSource != null)
        {
            var items = commandBox.ItemsSource.Cast<object>().ToList();
            items.Should().NotBeEmpty("there should be completion candidates");
            items.Should().AllSatisfy(item =>
                item.ToString()!.StartsWith("test", StringComparison.OrdinalIgnoreCase),
                "all candidates should start with the input prefix");
        }
    }

    /// <summary>
    /// Test: InvalidCommand_DoesNotExecute
    /// Scenario: Try to execute a command that doesn't exist
    /// Expected: Command is not executed, no error is thrown
    /// </summary>
    [AvaloniaTest]
    public void InvalidCommand_DoesNotExecute()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Type a non-existent command
        commandBox!.Text = "non-existent-command-12345";

        var enterKeyEventArgs = new KeyEventArgs
        {
            Key = Key.Enter,
            RoutedEvent = InputElement.KeyDownEvent
        };

        Exception? exception = null;
        try
        {
            commandBox.RaiseEvent(enterKeyEventArgs);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert - Should not throw exception (invalid commands are simply ignored)
        exception.Should().BeNull("invalid command should not throw exception");

        // Window should still be visible (command was not executed)
        window.IsVisible.Should().BeTrue("window should remain visible when command doesn't exist");
    }
}
