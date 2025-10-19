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

using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Infrastructure.UI;

namespace TeaLauncher.Avalonia.Tests.Infrastructure.UI;

/// <summary>
/// Unit tests for AvaloniaDialogService.
/// Tests dialog service behavior in headless mode.
/// Uses the AAA (Arrange-Act-Assert) pattern for clarity.
/// Note: Actual dialog display cannot be tested in headless mode,
/// so these tests focus on construction and headless mode behavior.
/// </summary>
[TestFixture]
public class AvaloniaDialogServiceTests
{
    private AvaloniaDialogService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new AvaloniaDialogService();
    }

    #region Constructor Tests

    /// <summary>
    /// Tests that the AvaloniaDialogService can be constructed successfully.
    /// </summary>
    [Test]
    public void Constructor_ShouldSucceed()
    {
        // Arrange & Act
        var service = new AvaloniaDialogService();

        // Assert
        service.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that the service implements IDialogService interface.
    /// </summary>
    [Test]
    public void Constructor_ShouldImplementIDialogService()
    {
        // Arrange & Act
        var service = new AvaloniaDialogService();

        // Assert
        service.Should().BeAssignableTo<IDialogService>();
    }

    #endregion

    #region Headless Mode Tests

    /// <summary>
    /// Tests that ShowMessageAsync completes without error in headless mode.
    /// In headless mode (no UI thread/window), the service should gracefully
    /// return without attempting to show a dialog.
    /// </summary>
    [Test]
    public async Task ShowMessageAsync_InHeadlessMode_ShouldCompleteWithoutError()
    {
        // Arrange
        var title = "Test Title";
        var message = "Test Message";

        // Act
        Func<Task> act = async () => await _service.ShowMessageAsync(title, message);

        // Assert - should not throw
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that ShowErrorAsync completes without error in headless mode.
    /// </summary>
    [Test]
    public async Task ShowErrorAsync_InHeadlessMode_ShouldCompleteWithoutError()
    {
        // Arrange
        var title = "Error";
        var message = "Test error message";

        // Act
        Func<Task> act = async () => await _service.ShowErrorAsync(title, message);

        // Assert - should not throw
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that ShowConfirmAsync returns true by default in headless mode.
    /// When no window is available, confirmation dialogs default to true.
    /// </summary>
    [Test]
    public async Task ShowConfirmAsync_InHeadlessMode_ShouldReturnTrue()
    {
        // Arrange
        var title = "Confirm";
        var message = "Are you sure?";

        // Act
        var result = await _service.ShowConfirmAsync(title, message);

        // Assert
        result.Should().BeTrue("headless mode should default to true for confirmations");
    }

    /// <summary>
    /// Tests that multiple dialog calls in sequence work correctly in headless mode.
    /// </summary>
    [Test]
    public async Task MultipleCalls_InHeadlessMode_ShouldAllSucceed()
    {
        // Arrange & Act
        await _service.ShowMessageAsync("Title1", "Message1");
        var confirm = await _service.ShowConfirmAsync("Title2", "Message2");
        await _service.ShowErrorAsync("Title3", "Error3");

        // Assert - all calls should complete without error
        confirm.Should().BeTrue();
    }

    #endregion

    #region Parameter Tests

    /// <summary>
    /// Tests that the service handles empty strings gracefully.
    /// </summary>
    [Test]
    public async Task ShowMessageAsync_WithEmptyStrings_ShouldNotThrow()
    {
        // Arrange & Act
        Func<Task> act = async () => await _service.ShowMessageAsync("", "");

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that the service handles long messages gracefully.
    /// </summary>
    [Test]
    public async Task ShowMessageAsync_WithLongMessage_ShouldNotThrow()
    {
        // Arrange
        var longMessage = new string('x', 10000);

        // Act
        Func<Task> act = async () => await _service.ShowMessageAsync("Title", longMessage);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion
}
