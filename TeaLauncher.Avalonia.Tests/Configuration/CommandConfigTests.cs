using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using TeaLauncher.Avalonia.Configuration;

namespace TeaLauncher.Avalonia.Tests.Configuration;

[TestFixture]
public class CommandConfigTests
{
    private IDeserializer _deserializer = null!;

    [SetUp]
    public void SetUp()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    [Test]
    public void CommandEntry_RequiredFields_CanBeDeserialized()
    {
        // Arrange
        var yaml = @"
name: google
linkto: https://google.com
";

        // Act
        var entry = _deserializer.Deserialize<CommandEntry>(yaml);

        // Assert
        Assert.That(entry.Name, Is.EqualTo("google"));
        Assert.That(entry.LinkTo, Is.EqualTo("https://google.com"));
        Assert.That(entry.Description, Is.Null);
        Assert.That(entry.Arguments, Is.Null);
    }

    [Test]
    public void CommandEntry_AllFields_CanBeDeserialized()
    {
        // Arrange
        var yaml = @"
name: notepad
linkto: C:\Windows\System32\notepad.exe
description: Text editor
arguments: file.txt
";

        // Act
        var entry = _deserializer.Deserialize<CommandEntry>(yaml);

        // Assert
        Assert.That(entry.Name, Is.EqualTo("notepad"));
        Assert.That(entry.LinkTo, Is.EqualTo(@"C:\Windows\System32\notepad.exe"));
        Assert.That(entry.Description, Is.EqualTo("Text editor"));
        Assert.That(entry.Arguments, Is.EqualTo("file.txt"));
    }

    [Test]
    public void CommandEntry_MissingName_DeserializesWithNullName()
    {
        // Arrange
        // Note: YamlDotNet doesn't automatically validate 'required' keyword.
        // Validation will be handled by YamlConfigLoader.
        var yaml = @"
linkto: https://google.com
description: Google search
";

        // Act
        var entry = _deserializer.Deserialize<CommandEntry>(yaml);

        // Assert
        // The required fields will be null when not provided
        Assert.That(entry.Name, Is.Null);
        Assert.That(entry.LinkTo, Is.EqualTo("https://google.com"));
    }

    [Test]
    public void CommandEntry_MissingLinkTo_DeserializesWithNullLinkTo()
    {
        // Arrange
        // Note: YamlDotNet doesn't automatically validate 'required' keyword.
        // Validation will be handled by YamlConfigLoader.
        var yaml = @"
name: google
description: Google search
";

        // Act
        var entry = _deserializer.Deserialize<CommandEntry>(yaml);

        // Assert
        // The required fields will be null when not provided
        Assert.That(entry.Name, Is.EqualTo("google"));
        Assert.That(entry.LinkTo, Is.Null);
    }

    [Test]
    public void CommandEntry_UnknownFields_AreIgnored()
    {
        // Arrange
        var yaml = @"
name: google
linkto: https://google.com
unknown_field: some_value
another_unknown: 123
";

        // Act
        var entry = _deserializer.Deserialize<CommandEntry>(yaml);

        // Assert
        Assert.That(entry.Name, Is.EqualTo("google"));
        Assert.That(entry.LinkTo, Is.EqualTo("https://google.com"));
    }

    [Test]
    public void CommandsConfig_EmptyCommands_CanBeDeserialized()
    {
        // Arrange
        var yaml = @"
commands: []
";

        // Act
        var config = _deserializer.Deserialize<CommandsConfig>(yaml);

        // Assert
        Assert.That(config.Commands, Is.Not.Null);
        Assert.That(config.Commands, Is.Empty);
    }

    [Test]
    public void CommandsConfig_MultipleCommands_CanBeDeserialized()
    {
        // Arrange
        var yaml = @"
commands:
  - name: google
    linkto: https://google.com
    description: Google search engine

  - name: notepad
    linkto: C:\Windows\System32\notepad.exe
    description: Text editor

  - name: github
    linkto: https://github.com
";

        // Act
        var config = _deserializer.Deserialize<CommandsConfig>(yaml);

        // Assert
        Assert.That(config.Commands, Has.Count.EqualTo(3));

        Assert.That(config.Commands[0].Name, Is.EqualTo("google"));
        Assert.That(config.Commands[0].LinkTo, Is.EqualTo("https://google.com"));
        Assert.That(config.Commands[0].Description, Is.EqualTo("Google search engine"));

        Assert.That(config.Commands[1].Name, Is.EqualTo("notepad"));
        Assert.That(config.Commands[1].LinkTo, Is.EqualTo(@"C:\Windows\System32\notepad.exe"));
        Assert.That(config.Commands[1].Description, Is.EqualTo("Text editor"));

        Assert.That(config.Commands[2].Name, Is.EqualTo("github"));
        Assert.That(config.Commands[2].LinkTo, Is.EqualTo("https://github.com"));
        Assert.That(config.Commands[2].Description, Is.Null);
    }

    [Test]
    public void CommandsConfig_NoCommandsKey_InitializesEmptyList()
    {
        // Arrange
        var yaml = @"{}";

        // Act
        var config = _deserializer.Deserialize<CommandsConfig>(yaml);

        // Assert
        Assert.That(config.Commands, Is.Not.Null);
        Assert.That(config.Commands, Is.Empty);
    }

    [Test]
    public void CommandsConfig_WithComments_IgnoresComments()
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

        // Act
        var config = _deserializer.Deserialize<CommandsConfig>(yaml);

        // Assert
        Assert.That(config.Commands, Has.Count.EqualTo(2));
        Assert.That(config.Commands[0].Name, Is.EqualTo("google"));
        Assert.That(config.Commands[1].Name, Is.EqualTo("github"));
    }

    [Test]
    public void CommandEntry_OptionalFieldsWithEmptyStrings_ArePreserved()
    {
        // Arrange
        var yaml = @"
name: test
linkto: https://example.com
description: ''
arguments: ''
";

        // Act
        var entry = _deserializer.Deserialize<CommandEntry>(yaml);

        // Assert
        Assert.That(entry.Description, Is.EqualTo(string.Empty));
        Assert.That(entry.Arguments, Is.EqualTo(string.Empty));
    }
}
