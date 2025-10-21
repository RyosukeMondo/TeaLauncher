using System;
using System.IO;
using System.Text.Json;
using Avalonia.Input;

namespace TeaLauncher.Avalonia.Infrastructure.Settings;

/// <summary>
/// Service for managing application settings.
/// </summary>
public class SettingsService
{
    private const string SettingsFileName = ".tealauncher-settings.json";

    /// <summary>
    /// Application settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Hotkey modifier (Control or Alt).
        /// </summary>
        public string HotkeyModifier { get; set; } = "Control";

        /// <summary>
        /// Gets the KeyModifiers enum value.
        /// </summary>
        public KeyModifiers GetKeyModifiers()
        {
            return HotkeyModifier == "Alt" ? KeyModifiers.Alt : KeyModifiers.Control;
        }
    }

    /// <summary>
    /// Loads settings from disk.
    /// </summary>
    /// <returns>The loaded settings, or default settings if file doesn't exist.</returns>
    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFileName))
            {
                string json = File.ReadAllText(SettingsFileName);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception)
        {
            // If there's any error reading settings, return defaults
        }

        return new AppSettings();
    }

    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    public void SaveSettings(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsFileName, json);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to save settings: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves the hotkey modifier setting.
    /// </summary>
    /// <param name="modifier">The modifier key to save.</param>
    public void SaveHotkeyModifier(KeyModifiers modifier)
    {
        var settings = LoadSettings();
        settings.HotkeyModifier = modifier == KeyModifiers.Alt ? "Alt" : "Control";
        SaveSettings(settings);
    }

    /// <summary>
    /// Gets the configured hotkey modifier, or default (Control) if not configured.
    /// </summary>
    /// <returns>The configured keyboard modifier.</returns>
    public KeyModifiers GetHotkeyModifier()
    {
        var settings = LoadSettings();
        return settings.GetKeyModifiers();
    }
}
