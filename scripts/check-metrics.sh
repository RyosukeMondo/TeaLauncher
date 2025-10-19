#!/bin/bash

# Check code metrics using MetricsChecker tool
# Wrapper script for cross-platform compatibility

set -e

PROJECT_PATH="${1:-TeaLauncher.Avalonia.csproj}"

echo "Checking code metrics for: $PROJECT_PATH"

# Run MetricsChecker tool
dotnet run --project tools/MetricsChecker/MetricsChecker.csproj -- "$PROJECT_PATH"

EXIT_CODE=$?

if [ $EXIT_CODE -ne 0 ]; then
    echo ""
    echo "❌ METRICS CHECK FAILED"
    echo ""
    echo "Metrics thresholds:"
    echo "  - File length: ≤ 500 lines"
    echo "  - Method length: ≤ 50 lines"
    echo "  - Cyclomatic complexity: ≤ 15"
    echo ""
    echo "To fix:"
    echo "1. Refactor large files into smaller, focused modules"
    echo "2. Break down long methods into smaller helper methods"
    echo "3. Reduce complexity by extracting conditional logic"
    exit 1
fi

echo "✅ Metrics check passed"
exit 0
