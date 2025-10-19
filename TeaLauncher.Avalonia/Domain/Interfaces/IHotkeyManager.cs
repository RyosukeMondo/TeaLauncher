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
using Avalonia.Input;

namespace TeaLauncher.Avalonia.Domain.Interfaces;

/// <summary>
/// Defines the contract for managing global hotkey registration.
/// This interface represents the hotkey management responsibility, extracted from WindowsHotkey.
/// </summary>
public interface IHotkeyManager
{
    /// <summary>
    /// Registers a global hotkey with the specified key and modifiers.
    /// </summary>
    /// <param name="key">The key to register.</param>
    /// <param name="modifiers">The modifier keys (e.g., Ctrl, Alt, Shift).</param>
    /// <param name="callback">The action to execute when the hotkey is pressed.</param>
    /// <exception cref="System.InvalidOperationException">Thrown when hotkey registration fails.</exception>
    void RegisterHotkey(Key key, KeyModifiers modifiers, Action callback);

    /// <summary>
    /// Unregisters the currently registered hotkey.
    /// </summary>
    void UnregisterHotkey();

    /// <summary>
    /// Gets a value indicating whether a hotkey is currently registered.
    /// </summary>
    bool IsRegistered { get; }
}
