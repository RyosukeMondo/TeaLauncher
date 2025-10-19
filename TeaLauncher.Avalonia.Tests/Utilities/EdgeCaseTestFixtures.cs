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

using System.Text;

namespace TeaLauncher.Avalonia.Tests.Utilities;

/// <summary>
/// Provides edge case test data for testing uncommon scenarios.
/// This centralizes edge case test data including unicode, special characters,
/// large datasets, and malformed input to ensure robust testing.
/// </summary>
public static class EdgeCaseTestFixtures
{
    /// <summary>
    /// Unicode command names covering various scripts and emoji.
    /// Tests unicode support across different languages and character sets:
    /// Chinese (Simplified/Traditional), Japanese (Hiragana/Katakana/Kanji),
    /// Korean (Hangul), Arabic, Cyrillic, Greek, Emoji, and mixed scripts.
    /// </summary>
    public static readonly List<string> UnicodeCommandNames = new()
    {
        "搜索",                    // Chinese (Simplified) - "Search"
        "搜尋",                    // Chinese (Traditional) - "Search"
        "けんさく",                // Japanese (Hiragana) - "Search"
        "ケンサク",                // Japanese (Katakana) - "Search"
        "検索",                    // Japanese (Kanji) - "Search"
        "검색",                    // Korean (Hangul) - "Search"
        "بحث",                     // Arabic - "Search"
        "поиск",                   // Russian (Cyrillic) - "Search"
        "αναζήτηση",               // Greek - "Search"
        "🔍search",                // Emoji prefix
        "search🚀",                // Emoji suffix
        "café",                    // Latin with accents
        "naïve",                   // Latin with diaeresis
        "Москва",                  // Russian city name
        "東京",                    // Japanese city name (Tokyo)
        "search検索",              // Mixed Latin-Japanese
        "αβγ123",                  // Mixed Greek-Latin-Numbers
        "файл",                    // Russian - "File"
        "مجلد",                    // Arabic - "Folder"
        "🎯目標",                  // Emoji with Japanese
    };

    /// <summary>
    /// Special character arguments that may cause parsing issues.
    /// Tests handling of quotes, backslashes, pipes, redirection operators,
    /// unicode filenames, and other shell-sensitive characters.
    /// </summary>
    public static readonly List<string> SpecialCharacterArguments = new()
    {
        "\"arg with double quotes\"",
        "'arg with single quotes'",
        "arg with \"nested quotes\" inside",
        "path\\to\\file",
        "path/to/file",
        "C:\\Program Files\\My App\\app.exe",
        "arg|with|pipes",
        "arg>with>redirect",
        "arg<with<input",
        "arg&with&ampersand",
        "arg;with;semicolon",
        "arg with spaces",
        "arg\twith\ttabs",
        "arg\nwith\nnewlines",
        "файл.txt",                // Unicode filename (Russian)
        "文件.doc",                // Unicode filename (Chinese)
        "ملف.pdf",                 // Unicode filename (Arabic)
        "file with émojis 🎉.txt",
        "$VAR_NAME",               // Environment variable syntax
        "%TEMP%",                  // Windows environment variable
        "arg`with`backticks",
        "arg~with~tilde",
        "arg!with!exclamation",
        "arg@with@at",
        "arg#with#hash",
        "arg$with$dollar",
        "arg%with%percent",
        "arg^with^caret",
        "arg*with*asterisk",
        "arg(with)parens",
        "arg[with]brackets",
        "arg{with}braces",
    };

    /// <summary>
    /// Large word list for autocomplete stress testing.
    /// Contains 1000 programmatically generated words to test performance
    /// with large datasets (requirement 3.3: sub-50ms autocomplete).
    /// </summary>
    public static readonly List<string> LargeWordList = GenerateLargeWordList();

