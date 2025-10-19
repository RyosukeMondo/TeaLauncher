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
        throw new NotImplementedException("Command registration logic will be implemented in task 5");
    }

    /// <inheritdoc />
    public bool RemoveCommand(string commandName)
    {
        throw new NotImplementedException("Command removal logic will be implemented in task 5");
    }

    /// <inheritdoc />
    public void ClearCommands()
    {
        throw new NotImplementedException("Command clearing logic will be implemented in task 5");
    }

    /// <inheritdoc />
    public bool HasCommand(string commandName)
    {
        throw new NotImplementedException("Command lookup logic will be implemented in task 5");
    }

    /// <inheritdoc />
    public IReadOnlyList<Command> GetAllCommands()
    {
        throw new NotImplementedException("Command retrieval logic will be implemented in task 5");
    }
}
