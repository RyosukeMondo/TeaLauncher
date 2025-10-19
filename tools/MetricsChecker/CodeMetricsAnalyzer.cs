using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricsChecker;

/// <summary>
/// Analyzes C# code for metrics violations using Roslyn syntax analysis.
/// </summary>
public class CodeMetricsAnalyzer
{
    private const int MaxFileLines = 500;
    private const int MaxMethodLines = 50;
    private const int MaxCyclomaticComplexity = 15;

    /// <summary>
    /// Analyzes a syntax tree for code metrics violations.
    /// </summary>
    /// <param name="tree">The syntax tree to analyze.</param>
    /// <param name="filePath">The path to the file being analyzed.</param>
    /// <returns>A MetricsResult containing any violations found.</returns>
    public MetricsResult AnalyzeFile(SyntaxTree tree, string filePath)
    {
        var result = new MetricsResult { FilePath = filePath };
        var root = tree.GetRoot();

        // Check file line count
        var lineCount = tree.GetText().Lines.Count;
        if (lineCount > MaxFileLines)
        {
            result.Violations.Add(new MetricsViolation
            {
                FilePath = filePath,
                LineNumber = 1,
                MetricName = "File Length",
                ActualValue = lineCount,
                Threshold = MaxFileLines,
                Message = $"File has {lineCount} lines (max: {MaxFileLines})"
            });
        }

        // Check methods for line count and cyclomatic complexity
        var methodVisitor = new MethodMetricsVisitor(filePath, MaxMethodLines, MaxCyclomaticComplexity);
        methodVisitor.Visit(root);
        result.Violations.AddRange(methodVisitor.Violations);

        return result;
    }

    /// <summary>
    /// Visitor that walks the syntax tree to analyze method metrics.
    /// </summary>
    private class MethodMetricsVisitor : CSharpSyntaxWalker
    {
        private readonly string _filePath;
        private readonly int _maxMethodLines;
        private readonly int _maxComplexity;
        public List<MetricsViolation> Violations { get; } = new();

        public MethodMetricsVisitor(string filePath, int maxMethodLines, int maxComplexity)
        {
            _filePath = filePath;
            _maxMethodLines = maxMethodLines;
            _maxComplexity = maxComplexity;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            AnalyzeMethod(node, node.Identifier.Text);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            AnalyzeMethod(node, node.Identifier.Text);
            base.VisitConstructorDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            // Only analyze properties with explicit getters/setters
            if (node.AccessorList != null)
            {
                foreach (var accessor in node.AccessorList.Accessors)
                {
                    if (accessor.Body != null || accessor.ExpressionBody != null)
                    {
                        AnalyzeMethod(accessor, $"{node.Identifier.Text}.{accessor.Keyword.Text}");
                    }
                }
            }
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            // Skip - handled in VisitPropertyDeclaration
            base.VisitAccessorDeclaration(node);
        }

        private void AnalyzeMethod(SyntaxNode node, string methodName)
        {
            // Calculate line count (excluding blank lines and comments)
            var lineCount = CountMeaningfulLines(node);
            if (lineCount > _maxMethodLines)
            {
                var lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                Violations.Add(new MetricsViolation
                {
                    FilePath = _filePath,
                    LineNumber = lineNumber,
                    MetricName = "Method Length",
                    ActualValue = lineCount,
                    Threshold = _maxMethodLines,
                    Message = $"Method '{methodName}' has {lineCount} lines (max: {_maxMethodLines})"
                });
            }

            // Calculate cyclomatic complexity
            var complexity = CalculateComplexity(node);
            if (complexity > _maxComplexity)
            {
                var lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                Violations.Add(new MetricsViolation
                {
                    FilePath = _filePath,
                    LineNumber = lineNumber,
                    MetricName = "Cyclomatic Complexity",
                    ActualValue = complexity,
                    Threshold = _maxComplexity,
                    Message = $"Method '{methodName}' has complexity {complexity} (max: {_maxComplexity})"
                });
            }
        }

        private int CountMeaningfulLines(SyntaxNode node)
        {
            var text = node.GetText();
            var meaningfulLines = 0;

            foreach (var line in text.Lines)
            {
                var lineText = line.ToString().Trim();
                // Skip blank lines and comment-only lines
                if (!string.IsNullOrWhiteSpace(lineText) &&
                    !lineText.StartsWith("//") &&
                    !lineText.StartsWith("/*") &&
                    !lineText.StartsWith("*") &&
                    lineText != "*/")
                {
                    meaningfulLines++;
                }
            }

            return meaningfulLines;
        }

        private int CalculateComplexity(SyntaxNode node)
        {
            // Base complexity is 1
            var complexity = 1;
            var complexityVisitor = new ComplexityVisitor();
            complexityVisitor.Visit(node);
            return complexity + complexityVisitor.ComplexityCount;
        }

        /// <summary>
        /// Visitor that counts decision points for cyclomatic complexity calculation.
        /// </summary>
        private class ComplexityVisitor : CSharpSyntaxWalker
        {
            public int ComplexityCount { get; private set; }

            public override void VisitIfStatement(IfStatementSyntax node)
            {
                ComplexityCount++;
                base.VisitIfStatement(node);
            }

            public override void VisitWhileStatement(WhileStatementSyntax node)
            {
                ComplexityCount++;
                base.VisitWhileStatement(node);
            }

            public override void VisitForStatement(ForStatementSyntax node)
            {
                ComplexityCount++;
                base.VisitForStatement(node);
            }

            public override void VisitForEachStatement(ForEachStatementSyntax node)
            {
                ComplexityCount++;
                base.VisitForEachStatement(node);
            }

            public override void VisitSwitchSection(SwitchSectionSyntax node)
            {
                // Each case adds to complexity
                ComplexityCount++;
                base.VisitSwitchSection(node);
            }

            public override void VisitCatchClause(CatchClauseSyntax node)
            {
                ComplexityCount++;
                base.VisitCatchClause(node);
            }

            public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
            {
                ComplexityCount++;
                base.VisitConditionalExpression(node);
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                // Count &&, || and ?? operators
                if (node.Kind() == SyntaxKind.LogicalAndExpression ||
                    node.Kind() == SyntaxKind.LogicalOrExpression ||
                    node.Kind() == SyntaxKind.CoalesceExpression)
                {
                    ComplexityCount++;
                }
                base.VisitBinaryExpression(node);
            }
        }
    }
}

/// <summary>
/// Represents the result of analyzing a file for metrics violations.
/// </summary>
public class MetricsResult
{
    public string FilePath { get; set; } = string.Empty;
    public List<MetricsViolation> Violations { get; set; } = new();
}

/// <summary>
/// Represents a single metrics violation.
/// </summary>
public class MetricsViolation
{
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public int ActualValue { get; set; }
    public int Threshold { get; set; }
    public string Message { get; set; } = string.Empty;
}
