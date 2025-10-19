using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace TeaLauncher.Avalonia.Configuration;

/// <summary>
/// Root configuration object representing the entire YAML configuration file.
/// </summary>
public record CommandsConfig
{
    /// <summary>
    /// List of command entries defined in the configuration.
    /// </summary>
    [YamlMember(Alias = "commands")]
    public List<CommandEntry> Commands { get; init; } = new();
}

/// <summary>
/// Represents a single command entry in the configuration.
/// </summary>
public record CommandEntry
{
    /// <summary>
    /// Command keyword/name used to trigger the command (e.g., "google", "notepad").
    /// This field is required.
    /// </summary>
    [YamlMember(Alias = "name")]
    public required string Name { get; init; }

    /// <summary>
    /// Target URL, file path, or executable path to launch.
    /// This field is required.
    /// </summary>
    [YamlMember(Alias = "linkto")]
    public required string LinkTo { get; init; }

    /// <summary>
    /// Optional description of the command for documentation purposes.
    /// </summary>
    [YamlMember(Alias = "description")]
    public string? Description { get; init; }

    /// <summary>
    /// Optional command-line arguments to pass to the executable.
    /// </summary>
    [YamlMember(Alias = "arguments")]
    public string? Arguments { get; init; }
}
