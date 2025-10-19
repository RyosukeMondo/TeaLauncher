using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace MetricsChecker;

/// <summary>
/// Main program for the MetricsChecker tool.
/// Analyzes C# code for metrics violations using Roslyn.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: MetricsChecker <project-path> [--exclude <dir1,dir2,...>]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --exclude    Comma-separated list of directories to exclude (e.g., obj,bin,.git)");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  MetricsChecker MyProject.csproj --exclude obj,bin");
            return 1;
        }

        var projectPath = args[0];
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "obj", "bin", ".git", ".vs" };

        // Parse command-line arguments
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--exclude" && i + 1 < args.Length)
            {
                var dirs = args[i + 1].Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var dir in dirs)
                {
                    excludeDirs.Add(dir.Trim());
                }
                i++;
            }
        }

        if (!File.Exists(projectPath))
        {
            Console.Error.WriteLine($"ERROR: Project file not found: {projectPath}");
            return 1;
        }

        try
        {
            // Register MSBuild defaults
            MSBuildLocator.RegisterDefaults();

            Console.WriteLine($"Analyzing project: {projectPath}");
            Console.WriteLine($"Excluding directories: {string.Join(", ", excludeDirs)}");
            Console.WriteLine();

            var allViolations = await AnalyzeProjectAsync(projectPath, excludeDirs);

            // Output results
            Console.WriteLine(MetricsReport.FormatSummary(allViolations));
            Console.WriteLine();

            if (allViolations.Count > 0)
            {
                Console.WriteLine(MetricsReport.FormatViolations(allViolations));
                return 1; // Exit with error code
            }

            return 0; // Success
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.Error.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
            return 1;
        }
    }

    /// <summary>
    /// Analyzes all C# files in a project for metrics violations.
    /// </summary>
    /// <param name="projectPath">Path to the .csproj file.</param>
    /// <param name="excludeDirs">Directories to exclude from analysis.</param>
    /// <returns>List of all violations found.</returns>
    static async Task<List<MetricsViolation>> AnalyzeProjectAsync(string projectPath, HashSet<string> excludeDirs)
    {
        var allViolations = new List<MetricsViolation>();

        using var workspace = MSBuildWorkspace.Create();

        // Suppress MSBuild warnings
        workspace.WorkspaceFailed += (sender, e) =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            {
                Console.Error.WriteLine($"Warning: {e.Diagnostic.Message}");
            }
        };

        Console.WriteLine("Loading project...");
        var project = await workspace.OpenProjectAsync(projectPath);

        if (project == null)
        {
            throw new InvalidOperationException("Failed to load project.");
        }

        var analyzer = new CodeMetricsAnalyzer();
        var documentsAnalyzed = 0;

        foreach (var document in project.Documents)
        {
            // Skip excluded directories
            if (ShouldExclude(document.FilePath ?? "", excludeDirs))
            {
                continue;
            }

            // Only analyze C# files
            if (document.SourceCodeKind != SourceCodeKind.Regular)
            {
                continue;
            }

            var syntaxTree = await document.GetSyntaxTreeAsync();
            if (syntaxTree == null)
            {
                continue;
            }

            documentsAnalyzed++;
            var result = analyzer.AnalyzeFile(syntaxTree, document.FilePath ?? document.Name);
            allViolations.AddRange(result.Violations);
        }

        Console.WriteLine($"Analyzed {documentsAnalyzed} file(s).");
        Console.WriteLine();

        return allViolations;
    }

    /// <summary>
    /// Checks if a file path should be excluded from analysis.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="excludeDirs">Directories to exclude.</param>
    /// <returns>True if the file should be excluded, false otherwise.</returns>
    static bool ShouldExclude(string filePath, HashSet<string> excludeDirs)
    {
        var pathParts = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return pathParts.Any(part => excludeDirs.Contains(part));
    }
}
