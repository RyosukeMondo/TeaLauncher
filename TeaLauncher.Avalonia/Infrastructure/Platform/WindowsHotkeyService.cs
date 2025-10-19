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

#if WINDOWS

using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using TeaLauncher.Avalonia.Domain.Interfaces;

namespace TeaLauncher.Avalonia.Infrastructure.Platform;

/// <summary>
/// Windows global hotkey registration service implementing IHotkeyManager.
/// Registers system-wide hotkeys using Windows user32.dll APIs.
/// Must be disposed to unregister the hotkey.
/// </summary>
public class WindowsHotkeyService : IHotkeyManager, IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int RegisterHotKey(IntPtr hWnd, int id, ModifierKeys modifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private const int GWL_WNDPROC = -4;
    private const int WM_HOTKEY = 0x0312;
    private const int MIN_HOTKEY_ID = 0x0000;
    private const int MAX_HOTKEY_ID = 0xBFFF;

    private IntPtr _hwnd;
    private int _hotkeyId = -1;
    private bool _disposed = false;
    private WndProcDelegate? _newWndProc;
    private IntPtr _oldWndProc = IntPtr.Zero;
    private Action? _callback;
    private Window? _window;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// Gets a value indicating whether a hotkey is currently registered.
    /// </summary>
    public bool IsRegistered => _hotkeyId >= 0;

    /// <summary>
    /// Registers a global hotkey with the specified key and modifiers.
    /// </summary>
    /// <param name="key">The key to register.</param>
    /// <param name="modifiers">The modifier keys (e.g., Ctrl, Alt, Shift).</param>
    /// <param name="callback">The action to execute when the hotkey is pressed.</param>
    /// <exception cref="InvalidOperationException">Thrown when hotkey registration fails.</exception>
    public void RegisterHotkey(Key key, KeyModifiers modifiers, Action callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        _callback = callback;

        // For initial registration, we need a window handle
        // This will be set when the window is available
        if (_window == null)
        {
            throw new InvalidOperationException("Window must be set before registering hotkey. Use SetWindow method.");
        }

        RegisterHotkeyInternal(key, modifiers);
    }

    /// <summary>
    /// Sets the window for hotkey registration. Must be called before RegisterHotkey.
    /// </summary>
    /// <param name="window">The Avalonia window to receive hotkey messages.</param>
    public void SetWindow(Window window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        _window = window;

        // Get the native Windows handle (HWND) from the Avalonia window
        var platformHandle = window.TryGetPlatformHandle();
        if (platformHandle == null)
        {
            throw new InvalidOperationException("Failed to get platform handle from window. Ensure the window is initialized.");
        }

        _hwnd = platformHandle.Handle;
        if (_hwnd == IntPtr.Zero)
        {
            throw new InvalidOperationException("Window handle is zero. Ensure the window is created before registering hotkey.");
        }
    }

    private void RegisterHotkeyInternal(Key key, KeyModifiers modifiers)
    {
        // Convert Avalonia KeyModifiers to Windows ModifierKeys
        ModifierKeys winModifiers = ConvertKeyModifiers(modifiers);

        // Convert Avalonia Key to Windows virtual key code
        int vkCode = ConvertKeyToVirtualKey(key);

        // Try to register the hotkey with different IDs in the valid range
        bool registered = false;
        for (int id = MIN_HOTKEY_ID; id <= MAX_HOTKEY_ID; id++)
        {
            if (RegisterHotKey(_hwnd, id, winModifiers, vkCode) != 0)
            {
                _hotkeyId = id;
                registered = true;
                break;
            }
        }

        if (!registered)
        {
            throw new InvalidOperationException(
                $"Failed to register hotkey {modifiers}+{key}. All hotkey IDs in range 0x{MIN_HOTKEY_ID:X4}-0x{MAX_HOTKEY_ID:X4} are in use.");
        }

        // Hook into Win32 message loop to intercept WM_HOTKEY messages
        _newWndProc = new WndProcDelegate(WndProc);
        _oldWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
    }

    /// <summary>
    /// Unregisters the currently registered hotkey.
    /// </summary>
    public void UnregisterHotkey()
    {
        if (_hotkeyId >= 0 && _hwnd != IntPtr.Zero)
        {
            UnregisterHotKey(_hwnd, _hotkeyId);
            _hotkeyId = -1;
        }

        // Restore the original window procedure
        if (_oldWndProc != IntPtr.Zero && _hwnd != IntPtr.Zero)
        {
            SetWindowLongPtr(_hwnd, GWL_WNDPROC, _oldWndProc);
            _oldWndProc = IntPtr.Zero;
        }

        _callback = null;
    }

    /// <summary>
    /// Window procedure to intercept WM_HOTKEY messages.
    /// </summary>
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (id == _hotkeyId)
            {
                _callback?.Invoke();
            }
        }

        // Call the original window procedure
        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    /// <summary>
    /// Converts Avalonia KeyModifiers to Windows ModifierKeys.
    /// </summary>
    private ModifierKeys ConvertKeyModifiers(KeyModifiers modifiers)
    {
        ModifierKeys result = ModifierKeys.None;

        if ((modifiers & KeyModifiers.Alt) != 0)
            result |= ModifierKeys.Alt;
        if ((modifiers & KeyModifiers.Control) != 0)
            result |= ModifierKeys.Control;
        if ((modifiers & KeyModifiers.Shift) != 0)
            result |= ModifierKeys.Shift;
        if ((modifiers & KeyModifiers.Meta) != 0)
            result |= ModifierKeys.Win;

        return result;
    }

    /// <summary>
    /// Converts Avalonia Key enum to Windows virtual key code.
    /// </summary>
    private int ConvertKeyToVirtualKey(Key key)
    {
        // For most keys, the Avalonia Key enum values match Windows VK codes
        // Space key is a common case for TeaLauncher
        return key switch
        {
            Key.Space => 0x20,  // VK_SPACE
            Key.A => 0x41,
            Key.B => 0x42,
            Key.C => 0x43,
            Key.D => 0x44,
            Key.E => 0x45,
            Key.F => 0x46,
            Key.G => 0x47,
            Key.H => 0x48,
            Key.I => 0x49,
            Key.J => 0x4A,
            Key.K => 0x4B,
            Key.L => 0x4C,
            Key.M => 0x4D,
            Key.N => 0x4E,
            Key.O => 0x4F,
            Key.P => 0x50,
            Key.Q => 0x51,
            Key.R => 0x52,
            Key.S => 0x53,
            Key.T => 0x54,
            Key.U => 0x55,
            Key.V => 0x56,
            Key.W => 0x57,
            Key.X => 0x58,
            Key.Y => 0x59,
            Key.Z => 0x5A,
            Key.D0 => 0x30,
            Key.D1 => 0x31,
            Key.D2 => 0x32,
            Key.D3 => 0x33,
            Key.D4 => 0x34,
            Key.D5 => 0x35,
            Key.D6 => 0x36,
            Key.D7 => 0x37,
            Key.D8 => 0x38,
            Key.D9 => 0x39,
            _ => (int)key  // For other keys, try direct cast
        };
    }

    /// <summary>
    /// Unregisters the hotkey and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        UnregisterHotkey();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~WindowsHotkeyService()
    {
        Dispose();
    }
}

/// <summary>
/// Modifier keys for hotkey registration (matches Windows MOD_* constants).
/// </summary>
[Flags]
internal enum ModifierKeys
{
    None = 0x0000,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008
}

#endif
