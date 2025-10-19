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

namespace TeaLauncher.Avalonia.Tests.Utilities;

/// <summary>
/// Tests for MockDialogService to ensure dialog recording and verification works correctly.
/// </summary>
[TestFixture]
public class MockDialogServiceTests
{
    private MockDialogService _mockDialogService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDialogService = new MockDialogService();
    }

    [Test]
    public async Task ShowMessageAsync_RecordsDialogCall()
    {
        // Arrange
        const string title = "Test Title";
        const string message = "Test Message";

        // Act
        await _mockDialogService.ShowMessageAsync(title, message);

        // Assert
        var calls = _mockDialogService.GetDialogCalls();
        calls.Should().HaveCount(1);
        calls[0].DialogType.Should().Be("Message");
        calls[0].Title.Should().Be(title);
        calls[0].Message.Should().Be(message);
        calls[0].UserResponse.Should().BeNull();
    }

    [Test]
    public async Task ShowConfirmAsync_RecordsDialogCallWithDefaultResponse()
    {
        // Arrange
        const string title = "Confirm Title";
        const string message = "Confirm Message";

        // Act
        var result = await _mockDialogService.ShowConfirmAsync(title, message);

        // Assert
        result.Should().BeTrue();
        var calls = _mockDialogService.GetDialogCalls();
        calls.Should().HaveCount(1);
        calls[0].DialogType.Should().Be("Confirm");
        calls[0].Title.Should().Be(title);
        calls[0].Message.Should().Be(message);
        calls[0].UserResponse.Should().BeTrue();
    }

    [Test]
    public async Task ShowConfirmAsync_ReturnsConfiguredResponse()
    {
        // Arrange
        const string title = "Confirm Title";
        const string message = "Confirm Message";
        _mockDialogService.SetConfirmResponse(false);

        // Act
        var result = await _mockDialogService.ShowConfirmAsync(title, message);

        // Assert
        result.Should().BeFalse();
        var calls = _mockDialogService.GetDialogCalls();
        calls[0].UserResponse.Should().BeFalse();
    }

    [Test]
    public async Task ShowErrorAsync_RecordsDialogCall()
    {
        // Arrange
        const string title = "Error Title";
        const string message = "Error Message";

        // Act
        await _mockDialogService.ShowErrorAsync(title, message);

        // Assert
        var calls = _mockDialogService.GetDialogCalls();
        calls.Should().HaveCount(1);
        calls[0].DialogType.Should().Be("Error");
        calls[0].Title.Should().Be(title);
        calls[0].Message.Should().Be(message);
        calls[0].UserResponse.Should().BeNull();
    }

    [Test]
    public async Task MultipleDialogCalls_RecordedInChronologicalOrder()
    {
        // Arrange & Act
        await _mockDialogService.ShowMessageAsync("Message 1", "Content 1");
        await _mockDialogService.ShowErrorAsync("Error 1", "Error Content");
        await _mockDialogService.ShowConfirmAsync("Confirm 1", "Confirm Content");

        // Assert
        var calls = _mockDialogService.GetDialogCalls();
        calls.Should().HaveCount(3);
        calls[0].DialogType.Should().Be("Message");
        calls[1].DialogType.Should().Be("Error");
        calls[2].DialogType.Should().Be("Confirm");

        // Verify timestamps are in chronological order
        calls[0].Timestamp.Should().BeBefore(calls[1].Timestamp);
        calls[1].Timestamp.Should().BeBefore(calls[2].Timestamp);
    }

    [Test]
    public async Task VerifyMessageShown_SucceedsWhenMessageExists()
    {
        // Arrange
        await _mockDialogService.ShowMessageAsync("Title", "Message");

        // Act & Assert
        _mockDialogService.VerifyMessageShown("Title", "Message");
    }

    [Test]
    public async Task VerifyConfirmShown_SucceedsWhenConfirmExists()
    {
        // Arrange
        await _mockDialogService.ShowConfirmAsync("Confirm Title", "Confirm Message");

        // Act & Assert
        _mockDialogService.VerifyConfirmShown("Confirm Title");
    }

    [Test]
    public async Task VerifyErrorShown_SucceedsWhenErrorExists()
    {
        // Arrange
        await _mockDialogService.ShowErrorAsync("Error Title", "Error Message");

        // Act & Assert
        _mockDialogService.VerifyErrorShown("Error Title", "Error Message");
    }

    [Test]
    public void ClearDialogHistory_RemovesAllCalls()
    {
        // Arrange
        _mockDialogService.ShowMessageAsync("Title", "Message").Wait();
        _mockDialogService.GetDialogCalls().Should().HaveCount(1);

        // Act
        _mockDialogService.ClearDialogHistory();

        // Assert
        _mockDialogService.GetDialogCalls().Should().BeEmpty();
    }
}
