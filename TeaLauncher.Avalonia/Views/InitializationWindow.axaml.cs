using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using TeaLauncher.Avalonia.Infrastructure.Initialization;

namespace TeaLauncher.Avalonia.Views;

/// <summary>
/// Window shown on first launch to guide users through initial setup.
/// </summary>
public partial class InitializationWindow : Window
{
    private readonly InitializationService _initService;
    private readonly Infrastructure.Settings.SettingsService _settingsService;
    private TextBlock? _configPathText;
    private RadioButton? _ctrlSpaceRadio;
    private RadioButton? _altSpaceRadio;

    /// <summary>
    /// Gets the selected hotkey modifier.
    /// </summary>
    public KeyModifiers SelectedModifier { get; private set; } = KeyModifiers.Control;

    /// <summary>
    /// Gets whether the initialization was completed successfully.
    /// </summary>
    public bool InitializationCompleted { get; private set; }

    /// <summary>
    /// Initializes a new instance of the InitializationWindow.
    /// </summary>
    public InitializationWindow() : this(new InitializationService(), new Infrastructure.Settings.SettingsService())
    {
    }

    /// <summary>
    /// Initializes a new instance of the InitializationWindow with dependency injection.
    /// </summary>
    /// <param name="initService">Service for handling initialization.</param>
    /// <param name="settingsService">Service for handling settings.</param>
    public InitializationWindow(InitializationService initService, Infrastructure.Settings.SettingsService settingsService)
    {
        var logger = Infrastructure.Logging.FileLogger.Instance;
        logger.Info("InitializationWindow constructor called");

        _initService = initService;
        _settingsService = settingsService;

        try
        {
            InitializeComponent();
            logger.Info("InitializationWindow component initialized");

            InitializeControls();
            logger.Info("InitializationWindow controls initialized");

            UpdateConfigPath();
            logger.Info("InitializationWindow config path updated");
        }
        catch (Exception ex)
        {
            logger.Error("Error in InitializationWindow constructor", ex);
            throw;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeControls()
    {
        _configPathText = this.FindControl<TextBlock>("ConfigPathText");
        _ctrlSpaceRadio = this.FindControl<RadioButton>("CtrlSpaceRadio");
        _altSpaceRadio = this.FindControl<RadioButton>("AltSpaceRadio");

        // Set default selection
        if (_ctrlSpaceRadio != null)
        {
            _ctrlSpaceRadio.IsChecked = true;
        }
    }

    private void UpdateConfigPath()
    {
        if (_configPathText != null)
        {
            string fullPath = _initService.GetFullConfigPath();
            _configPathText.Text = fullPath;
        }
    }

    private async void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        var logger = Infrastructure.Logging.FileLogger.Instance;
        logger.Info("Start button clicked");

        try
        {
            // Determine selected hotkey modifier
            if (_altSpaceRadio?.IsChecked == true)
            {
                SelectedModifier = KeyModifiers.Alt;
                logger.Info("Selected hotkey: Alt+Space");
            }
            else
            {
                SelectedModifier = KeyModifiers.Control;
                logger.Info("Selected hotkey: Ctrl+Space");
            }

            // Generate sample config file
            logger.Info("Generating sample config file...");
            _initService.GenerateSampleConfig();

            // Save the selected hotkey to settings
            logger.Info("Saving hotkey preference...");
            _settingsService.SaveHotkeyModifier(SelectedModifier);
            logger.Info("Settings saved successfully");

            // Mark initialization as completed
            InitializationCompleted = true;
            logger.Info("Initialization marked as completed");

            // Close the window
            logger.Info("Closing initialization window");
            Close();
        }
        catch (Exception ex)
        {
            logger.Error("Error during initialization", ex);

            // Show error message
            await ShowErrorDialog("Initialization Error",
                $"Failed to complete initialization:\n\n{ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task ShowErrorDialog(string title, string message)
    {
        var msgBox = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        });

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(20, 5)
        };

        okButton.Click += (s, e) => msgBox.Close();

        panel.Children.Add(okButton);
        msgBox.Content = panel;

        await msgBox.ShowDialog(this);
    }
}
