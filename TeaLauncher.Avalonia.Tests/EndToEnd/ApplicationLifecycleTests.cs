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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using CommandLauncher;
using FluentAssertions;
using NUnit.Framework;
using TeaLauncher.Avalonia.Views;

namespace TeaLauncher.Avalonia.Tests.EndToEnd;

/// <summary>
/// End-to-end tests for application lifecycle scenarios.
/// Tests the complete application startup, initialization, and shutdown flows.
/// </summary>
[TestFixture]
public class ApplicationLifecycleTests
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
    /// Test: CompleteLifecycle_StartInitializeExit
    /// Scenario: Start application with headless mode, initialize with config, verify state, and exit cleanly
    /// Expected: Application starts successfully, config loads, and exits without errors
    /// </summary>
    [AvaloniaTest]
    public void CompleteLifecycle_StartInitializeExit_ShouldSucceed()
    {
        // Arrange
        MainWindow? window = null;
        Exception? exception = null;

        try
        {
            // Act - Create and initialize the MainWindow with test config
            window = new MainWindow(_testConfigPath);

            // Assert - Window should be created successfully
            window.Should().NotBeNull("the window should be created successfully");
            window.IsVisible.Should().BeFalse("the window should be hidden on startup");

            // Verify the window can be shown and hidden
            window.Show();
            window.IsVisible.Should().BeTrue("the window should be visible after Show()");

            window.Hide();
            window.IsVisible.Should().BeFalse("the window should be hidden after Hide()");
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        finally
        {
            // Act - Exit cleanly
            if (window != null)
            {
                window.Close();
            }
        }

        // Assert - No exceptions should have occurred
        exception.Should().BeNull("the application lifecycle should complete without errors");
    }

    /// <summary>
    /// Test: InitializationFailure_InvalidConfig
    /// Scenario: Start application with invalid configuration file
    /// Expected: Application handles the error gracefully
    /// </summary>
    [AvaloniaTest]
    public void InitializationFailure_InvalidConfig_ShouldHandleError()
    {
        // Arrange
        string invalidConfigPath = Path.Combine(Path.GetTempPath(), $"invalid-config-{Guid.NewGuid()}.yaml");
        File.WriteAllText(invalidConfigPath, "invalid: yaml: content: [unclosed");

        MainWindow? window = null;
        Exception? caughtException = null;

        try
        {
            // Act - Try to create MainWindow with invalid config
            // Note: The current implementation shows an error dialog but doesn't throw
            // We're testing that the window is still created (error handling is graceful)
            window = new MainWindow(invalidConfigPath);

            // Assert - Window should still be created (error is shown via dialog, not exception)
            window.Should().NotBeNull("the window should be created even with invalid config");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }
        finally
        {
            // Cleanup
            if (window != null)
            {
                window.Close();
            }
            if (File.Exists(invalidConfigPath))
            {
                File.Delete(invalidConfigPath);
            }
        }

        // Assert - Exception handling verification
        // The application should handle errors gracefully (either by dialog or exception)
        // For headless testing on Linux, we accept either behavior
        if (caughtException != null)
        {
            caughtException.Should().NotBeOfType<NullReferenceException>(
                "invalid config should not cause null reference exceptions");
        }
    }

    /// <summary>
    /// Test: WindowVisibility_Toggle
    /// Scenario: Test window visibility state changes
    /// Expected: Window visibility state is correctly managed
    /// </summary>
    [AvaloniaTest]
    public void WindowVisibility_Toggle_ShouldUpdateState()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);

        // Act & Assert - Initial state
        window.IsVisible.Should().BeFalse("window should be hidden on creation");

        // Act - Show window
        window.Show();

        // Assert - Window should be visible
        window.IsVisible.Should().BeTrue("window should be visible after Show()");

        // Act - Hide window
        window.Hide();

        // Assert - Window should be hidden
        window.IsVisible.Should().BeFalse("window should be hidden after Hide()");

        // Act - Show again
        window.Show();

        // Assert - Window should be visible again
        window.IsVisible.Should().BeTrue("window should be visible after second Show()");

        // Cleanup
        window.Close();
    }

    /// <summary>
    /// Test: Configuration_Reload
    /// Scenario: Reload configuration after initial load
    /// Expected: Configuration is reloaded successfully without errors
    /// </summary>
    [AvaloniaTest]
    public void Configuration_Reload_ShouldSucceed()
    {
        // Arrange
        var window = new MainWindow(_testConfigPath);

        // Act - Trigger reinitialize (simulates !reload command behavior)
        Exception? exception = null;
        try
        {
            // Call the Reinitialize method directly
            // Note: MainWindow implements ICommandManagerInitializer, but the interface is not public
            window.Reinitialize();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert - Reload should succeed without exceptions
        exception.Should().BeNull("configuration reload should complete without errors");

        // Cleanup
        window.Close();
    }
}
