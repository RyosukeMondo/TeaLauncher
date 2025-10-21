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
        var resultHolder = new DialogResult();

        var dialog = CreateDialogWindow(title);
        var contentPanel = ConfigureDialogContent(message, dialogType);
        var buttonPanel = CreateDialogButtons(dialogType, dialog, resultHolder);

        contentPanel.Children.Add(buttonPanel);
        dialog.Content = contentPanel;

        // Show as modal dialog
        await dialog.ShowDialog(owner);

        return resultHolder.Value;
    }

    /// <summary>
    /// Creates the base dialog window with standard properties.
    /// </summary>
    /// <param name="title">The title for the dialog window.</param>
    /// <returns>A configured Window instance ready for content.</returns>
    private Window CreateDialogWindow(string title)
    {
        return new Window
        {
            Title = title,
            Width = 500,
            Height = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false
        };
    }

    /// <summary>
    /// Configures the dialog content panel with message text and appropriate styling.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="dialogType">The dialog type for conditional styling.</param>
    /// <returns>A StackPanel containing the styled message content.</returns>
    private StackPanel ConfigureDialogContent(string message, DialogType dialogType)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20
        };

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

        return panel;
    }

    /// <summary>
    /// Creates the button panel with event handlers based on dialog type.
    /// </summary>
    /// <param name="dialogType">The dialog type determining button configuration.</param>
    /// <param name="dialog">The dialog window to close on button click.</param>
    /// <param name="resultHolder">Holder for result value modified by button clicks.</param>
    /// <returns>A StackPanel containing the configured buttons.</returns>
    private StackPanel CreateDialogButtons(DialogType dialogType, Window dialog, DialogResult resultHolder)
    {
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10
        };

        if (dialogType == DialogType.Confirm)
        {
            AddConfirmButtons(buttonPanel, dialog, resultHolder);
        }
        else
        {
            AddOkButton(buttonPanel, dialog);
        }

        return buttonPanel;
    }

    /// <summary>
    /// Adds Yes/No buttons to the button panel for confirmation dialogs.
    /// </summary>
    /// <param name="buttonPanel">The panel to add buttons to.</param>
    /// <param name="dialog">The dialog window to close on button click.</param>
    /// <param name="resultHolder">Holder for result value modified by button clicks.</param>
    private void AddConfirmButtons(StackPanel buttonPanel, Window dialog, DialogResult resultHolder)
    {
        var yesButton = new Button
        {
            Content = "Yes",
            Width = 80,
            Height = 30
        };
        yesButton.Click += (_, _) =>
        {
            resultHolder.Value = true;
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
            resultHolder.Value = false;
            dialog.Close();
        };

        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);
    }

    /// <summary>
    /// Adds an OK button to the button panel for message and error dialogs.
    /// </summary>
    /// <param name="buttonPanel">The panel to add button to.</param>
    /// <param name="dialog">The dialog window to close on button click.</param>
    private void AddOkButton(StackPanel buttonPanel, Window dialog)
    {
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
    /// Helper class to hold dialog result value for lambda closures.
    /// </summary>
    private class DialogResult
    {
        public bool Value { get; set; }
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
