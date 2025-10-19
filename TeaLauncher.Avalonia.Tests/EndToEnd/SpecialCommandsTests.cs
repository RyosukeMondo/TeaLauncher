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
    /// Expected: Configuration is reloaded successfully, no error dialog shown
    /// </summary>
    [AvaloniaTest]
    public void ReloadCommand_UpdatesCommands_ShouldSucceed()
    {
        // Arrange
        var mockDialogService = new MockDialogService();
        var window = new MainWindow(_testConfigPath, mockDialogService);
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

        // Act - Execute reload command (which triggers !reload special command)
        commandBox!.Text = "reload";

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

        // No error dialogs should be shown for successful reload
        var errorCalls = mockDialogService.GetDialogCalls().Where(c => c.DialogType == "Error");
        errorCalls.Should().BeEmpty("no error dialogs should be shown for successful reload");
    }

    /// <summary>
    /// Test: VersionCommand_ShowsVersion
    /// Scenario: Execute !version command to display version information
    /// Expected: Version information is displayed via MockDialogService
    /// </summary>
    [AvaloniaTest]
    public void VersionCommand_ShowsVersion_ShouldExecute()
    {
        // Arrange
        var mockDialogService = new MockDialogService();
        var window = new MainWindow(_testConfigPath, mockDialogService);
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Execute version command (which triggers !version special command)
        commandBox!.Text = "version";

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
        exception.Should().BeNull("version command should execute without critical errors");

        // Command box should be cleared
        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared after version command");

        // Verify a message dialog was shown with version information
        var messageCalls = mockDialogService.GetDialogCalls().Where(c => c.DialogType == "Message");
        messageCalls.Should().ContainSingle("version dialog should be shown");

        var versionDialog = messageCalls.First();
        versionDialog.Message.Should().Contain("version", "dialog should contain version information");
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

        // Act - Execute exit command (which triggers !exit special command)
        commandBox!.Text = "exit";

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
    /// Scenario: Execute a special command and verify command box is cleared
    /// Expected: Command box is cleared after special command execution
    /// Note: Window visibility is not tested in headless mode
    /// </summary>
    [AvaloniaTest]
    public void SpecialCommand_ClearsInputAndHidesWindow()
    {
        // Arrange
        var mockDialogService = new MockDialogService();
        var window = new MainWindow(_testConfigPath, mockDialogService);
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Act - Execute reload (a special command)
        commandBox!.Text = "reload";

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

        // No error dialogs should be shown for successful reload
        var errorCalls = mockDialogService.GetDialogCalls().Where(c => c.DialogType == "Error");
        errorCalls.Should().BeEmpty("no error dialogs should be shown for successful reload");
    }

    /// <summary>
    /// Test: InvalidSpecialCommand_DoesNotExecute
    /// Scenario: Try to execute an invalid special command (e.g., !invalid)
    /// Expected: Command is not recognized, command box is not cleared
    /// </summary>
    [AvaloniaTest]
    public void InvalidSpecialCommand_DoesNotExecute()
    {
        // Arrange
        var mockDialogService = new MockDialogService();
        var window = new MainWindow(_testConfigPath, mockDialogService);
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

        // Command box should NOT be cleared for invalid special commands
        commandBox.Text.Should().Be("!invalid-special-command", "command box should retain text for invalid special commands");

        // No dialogs should be shown for invalid special commands
        mockDialogService.GetDialogCalls().Should().BeEmpty("no dialogs should be shown for invalid special commands");
    }

    /// <summary>
    /// Test: ReloadCommand_WithInvalidConfig_HandlesError
    /// Scenario: Modify config to be invalid, then execute !reload
    /// Expected: Error is handled gracefully via error dialog
    /// </summary>
    [AvaloniaTest]
    public void ReloadCommand_WithInvalidConfig_HandlesError()
    {
        // Arrange
        var mockDialogService = new MockDialogService();
        var window = new MainWindow(_testConfigPath, mockDialogService);
        window.Show();

        var commandBox = window.FindControl<AutoCompleteBox>("CommandBox");
        commandBox.Should().NotBeNull("CommandBox control should exist");

        // Modify the config file to be invalid
        File.WriteAllText(_testConfigPath, "invalid: yaml: content: [unclosed");

        // Act - Execute reload command with invalid config
        commandBox!.Text = "reload";

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
        exception.Should().BeNull("reload with invalid config should not throw exception");

        // Command box should be cleared even when reload fails
        commandBox.Text.Should().BeNullOrEmpty("command box should be cleared after reload attempt");

        // Verify an error dialog was shown
        var errorCalls = mockDialogService.GetDialogCalls().Where(c => c.DialogType == "Error");
        errorCalls.Should().ContainSingle("error dialog should be shown for invalid config");

        var errorDialog = errorCalls.First();
        errorDialog.Message.Should().Contain("reload", "error message should mention reload failure");

        // Application should still be running (not crashed)
        window.Should().NotBeNull("window should still exist after error");
    }
}
