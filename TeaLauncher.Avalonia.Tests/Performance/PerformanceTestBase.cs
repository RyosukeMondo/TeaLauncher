using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace TeaLauncher.Avalonia.Tests.Performance;

/// <summary>
/// Base class for performance tests providing timing utilities and assertion methods.
/// Performance tests verify that critical operations meet their performance requirements:
/// - Command execution: 100ms
/// - Autocomplete: 50ms
/// - Config loading: 200ms
///
/// Use TimeOperation or TimeOperationAsync to measure operations, then AssertDuration to verify results.
/// Performance tests should be run in Release mode for accurate measurements.
/// </summary>
[TestFixture]
public abstract class PerformanceTestBase
{
    /// <summary>
    /// Times a synchronous operation and returns performance metrics.
    /// Uses high-resolution Stopwatch for accurate timing.
    /// </summary>
    /// <param name="operationName">Descriptive name of the operation being measured.</param>
    /// <param name="action">The operation to measure.</param>
    /// <param name="maxAllowedMs">Maximum allowed duration in milliseconds for the operation to pass.</param>
    /// <returns>PerformanceResult containing timing data and pass/fail status.</returns>
    protected PerformanceResult TimeOperation(string operationName, Action action, int maxAllowedMs)
    {
        // Warm up JIT compiler by running the operation once
        // This prevents JIT compilation overhead from affecting the measurement
        try
        {
            action();
        }
        catch
        {
            // Ignore warmup failures
        }

        // Perform the actual measurement
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();

        return CreatePerformanceResult(operationName, stopwatch.Elapsed, maxAllowedMs);
    }

    /// <summary>
    /// Times an asynchronous operation and returns performance metrics.
    /// Uses high-resolution Stopwatch for accurate timing.
    /// </summary>
    /// <param name="operationName">Descriptive name of the operation being measured.</param>
    /// <param name="action">The async operation to measure.</param>
    /// <param name="maxAllowedMs">Maximum allowed duration in milliseconds for the operation to pass.</param>
    /// <returns>Task containing PerformanceResult with timing data and pass/fail status.</returns>
    protected async Task<PerformanceResult> TimeOperationAsync(string operationName, Func<Task> action, int maxAllowedMs)
    {
        // Warm up JIT compiler by running the operation once
        // This prevents JIT compilation overhead from affecting the measurement
        try
        {
            await action();
        }
        catch
        {
            // Ignore warmup failures
        }

        // Perform the actual measurement
        var stopwatch = Stopwatch.StartNew();
        await action();
        stopwatch.Stop();

        return CreatePerformanceResult(operationName, stopwatch.Elapsed, maxAllowedMs);
    }

    /// <summary>
    /// Creates a PerformanceResult with the given timing data.
    /// </summary>
    /// <param name="operationName">Name of the operation.</param>
    /// <param name="duration">Actual duration measured.</param>
    /// <param name="maxAllowedMs">Maximum allowed duration in milliseconds.</param>
    /// <returns>PerformanceResult with calculated pass/fail status.</returns>
    protected PerformanceResult CreatePerformanceResult(string operationName, TimeSpan duration, int maxAllowedMs)
    {
        var passed = duration.TotalMilliseconds <= maxAllowedMs;
        return new PerformanceResult(operationName, duration, maxAllowedMs, passed);
    }

    /// <summary>
    /// Asserts that a performance result passed the threshold.
    /// Provides detailed failure message showing actual vs. expected duration.
    /// </summary>
    /// <param name="result">The performance result to assert.</param>
    protected void AssertDuration(PerformanceResult result)
    {
        result.Passed.Should().BeTrue(
            because: $"{result.OperationName} took {result.DurationMs:F2}ms but should complete within {result.MaxAllowedMs}ms ({result.PercentageOfLimit:F1}% of limit used)");
    }
}
