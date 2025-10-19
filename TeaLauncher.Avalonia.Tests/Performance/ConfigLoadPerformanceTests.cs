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
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Tests.Utilities;

namespace TeaLauncher.Avalonia.Tests.Performance;

/// <summary>
/// Performance tests for configuration loading operations.
/// Validates that config loading meets the 200ms performance requirement (4.4).
/// Tests LoadConfiguration with various config file sizes.
/// </summary>
[TestFixture]
public class ConfigLoadPerformanceTests : PerformanceTestBase
{
    private IServiceProvider? _serviceProvider;
    private IConfigurationLoader? _configLoader;
    private string? _tempConfigPath;

    /// <summary>
    /// Sets up test services for performance testing.
    /// Uses real service implementations to measure actual performance.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _serviceProvider = TestServiceProvider.CreateWithRealServices();
        _configLoader = _serviceProvider.GetRequiredService<IConfigurationLoader>();
    }

    /// <summary>
    /// Cleans up test resources and deletes temporary config files.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Delete temporary config file if it exists
        if (_tempConfigPath != null && File.Exists(_tempConfigPath))
        {
            try
            {
                File.Delete(_tempConfigPath);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }

        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Tests that loading a config with 100 commands completes within 200ms.
    /// Performance requirement 4.4: Config loading should complete within 200ms for 100 commands.
    /// </summary>
    [Test]
    public void ConfigLoad_100Commands_Within200ms()
    {
        // Arrange
        _tempConfigPath = CreateTempConfigFile(100);
        const int maxAllowedMs = 200;

        // Act - measure LoadConfiguration performance
        var result = TimeOperation(
            "Load configuration with 100 commands",
            () => _configLoader!.LoadConfiguration(_tempConfigPath),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that loading a small config (10 commands) is fast.
    /// This is a baseline test for minimal config loading overhead.
    /// </summary>
    [Test]
    public void ConfigLoad_10Commands_Fast()
    {
        // Arrange
        _tempConfigPath = CreateTempConfigFile(10);
        const int maxAllowedMs = 200;

        // Act
        var result = TimeOperation(
            "Load configuration with 10 commands",
            () => _configLoader!.LoadConfiguration(_tempConfigPath),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that loading a config with unicode command names maintains performance.
    /// Verifies that unicode processing in YAML doesn't cause performance degradation.
    /// </summary>
    [Test]
    public void ConfigLoad_UnicodeCommands_NoSlowdown()
    {
        // Arrange
        _tempConfigPath = CreateTempConfigFileWithUnicode(50);
        const int maxAllowedMs = 200;

        // Act
        var result = TimeOperation(
            "Load configuration with Unicode commands",
            () => _configLoader!.LoadConfiguration(_tempConfigPath),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that loading a large config (200 commands) completes reasonably fast.
    /// While the requirement is for 100 commands, testing 200 helps verify scalability.
    /// Allows more time (300ms) for the larger dataset.
    /// </summary>
    [Test]
    public void ConfigLoad_200Commands_Scalable()
    {
        // Arrange
        _tempConfigPath = CreateTempConfigFile(200);
        const int maxAllowedMs = 300; // More lenient for larger dataset

        // Act
        var result = TimeOperation(
            "Load configuration with 200 commands",
            () => _configLoader!.LoadConfiguration(_tempConfigPath),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that loading an empty config is fast.
    /// This is a baseline test for minimal YAML parsing overhead.
    /// </summary>
    [Test]
    public void ConfigLoad_EmptyConfig_Fast()
    {
        // Arrange
        _tempConfigPath = CreateTempConfigFile(0);
        const int maxAllowedMs = 200;

        // Act
        var result = TimeOperation(
            "Load empty configuration",
            () => _configLoader!.LoadConfiguration(_tempConfigPath),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Creates a temporary YAML config file with the specified number of commands.
    /// Commands are simple and follow the pattern: cmd1, cmd2, ..., cmdN
    /// Each command has a linkto URL for realistic config size.
    /// </summary>
    /// <param name="commandCount">Number of commands to generate.</param>
    /// <returns>Path to the temporary config file.</returns>
    private string CreateTempConfigFile(int commandCount)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"tealauncher_perf_test_{Guid.NewGuid()}.yaml");
        var yaml = new StringBuilder();

        yaml.AppendLine("commands:");

        if (commandCount == 0)
        {
            // For empty config, provide empty array instead of null
            yaml.AppendLine("  []");
        }
        else
        {
            for (int i = 1; i <= commandCount; i++)
            {
                yaml.AppendLine($"  - name: cmd{i}");
                yaml.AppendLine($"    linkto: https://example.com/cmd{i}");
                yaml.AppendLine($"    description: Performance test command {i}");
            }
        }

        File.WriteAllText(tempPath, yaml.ToString());
        return tempPath;
    }

    /// <summary>
    /// Creates a temporary YAML config file with unicode command names.
    /// Uses commands from EdgeCaseTestFixtures.UnicodeCommandNames.
    /// </summary>
    /// <param name="commandCount">Number of commands to generate.</param>
    /// <returns>Path to the temporary config file.</returns>
    private string CreateTempConfigFileWithUnicode(int commandCount)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"tealauncher_perf_test_unicode_{Guid.NewGuid()}.yaml");
        var yaml = new StringBuilder();

        yaml.AppendLine("commands:");

        var unicodeNames = EdgeCaseTestFixtures.UnicodeCommandNames;
        for (int i = 0; i < commandCount; i++)
        {
            var unicodeName = unicodeNames[i % unicodeNames.Count];
            yaml.AppendLine($"  - name: {unicodeName}{i}");
            yaml.AppendLine($"    linkto: https://example.com/unicode{i}");
            yaml.AppendLine($"    description: Unicode test command {i}");
        }

        File.WriteAllText(tempPath, yaml.ToString(), Encoding.UTF8);
        return tempPath;
    }
}
