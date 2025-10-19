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

using TeaLauncher.Avalonia.Infrastructure.Configuration;
using TeaLauncher.Avalonia.Tests.Utilities;
using YamlDotNet.Core;

namespace TeaLauncher.Avalonia.Tests.Infrastructure.Configuration;

/// <summary>
/// Unit tests for YamlConfigLoaderService.
/// Tests configuration loading, validation, and error handling for YAML files.
/// Uses the AAA (Arrange-Act-Assert) pattern for clarity.
/// </summary>
[TestFixture]
public class YamlConfigLoaderServiceTests
{
    private YamlConfigLoaderService _service = null!;
    private string? _tempFilePath;

    [SetUp]
    public void SetUp()
    {
        _service = new YamlConfigLoaderService();
        _tempFilePath = null;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up temporary files created during tests
        if (_tempFilePath != null && File.Exists(_tempFilePath))
        {
            try
            {
                File.Delete(_tempFilePath);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }

    #region Constructor Tests

    [Test]
    public void Constructor_ShouldSucceed()
    {
        // Arrange & Act
        var service = new YamlConfigLoaderService();

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region LoadConfiguration Tests

    [Test]
    public void LoadConfiguration_WithValidYaml_ShouldReturnConfig()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.SampleYamlConfig);

        // Act
        var config = _service.LoadConfiguration(_tempFilePath);

        // Assert
        config.Should().NotBeNull();
        config.Commands.Should().NotBeNull();
        config.Commands.Should().HaveCount(3);
        config.Commands[0].Name.Should().Be("google");
        config.Commands[0].LinkTo.Should().Be("https://www.google.com/search?q={0}");
        config.Commands[1].Name.Should().Be("docs");
        config.Commands[2].Name.Should().Be("notepad");
    }

    [Test]
    public void LoadConfiguration_WithEmptyYaml_ShouldReturnEmptyConfig()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.EmptyYamlConfig);

        // Act
        var config = _service.LoadConfiguration(_tempFilePath);

        // Assert
        config.Should().NotBeNull();
        config.Commands.Should().NotBeNull();
        config.Commands.Should().BeEmpty();
    }

    [Test]
    public void LoadConfiguration_WithMissingFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"non-existent-{Guid.NewGuid()}.yaml");

        // Act
        var act = () => _service.LoadConfiguration(nonExistentPath);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage($"*{nonExistentPath}*");
    }

    [Test]
    public void LoadConfiguration_WithInvalidYamlSyntax_ShouldThrowYamlException()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.InvalidYamlConfig);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<YamlException>()
            .WithMessage("*YAML syntax error*");
    }

    [Test]
    public void LoadConfiguration_WithMalformedStructure_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.MalformedYamlConfig);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*validation failed*");
    }

    [Test]
    public void LoadConfiguration_WithMissingNameField_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidYaml = @"commands:
  - linkto: https://www.google.com
    description: Search Google
";
        _tempFilePath = TestFixtures.CreateTempYamlFile(invalidYaml);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required field 'name'*");
    }

    [Test]
    public void LoadConfiguration_WithMissingLinkToField_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidYaml = @"commands:
  - name: google
    description: Search Google
";
        _tempFilePath = TestFixtures.CreateTempYamlFile(invalidYaml);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required field 'linkto'*");
    }

    [Test]
    public void LoadConfiguration_WithEmptyNameField_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidYaml = @"commands:
  - name: 
    linkto: https://www.google.com
    description: Search Google
";
        _tempFilePath = TestFixtures.CreateTempYamlFile(invalidYaml);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required field 'name'*");
    }

    [Test]
    public void LoadConfiguration_WithEmptyLinkToField_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidYaml = @"commands:
  - name: google
    linkto: 
    description: Search Google
