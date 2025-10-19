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

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TeaLauncher.Avalonia.Domain.Interfaces;
using TeaLauncher.Avalonia.Domain.Models;
using TeaLauncher.Avalonia.Tests.Utilities;

namespace TeaLauncher.Avalonia.Tests.Performance;

/// <summary>
/// Performance tests for command execution operations.
/// Validates that command execution meets the 100ms performance requirement (4.2).
/// Tests measure time from ExecuteAsync call to method return (process start, not completion).
/// </summary>
[TestFixture]
public class CommandExecutionPerformanceTests : PerformanceTestBase
{
    private IServiceProvider? _serviceProvider;
    private ICommandExecutor? _commandExecutor;
    private ICommandRegistry? _commandRegistry;

    /// <summary>
    /// Sets up test services for performance testing.
    /// Uses real service implementations to measure actual performance.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _serviceProvider = TestServiceProvider.CreateWithRealServices();
        _commandExecutor = _serviceProvider.GetRequiredService<ICommandExecutor>();
        _commandRegistry = _serviceProvider.GetRequiredService<ICommandRegistry>();

        // Register a test command
        _commandRegistry.RegisterCommand(new Command("testcmd", "echo", "Test command for performance testing"));
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
    /// Tests that command execution from input to start completes within 100ms.
    /// Measures time from ExecuteAsync call to return (process start, not process completion).
    /// Performance requirement 4.2: Command execution should complete within 100ms.
    /// </summary>
    [Test]
    public async Task CommandExecution_FromInputToStart_Within100ms()
    {
        // Arrange
        const string input = "testcmd argument";
        const int maxAllowedMs = 100;

        // Act - measure execution time (this starts the process but doesn't wait for completion)
        var result = await TimeOperationAsync(
            "Command Execution (Input to Process Start)",
            async () => await _commandExecutor!.ExecuteAsync(input),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests that concurrent command execution maintains performance without degradation.
    /// Executes 10 commands concurrently and verifies average execution time is within 100ms.
    /// This tests thread safety and concurrent execution performance.
    /// </summary>
    [Test]
    public async Task ConcurrentCommands_NoPerformanceDegradation()
    {
        // Arrange
        const int commandCount = 10;
        const int maxAllowedMs = 100;
        var tasks = new List<Task<PerformanceResult>>();

        // Act - execute multiple commands concurrently and measure each
        for (int i = 0; i < commandCount; i++)
        {
            var commandIndex = i; // Capture for closure
            var task = TimeOperationAsync(
                $"Concurrent Command {commandIndex + 1}",
                async () => await _commandExecutor!.ExecuteAsync($"testcmd arg{commandIndex}"),
                maxAllowedMs);
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        // Assert - verify average execution time is within limit
        var averageDuration = results.Average(r => r.DurationMs);
        var averageResult = CreatePerformanceResult(
            "Average Concurrent Command Execution",
            TimeSpan.FromMilliseconds(averageDuration),
            maxAllowedMs);

        AssertDuration(averageResult);

        // Also verify that all individual commands met the threshold
        foreach (var result in results)
        {
            AssertDuration(result);
        }
    }

    /// <summary>
    /// Tests that executing a simple echo command is fast.
    /// This is a baseline test for minimal command execution overhead.
    /// </summary>
    [Test]
    public async Task SimpleCommand_ExecutesQuickly()
    {
        // Arrange
        const string input = "testcmd";
        const int maxAllowedMs = 100;

        // Act
        var result = await TimeOperationAsync(
            "Simple Command Execution",
            async () => await _commandExecutor!.ExecuteAsync(input),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }

    /// <summary>
    /// Tests command execution with quoted arguments containing special characters.
    /// Verifies that argument parsing overhead doesn't degrade performance.
    /// Uses special characters from EdgeCaseTestFixtures.
    /// </summary>
    [Test]
    public async Task CommandWithSpecialCharacters_MaintainsPerformance()
    {
        // Arrange
        var specialArg = EdgeCaseTestFixtures.SpecialCharacterArguments[0]; // "arg with double quotes"
        var input = $"testcmd {specialArg}";
        const int maxAllowedMs = 100;

        // Act
        var result = await TimeOperationAsync(
            "Command Execution with Special Characters",
            async () => await _commandExecutor!.ExecuteAsync(input),
            maxAllowedMs);

        // Assert
        AssertDuration(result);
    }
}
