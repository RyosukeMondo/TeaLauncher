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

using System.Threading.Tasks;

namespace TeaLauncher.Avalonia.Domain.Interfaces;

/// <summary>
/// Defines the contract for displaying UI dialogs to the user.
/// This interface abstracts dialog operations to enable testability and support headless testing scenarios.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an informational message dialog to the user.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message content to display.</param>
    /// <returns>A task that completes when the user dismisses the dialog.</returns>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Shows a confirmation dialog to the user with Yes/No options.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The confirmation question to display.</param>
    /// <returns>A task that returns true if the user clicked Yes, false if the user clicked No.</returns>
    Task<bool> ShowConfirmAsync(string title, string message);

    /// <summary>
    /// Shows an error message dialog to the user.
    /// </summary>
    /// <param name="title">The title of the error dialog.</param>
    /// <param name="message">The error message to display.</param>
    /// <returns>A task that completes when the user dismisses the dialog.</returns>
    Task ShowErrorAsync(string title, string message);
}
