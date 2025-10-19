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
using TeaLauncher.Avalonia.Platform;

namespace TeaLauncher.Avalonia.Tests.Platform;

/// <summary>
/// Tests for WindowsIMEController component.
/// Note: Full integration tests require a real window handle and IME environment.
/// These tests focus on testable aspects like argument validation and disposal.
/// </summary>
[TestFixture]
public class WindowsIMEControllerTests
{
    [Test]
    public void Constructor_WithZeroHandle_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new WindowsIMEController(IntPtr.Zero));

        Assert.That(exception!.Message, Does.Contain("Window handle cannot be IntPtr.Zero"));
        Assert.That(exception.ParamName, Is.EqualTo("windowHandle"));
    }

    [Test]
    public void Constructor_WithValidHandle_DoesNotThrow()
    {
        // Arrange
        var fakeHandle = new IntPtr(12345);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            using var controller = new WindowsIMEController(fakeHandle);
        });
    }

    [Test]
    public void On_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var fakeHandle = new IntPtr(12345);
        var controller = new WindowsIMEController(fakeHandle);
        controller.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => controller.On());
    }

    [Test]
    public void Off_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var fakeHandle = new IntPtr(12345);
        var controller = new WindowsIMEController(fakeHandle);
        controller.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => controller.Off());
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var fakeHandle = new IntPtr(12345);
        var controller = new WindowsIMEController(fakeHandle);

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() =>
        {
            controller.Dispose();
            controller.Dispose();
            controller.Dispose();
        });
    }

    [Test]
    public void Dispose_AsIDisposable_WorksCorrectly()
    {
        // Arrange
        var fakeHandle = new IntPtr(12345);
        IDisposable controller = new WindowsIMEController(fakeHandle);

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() => controller.Dispose());
    }

    // Note: The following tests would require a real window handle and IME environment:
    // - Test that On() successfully opens the IME
    // - Test that Off() successfully closes the IME
    // - Test that ImmGetContext returns a valid context
    // - Test that ImmReleaseContext is called properly
    // - Test the IME reset sequence (Off -> On -> Off)
    //
    // These should be covered by integration tests or manual testing on Windows with IME enabled.
}

#endif