    /// <summary>
    /// Malformed YAML samples for testing error handling.
    /// Each entry contains a description as key and the malformed YAML as value.
    /// Tests various YAML syntax errors and structural issues.
    /// </summary>
    public static readonly Dictionary<string, string> MalformedYamlSamples = new()
    {
        ["Missing colon after key"] = @"commands
  - name: test
    linkto: https://example.com",

        ["Missing space after colon"] = @"commands:
  - name:test
    linkto: https://example.com",

        ["Wrong indentation"] = @"commands:
- name: test
  linkto: https://example.com
    description: Bad indent",

        ["Invalid escape sequence"] = @"commands:
  - name: test
    linkto: https://example.com\x",

        ["Duplicate key in mapping"] = @"commands:
  - name: test
    name: duplicate
    linkto: https://example.com",

        ["Unclosed quote"] = @"commands:
  - name: ""test
    linkto: https://example.com",

        ["Invalid list syntax"] = @"commands:
  name: test
  linkto: https://example.com",

        ["Tab instead of spaces"] = "commands:\n\t- name: test\n\t  linkto: https://example.com",

        ["Missing list item dash"] = @"commands:
  name: test
  linkto: https://example.com",

        ["Invalid flow sequence"] = @"commands: [name: test, linkto: https://example.com]",

        ["Unmatched bracket"] = @"commands:
  - name: test
    linkto: [https://example.com",

        ["Invalid anchor reference"] = @"commands:
  - &anchor
    name: test
    linkto: *undefined",

        ["Mixed tabs and spaces"] = "commands:\n  - name: test\n\tlinkto: https://example.com",

        ["Invalid boolean value"] = @"commands:
  - name: test
    linkto: https://example.com
    enabled: yesno",

        ["Trailing comma"] = @"commands:
  - name: test,
    linkto: https://example.com,",
    };

    /// <summary>
    /// Generates a large word list with 1000 entries for performance testing.
    /// Creates words in pattern: word1, word2, ..., word1000
    /// Also includes some common prefixes to test autocomplete filtering.
    /// </summary>
    private static List<string> GenerateLargeWordList()
    {
        var words = new List<string>(1000);

        // Add 900 numbered words (word1, word2, etc.)
        for (int i = 1; i <= 900; i++)
        {
            words.Add($"word{i}");
        }

        // Add 100 words with common prefixes to test filtering
        for (int i = 1; i <= 25; i++)
        {
            words.Add($"search{i}");
            words.Add($"open{i}");
            words.Add($"launch{i}");
            words.Add($"run{i}");
        }

        return words;
    }

    /// <summary>
    /// Long text strings for testing input limits and buffer handling.
    /// </summary>
    public static readonly List<string> LongTextInputs = new()
    {
        new string('a', 1000),     // 1000 character string
        new string('b', 5000),     // 5000 character string
        new string('文', 500),     // 500 unicode characters
        string.Join(" ", Enumerable.Repeat("word", 100)), // 100 words
    };

    /// <summary>
    /// Empty and whitespace-only inputs for testing edge cases.
    /// </summary>
    public static readonly List<string> EmptyAndWhitespaceInputs = new()
    {
        "",                        // Empty string
        " ",                       // Single space
        "  ",                      // Multiple spaces
        "\t",                      // Tab
        "\n",                      // Newline
        "\r\n",                    // Windows newline
        "   \t   \n   ",          // Mixed whitespace
    };

    /// <summary>
    /// Case sensitivity test pairs.
    /// Each tuple contains (lowercase, uppercase, mixedcase) versions of the same command.
    /// </summary>
    public static readonly List<(string Lower, string Upper, string Mixed)> CaseSensitivityPairs = new()
    {
        ("search", "SEARCH", "SeArCh"),
        ("notepad", "NOTEPAD", "NotePad"),
        ("google", "GOOGLE", "GooGle"),
        ("файл", "ФАЙЛ", "Файл"),
        ("検索", "検索", "検索"),  // Japanese has no case
    };
}
