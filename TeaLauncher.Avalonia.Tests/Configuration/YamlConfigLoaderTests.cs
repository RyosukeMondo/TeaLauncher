using System.Linq;
using NUnit.Framework;
using YamlDotNet.Core;
using TeaLauncher.Avalonia.Configuration;
using TeaLauncher.Avalonia.Infrastructure.Configuration;

namespace TeaLauncher.Avalonia.Tests.Configuration;

[TestFixture]
public class YamlConfigLoaderTests
{
    private YamlConfigLoaderService _loader = null!;
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _loader = new YamlConfigLoaderService();
        _testDirectory = Path.Combine(Path.GetTempPath(), "TeaLauncherTests_" + Guid.NewGuid().ToString());
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

    [Test]
    public void LoadConfigFile_ValidYaml_ReturnsCommandsConfig()
    {
        // Arrange
        var yaml = @"
commands:
  - name: google
    linkto: https://google.com
    description: Google search engine

  - name: notepad
    linkto: C:\Windows\System32\notepad.exe
";
        var filePath = CreateTempYamlFile(yaml);

        // Act
        var config = _loader.LoadConfiguration(filePath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Has.Count.EqualTo(2));
        Assert.That(config.Commands[0].Name, Is.EqualTo("google"));
        Assert.That(config.Commands[0].LinkTo, Is.EqualTo("https://google.com"));
        Assert.That(config.Commands[0].Description, Is.EqualTo("Google search engine"));
        Assert.That(config.Commands[1].Name, Is.EqualTo("notepad"));
        Assert.That(config.Commands[1].LinkTo, Is.EqualTo(@"C:\Windows\System32\notepad.exe"));
    }

    [Test]
    public void LoadConfigFile_EmptyCommandsList_ReturnsConfigWithEmptyList()
    {
        // Arrange
        var yaml = @"
commands: []
";
        var filePath = CreateTempYamlFile(yaml);

        // Act
        var config = _loader.LoadConfiguration(filePath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Is.Empty);
    }

    [Test]
    public void LoadConfigFile_MissingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.yaml");

