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
using System.IO;
using System.Threading.Tasks;
using TeaLauncher.Avalonia.Configuration;
using TeaLauncher.Avalonia.Domain.Interfaces;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TeaLauncher.Avalonia.Infrastructure.Configuration;

/// <summary>
/// Loads and parses YAML configuration files for TeaLauncher commands.
/// Provides validation and clear error messages for configuration issues.
/// Implements IConfigurationLoader interface for dependency injection.
/// </summary>
public class YamlConfigLoaderService : IConfigurationLoader
{
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// Initializes a new instance of the YamlConfigLoaderService with default settings.
    /// </summary>
    public YamlConfigLoaderService()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Loads a configuration file from the specified path.
    /// </summary>
    /// <param name="filePath">The path to the configuration file.</param>
    /// <returns>A CommandsConfig object containing the parsed configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="YamlException">Thrown when the configuration file contains syntax errors.</exception>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public CommandsConfig LoadConfiguration(string filePath)
    {
        // Check if file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Configuration file not found: {filePath}",
                filePath);
        }

        try
        {
            // Read file content
            string yamlContent = File.ReadAllText(filePath);

            // Deserialize YAML
            CommandsConfig? config = _deserializer.Deserialize<CommandsConfig>(yamlContent);

            // Validate deserialized config
            if (config == null)
            {
                throw new InvalidOperationException(
                    $"Failed to parse YAML configuration from file: {filePath}. " +
                    "The file may be empty or contain invalid YAML structure.");
            }

            // Validate required fields in each command entry
            ValidateConfiguration(config);

            return config;
        }
        catch (YamlException ex)
        {
            // Enhance YAML syntax errors with location information
            string errorMessage = $"YAML syntax error in file '{filePath}'";

            if (ex.Start.Line > 0)
            {
                errorMessage += $" at line {ex.Start.Line}, column {ex.Start.Column}";
            }

            errorMessage += $": {ex.InnerException?.Message ?? ex.Message}";

            throw new YamlException(ex.Start, ex.End, errorMessage, ex.InnerException ?? ex);
        }
        catch (FileNotFoundException)
        {
            // Re-throw file not found exceptions as-is
            throw;
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            // Wrap unexpected exceptions
            throw new InvalidOperationException(
                $"Unexpected error while loading configuration from '{filePath}': {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Asynchronously loads a configuration file from the specified path.
    /// </summary>
    /// <param name="filePath">The path to the configuration file.</param>
    /// <returns>A task representing the asynchronous operation, containing the parsed configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="YamlException">Thrown when the configuration file contains syntax errors.</exception>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public Task<CommandsConfig> LoadConfigurationAsync(string filePath)
    {
        // Wrap synchronous method in Task for async interface
        return Task.Run(() => LoadConfiguration(filePath));
    }

    /// <summary>
    /// Validates the configuration for required fields and proper structure.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    private void ValidateConfiguration(CommandsConfig config)
    {
        // Commands will always have a default value (empty list) if not specified in YAML,
        // so we don't need to check for null

        for (int i = 0; i < config.Commands.Count; i++)
        {
            var command = config.Commands[i];

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                throw new InvalidOperationException(
                    $"Configuration validation failed: Command at index {i} is missing required field 'name'.");
            }

            if (string.IsNullOrWhiteSpace(command.LinkTo))
            {
                throw new InvalidOperationException(
                    $"Configuration validation failed: Command '{command.Name}' at index {i} is missing required field 'linkto'.");
            }
        }
    }
}
