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

using System.Collections.Generic;
using TeaLauncher.Avalonia.Domain.Models;

namespace TeaLauncher.Avalonia.Domain.Interfaces;

/// <summary>
/// Defines the contract for managing command registration and lookup.
/// This interface represents the command registry responsibility, extracted from CommandManager.
/// </summary>
public interface ICommandRegistry
{
    /// <summary>
    /// Registers a new command in the registry.
    /// If a command with the same name already exists, it will be replaced.
    /// </summary>
    /// <param name="command">The command to register.</param>
    void RegisterCommand(Command command);

    /// <summary>
    /// Removes a command from the registry by name.
    /// </summary>
    /// <param name="commandName">The name of the command to remove.</param>
    /// <returns>True if the command was found and removed; otherwise, false.</returns>
    bool RemoveCommand(string commandName);

    /// <summary>
    /// Removes all commands from the registry.
    /// </summary>
    void ClearCommands();

    /// <summary>
    /// Checks if a command with the specified name exists in the registry.
    /// </summary>
    /// <param name="commandName">The name of the command to check.</param>
    /// <returns>True if the command exists; otherwise, false.</returns>
    bool HasCommand(string commandName);

    /// <summary>
    /// Gets all registered commands.
    /// </summary>
    /// <returns>A read-only list of all registered commands.</returns>
    IReadOnlyList<Command> GetAllCommands();
}