        // Act & Assert
        var ex = Assert.Throws<FileNotFoundException>(() => _loader.LoadConfiguration(nonExistentPath));
        Assert.That(ex!.Message, Does.Contain(nonExistentPath));
        Assert.That(ex.FileName, Is.EqualTo(nonExistentPath));
    }

    [Test]
    public void LoadConfigFile_InvalidYamlSyntax_ThrowsYamlException()
    {
        // Arrange
        var invalidYaml = @"
commands:
  - name: google
    linkto: https://google.com
  - name: notepad
  linkto: invalid_indentation
";
        var filePath = CreateTempYamlFile(invalidYaml);

        // Act & Assert
        var ex = Assert.Throws<YamlException>(() => _loader.LoadConfiguration(filePath));
        Assert.That(ex!.Message, Does.Contain("syntax error"));
        Assert.That(ex.Message, Does.Contain(Path.GetFileName(filePath)));
    }

    [Test]
    public void LoadConfigFile_MissingNameField_ThrowsInvalidOperationException()
    {
        // Arrange
        var yaml = @"
commands:
  - linkto: https://google.com
    description: Missing name field
";
        var filePath = CreateTempYamlFile(yaml);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _loader.LoadConfiguration(filePath));
        Assert.That(ex!.Message, Does.Contain("missing required field 'name'"));
        Assert.That(ex.Message, Does.Contain("index 0"));
    }

    [Test]
    public void LoadConfigFile_MissingLinkToField_ThrowsInvalidOperationException()
    {
        // Arrange
        var yaml = @"
commands:
  - name: google
    description: Missing linkto field
";
        var filePath = CreateTempYamlFile(yaml);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _loader.LoadConfiguration(filePath));
        Assert.That(ex!.Message, Does.Contain("missing required field 'linkto'"));
        Assert.That(ex.Message, Does.Contain("google"));
        Assert.That(ex.Message, Does.Contain("index 0"));
    }

    [Test]
    public void LoadConfigFile_EmptyNameField_ThrowsInvalidOperationException()
    {
        // Arrange
        var yaml = @"
commands:
  - name: ''
    linkto: https://google.com
";
        var filePath = CreateTempYamlFile(yaml);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _loader.LoadConfiguration(filePath));
        Assert.That(ex!.Message, Does.Contain("missing required field 'name'"));
    }

    [Test]
    public void LoadConfigFile_EmptyLinkToField_ThrowsInvalidOperationException()
    {
        // Arrange
        var yaml = @"
commands:
  - name: google
    linkto: ''
";
        var filePath = CreateTempYamlFile(yaml);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _loader.LoadConfiguration(filePath));
        Assert.That(ex!.Message, Does.Contain("missing required field 'linkto'"));
        Assert.That(ex.Message, Does.Contain("google"));
    }

    [Test]
    public void LoadConfigFile_WhitespaceOnlyName_ThrowsInvalidOperationException()
    {
        // Arrange
        var yaml = @"
commands:
  - name: '   '
    linkto: https://google.com
";
        var filePath = CreateTempYamlFile(yaml);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _loader.LoadConfiguration(filePath));
        Assert.That(ex!.Message, Does.Contain("missing required field 'name'"));
    }

    [Test]
    public void LoadConfigFile_UnknownFields_AreIgnored()
    {
        // Arrange
        var yaml = @"
commands:
  - name: google
    linkto: https://google.com
    unknown_field: some_value
    another_unknown: 123
";
        var filePath = CreateTempYamlFile(yaml);

        // Act
        var config = _loader.LoadConfiguration(filePath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Has.Count.EqualTo(1));
        Assert.That(config.Commands[0].Name, Is.EqualTo("google"));
        Assert.That(config.Commands[0].LinkTo, Is.EqualTo("https://google.com"));
    }

    [Test]
    public void LoadConfigFile_WithComments_IgnoresComments()
    {
        // Arrange
        var yaml = @"
# This is a configuration file
commands:
  # Google search
  - name: google
    linkto: https://google.com
    # This is the description
    description: Google search engine

  # Another command
  - name: github
    linkto: https://github.com
";
        var filePath = CreateTempYamlFile(yaml);

        // Act
        var config = _loader.LoadConfiguration(filePath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Has.Count.EqualTo(2));
        Assert.That(config.Commands[0].Name, Is.EqualTo("google"));
        Assert.That(config.Commands[1].Name, Is.EqualTo("github"));
    }

    [Test]
    public void LoadConfigFile_OptionalFields_CanBeOmitted()
    {
        // Arrange
        var yaml = @"
commands:
  - name: google
    linkto: https://google.com
";
        var filePath = CreateTempYamlFile(yaml);

        // Act
        var config = _loader.LoadConfiguration(filePath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Has.Count.EqualTo(1));
        Assert.That(config.Commands[0].Name, Is.EqualTo("google"));
        Assert.That(config.Commands[0].LinkTo, Is.EqualTo("https://google.com"));
        Assert.That(config.Commands[0].Description, Is.Null);
        Assert.That(config.Commands[0].Arguments, Is.Null);
    }

    [Test]
    public void LoadConfigFile_AllFields_AreDeserialized()
    {
        // Arrange
        var yaml = @"
commands:
  - name: notepad
    linkto: C:\Windows\System32\notepad.exe
    description: Text editor
    arguments: file.txt
";
        var filePath = CreateTempYamlFile(yaml);

        // Act
        var config = _loader.LoadConfiguration(filePath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Has.Count.EqualTo(1));
        Assert.That(config.Commands[0].Name, Is.EqualTo("notepad"));
        Assert.That(config.Commands[0].LinkTo, Is.EqualTo(@"C:\Windows\System32\notepad.exe"));
        Assert.That(config.Commands[0].Description, Is.EqualTo("Text editor"));
        Assert.That(config.Commands[0].Arguments, Is.EqualTo("file.txt"));
    }

    [Test]
    public void LoadConfigFile_EmptyFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var filePath = CreateTempYamlFile("");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _loader.LoadConfiguration(filePath));
        Assert.That(ex!.Message, Does.Contain("Failed to parse YAML"));
    }

    [Test]
    public void LoadConfigFile_MissingCommandsKey_ReturnsEmptyCommands()
    {
        // Arrange
        // When commands key is missing, the default value (empty list) is used
        var yaml = @"
some_other_key: value
";
        var filePath = CreateTempYamlFile(yaml);

        // Act
        var config = _loader.LoadConfiguration(filePath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Is.Not.Null);
        Assert.That(config.Commands, Is.Empty);
    }

    [Test]
    public void LoadConfigFile_MultipleCommandsWithMixedValidation_ValidatesAll()
    {
        // Arrange
        var yaml = @"
commands:
  - name: google
    linkto: https://google.com

  - name: invalid
    # Missing linkto

  - name: notepad
    linkto: notepad.exe
";
        var filePath = CreateTempYamlFile(yaml);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _loader.LoadConfiguration(filePath));
        Assert.That(ex!.Message, Does.Contain("invalid"));
        Assert.That(ex.Message, Does.Contain("index 1"));
        Assert.That(ex.Message, Does.Contain("missing required field 'linkto'"));
    }

    [Test]
    public void LoadConfigFile_LargeConfigFile_LoadsSuccessfully()
    {
        // Arrange
        var yamlBuilder = new System.Text.StringBuilder();
        yamlBuilder.AppendLine("commands:");

        for (int i = 0; i < 100; i++)
        {
            yamlBuilder.AppendLine($"  - name: command{i}");
            yamlBuilder.AppendLine($"    linkto: https://example{i}.com");
            yamlBuilder.AppendLine($"    description: Command {i}");
        }

        var filePath = CreateTempYamlFile(yamlBuilder.ToString());

        // Act
        var config = _loader.LoadConfiguration(filePath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Has.Count.EqualTo(100));
        Assert.That(config.Commands[0].Name, Is.EqualTo("command0"));
        Assert.That(config.Commands[99].Name, Is.EqualTo("command99"));
    }

    [Test]
    public void LoadConfigFile_ExampleCommandsYaml_LoadsSuccessfully()
    {
        // Arrange
        var exampleYamlPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..",
            "TeaLauncher.Avalonia",
            "commands.yaml");

        // Skip test if example file doesn't exist (for CI environments)
        if (!File.Exists(exampleYamlPath))
        {
            Assert.Ignore($"Example commands.yaml not found at {exampleYamlPath}");
            return;
        }

        // Act
        var config = _loader.LoadConfigFile(exampleYamlPath);

        // Assert
        Assert.That(config, Is.Not.Null);
        Assert.That(config.Commands, Is.Not.Empty);

        // Verify some expected commands exist
        var commandNames = config.Commands.Select(c => c.Name).ToList();
        Assert.That(commandNames, Contains.Item("google"));
        Assert.That(commandNames, Contains.Item("reload"));
        Assert.That(commandNames, Contains.Item("exit"));
        Assert.That(commandNames, Contains.Item("version"));

        // Verify special commands have correct linkto values
        var reloadCmd = config.Commands.FirstOrDefault(c => c.Name == "reload");
        Assert.That(reloadCmd, Is.Not.Null);
        Assert.That(reloadCmd!.LinkTo, Is.EqualTo("!reload"));

        var exitCmd = config.Commands.FirstOrDefault(c => c.Name == "exit");
        Assert.That(exitCmd, Is.Not.Null);
        Assert.That(exitCmd!.LinkTo, Is.EqualTo("!exit"));

        var versionCmd = config.Commands.FirstOrDefault(c => c.Name == "version");
        Assert.That(versionCmd, Is.Not.Null);
        Assert.That(versionCmd!.LinkTo, Is.EqualTo("!version"));
    }

    private string CreateTempYamlFile(string content)
    {
        var fileName = $"test_{Guid.NewGuid()}.yaml";
        var filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }
}
