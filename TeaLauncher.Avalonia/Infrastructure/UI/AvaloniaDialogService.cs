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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using TeaLauncher.Avalonia.Domain.Interfaces;

namespace TeaLauncher.Avalonia.Infrastructure.UI;

/// <summary>
/// Production implementation of IDialogService using Avalonia UI framework.
/// Provides message, confirmation, and error dialogs with async support.
/// Handles headless mode gracefully by detecting when no window owner is available.
/// </summary>
public class AvaloniaDialogService : IDialogService
{
    /// <summary>
    /// Shows an informational message dialog to the user with an OK button.
    /// In headless mode (no window owner), returns immediately without displaying.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message content to display.</param>
    /// <returns>A task that completes when the user dismisses the dialog.</returns>
    public async Task ShowMessageAsync(string title, string message)
    {
        await ShowDialogAsync(title, message, DialogType.Message);
    }

    /// <summary>
    /// Shows a confirmation dialog to the user with Yes/No buttons.
    /// In headless mode (no window owner), returns true as default response.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The confirmation question to display.</param>
    /// <returns>A task that returns true if the user clicked Yes, false if the user clicked No.</returns>
    public async Task<bool> ShowConfirmAsync(string title, string message)
    {
        return await ShowDialogAsync(title, message, DialogType.Confirm);
    }

    /// <summary>
    /// Shows an error message dialog to the user with an OK button and error styling.
    /// In headless mode (no window owner), returns immediately without displaying.
    /// </summary>
    /// <param name="title">The title of the error dialog.</param>
    /// <param name="message">The error message to display.</param>
    /// <returns>A task that completes when the user dismisses the dialog.</returns>
    public async Task ShowErrorAsync(string title, string message)
    {
        await ShowDialogAsync(title, message, DialogType.Error);
    }

    /// <summary>
    /// Internal method to show a dialog of the specified type.
    /// Handles async marshalling to UI thread and graceful degradation in headless mode.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="dialogType">The type of dialog to show (Message, Confirm, or Error).</param>
    /// <returns>A task that returns true for Yes in confirmation dialogs, false otherwise.</returns>
    private async Task<bool> ShowDialogAsync(string title, string message, DialogType dialogType)
    {
        // Check if we're in headless mode (no active window available)
        Window? mainWindow = GetActiveWindow();

        if (mainWindow == null)
        {
            // Headless mode: cannot show dialogs, return default values
            // Confirm dialogs default to true, others just return
            return dialogType == DialogType.Confirm;
        }

        // Marshal to UI thread and show dialog
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return await ShowDialogInternalAsync(mainWindow, title, message, dialogType);
        });
    }

    /// <summary>
    /// Shows the actual dialog window on the UI thread.
    /// Must be called from the UI thread.
    /// </summary>
    /// <param name="owner">The owner window for the dialog.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="dialogType">The type of dialog to show.</param>
    /// <returns>A task that returns true for Yes in confirmation dialogs, false otherwise.</returns>
    private async Task<bool> ShowDialogInternalAsync(Window owner, string title, string message, DialogType dialogType)
    {
        bool result = false;

        var dialog = new Window
        {
            Title = title,
            Width = 500,
            Height = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20
        };

        // Add message text
        var textBlock = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Apply error styling if needed
        if (dialogType == DialogType.Error)
        {
            textBlock.Foreground = Brushes.Red;
            textBlock.FontWeight = FontWeight.Bold;
        }

        panel.Children.Add(textBlock);

        // Add buttons based on dialog type
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10
        };

        if (dialogType == DialogType.Confirm)
        {
            // Yes/No buttons for confirmation
            var yesButton = new Button
            {
                Content = "Yes",
                Width = 80,
                Height = 30
            };
            yesButton.Click += (_, _) =>
            {
                result = true;
                dialog.Close();
            };

            var noButton = new Button
            {
                Content = "No",
                Width = 80,
                Height = 30
            };
            noButton.Click += (_, _) =>
            {
                result = false;
                dialog.Close();
            };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
        }
        else
        {
            // OK button for message and error dialogs
            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30
            };
            okButton.Click += (_, _) =>
            {
                dialog.Close();
            };

            buttonPanel.Children.Add(okButton);
        }

        panel.Children.Add(buttonPanel);
        dialog.Content = panel;

        // Show as modal dialog
        await dialog.ShowDialog(owner);

        return result;
    }

    /// <summary>
    /// Gets the active window to use as dialog owner.
    /// Returns null if no window is available (headless mode).
    /// </summary>
    /// <returns>The active window or null if in headless mode.</returns>
    private Window? GetActiveWindow()
    {
        // Try to get the main window from the application
        if (global::Avalonia.Application.Current?.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }

    /// <summary>
    /// Enumeration of dialog types.
    /// </summary>
    private enum DialogType
    {
        Message,
        Confirm,
        Error
    }
}
