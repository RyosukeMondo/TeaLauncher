using System;

namespace TeaLauncher.Avalonia.Tests.Performance;

/// <summary>
/// Represents the result of a performance test measurement.
/// Contains timing information and threshold comparison for performance assertions.
/// </summary>
/// <param name="OperationName">The name of the operation being measured.</param>
/// <param name="Duration">The actual duration the operation took to complete.</param>
/// <param name="MaxAllowedMs">The maximum allowed duration in milliseconds for the operation to pass.</param>
/// <param name="Passed">Whether the operation completed within the allowed threshold.</param>
public record PerformanceResult(
    string OperationName,
    TimeSpan Duration,
    int MaxAllowedMs,
    bool Passed)
{
    /// <summary>
    /// Gets the duration of the operation in milliseconds.
    /// </summary>
    public double DurationMs => Duration.TotalMilliseconds;

    /// <summary>
    /// Gets the percentage of the allowed limit that was used.
    /// For example, if the operation took 75ms with a 100ms limit, this returns 75.0.
    /// </summary>
    public double PercentageOfLimit => (DurationMs / MaxAllowedMs) * 100.0;
}
