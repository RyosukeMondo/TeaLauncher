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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommandLauncher;
using TeaLauncher.Avalonia.Configuration;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Infrastructure.Configuration;

#if WINDOWS
using TeaLauncher.Avalonia.Infrastructure.Platform;
using ModifierKeys = TeaLauncher.Avalonia.Infrastructure.Platform.ModifierKeys;
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
    private readonly YamlConfigLoaderService _configLoader;
    private readonly IDialogService _dialogService;
    private string _configFileName;
    private AutoCompleteBox? _commandBox;

#if WINDOWS
    private WindowsHotkeyService? _hotkey;
    private WindowsIMEControllerService? _imeController;
#endif

    /// <summary>
    /// Initializes a new instance of the MainWindow class for design-time use.
    /// </summary>
    public MainWindow() : this("commands.yaml", null!)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MainWindow class with a custom config file path.
    /// </summary>
    /// <param name="configFileName">Path to the configuration file.</param>
    public MainWindow(string configFileName) : this(configFileName, null!)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MainWindow class with dependency injection.
    /// </summary>
    /// <param name="dialogService">Service for displaying dialogs.</param>
    public MainWindow(IDialogService dialogService) : this("commands.yaml", dialogService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    /// <param name="configFileName">Path to the configuration file.</param>
    /// <param name="dialogService">Service for displaying dialogs.</param>
    public MainWindow(string configFileName, IDialogService dialogService)
    {
        var logger = Infrastructure.Logging.FileLogger.Instance;
        logger.Info($"MainWindow constructor: config={configFileName}");

        try
        {
            _commandManager = new CommandManager(this, this, this);
            _configLoader = new YamlConfigLoaderService();
            _dialogService = dialogService;
            _configFileName = configFileName;

            InitializeComponent();
            InitializeControls();
            InitializeConfiguration();
            InitializeHotkey();

            // Hide window on first open only
            bool firstOpen = true;
            Opened += (s, e) => { if (firstOpen) { logger.Info("First window open, hiding"); firstOpen = false; HideWindow(); } };
            logger.Info("MainWindow constructor completed");
        }
        catch (Exception ex)
        {
            logger.Error("Error in MainWindow constructor", ex);
            throw;
        }
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
        var logger = Infrastructure.Logging.FileLogger.Instance;

#if WINDOWS
        logger.Info("Initializing Windows hotkey");

        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    var settingsService = new Infrastructure.Settings.SettingsService();
                    var modifier = settingsService.GetHotkeyModifier();
                    logger.Info($"Registering hotkey: Space+{modifier}");

                    _hotkey = new WindowsHotkeyService();
                    _hotkey.SetWindow(this);
                    _hotkey.RegisterHotkey(Key.Space, modifier, () => Hotkey_Pressed(null, EventArgs.Empty));

                    logger.Info("Hotkey registered successfully");
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to register hotkey", ex);
                    ShowError($"Failed to register hotkey:\n{ex.Message}");
                }
            }, DispatcherPriority.ApplicationIdle);
        }
        catch (Exception ex)
        {
            logger.Error("Failed to initialize hotkey", ex);
            ShowError($"Failed to initialize hotkey:\n{ex.Message}");
        }
#else
        logger.Info("Not on Windows - skipping hotkey");
#endif
    }

    private void LoadConfigFile(string fileName)
    {
        _configFileName = fileName;
        _commandManager.ClearCommands();

        var config = _configLoader.LoadConfiguration(fileName);

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
                _imeController = new WindowsIMEControllerService(handle);
            }
        }

        if (_imeController != null)
        {
            try
            {
                _imeController.DisableIME();
                _imeController.EnableIME();
                _imeController.DisableIME();
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
        Infrastructure.Logging.FileLogger.Instance.Info("Hiding window");
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
        var logger = Infrastructure.Logging.FileLogger.Instance;
        logger.Info("Exit() called, closing window");

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

        // Fire and forget - we don't need to await this
        _ = _dialogService.ShowMessageAsync(version.FileDescription ?? "TeaLauncher", message);
    }

    public void ShowError(string message)
    {
        // Fire and forget - we don't need to await this
        _ = _dialogService.ShowErrorAsync("Error", message);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        var logger = Infrastructure.Logging.FileLogger.Instance;
        logger.Info($"OnClosing called. Close reason: {e.CloseReason}");

        base.OnClosing(e);

#if WINDOWS
        _hotkey?.Dispose();
        _imeController?.Dispose();
        logger.Info("Windows resources disposed in OnClosing");
#endif

        logger.Info("=== MainWindow closing ===");
    }
}