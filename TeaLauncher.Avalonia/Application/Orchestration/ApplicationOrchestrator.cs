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
using TeaLauncher.Avalonia.Domain.Interfaces;

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

    // Orchestration methods will be implemented in task 8
}
