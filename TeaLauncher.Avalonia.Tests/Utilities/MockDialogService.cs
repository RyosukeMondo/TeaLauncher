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

namespace TeaLauncher.Avalonia.Tests.Utilities;

/// <summary>
/// Mock implementation of IDialogService for testing purposes.
/// Records all dialog calls for verification without requiring a display server.
/// Enables E2E tests to verify dialog interactions in headless environments.
/// </summary>
public class MockDialogService : IDialogService
{
    private readonly List<DialogCall> _dialogCalls = new();
    private bool _confirmResponse = true;

    /// <summary>
    /// Represents a recorded dialog call with all relevant details.
    /// </summary>
    public record DialogCall(
        DateTime Timestamp,
        string DialogType,
        string Title,
        string Message,
        bool? UserResponse);

    /// <summary>
    /// Shows an informational message dialog (recorded only, not displayed).
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message content to display.</param>
    /// <returns>A completed task.</returns>
    public Task ShowMessageAsync(string title, string message)
    {
        _dialogCalls.Add(new DialogCall(
            DateTime.Now,
            "Message",
            title,
            message,
            null));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Shows a confirmation dialog (recorded only, not displayed).
    /// Returns the pre-configured response set via SetConfirmResponse.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The confirmation question to display.</param>
    /// <returns>A task that returns the pre-configured confirm response.</returns>
    public Task<bool> ShowConfirmAsync(string title, string message)
    {
        _dialogCalls.Add(new DialogCall(
            DateTime.Now,
            "Confirm",
            title,
            message,
            _confirmResponse));

        return Task.FromResult(_confirmResponse);
    }

    /// <summary>
    /// Shows an error message dialog (recorded only, not displayed).
    /// </summary>
    /// <param name="title">The title of the error dialog.</param>
    /// <param name="message">The error message to display.</param>
    /// <returns>A completed task.</returns>
    public Task ShowErrorAsync(string title, string message)
    {
        _dialogCalls.Add(new DialogCall(
            DateTime.Now,
            "Error",
            title,
            message,
            null));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the response that ShowConfirmAsync will return.
    /// </summary>
    /// <param name="response">True for Yes, false for No.</param>
    public void SetConfirmResponse(bool response)
    {
        _confirmResponse = response;
    }

    /// <summary>
    /// Verifies that a message dialog was shown with the specified title and message.
    /// Uses FluentAssertions for detailed error messages.
    /// </summary>
    /// <param name="title">Expected dialog title.</param>
    /// <param name="message">Expected dialog message.</param>
    public void VerifyMessageShown(string title, string message)
    {
        _dialogCalls.Should().Contain(call =>
            call.DialogType == "Message" &&
            call.Title == title &&
            call.Message == message,
            $"Expected message dialog with title '{title}' and message '{message}' to have been shown");
    }

    /// <summary>
    /// Verifies that a confirm dialog was shown with the specified title.
    /// Uses FluentAssertions for detailed error messages.
    /// </summary>
    /// <param name="title">Expected dialog title.</param>
    public void VerifyConfirmShown(string title)
    {
        _dialogCalls.Should().Contain(call =>
            call.DialogType == "Confirm" &&
            call.Title == title,
            $"Expected confirm dialog with title '{title}' to have been shown");
    }

    /// <summary>
    /// Verifies that an error dialog was shown with the specified title and message.
    /// Uses FluentAssertions for detailed error messages.
    /// </summary>
    /// <param name="title">Expected dialog title.</param>
    /// <param name="message">Expected dialog message.</param>
    public void VerifyErrorShown(string title, string message)
    {
        _dialogCalls.Should().Contain(call =>
            call.DialogType == "Error" &&
            call.Title == title &&
            call.Message == message,
            $"Expected error dialog with title '{title}' and message '{message}' to have been shown");
    }

    /// <summary>
    /// Gets all recorded dialog calls in chronological order.
    /// </summary>
    /// <returns>A read-only list of all dialog calls.</returns>
    public IReadOnlyList<DialogCall> GetDialogCalls()
    {
        return _dialogCalls.AsReadOnly();
    }

    /// <summary>
    /// Clears all recorded dialog history.
    /// Useful for resetting state between tests.
    /// </summary>
    public void ClearDialogHistory()
    {
        _dialogCalls.Clear();
    }
}
