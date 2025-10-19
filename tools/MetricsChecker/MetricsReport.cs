namespace MetricsChecker;

/// <summary>
/// Formats metrics violations as human-readable text reports.
/// </summary>
public static class MetricsReport
{
    /// <summary>
    /// Formats violations as a text report.
    /// </summary>
    /// <param name="violations">The list of violations to format.</param>
    /// <returns>A formatted text report.</returns>
    public static string FormatViolations(List<MetricsViolation> violations)
    {
        if (violations.Count == 0)
        {
            return "No metrics violations found.";
        }

        var lines = new List<string>
        {
            $"Found {violations.Count} metrics violation(s):",
            ""
        };

        foreach (var violation in violations.OrderBy(v => v.FilePath).ThenBy(v => v.LineNumber))
        {
            lines.Add($"VIOLATION: {violation.FilePath}:{violation.LineNumber} - {violation.Message}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Formats a summary of violations by type.
    /// </summary>
    /// <param name="violations">The list of violations to summarize.</param>
    /// <returns>A formatted summary.</returns>
    public static string FormatSummary(List<MetricsViolation> violations)
    {
        if (violations.Count == 0)
        {
            return "✓ All code metrics checks passed.";
        }

        var groupedByType = violations.GroupBy(v => v.MetricName);
        var lines = new List<string> { "Metrics Violations Summary:" };

        foreach (var group in groupedByType.OrderBy(g => g.Key))
        {
            lines.Add($"  {group.Key}: {group.Count()} violation(s)");
        }

        lines.Add("");
        lines.Add($"Total: {violations.Count} violation(s)");

        return string.Join(Environment.NewLine, lines);
    }
}
