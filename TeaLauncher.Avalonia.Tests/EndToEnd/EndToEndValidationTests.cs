/*
 * TeaLauncher. Simple command launcher.
 * Copyright (C) Toshiyuki Hirooka <toshi.hirooka@gmail.com> http://wasabi.in/
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
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
using System.IO;
using System.Linq;
using NUnit.Framework;
using TeaLauncher.Avalonia.Configuration;

namespace TeaLauncher.Avalonia.Tests.EndToEnd;

/// <summary>
/// End-to-end validation tests for TeaLauncher Avalonia migration.
/// Tests comprehensive workflows that can be validated programmatically.
/// For UI-specific tests requiring Windows, see docs/E2E_MANUAL_TEST_PLAN.md
/// </summary>
[TestFixture]
public class EndToEndValidationTests
{
    private IConfigurationLoader _configLoader = null!;
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _configLoader = new YamlConfigLoaderService();
        _testDirectory = Path.Combine(Path.GetTempPath(), "TeaLauncherE2E_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    #region Requirement 4: YAML Configuration Format

    [Test]
    [Category("Requirement-4.1")]
    public void E2E_Req4_1_ApplicationLoadsCommandsYaml()
    {
        // WHEN the application starts THEN it SHALL load commands from a `commands.yaml` file
        var yaml = @"
commands:
  - name: test
    linkto: https://example.com
";
        var filePath = CreateTempYamlFile("commands.yaml", yaml);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config, Is.Not.Null, "Configuration should be loaded");
        Assert.That(config.Commands, Is.Not.Empty, "Commands should be loaded from YAML");
    }

    [Test]
    [Category("Requirement-4.2")]
    public void E2E_Req4_2_MissingConfigFileDisplaysError()
    {
        // WHEN the configuration file is missing THEN the system SHALL display an error
        var nonExistentPath = Path.Combine(_testDirectory, "missing.yaml");

        var ex = Assert.Throws<FileNotFoundException>(() =>
            _configLoader.LoadConfigFile(nonExistentPath));

        Assert.That(ex!.Message, Does.Contain("missing.yaml"),
            "Error message should contain the expected file path");
    }

    [Test]
    [Category("Requirement-4.3")]
    public void E2E_Req4_3_InvalidYamlSyntaxDisplaysClearError()
    {
        // WHEN configuration has invalid YAML syntax THEN system SHALL display clear error with line number
        var invalidYaml = @"
commands:
  - name: test
    linkto: https://example.com
  - invalid yaml here: : :
    bad syntax
";
        var filePath = CreateTempYamlFile("invalid.yaml", invalidYaml);

        var ex = Assert.Throws<YamlDotNet.Core.YamlException>(() =>
            _configLoader.LoadConfigFile(filePath));

        Assert.That(ex, Is.Not.Null, "Should throw YamlException");
        Assert.That(ex!.Message, Is.Not.Empty, "Error message should be clear");
    }

    [Test]
    [Category("Requirement-4.4")]
    public void E2E_Req4_4_MissingRequiredFieldsDisplaysValidationError()
    {
        // WHEN configuration lacks required fields THEN system SHALL display validation errors
        var missingLinkTo = @"
commands:
  - name: test
    description: Missing linkto field
";
        var filePath = CreateTempYamlFile("missing-field.yaml", missingLinkTo);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _configLoader.LoadConfigFile(filePath));

        Assert.That(ex!.Message, Does.Contain("required"),
            "Error should indicate missing required field");
    }

    [Test]
    [Category("Requirement-4.5")]
    public void E2E_Req4_5_UnknownFieldsIgnoredGracefully()
    {
        // WHEN configuration contains unknown fields THEN system SHALL ignore them gracefully
        var yamlWithUnknownFields = @"
commands:
  - name: test
    linkto: https://example.com
    unknown_field: some value
    another_unknown: 123
";
        var filePath = CreateTempYamlFile("unknown-fields.yaml", yamlWithUnknownFields);

        Assert.DoesNotThrow(() => _configLoader.LoadConfigFile(filePath),
            "Unknown fields should be ignored without error");
    }

    [Test]
    [Category("Requirement-4.6")]
    public void E2E_Req4_6_OptionalPropertiesUseSensibleDefaults()
    {
        // WHEN command entries include optional properties THEN system SHALL use sensible defaults
        var yamlMinimal = @"
commands:
  - name: minimal
    linkto: https://example.com
";
        var filePath = CreateTempYamlFile("minimal.yaml", yamlMinimal);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config.Commands[0].Description, Is.Null,
            "Optional description should be null when not provided");
        Assert.That(config.Commands[0].Arguments, Is.Null,
            "Optional arguments should be null when not provided");
    }

    [Test]
    [Category("Requirement-4.7")]
    public void E2E_Req4_7_ValidYamlDeserializesToStronglyTypedObjects()
    {
        // WHEN YAML is valid THEN system SHALL deserialize to strongly-typed configuration objects
        var yaml = @"
commands:
  - name: google
    linkto: https://google.com
    description: Search engine
    arguments: --new-window
";
        var filePath = CreateTempYamlFile("valid.yaml", yaml);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config, Is.InstanceOf<CommandsConfig>(), "Should be strongly-typed CommandsConfig");
        Assert.That(config.Commands[0], Is.InstanceOf<CommandEntry>(), "Commands should be strongly-typed");
        Assert.That(config.Commands[0].Name, Is.EqualTo("google"));
        Assert.That(config.Commands[0].LinkTo, Is.EqualTo("https://google.com"));
        Assert.That(config.Commands[0].Description, Is.EqualTo("Search engine"));
        Assert.That(config.Commands[0].Arguments, Is.EqualTo("--new-window"));
    }

    [Test]
    [Category("Requirement-4.8")]
    public void E2E_Req4_8_YamlCommentsPreservedAndIgnored()
    {
        // WHEN YAML contains comments THEN they SHALL be ignored during parsing
        var yamlWithComments = @"
# This is a comment
commands:
  # Command list starts here
  - name: test  # inline comment
    linkto: https://example.com  # another inline comment
    # description: commented out field
";
        var filePath = CreateTempYamlFile("with-comments.yaml", yamlWithComments);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config.Commands, Has.Count.EqualTo(1), "Comments should not affect parsing");
        Assert.That(config.Commands[0].Name, Is.EqualTo("test"));
    }

    #endregion

    #region Requirement 5: Business Logic Preservation

    [Test]
    [Category("Requirement-5.2")]
    public void E2E_Req5_2_SpecialCommandsAreRecognized()
    {
        // WHEN user runs special commands THEN system SHALL recognize them
        var yaml = @"
commands:
  - name: reload
    linkto: '!reload'
  - name: exit
    linkto: '!exit'
  - name: version
    linkto: '!version'
";
        var filePath = CreateTempYamlFile("special.yaml", yaml);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config.Commands.Any(c => c.LinkTo == "!reload"), Is.True);
        Assert.That(config.Commands.Any(c => c.LinkTo == "!exit"), Is.True);
        Assert.That(config.Commands.Any(c => c.LinkTo == "!version"), Is.True);
    }

    [Test]
    [Category("Requirement-5.5")]
    public void E2E_Req5_5_CommandsWithArgumentsLoadCorrectly()
    {
        // WHEN configuration defines command arguments THEN they SHALL be available
        var yaml = @"
commands:
  - name: notepad
    linkto: notepad.exe
    arguments: test.txt
";
        var filePath = CreateTempYamlFile("args.yaml", yaml);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config.Commands[0].Arguments, Is.EqualTo("test.txt"));
    }

    #endregion

    #region Requirement 8: Configuration Management

    [Test]
    [Category("Requirement-8.1")]
    public void E2E_Req8_1_ConfigurationCanBeReloaded()
    {
        // WHEN user executes !reload THEN system SHALL re-parse configuration
        // This test simulates the reload workflow
        var yaml1 = @"
commands:
  - name: cmd1
    linkto: https://example1.com
";
        var filePath = CreateTempYamlFile("config.yaml", yaml1);
        var config1 = _configLoader.LoadConfigFile(filePath);

        Assert.That(config1.Commands, Has.Count.EqualTo(1));
        Assert.That(config1.Commands[0].Name, Is.EqualTo("cmd1"));

        // Simulate file modification and reload
        var yaml2 = @"
commands:
  - name: cmd1
    linkto: https://example1.com
  - name: cmd2
    linkto: https://example2.com
";
        File.WriteAllText(filePath, yaml2);
        var config2 = _configLoader.LoadConfigFile(filePath);

        Assert.That(config2.Commands, Has.Count.EqualTo(2),
            "Reloaded configuration should have new commands");
    }

    [Test]
    [Category("Requirement-8.2")]
    public void E2E_Req8_2_MissingConfigFileOnReloadDisplaysError()
    {
        // WHEN config file is missing during reload THEN display error with file path
        var missingFile = Path.Combine(_testDirectory, "missing-on-reload.yaml");

        var ex = Assert.Throws<FileNotFoundException>(() =>
            _configLoader.LoadConfigFile(missingFile));

        Assert.That(ex!.Message, Does.Contain("missing-on-reload.yaml"));
    }

    [Test]
    [Category("Requirement-8.3")]
    public void E2E_Req8_3_SyntaxErrorsOnReloadDisplayParsingError()
    {
        // WHEN config has syntax errors during reload THEN display parsing error
        var invalidYaml = @"
commands:
  - name test  # Missing colon
    linkto: https://example.com
";
        var filePath = CreateTempYamlFile("syntax-error-reload.yaml", invalidYaml);

        Assert.Throws<YamlDotNet.Core.YamlException>(() =>
            _configLoader.LoadConfigFile(filePath));
    }

    #endregion

    #region Cross-Cutting Validation Tests

    [Test]
    [Category("Integration")]
    public void E2E_FullWorkflow_LoadConfigAndVerifyAllCommandTypes()
    {
        // Test loading a realistic configuration with URLs, executables, and special commands
        var yaml = @"
commands:
  # Web URLs
  - name: google
    linkto: https://google.com
    description: Google search

  - name: github
    linkto: https://github.com

  # Executables
  - name: notepad
    linkto: C:\Windows\System32\notepad.exe
    description: Text editor

  # Executables with arguments
  - name: calc
    linkto: calc.exe
    arguments: /advanced

  # Special commands
  - name: reload
    linkto: '!reload'
    description: Reload configuration

  - name: exit
    linkto: '!exit'
    description: Exit application

  - name: version
    linkto: '!version'
    description: Show version
";
        var filePath = CreateTempYamlFile("full-workflow.yaml", yaml);
        var config = _configLoader.LoadConfigFile(filePath);

        // Verify all command types loaded correctly
        Assert.That(config.Commands, Has.Count.EqualTo(7));

        // Verify URLs
        Assert.That(config.Commands.Any(c => c.Name == "google" && c.LinkTo.StartsWith("https://")), Is.True);
        Assert.That(config.Commands.Any(c => c.Name == "github" && c.LinkTo.StartsWith("https://")), Is.True);

        // Verify executables
        Assert.That(config.Commands.Any(c => c.Name == "notepad" && c.LinkTo.Contains("notepad.exe")), Is.True);

        // Verify executables with arguments
        var calcCmd = config.Commands.FirstOrDefault(c => c.Name == "calc");
        Assert.That(calcCmd, Is.Not.Null);
        Assert.That(calcCmd!.Arguments, Is.EqualTo("/advanced"));

        // Verify special commands
        Assert.That(config.Commands.Any(c => c.LinkTo == "!reload"), Is.True);
        Assert.That(config.Commands.Any(c => c.LinkTo == "!exit"), Is.True);
        Assert.That(config.Commands.Any(c => c.LinkTo == "!version"), Is.True);
    }

    [Test]
    [Category("Performance")]
    public void E2E_Performance_LargeConfigurationLoadsQuickly()
    {
        // Requirement: Configuration Load ≤ 200ms for typical config files (< 100 commands)
        var commands = Enumerable.Range(1, 100).Select(i => $@"
  - name: cmd{i}
    linkto: https://example{i}.com
    description: Command {i}
").ToArray();

        var yaml = "commands:" + string.Join("", commands);
        var filePath = CreateTempYamlFile("large-config.yaml", yaml);

        var startTime = DateTime.Now;
        var config = _configLoader.LoadConfigFile(filePath);
        var elapsed = DateTime.Now - startTime;

        Assert.That(config.Commands, Has.Count.EqualTo(100));
        Assert.That(elapsed.TotalMilliseconds, Is.LessThan(200),
            $"Configuration load should be under 200ms, was {elapsed.TotalMilliseconds}ms");
    }

    [Test]
    [Category("Robustness")]
    public void E2E_Robustness_EmptyCommandsListHandledGracefully()
    {
        var yaml = @"
commands: []
";
        var filePath = CreateTempYamlFile("empty.yaml", yaml);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config.Commands, Is.Not.Null);
        Assert.That(config.Commands, Is.Empty);
    }

    [Test]
    [Category("Robustness")]
    public void E2E_Robustness_UnicodeCharactersInCommandsSupported()
    {
        var yaml = @"
commands:
  - name: 日本語
    linkto: https://example.jp
    description: Japanese characters テスト

  - name: emoji
    linkto: https://example.com
    description: Test with emoji 🚀 🎉
";
        var filePath = CreateTempYamlFile("unicode.yaml", yaml);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config.Commands, Has.Count.EqualTo(2));
        Assert.That(config.Commands[0].Name, Is.EqualTo("日本語"));
        Assert.That(config.Commands[0].Description, Does.Contain("テスト"));
        Assert.That(config.Commands[1].Description, Does.Contain("🚀"));
    }

    [Test]
    [Category("Robustness")]
    public void E2E_Robustness_VeryLongCommandNamesAndPathsSupported()
    {
        var longName = new string('a', 500);
        var longPath = "https://example.com/" + new string('b', 1000);

        var yaml = $@"
commands:
  - name: {longName}
    linkto: {longPath}
";
        var filePath = CreateTempYamlFile("long-values.yaml", yaml);
        var config = _configLoader.LoadConfigFile(filePath);

        Assert.That(config.Commands[0].Name, Is.EqualTo(longName));
        Assert.That(config.Commands[0].LinkTo, Is.EqualTo(longPath));
    }

    #endregion

    #region Helper Methods

    private string CreateTempYamlFile(string fileName, string content)
    {
        var filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    #endregion
}
