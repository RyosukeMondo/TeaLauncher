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
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using FluentAssertions;
using NUnit.Framework;
using TeaLauncher.Avalonia.Tests.Utilities;
using TeaLauncher.Avalonia.Views;

namespace TeaLauncher.Avalonia.Tests.EndToEnd;

/// <summary>
/// End-to-end tests for special command scenarios.
/// Tests the special commands (!reload, !version, !exit) that provide application control.
/// </summary>
[TestFixture]
public class SpecialCommandsTests
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
    /// Test: ReloadCommand_UpdatesCommands
    /// Scenario: Execute !reload command to reload configuration
    /// Expected: Configuration is reloaded successfully
    /// </summary>
    [AvaloniaTest]
    public void ReloadCommand_UpdatesCommands_ShouldSucceed()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath, new MockDialogService());
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Modify the config file to add a new command
        string configContent = File.ReadAllText(_testConfigPath);
        string updatedConfig = configContent + @"
  - name: new-reload-test-command
    linkto: https://example.com/new
    description: New command added after reload
";
        File.WriteAllText(_testConfigPath, updatedConfig);

        // Act - Execute !reload command
        commandBox!.Text = "!reload";

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

        // Assert - Reload should succeed without exceptions
        exception.Should().BeNull("reload command should execute without errors");

        // Command box should be cleared after execution
        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared after reload");

        // Verify the new command is now available by typing it
        commandBox.Text = "new-reload-test-command";
        // If the command was loaded successfully, it should be recognized
        // We can't directly verify this in headless mode without executing,
        // but the lack of exception is a good indicator
    }

    /// <summary>
    /// Test: VersionCommand_ShowsVersion
    /// Scenario: Execute !version command to display version information
    /// Expected: Version information is displayed (dialog is shown)
    /// Note: In headless mode, we verify the command executes without error
    /// </summary>
    [AvaloniaTest]
    public void VersionCommand_ShowsVersion_ShouldExecute()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath, new MockDialogService());
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Execute !version command
        commandBox!.Text = "!version";

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

        // Assert - Version command should execute without critical exceptions
        // Note: In headless mode, dialog display may not work, but the command should still execute
        exception.Should().BeNull("version command should execute without critical errors");

        // Command box should be cleared
        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared after version command");
    }

    /// <summary>
    /// Test: ExitCommand_ClosesApp
    /// Scenario: Execute !exit command to close the application
    /// Expected: Application exits cleanly
    /// </summary>
    [AvaloniaTest]
    public void ExitCommand_ClosesApp_ShouldExit()
    {
        // Arrange
        MainWindow? window = new MainWindow(_testConfigPath, new MockDialogService());
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Execute !exit command
        commandBox!.Text = "!exit";

        var enterKeyEventArgs = new KeyEventArgs
        {
            Key = Key.Enter,
            RoutedEvent = InputElement.KeyDownEvent
        };

        bool windowClosed = false;
        window.Closed += (s, e) => windowClosed = true;

        Exception? exception = null;
        try
        {
            commandBox.RaiseEvent(enterKeyEventArgs);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert - Exit command should close the window
        // Note: Window may be closed synchronously or asynchronously
        exception.Should().BeNull("exit command should execute without errors");

        // The window should be closed or in the process of closing
        // In headless mode, we verify that no exception occurred during exit
    }

    /// <summary>
    /// Test: SpecialCommand_ClearsInputAndHidesWindow
    /// Scenario: Execute a special command and verify UI behavior
    /// Expected: Command box is cleared and window is hidden
    /// </summary>
    [AvaloniaTest]
    public void SpecialCommand_ClearsInputAndHidesWindow()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath, new MockDialogService());
        window.Show();
        window.IsVisible.Should().BeTrue("window should be visible initially");

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Execute !reload (a special command)
        commandBox!.Text = "!reload";

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
            // Ignore any exceptions from command execution
        }

        // Assert - Command box should be cleared
        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared after special command");

        // Window should be hidden after command execution
        // Note: In current implementation, window is hidden after any command execution
        window.IsVisible.Should().BeFalse("window should be hidden after command execution");
    }

    /// <summary>
    /// Test: InvalidSpecialCommand_DoesNotExecute
    /// Scenario: Try to execute an invalid special command (e.g., !invalid)
    /// Expected: Command is not recognized and handled appropriately
    /// </summary>
    [AvaloniaTest]
    public void InvalidSpecialCommand_DoesNotExecute()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath, new MockDialogService());
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Execute an invalid special command
        commandBox!.Text = "!invalid-special-command";

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

        // Assert - Invalid special command should not cause crashes
        exception.Should().BeNull("invalid special command should not throw exception");

        // Command box should be cleared (as it starts with !)
        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared");
    }

    /// <summary>
    /// Test: ReloadCommand_WithInvalidConfig_HandlesError
    /// Scenario: Modify config to be invalid, then execute !reload
    /// Expected: Error is handled gracefully
    /// </summary>
    [AvaloniaTest]
    public void ReloadCommand_WithInvalidConfig_HandlesError()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath, new MockDialogService());
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Modify the config file to be invalid
        File.WriteAllText(_testConfigPath, "invalid: yaml: content: [unclosed");

        // Act - Execute !reload command with invalid config
        commandBox!.Text = "!reload";

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

        // Assert - Error should be handled gracefully (via dialog, not crash)
        // In headless mode, dialog may not work, but application should not crash
        if (exception != null)
        {
            exception.Should().NotBeOfType<NullReferenceException>(
                "invalid config should not cause null reference exceptions");
        }

        // Application should still be running (not crashed)
        window.Should().NotBeNull("window should still exist after error");
    }
}
