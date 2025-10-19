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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommandLauncher;
using TeaLauncher.Avalonia.Configuration;

#if WINDOWS
using TeaLauncher.Avalonia.Platform;
using ModifierKeys = TeaLauncher.Avalonia.Platform.ModifierKeys;
#endif

namespace TeaLauncher.Avalonia.Views;

public partial class MainWindow : Window, ICommandManagerInitializer, ICommandManagerFinalizer, ICommandManagerDialogShower
{
    private const string LICENSE = @"TeaLauncher. Simple command launcher.
Copyright (C) Toshiyuki Hirooka <toshi.hirooka@gmail.com> http://wasabi.in/

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.";

    private readonly CommandManager _commandManager;
    private readonly YamlConfigLoader _configLoader;
    private string _configFileName;
    private AutoCompleteBox? _commandBox;

#if WINDOWS
    private WindowsHotkey? _hotkey;
    private WindowsIMEController? _imeController;
#endif

    public MainWindow() : this("commands.yaml")
    {
    }

    public MainWindow(string configFileName)
    {
        _commandManager = new CommandManager(this, this, this);
        _configLoader = new YamlConfigLoader();
        _configFileName = configFileName;

        InitializeComponent();
        InitializeControls();
        InitializeConfiguration();
        InitializeHotkey();

        // Hide window on startup
        Opened += (s, e) => HideWindow();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeControls()
    {
        _commandBox = this.FindControl<AutoCompleteBox>("CommandBox");

        if (_commandBox != null)
        {
            // Wire up keyboard event handler
            _commandBox.KeyDown += CommandBox_KeyDown;

            // Handle text changed for auto-completion
            _commandBox.TextFilter = (search, item) =>
            {
                // Use CommandManager's autocomplete logic instead
                return true;
            };
        }
    }

    private void InitializeConfiguration()
    {
        try
        {
            LoadConfigFile(_configFileName);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load configuration file '{_configFileName}':\n{ex.Message}");
        }
    }

    private void InitializeHotkey()
    {
#if WINDOWS
        try
        {
            // Wait for window to be fully created before getting handle
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
#if DEBUG
                    _hotkey = new WindowsHotkey(this, ModifierKeys.Alt, Key.Space);
#else
                    _hotkey = new WindowsHotkey(this, ModifierKeys.Control, Key.Space);
#endif
                    _hotkey.HotKeyPressed += Hotkey_Pressed;
                }
                catch (Exception ex)
                {
                    ShowError($"Failed to register hotkey:\n{ex.Message}");
                }
            }, DispatcherPriority.ApplicationIdle);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to initialize hotkey:\n{ex.Message}");
        }