";
        _tempFilePath = TestFixtures.CreateTempYamlFile(invalidYaml);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing required field 'linkto'*");
    }

    [Test]
    public void LoadConfiguration_WithOptionalFields_ShouldSucceed()
    {
        // Arrange
        var yamlWithOptionals = @"commands:
  - name: google
    linkto: https://www.google.com/search?q={0}
    description: Search Google
    arguments: --verbose
";
        _tempFilePath = TestFixtures.CreateTempYamlFile(yamlWithOptionals);

        // Act
        var config = _service.LoadConfiguration(_tempFilePath);

        // Assert
        config.Should().NotBeNull();
        config.Commands.Should().ContainSingle();
        config.Commands[0].Description.Should().Be("Search Google");
        config.Commands[0].Arguments.Should().Be("--verbose");
    }

    [Test]
    public void LoadConfiguration_WithMissingDescriptionField_ShouldSucceed()
    {
        // Arrange
        var yamlWithoutDescription = @"commands:
  - name: google
    linkto: https://www.google.com
";
        _tempFilePath = TestFixtures.CreateTempYamlFile(yamlWithoutDescription);

        // Act
        var config = _service.LoadConfiguration(_tempFilePath);

        // Assert
        config.Should().NotBeNull();
        config.Commands.Should().ContainSingle();
        config.Commands[0].Description.Should().BeNullOrEmpty();
    }

    #endregion

    #region LoadConfigurationAsync Tests

    [Test]
    public async Task LoadConfigurationAsync_WithValidYaml_ShouldReturnConfig()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.SampleYamlConfig);

        // Act
        var config = await _service.LoadConfigurationAsync(_tempFilePath);

        // Assert
        config.Should().NotBeNull();
        config.Commands.Should().HaveCount(3);
        config.Commands[0].Name.Should().Be("google");
    }

    [Test]
    public async Task LoadConfigurationAsync_WithMissingFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"non-existent-{Guid.NewGuid()}.yaml");

        // Act
        var act = async () => await _service.LoadConfigurationAsync(nonExistentPath);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Test]
    public async Task LoadConfigurationAsync_WithInvalidYaml_ShouldThrowYamlException()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.InvalidYamlConfig);

        // Act
        var act = async () => await _service.LoadConfigurationAsync(_tempFilePath);

        // Assert
        await act.Should().ThrowAsync<YamlException>();
    }

    [Test]
    public async Task LoadConfigurationAsync_WithMalformedYaml_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.MalformedYamlConfig);

        // Act
        var act = async () => await _service.LoadConfigurationAsync(_tempFilePath);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Error Message Tests

    [Test]
    public void LoadConfiguration_WithYamlSyntaxError_ShouldIncludeLineNumber()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.InvalidYamlConfig);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<YamlException>()
            .WithMessage("*line*");
    }

    [Test]
    public void LoadConfiguration_WithMissingRequiredField_ShouldIncludeFieldName()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.MalformedYamlConfig);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .And.Message.Should().Match(msg =>
                msg.Contains("name") || msg.Contains("linkto"));
    }

    [Test]
    public void LoadConfiguration_WithValidationError_ShouldIncludeCommandIndex()
    {
        // Arrange
        var invalidYaml = @"commands:
  - name: google
    linkto: https://www.google.com
  - name: docs
    # Missing linkto field
    description: Microsoft Documentation
";
        _tempFilePath = TestFixtures.CreateTempYamlFile(invalidYaml);

        // Act
        var act = () => _service.LoadConfiguration(_tempFilePath);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*index*");
    }

    #endregion

    #region Integration Tests

    [Test]
    public void LoadConfiguration_WithRealWorldConfig_ShouldParseAllFields()
    {
        // Arrange
        var realWorldYaml = @"commands:
  - name: google
    linkto: https://www.google.com/search?q={0}
    description: Search on Google
    arguments: null
  - name: notepad
    linkto: notepad.exe
    description: Open Notepad
    arguments: /multiline
  - name: github
    linkto: https://github.com
    description: Open GitHub
";
        _tempFilePath = TestFixtures.CreateTempYamlFile(realWorldYaml);

        // Act
        var config = _service.LoadConfiguration(_tempFilePath);

        // Assert
        config.Commands.Should().HaveCount(3);

        var googleCommand = config.Commands[0];
        googleCommand.Name.Should().Be("google");
        googleCommand.LinkTo.Should().Be("https://www.google.com/search?q={0}");
        googleCommand.Description.Should().Be("Search on Google");

        var notepadCommand = config.Commands[1];
        notepadCommand.Name.Should().Be("notepad");
        notepadCommand.LinkTo.Should().Be("notepad.exe");
        notepadCommand.Arguments.Should().Be("/multiline");

        var githubCommand = config.Commands[2];
        githubCommand.Name.Should().Be("github");
        githubCommand.LinkTo.Should().Be("https://github.com");
        githubCommand.Description.Should().Be("Open GitHub");
    }

    [Test]
    public void LoadConfiguration_MultipleTimesWithSameFile_ShouldReturnConsistentResults()
    {
        // Arrange
        _tempFilePath = TestFixtures.CreateTempYamlFile(TestFixtures.SampleYamlConfig);

        // Act
        var config1 = _service.LoadConfiguration(_tempFilePath);
        var config2 = _service.LoadConfiguration(_tempFilePath);

        // Assert
        config1.Commands.Should().HaveCount(config2.Commands.Count);
        for (int i = 0; i < config1.Commands.Count; i++)
        {
            config1.Commands[i].Name.Should().Be(config2.Commands[i].Name);
            config1.Commands[i].LinkTo.Should().Be(config2.Commands[i].LinkTo);
        }
    }

    #endregion
}
