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
using System.ComponentModel;
using TeaLauncher.Avalonia.Domain.Interfaces;

namespace TeaLauncher.Avalonia.Application.Services;

/// <summary>
/// Service responsible for executing commands.
/// Implements the command execution functionality extracted from CommandManager,
/// following the Single Responsibility Principle.
/// </summary>
public class CommandExecutorService : ICommandExecutor
{
    private readonly ICommandRegistry _commandRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandExecutorService"/> class.
    /// </summary>
    /// <param name="commandRegistry">The command registry service to look up commands.</param>
    public CommandExecutorService(ICommandRegistry commandRegistry)
    {
        _commandRegistry = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(string commandInput)
    {
        if (string.IsNullOrWhiteSpace(commandInput))
        {
            throw new ArgumentException("Command input cannot be null or empty.", nameof(commandInput));
        }

        await Task.Run(() =>
        {
            ValidateNotSpecialCommand(commandInput);
            var (filename, args) = ResolveCommandTarget(commandInput);
            ExecuteProcess(filename, args);
        });
    }

    /// <summary>
    /// Validates that the command is not a special command that should be handled elsewhere.
    /// </summary>
    private void ValidateNotSpecialCommand(string commandInput)
    {
        string commandName = GetExecution(commandInput);
        if (IsSpecialCommand(commandName))
        {
            Debug.WriteLine($"SpecialCommand: {commandName}");
            throw new NotSupportedException($"Special commands like '{commandName}' must be handled by ApplicationOrchestrator.");
        }
    }

    /// <summary>
    /// Resolves the command input to a filename and arguments.
    /// </summary>
    private (string filename, string args) ResolveCommandTarget(string commandInput)
    {
        if (IsPath(commandInput))
        {
            return ResolveDirectPath(commandInput);
        }
        else
        {
            return ResolveRegisteredCommand(commandInput);
        }
    }

    /// <summary>
    /// Resolves a direct path or URL command.
    /// </summary>
    private (string filename, string args) ResolveDirectPath(string commandInput)
    {
        string filename = GetExecution(commandInput);
        string args = string.Join(" ", GetArguments(commandInput).ToArray());
        return (filename, args);
    }

    /// <summary>
    /// Resolves a registered command from the command registry.
    /// </summary>
    private (string filename, string args) ResolveRegisteredCommand(string commandInput)
    {
        string commandName = GetExecution(commandInput);
        var commands = _commandRegistry.GetAllCommands();
        var foundCommand = commands.FirstOrDefault(cmd =>
            string.Equals(cmd.Name, commandName, StringComparison.OrdinalIgnoreCase));

        if (foundCommand == null)
        {
            throw new InvalidOperationException($"Command '{commandName}' not found in registry.");
        }

        string filename = GetExecution(foundCommand.LinkTo);
        if (IsSpecialCommand(filename))
        {
            Debug.WriteLine($"SpecialCommand from registry: {filename}");
            throw new NotSupportedException($"Special commands like '{filename}' must be handled by ApplicationOrchestrator.");
        }

        string args = BuildCommandArguments(foundCommand, commandInput);
        return (filename, args);
    }

    /// <summary>
    /// Builds the combined arguments from command definition and input.
    /// </summary>
    private string BuildCommandArguments(CommandLauncher.Command foundCommand, string commandInput)
    {
        var commandArgs = GetArguments(foundCommand.LinkTo).ToList();
        var inputArgs = GetArguments(commandInput).ToList();

        if (!string.IsNullOrWhiteSpace(foundCommand.Arguments))
        {
            commandArgs.AddRange(Split(foundCommand.Arguments));
        }

        commandArgs.AddRange(inputArgs);
        return string.Join(" ", commandArgs);
    }

    /// <summary>
    /// Executes a process with the given filename and arguments.
    /// </summary>
    private void ExecuteProcess(string filename, string args)
    {
        Debug.WriteLine($"Execute: {filename} {args}");

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = args,
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to execute command '{filename}'. The system cannot find the file specified or access is denied.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to execute command '{filename}': {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public string GetExecution(string commandInput)
    {
        if (string.IsNullOrWhiteSpace(commandInput))
        {
            throw new ArgumentException("Command input cannot be null or empty.", nameof(commandInput));
        }

        var parts = Split(commandInput);
        if (parts.Count >= 1)
        {
            return parts[0];
        }

        return string.Empty;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetArguments(string commandInput)
    {
        if (string.IsNullOrWhiteSpace(commandInput))
        {
            return new List<string>().AsReadOnly();
        }

        var parts = Split(commandInput);
        var execution = GetExecution(commandInput);
        parts.Remove(execution);
        return parts.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified string is a direct path or URL.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string is a path or URL; otherwise, false.</returns>
    private bool IsPath(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        // Check for URL schemes
        if (str.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return true;
        if (str.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return true;
        if (str.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
            return true;

        // Check for Windows drive letter path (e.g., C:\)
        if (str.Length >= 3)
        {
            if (str[1] == ':' && str[2] == '\\')
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified string is a special command (starts with !).
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string is a special command; otherwise, false.</returns>
    private bool IsSpecialCommand(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        if (str.Length >= 2 && str[0] == '!')
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Splits a command string into parts, respecting quoted strings.
    /// </summary>
    /// <param name="str">The string to split.</param>
    /// <returns>A list of string parts.</returns>
    private List<string> Split(string str)
    {
        var args = new List<string>();

        if (string.IsNullOrWhiteSpace(str))
        {
            return args;
        }

        var workStr = string.Empty;
        var isInQuotes = false;

        for (int i = 0; i < str.Length; i++)
        {
            switch (str[i])
            {
                case ' ':
                    if (!isInQuotes)
                    {
                        if (!string.IsNullOrEmpty(workStr))
                        {
                            args.Add(workStr);
                            workStr = string.Empty;
                        }
                    }
                    else
                    {
                        workStr += " ";
                    }
                    break;

                case '"':
                case '\'':
                    isInQuotes = !isInQuotes;
                    break;

                default:
                    workStr += str[i];
                    break;
            }
        }

        // Add the last part
        if (!string.IsNullOrEmpty(workStr))
        {
            args.Add(workStr);
        }

        return args;
    }
}