#endif
    }

    private void LoadConfigFile(string fileName)
    {
        _configFileName = fileName;
        _commandManager.ClearCommands();

        var config = _configLoader.LoadConfigFile(fileName);

        foreach (var entry in config.Commands)
        {
            var cmd = new Command
            {
                command = entry.Name,
                execution = entry.LinkTo
            };
            _commandManager.RegisterCommand(cmd);
        }
    }

    private void CommandBox_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Tab:
                e.Handled = true;
                CompleteCommand();
                break;

            case Key.Enter:
                e.Handled = true;
                ExecuteCommand();
                break;

            case Key.Escape:
                e.Handled = true;
                HandleEscape();
                break;
        }
    }

    private void CompleteCommand()
    {
        if (_commandBox == null)
            return;

        string inputText = _commandBox.Text ?? string.Empty;
        List<string> candidates = _commandManager.GetCandidates(inputText);
        string completedText = _commandManager.AutoCompleteWord(inputText);

        if (candidates.Count != 0)
        {
            // Update the text with the completed version
            _commandBox.Text = completedText;

            // If there are multiple candidates, show them in the dropdown
            if (candidates.Count >= 2)
            {
                _commandBox.ItemsSource = candidates;
                _commandBox.IsDropDownOpen = true;
            }
            else
            {
                // Only one candidate, close dropdown
                _commandBox.IsDropDownOpen = false;
                _commandBox.ItemsSource = null;
            }

            // Move cursor to end
            if (_commandBox.Text != null)
            {
                // Set selection to end of text
                Dispatcher.UIThread.Post(() =>
                {
                    var textBox = FindTextBoxInAutoCompleteBox(_commandBox);
                    if (textBox != null)
                    {
                        textBox.SelectionStart = _commandBox.Text.Length;
                        textBox.SelectionEnd = _commandBox.Text.Length;
                    }
                });
            }
        }
        else
        {
            // No candidates, clear dropdown
            _commandBox.IsDropDownOpen = false;
            _commandBox.ItemsSource = null;
        }
    }

    private TextBox? FindTextBoxInAutoCompleteBox(AutoCompleteBox autoCompleteBox)
    {
        // Find the internal TextBox in the AutoCompleteBox
        return autoCompleteBox.GetVisualDescendants()
            .OfType<TextBox>()
            .FirstOrDefault();
    }

    private void ExecuteCommand()
    {
        if (_commandBox == null)
            return;

        string command = _commandBox.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(command))
            return;

        if (_commandManager.HasCommand(command))
        {
            Debug.WriteLine($"Run: {command}");

            ClearCommandBox();
            HideWindow();

            try
            {
                _commandManager.Run(command);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to execute command '{command}':\n{ex.Message}");
            }
        }
    }

    private void HandleEscape()
    {
        if (_commandBox == null)
            return;

        if (!string.IsNullOrEmpty(_commandBox.Text))
        {
            // Clear the input
            ClearCommandBox();
        }
        else
        {
            // Hide the window
            HideWindow();
        }
    }

    private void ClearCommandBox()
    {
        if (_commandBox == null)
            return;

        _commandBox.Text = string.Empty;
        _commandBox.IsDropDownOpen = false;
        _commandBox.ItemsSource = null;
    }

    private void ShowWindow()
    {
#if WINDOWS
        // Reset IME state (Off -> On -> Off) to ensure alphanumeric input
        if (_imeController == null)
        {
            var handle = this.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (handle != IntPtr.Zero)
            {
                _imeController = new WindowsIMEController(handle);
            }
        }

        if (_imeController != null)
        {
            try
            {
                _imeController.Off();
                _imeController.On();
                _imeController.Off();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IME control failed: {ex.Message}");
            }
        }
#endif

        this.Show();
    }

    private void HideWindow()
    {
        ClearCommandBox();
        this.Hide();
    }

    private bool IsWindowShown()
    {
        return this.IsVisible;
    }

#if WINDOWS
    private void Hotkey_Pressed(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (IsWindowShown())
            {
                if (!this.IsActive)
                {
                    this.Activate();
                }
                else
                {
                    HideWindow();
                }
            }
            else
            {
                ShowWindow();
                this.Activate();

                // Focus the command box
                if (_commandBox != null)
                {
                    _commandBox.Focus();
                }
            }
        });
    }
#endif

    // ICommandManagerInitializer implementation
    public void Reinitialize()
    {
        try
        {
            LoadConfigFile(_configFileName);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to reload configuration:\n{ex.Message}");
        }
    }

    // ICommandManagerFinalizer implementation
    public void Exit()
    {
#if WINDOWS
        _hotkey?.Dispose();
        _imeController?.Dispose();
#endif
        Close();
    }

    // ICommandManagerDialogShower implementation
    public void ShowVersionInfo()
    {
        // Use Environment.ProcessPath for single-file publish compatibility
        // Assembly.Location returns empty string in single-file apps
#pragma warning disable IL3000
        string executablePath = Environment.ProcessPath ??
            System.Reflection.Assembly.GetExecutingAssembly().Location;
#pragma warning restore IL3000

        System.Diagnostics.FileVersionInfo version =
            System.Diagnostics.FileVersionInfo.GetVersionInfo(executablePath);

        string message = $"{version.FileDescription} version {version.FileVersion}\n" +
                        "--------------------------------\n" +
                        LICENSE;

        ShowMessageBox(version.FileDescription ?? "TeaLauncher", message);
    }

    public void ShowError(string message)
    {
        ShowMessageBox("Error", message);
    }

    private void ShowMessageBox(string title, string message)
    {
        // Use Avalonia's message box
        Dispatcher.UIThread.Post(async () =>
        {
            var msgBox = new Window
            {
                Title = title,
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false,
                Content = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(20)
                }
            };

            await msgBox.ShowDialog(this);
        });
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

#if WINDOWS
        _hotkey?.Dispose();
        _imeController?.Dispose();
#endif
    }
}
