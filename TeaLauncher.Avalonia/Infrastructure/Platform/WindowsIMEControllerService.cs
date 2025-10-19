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
using System.Runtime.InteropServices;
using TeaLauncher.Avalonia.Domain.Interfaces;

namespace TeaLauncher.Avalonia.Infrastructure.Platform;

/// <summary>
/// Controls Windows Input Method Editor (IME) state for a window.
/// Provides methods to turn IME on and off programmatically.
/// Implements IIMEController interface for dependency injection.
/// </summary>
public sealed class WindowsIMEControllerService : IIMEController, IDisposable
{
    [DllImport("Imm32.dll")]
    private static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("Imm32.dll")]
    private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

    [DllImport("Imm32.dll")]
    private static extern bool ImmSetOpenStatus(IntPtr hIMC, bool fOpen);

    private readonly IntPtr _windowHandle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the WindowsIMEControllerService class.
    /// </summary>
    /// <param name="windowHandle">The window handle (HWND) to control IME for.</param>
    /// <exception cref="ArgumentException">Thrown when windowHandle is IntPtr.Zero.</exception>
    public WindowsIMEControllerService(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentException("Window handle cannot be IntPtr.Zero.", nameof(windowHandle));
        }

        _windowHandle = windowHandle;
    }

    /// <summary>
    /// Disables the IME (turns it off).
    /// </summary>
    public void DisableIME()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WindowsIMEControllerService));
        }

        IntPtr imeHandle = ImmGetContext(_windowHandle);
        try
        {
            if (imeHandle != IntPtr.Zero)
            {
                ImmSetOpenStatus(imeHandle, false);
            }
        }
        finally
        {
            if (imeHandle != IntPtr.Zero)
            {
                ImmReleaseContext(_windowHandle, imeHandle);
            }
        }
    }

    /// <summary>
    /// Enables the IME (turns it on).
    /// </summary>
    public void EnableIME()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WindowsIMEControllerService));
        }

        IntPtr imeHandle = ImmGetContext(_windowHandle);
        try
        {
            if (imeHandle != IntPtr.Zero)
            {
                ImmSetOpenStatus(imeHandle, true);
            }
        }
        finally
        {
            if (imeHandle != IntPtr.Zero)
            {
                ImmReleaseContext(_windowHandle, imeHandle);
            }
        }
    }

    /// <summary>
    /// Releases all resources used by the WindowsIMEControllerService.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }
}
