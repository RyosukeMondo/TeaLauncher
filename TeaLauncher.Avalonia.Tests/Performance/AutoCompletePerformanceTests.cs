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

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Tests.Utilities;

namespace TeaLauncher.Avalonia.Tests.Performance;

/// <summary>
/// Performance tests for autocomplete operations.
/// Validates that autocomplete meets the 50ms performance requirement (4.3).
/// Tests both AutoCompleteWord and GetCandidates methods with various dataset sizes.
/// </summary>
[TestFixture]
public class AutoCompletePerformanceTests : PerformanceTestBase
{
    private IServiceProvider? _serviceProvider;
    private IAutoCompleter? _autoCompleter;

    /// <summary>
    /// Sets up test services for performance testing.
    /// Uses real service implementations to measure actual performance.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _serviceProvider = TestServiceProvider.CreateWithRealServices();
        _autoCompleter = _serviceProvider.GetRequiredService<IAutoCompleter>();
    }

    /// <summary>
    /// Cleans up test resources.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Tests that autocomplete with 1000 words completes within 50ms.
    /// Uses EdgeCaseTestFixtures.LargeWordList for stress testing.
    /// Performance requirement 4.3: Autocomplete should complete within 50ms.
    /// </summary>
    [Test]
    public void AutoComplete_With1000Words_Within50ms()
    {
        // Arrange
        _autoCompleter!.UpdateWordList(EdgeCaseTestFixtures.LargeWordList);
        const string prefix = "word";
        const int maxAllowedMs = 50;

        // Act - measure AutoCompleteWord performance
        var result = TimeOperation(
            "AutoComplete with 1000 words (AutoCompleteWord)",
            () => _autoCompleter.AutoCompleteWord(prefix),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that GetCandidates with 1000 words completes within 50ms.
    /// Uses EdgeCaseTestFixtures.LargeWordList for stress testing.
    /// Performance requirement 4.3: Autocomplete should complete within 50ms.
    /// </summary>
    [Test]
    public void GetCandidates_With1000Words_Within50ms()
    {
        // Arrange
        _autoCompleter!.UpdateWordList(EdgeCaseTestFixtures.LargeWordList);
        const string prefix = "word";
        const int maxAllowedMs = 50;

        // Act - measure GetCandidates performance
        var result = TimeOperation(
            "AutoComplete with 1000 words (GetCandidates)",
            () => _autoCompleter.GetCandidates(prefix),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that autocomplete with unicode words maintains performance.
    /// Verifies that unicode processing doesn't cause performance degradation.
    /// Uses EdgeCaseTestFixtures.UnicodeCommandNames for unicode testing.
    /// </summary>
    [Test]
    public void AutoComplete_WithUnicode_NoSlowdown()
    {
        // Arrange
        _autoCompleter!.UpdateWordList(EdgeCaseTestFixtures.UnicodeCommandNames);
        const string prefix = "検"; // Japanese kanji prefix
        const int maxAllowedMs = 50;

        // Act
        var result = TimeOperation(
            "AutoComplete with Unicode words",
            () => _autoCompleter.AutoCompleteWord(prefix),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that GetCandidates with unicode prefix maintains performance.
    /// Verifies unicode string comparison doesn't degrade performance.
    /// </summary>
    [Test]
    public void GetCandidates_WithUnicode_NoSlowdown()
    {
        // Arrange
        _autoCompleter!.UpdateWordList(EdgeCaseTestFixtures.UnicodeCommandNames);
        const string prefix = "검"; // Korean hangul prefix
        const int maxAllowedMs = 50;

        // Act
        var result = TimeOperation(
            "GetCandidates with Unicode prefix",
            () => _autoCompleter.GetCandidates(prefix),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests autocomplete performance with empty word list.
    /// This is a baseline test for minimal autocomplete overhead.
    /// </summary>
    [Test]
    public void AutoComplete_EmptyWordList_Fast()
    {
        // Arrange
        _autoCompleter!.UpdateWordList(new List<string>());
        const string prefix = "test";
        const int maxAllowedMs = 50;

        // Act
        var result = TimeOperation(
            "AutoComplete with empty word list",
            () => _autoCompleter.AutoCompleteWord(prefix),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests autocomplete performance with common prefix matching many words.
    /// Uses LargeWordList which contains many "search", "open", "launch", "run" prefixes.
    /// This tests worst-case scenario where many words match the prefix.
    /// </summary>
    [Test]
    public void AutoComplete_CommonPrefix_ManyMatches_Within50ms()
    {
        // Arrange
        _autoCompleter!.UpdateWordList(EdgeCaseTestFixtures.LargeWordList);
        const string prefix = "search"; // Will match search1, search2, ..., search25
        const int maxAllowedMs = 50;

        // Act
        var result = TimeOperation(
            "AutoComplete with common prefix (many matches)",
            () => _autoCompleter.GetCandidates(prefix),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that UpdateWordList operation is fast.
    /// While not directly part of autocomplete performance requirement,
    /// slow updates could impact overall application responsiveness.
    /// Uses 1000 words from LargeWordList.
    /// </summary>
    [Test]
    public void UpdateWordList_1000Words_Fast()
    {
        // Arrange
        var largeWordList = EdgeCaseTestFixtures.LargeWordList;
        const int maxAllowedMs = 50;

        // Act - measure UpdateWordList performance
        var result = TimeOperation(
            "UpdateWordList with 1000 words",
            () => _autoCompleter!.UpdateWordList(largeWordList),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }
}
