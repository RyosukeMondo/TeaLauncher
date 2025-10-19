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
using System.Threading.Tasks;

namespace TeaLauncher.Avalonia.Domain.Interfaces;

/// <summary>
/// Defines the contract for executing commands.
/// This interface represents the command execution responsibility, extracted from CommandManager.
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Executes a command asynchronously.
    /// The command can be a registered command name, a direct URL, or a file path.
    /// </summary>
    /// <param name="commandInput">The command input string (command name or direct path/URL).</param>
    /// <returns>A task representing the asynchronous execution operation.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when the command cannot be executed.</exception>
    Task ExecuteAsync(string commandInput);

    /// <summary>
    /// Gets the execution target (file path or URL) for a given command or input.
    /// </summary>
    /// <param name="commandInput">The command input string.</param>
    /// <returns>The execution target path or URL.</returns>
    string GetExecution(string commandInput);

    /// <summary>
    /// Gets the arguments for a given command or input.
    /// </summary>
    /// <param name="commandInput">The command input string.</param>
    /// <returns>A list of arguments to pass to the execution target.</returns>
    IReadOnlyList<string> GetArguments(string commandInput);
}
