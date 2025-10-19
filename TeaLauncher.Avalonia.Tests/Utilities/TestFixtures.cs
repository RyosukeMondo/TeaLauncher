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

using TeaLauncher.Avalonia.Domain.Models;

namespace TeaLauncher.Avalonia.Tests.Utilities;

/// <summary>
/// Provides reusable test data and fixtures for unit and integration tests.
/// This centralizes common test data to ensure consistency across tests.
/// </summary>
public static class TestFixtures
{
    /// <summary>
    /// Sample commands for testing command registry and execution.
    /// These represent common use cases: web search, documentation, and repository access.
    /// </summary>
    public static List<Command> SampleCommands => new()
    {
        new Command(
            Name: "google",
            LinkTo: "https://www.google.com/search?q={0}",
            Description: "Search Google",
            Arguments: null
        ),
        new Command(
            Name: "docs",
            LinkTo: "https://docs.microsoft.com",
            Description: "Microsoft Documentation",
            Arguments: null
        ),
        new Command(
            Name: "github",
            LinkTo: "https://github.com",
            Description: "GitHub",
            Arguments: null
        ),
        new Command(
            Name: "notepad",
            LinkTo: "notepad.exe",
            Description: "Open Notepad",
            Arguments: null
        ),
        new Command(
            Name: "calc",
            LinkTo: "calc.exe",
            Description: "Open Calculator",
            Arguments: null
        )
    };

    /// <summary>
    /// Sample word list for testing auto-completion functionality.
    /// </summary>
    public static List<string> SampleWords => new()
    {
        "google",
        "docs",
        "github",
        "notepad",
        "calc",
        "command",
        "calculator",
        "documentation"
    };

    /// <summary>
    /// Valid YAML configuration for testing configuration loading.
    /// Contains three sample commands with various configurations.
    /// </summary>
    public static string SampleYamlConfig => @"commands:
  - name: google
    linkto: https://www.google.com/search?q={0}
    description: Search Google
  - name: docs
    linkto: https://docs.microsoft.com
    description: Microsoft Documentation
  - name: notepad
    linkto: notepad.exe
    description: Open Notepad
    arguments: /multiline
";

    /// <summary>
    /// Invalid YAML configuration with syntax error for testing error handling.
    /// Contains a syntax error on line 3 (missing space after colon).
    /// </summary>
    public static string InvalidYamlConfig => @"commands:
  - name: google
    linkto:https://www.google.com
    description: Search Google
";

    /// <summary>
    /// YAML configuration with malformed structure (missing required fields).
    /// </summary>
    public static string MalformedYamlConfig => @"commands:
  - name: google
    # Missing linkto field
    description: Search Google
  - linkto: https://docs.microsoft.com
    # Missing name field
    description: Microsoft Documentation
";

    /// <summary>
    /// Empty YAML configuration for testing edge cases.
    /// </summary>
    public static string EmptyYamlConfig => @"commands: []
";

    /// <summary>
    /// Complex command line with quotes and arguments for testing command parsing.
    /// </summary>
    public static string ComplexCommandLine => @"notepad ""C:\Program Files\My App\config.txt"" /readonly";

    /// <summary>
    /// Sample URL inputs for testing URL detection and execution.
    /// </summary>
    public static List<string> SampleUrls => new()
    {
        "https://www.google.com",
        "http://example.com",
        "ftp://ftp.example.com/file.txt"
    };

    /// <summary>
    /// Sample file paths for testing path detection and execution.
    /// </summary>
    public static List<string> SampleFilePaths => new()
    {
        @"C:\Windows\notepad.exe",
        @"D:\Projects\MyApp\app.exe",
        @"E:\Documents\file.txt"
    };

    /// <summary>
    /// Sample special commands for testing special command handling.
    /// </summary>
    public static List<string> SpecialCommands => new()
    {
        "!reload",
        "!version",
        "!exit"
    };

    /// <summary>
    /// Sample command lines with quotes for testing argument parsing.
    /// Each tuple contains (input, expected_command, expected_args).
    /// </summary>
    public static List<(string Input, string Command, string Args)> QuotedCommandLines => new()
    {
        ("google test", "google", "test"),
        ("notepad \"my file.txt\"", "notepad", "\"my file.txt\""),
        ("calc", "calc", ""),
        ("google \"search query\" --verbose", "google", "\"search query\" --verbose")
    };

    /// <summary>
    /// Creates a temporary YAML configuration file for testing.
    /// The file is created in the system temp directory.
    /// </summary>
    /// <param name="content">The YAML content to write to the file.</param>
    /// <returns>The path to the created temporary file.</returns>
    /// <remarks>
    /// Caller is responsible for deleting the file after use.
    /// </remarks>
    public static string CreateTempYamlFile(string content)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}.yaml");
        File.WriteAllText(tempPath, content);
        return tempPath;
    }

    /// <summary>
    /// Creates a temporary directory for testing.
    /// The directory is created in the system temp directory.
    /// </summary>
    /// <returns>The path to the created temporary directory.</returns>
    /// <remarks>
    /// Caller is responsible for deleting the directory after use.
    /// </remarks>
    public static string CreateTempDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test-dir-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }
}
