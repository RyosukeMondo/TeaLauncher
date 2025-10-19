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
using System.Linq;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Domain.Models;

namespace TeaLauncher.Avalonia.Application.Services;

/// <summary>
/// Service responsible for managing command registration and lookup.
/// Implements the command registry functionality extracted from CommandManager,
/// following the Single Responsibility Principle.
/// </summary>
public class CommandRegistryService : ICommandRegistry
{
    private readonly IAutoCompleter _autoCompleter;
    private readonly List<Command> _commands = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRegistryService"/> class.
    /// </summary>
    /// <param name="autoCompleter">The auto-completer service to synchronize command names with.</param>
    public CommandRegistryService(IAutoCompleter autoCompleter)
    {
        _autoCompleter = autoCompleter ?? throw new ArgumentNullException(nameof(autoCompleter));
    }

    /// <inheritdoc />
    public void RegisterCommand(Command command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Command name cannot be null or whitespace.", nameof(command));
        }

        // Remove existing command with the same name (case-insensitive) to replace it
        _commands.RemoveAll(cmd =>
            string.Equals(cmd.Name, command.Name, StringComparison.OrdinalIgnoreCase));

        // Add the new command
        _commands.Add(command);

        // Update auto-completer with current command names
        UpdateAutoCompleter();
    }

    /// <inheritdoc />
    public bool RemoveCommand(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return false;
        }

        // Remove command with case-insensitive comparison
        int removedCount = _commands.RemoveAll(cmd =>
            string.Equals(cmd.Name, commandName, StringComparison.OrdinalIgnoreCase));

        if (removedCount > 0)
        {
            // Update auto-completer with current command names
            UpdateAutoCompleter();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void ClearCommands()
    {
        _commands.Clear();

        // Clear auto-completer word list
        _autoCompleter.UpdateWordList(Array.Empty<string>());
    }

    /// <inheritdoc />
    public bool HasCommand(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return false;
        }

        // Case-insensitive command lookup
        return _commands.Exists(cmd =>
            string.Equals(cmd.Name, commandName, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public IReadOnlyList<Command> GetAllCommands()
    {
        return _commands.AsReadOnly();
    }

    /// <summary>
    /// Updates the auto-completer with the current list of command names.
    /// </summary>
    private void UpdateAutoCompleter()
    {
        var commandNames = _commands.Select(cmd => cmd.Name).ToList();
        _autoCompleter.UpdateWordList(commandNames);
    }
}
