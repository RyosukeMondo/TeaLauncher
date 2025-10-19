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

using FluentAssertions;
using NUnit.Framework;
using TeaLauncher.Avalonia.Application.Services;

namespace TeaLauncher.Avalonia.Tests.Application.Services;

/// <summary>
/// Unit tests for <see cref="AutoCompleterService"/>.
/// Tests the auto-completion algorithm extracted from AutoCompleteMachine.
/// Follows the AAA (Arrange-Act-Assert) pattern.
/// </summary>
[TestFixture]
public class AutoCompleterServiceTests
{
    private AutoCompleterService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new AutoCompleterService();
    }

    #region UpdateWordList Tests

    [Test]
    public void UpdateWordList_WithValidWords_ShouldReplaceWordList()
    {
        // Arrange
        var words = new[] { "google", "github", "gmail" };

        // Act
        _service.UpdateWordList(words);
        var candidates = _service.GetCandidates("g");

        // Assert
        candidates.Should().HaveCount(3);
        candidates.Should().Contain("google");
        candidates.Should().Contain("github");
        candidates.Should().Contain("gmail");
    }

    [Test]
    public void UpdateWordList_CalledTwice_ShouldReplaceExistingWords()
    {
        // Arrange
        var initialWords = new[] { "google", "github" };
        var newWords = new[] { "yahoo", "youtube" };

        // Act
        _service.UpdateWordList(initialWords);
        _service.UpdateWordList(newWords);
        var candidates = _service.GetCandidates("y");

        // Assert
        candidates.Should().HaveCount(2);
        candidates.Should().Contain("yahoo");
        candidates.Should().Contain("youtube");
        candidates.Should().NotContain("google");
        candidates.Should().NotContain("github");
    }

    [Test]
    public void UpdateWordList_WithEmptyList_ShouldClearWords()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github" });

        // Act
        _service.UpdateWordList(Array.Empty<string>());
        var candidates = _service.GetCandidates("g");

        // Assert
        candidates.Should().BeEmpty();
    }

    #endregion

    #region GetCandidates Tests

    [Test]
    public void GetCandidates_WithMatchingPrefix_ShouldReturnMatchingWords()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github", "gitlab", "yahoo" });

        // Act
        var candidates = _service.GetCandidates("gi");

        // Assert
        candidates.Should().HaveCount(2);
        candidates.Should().Contain("github");
        candidates.Should().Contain("gitlab");
    }

    [Test]
    public void GetCandidates_WithNoMatchingPrefix_ShouldReturnEmptyList()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github", "gitlab" });

        // Act
        var candidates = _service.GetCandidates("x");

        // Assert
        candidates.Should().BeEmpty();
    }

    [Test]
    public void GetCandidates_WithEmptyPrefix_ShouldReturnAllWords()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github", "yahoo" });

        // Act
        var candidates = _service.GetCandidates("");

        // Assert
        candidates.Should().HaveCount(3);
    }

    [Test]
    public void GetCandidates_WithCaseInsensitiveMatch_ShouldReturnMatches()
    {
        // Arrange
        _service.UpdateWordList(new[] { "Google", "GitHub", "GitLab" });

        // Act
        var candidates = _service.GetCandidates("gi");

        // Assert
        candidates.Should().HaveCount(2);
        candidates.Should().Contain("GitHub");
        candidates.Should().Contain("GitLab");
    }

    [Test]
    public void GetCandidates_WithUpperCasePrefix_ShouldMatchCaseInsensitively()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github", "gitlab" });

        // Act
        var candidates = _service.GetCandidates("GI");

        // Assert
        candidates.Should().HaveCount(2);
        candidates.Should().Contain("github");
        candidates.Should().Contain("gitlab");
    }

    [Test]
    public void GetCandidates_WithEmptyWordList_ShouldReturnEmptyList()
    {
        // Arrange
        // No words added

        // Act
        var candidates = _service.GetCandidates("g");

        // Assert
        candidates.Should().BeEmpty();
    }

    #endregion

    #region AutoCompleteWord Tests

    [Test]
    public void AutoCompleteWord_WithSingleMatch_ShouldReturnFullWord()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github", "yahoo" });

        // Act
        var result = _service.AutoCompleteWord("goo");

        // Assert
        result.Should().Be("google");
    }

    [Test]
    public void AutoCompleteWord_WithMultipleMatches_ShouldReturnLongestCommonPrefix()
    {
        // Arrange
        _service.UpdateWordList(new[] { "github", "gitlab", "gitbucket" });

        // Act
        var result = _service.AutoCompleteWord("gi");

        // Assert
        result.Should().Be("git");
    }

    [Test]
    public void AutoCompleteWord_WithNoMatches_ShouldReturnEmptyString()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github" });

        // Act
        var result = _service.AutoCompleteWord("x");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void AutoCompleteWord_WithEmptyInput_ShouldHandleGracefully()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github", "yahoo" });

        // Act
        var result = _service.AutoCompleteWord("");

        // Assert
        // Empty input should return empty string or common prefix of all words
        result.Should().NotBeNull();
    }

    [Test]
    public void AutoCompleteWord_WithExactMatch_ShouldReturnExactWord()
    {
        // Arrange
        _service.UpdateWordList(new[] { "google", "github" });

        // Act
        var result = _service.AutoCompleteWord("google");

        // Assert
        result.Should().Be("google");
    }

    [Test]
    public void AutoCompleteWord_WithCaseInsensitiveMatch_ShouldReturnCommonPrefix()
    {
        // Arrange
        _service.UpdateWordList(new[] { "GitHub", "GitLab" });

        // Act
        var result = _service.AutoCompleteWord("gi");

        // Assert
        result.Should().Be("Git");
    }

    [Test]
    public void AutoCompleteWord_WithDifferingCase_ShouldPreserveCaseFromCandidates()
    {
        // Arrange
        _service.UpdateWordList(new[] { "Google", "GitHub" });

        // Act
        var result = _service.AutoCompleteWord("g");

        // Assert
        result.Should().Be("G");
    }

    [Test]
    public void AutoCompleteWord_WithPartialCommonPrefix_ShouldReturnCorrectLength()
    {
        // Arrange
        _service.UpdateWordList(new[] { "test123", "test456", "test789" });

        // Act
        var result = _service.AutoCompleteWord("te");

        // Assert
        result.Should().Be("test");
    }

    [Test]
    public void AutoCompleteWord_WithNoDivergence_ShouldReturnFullCommonPrefix()
    {
        // Arrange
        _service.UpdateWordList(new[] { "command", "commander", "commanding" });

        // Act
        var result = _service.AutoCompleteWord("com");

        // Assert
        result.Should().Be("command");
    }

    [Test]
    public void AutoCompleteWord_WithEmptyWordList_ShouldReturnEmptyString()
    {
        // Arrange
        // No words added

        // Act
        var result = _service.AutoCompleteWord("g");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Integration Tests

    [Test]
    public void UpdateWordList_ThenAutoComplete_ShouldWorkCorrectly()
    {
        // Arrange
        var words = new[] { "google", "github", "gitlab", "yahoo", "youtube" };
        _service.UpdateWordList(words);

        // Act
        var googleCompletion = _service.AutoCompleteWord("goo");
        var gitCompletion = _service.AutoCompleteWord("gi");
        var youtubeCompletion = _service.AutoCompleteWord("you");

        // Assert
        googleCompletion.Should().Be("google");
        gitCompletion.Should().Be("git"); // Common prefix of github, gitlab
        youtubeCompletion.Should().Be("youtube");
    }

    [Test]
    public void GetCandidates_AfterUpdateWordList_ShouldReflectNewWords()
    {
        // Arrange
        _service.UpdateWordList(new[] { "old1", "old2" });

        // Act
        _service.UpdateWordList(new[] { "new1", "new2", "new3" });
        var candidates = _service.GetCandidates("new");

        // Assert
        candidates.Should().HaveCount(3);
        candidates.Should().NotContain("old1");
        candidates.Should().NotContain("old2");
    }

    #endregion
}
