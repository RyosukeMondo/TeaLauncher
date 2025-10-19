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

namespace TeaLauncher.Avalonia.Domain.Models;

/// <summary>
/// Represents a command that can be executed by TeaLauncher.
/// This is the domain model for commands, extracted from the legacy CommandManager implementation.
/// </summary>
/// <param name="Name">The command keyword/name used to trigger the command (e.g., "google", "notepad").</param>
/// <param name="LinkTo">The target URL, file path, or executable path to launch.</param>
/// <param name="Description">Optional description of the command for documentation purposes.</param>
/// <param name="Arguments">Optional command-line arguments to pass to the executable.</param>
public record Command(
    string Name,
    string LinkTo,
    string? Description = null,
    string? Arguments = null
);
