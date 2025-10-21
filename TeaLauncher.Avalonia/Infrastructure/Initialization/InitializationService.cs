using System;
using System.IO;

namespace TeaLauncher.Avalonia.Infrastructure.Initialization;

/// <summary>
/// Service responsible for first-time initialization of TeaLauncher.
/// </summary>
public class InitializationService
{
    private const string DefaultConfigFileName = "commands.yaml";
    private const string SampleConfigContent = @"# TeaLauncher Commands Configuration
# This YAML file defines the commands available in TeaLauncher.
# Each command has a name (keyword) and a linkto (target URL/executable/path).
# Optional fields: description and arguments

commands:
  # Web URLs - Open websites in default browser
  - name: google
    linkto: https://www.google.com/
    description: Open Google search

  - name: github
    linkto: https://github.com/
    description: Open GitHub homepage

  # Applications - Launch executables
  - name: notepad
    linkto: notepad
    description: Open Windows Notepad

  - name: cmd
    linkto: cmd.exe
    description: Open Command Prompt

  - name: explorer
    linkto: explorer.exe
    description: Open Windows Explorer

  # File paths - Open specific files or folders
  - name: edit_config
    linkto: notepad
    arguments: commands.yaml
    description: Edit this configuration file

  # Special commands - Built-in TeaLauncher commands
  - name: reload
    linkto: ""!reload""
    description: Reload commands.yaml without restarting

  - name: version
    linkto: ""!version""
    description: Show TeaLauncher version information

  - name: exit
    linkto: ""!exit""
    description: Exit TeaLauncher application

# YAML Syntax Guide:
# - Use 2-space indentation (no tabs)
# - Strings with special characters should be quoted
# - Required fields: name, linkto
# - Optional fields: description, arguments
#
# Example with arguments:
# - name: myapp
#   linkto: C:\Program Files\MyApp\myapp.exe
#   arguments: --flag value
#   description: Launch MyApp with specific flags
";

    /// <summary>
    /// Checks if initialization is needed (i.e., config file doesn't exist).
    /// </summary>
    /// <param name="configFilePath">Path to the configuration file. If null, uses default.</param>
    /// <returns>True if initialization is needed, false otherwise.</returns>
    public bool IsInitializationNeeded(string? configFilePath = null)
    {
        string path = configFilePath ?? DefaultConfigFileName;
        return !File.Exists(path);
    }

    /// <summary>
    /// Generates a sample configuration file.
    /// </summary>
    /// <param name="configFilePath">Path where to create the config file. If null, uses default.</param>
    /// <exception cref="IOException">Thrown when file creation fails.</exception>
    public void GenerateSampleConfig(string? configFilePath = null)
    {
        string path = configFilePath ?? DefaultConfigFileName;

        try
        {
            // Get the directory path
            string? directory = Path.GetDirectoryName(path);

            // Create directory if it doesn't exist and path has a directory component
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write the sample config
            File.WriteAllText(path, SampleConfigContent);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to create configuration file at '{path}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the default configuration file path.
    /// </summary>
    /// <returns>The default configuration file path.</returns>
    public string GetDefaultConfigPath()
    {
        return DefaultConfigFileName;
    }

    /// <summary>
    /// Gets the full path to the configuration file in the current directory.
    /// </summary>
    /// <param name="configFilePath">Path to the configuration file. If null, uses default.</param>
    /// <returns>Full path to the configuration file.</returns>
    public string GetFullConfigPath(string? configFilePath = null)
    {
        string path = configFilePath ?? DefaultConfigFileName;
        return Path.GetFullPath(path);
    }
}
