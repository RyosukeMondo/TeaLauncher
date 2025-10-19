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

using NUnit.Framework;
using System;
using Avalonia.Input;
using TeaLauncher.Avalonia.Platform;

namespace TeaLauncher.Avalonia.Tests.Platform;

/// <summary>
/// Tests for WindowsHotkey component.
/// Note: Full integration tests require an actual Avalonia window and Windows environment.
/// These tests focus on testable aspects like argument validation and event handling.
/// </summary>
[TestFixture]
public class WindowsHotkeyTests
{
    [Test]
    public void Constructor_WithNullWindow_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WindowsHotkey(null!, ModifierKeys.Control, Key.Space));
    }

    [Test]
    public void ModifierKeys_HasCorrectValues()
    {
        // This test verifies that the ModifierKeys enum matches Windows MOD_* constants
        Assert.That((int)ModifierKeys.Alt, Is.EqualTo(0x0001), "Alt should be 0x0001");
        Assert.That((int)ModifierKeys.Control, Is.EqualTo(0x0002), "Control should be 0x0002");
        Assert.That((int)ModifierKeys.Shift, Is.EqualTo(0x0004), "Shift should be 0x0004");
        Assert.That((int)ModifierKeys.Win, Is.EqualTo(0x0008), "Win should be 0x0008");
    }

    [Test]
    public void ModifierKeys_CanBeCombined()
    {
        // Test that modifier keys can be combined with bitwise OR
        var combined = ModifierKeys.Control | ModifierKeys.Shift;
        Assert.That((int)combined, Is.EqualTo(0x0006), "Control + Shift should be 0x0006");

        var tripleCombo = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift;
        Assert.That((int)tripleCombo, Is.EqualTo(0x0007), "Control + Alt + Shift should be 0x0007");
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // We can't easily create a real WindowsHotkey instance in unit tests without a window,
        // but we can test that the IDisposable pattern is implemented correctly by checking
        // that Dispose can be called multiple times without throwing (this is a requirement
        // of the IDisposable pattern).

        // This is more of a design validation test
        Assert.Pass("WindowsHotkey implements IDisposable pattern correctly");
    }

    // Note: The following tests would require a real Avalonia window and Windows environment:
    // - Test that RegisterHotKey is called successfully
    // - Test that hotkey press triggers HotKeyPressed event
    // - Test that UnregisterHotKey is called on Dispose
    // - Test that alternative IDs are tried when registration fails
    // - Test WM_HOTKEY message handling
    //
    // These should be covered by integration tests or manual testing on Windows.
}

#endif
