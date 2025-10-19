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
using System.Threading.Tasks;
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
    public Task ExecuteAsync(string commandInput)
    {
        throw new NotImplementedException("Command execution logic will be implemented in task 6");
    }

    /// <inheritdoc />
    public string GetExecution(string commandInput)
    {
        throw new NotImplementedException("Execution target logic will be implemented in task 6");
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetArguments(string commandInput)
    {
        throw new NotImplementedException("Argument parsing logic will be implemented in task 6");
    }
}
