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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TeaLauncher.Avalonia.Application.Exceptions;
using TeaLauncher.Avalonia.Configuration;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Domain.Models;

namespace TeaLauncher.Avalonia.Application.Orchestration;

/// <summary>
/// Orchestrates the application workflow, coordinating between services for initialization,
/// configuration reloading, and special command handling.
/// This class represents the application-level coordination logic extracted from CommandManager.
/// </summary>
public class ApplicationOrchestrator
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly ICommandRegistry _commandRegistry;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IHotkeyManager _hotkeyManager;
    private string? _configFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationOrchestrator"/> class.
    /// </summary>
    /// <param name="configurationLoader">The configuration loader service.</param>
    /// <param name="commandRegistry">The command registry service.</param>
    /// <param name="commandExecutor">The command executor service.</param>
    /// <param name="hotkeyManager">The hotkey manager service.</param>
    public ApplicationOrchestrator(
        IConfigurationLoader configurationLoader,
        ICommandRegistry commandRegistry,
        ICommandExecutor commandExecutor,
        IHotkeyManager hotkeyManager)
    {
        _configurationLoader = configurationLoader ?? throw new ArgumentNullException(nameof(configurationLoader));
        _commandRegistry = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
        _commandExecutor = commandExecutor ?? throw new ArgumentNullException(nameof(commandExecutor));
        _hotkeyManager = hotkeyManager ?? throw new ArgumentNullException(nameof(hotkeyManager));
    }

    /// <summary>
    /// Initializes the application by loading configuration and registering commands.
    /// </summary>
    /// <param name="configPath">The path to the configuration file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when configPath is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when configuration loading fails.</exception>
    public async Task InitializeAsync(string configPath)
    {
        if (string.IsNullOrWhiteSpace(configPath))
            throw new ArgumentException("Configuration path cannot be null or empty.", nameof(configPath));

        try
        {
            _configFilePath = configPath;

            // Load configuration
            var config = await _configurationLoader.LoadConfigurationAsync(configPath);

            // Register all commands from configuration
            foreach (var commandEntry in config.Commands)
            {
                // Convert CommandEntry to Command domain model
                var command = new Command(
                    commandEntry.Name,
                    commandEntry.LinkTo,
                    commandEntry.Description,
                    commandEntry.Arguments);

                _commandRegistry.RegisterCommand(command);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize application from configuration file '{configPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Reloads the configuration by clearing existing commands and re-registering them from the configuration file.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no configuration file path is set or reload fails.</exception>
    public async Task ReloadConfigurationAsync()
    {
        if (string.IsNullOrWhiteSpace(_configFilePath))
            throw new InvalidOperationException("Cannot reload configuration: no configuration file path is set. Call InitializeAsync first.");

        try
        {
            // Clear existing commands
            _commandRegistry.ClearCommands();

            // Reload configuration
            var config = await _configurationLoader.LoadConfigurationAsync(_configFilePath);

            // Re-register all commands
            foreach (var commandEntry in config.Commands)
            {
                // Convert CommandEntry to Command domain model
                var command = new Command(
                    commandEntry.Name,
                    commandEntry.LinkTo,
                    commandEntry.Description,
                    commandEntry.Arguments);

                _commandRegistry.RegisterCommand(command);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to reload configuration from '{_configFilePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Handles special commands that start with '!' prefix.
    /// </summary>
    /// <param name="command">The special command to handle (including the '!' prefix).</param>
    /// <returns>A result message for display commands, or null for action commands.</returns>
    /// <exception cref="ExitApplicationException">Thrown when the !exit command is executed.</exception>
    /// <exception cref="ArgumentException">Thrown when the command is not recognized.</exception>
    public async Task<string?> HandleSpecialCommandAsync(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            throw new ArgumentException("Command cannot be null or empty.", nameof(command));

        if (!command.StartsWith('!'))
            throw new ArgumentException($"Not a special command: '{command}'. Special commands must start with '!'", nameof(command));

        return command.ToLowerInvariant() switch
        {
            "!reload" => await HandleReloadAsync(),
            "!version" => GetVersion(),
            "!exit" => throw new ExitApplicationException("Application exit requested via !exit command"),
            _ => throw new ArgumentException($"Unknown special command: '{command}'. Known commands are: !reload, !version, !exit", nameof(command))
        };
    }

    /// <summary>
    /// Handles the !reload command.
    /// </summary>
    /// <returns>A success message.</returns>
    private async Task<string?> HandleReloadAsync()
    {
        await ReloadConfigurationAsync();
        return "Configuration reloaded successfully";
    }

    /// <summary>
    /// Gets the application version information.
    /// </summary>
    /// <returns>A string containing the version information.</returns>
    public string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var informationalVersion = assembly
            .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()?.InformationalVersion;

        var versionString = informationalVersion ?? version?.ToString() ?? "Unknown";

        return $"TeaLauncher Version {versionString}";
    }
}
