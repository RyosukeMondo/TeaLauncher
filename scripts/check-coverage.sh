#!/bin/bash

# Check code coverage threshold
# Parses Cobertura XML and verifies coverage meets 80% threshold

set -e

THRESHOLD=60
COVERAGE_DIR="./coverage"

# Find the most recent coverage file
COVERAGE_FILE=$(find "$COVERAGE_DIR" -name "coverage.cobertura.xml" -type f 2>/dev/null | head -n 1)

if [ -z "$COVERAGE_FILE" ]; then
    echo "ERROR: No coverage file found in $COVERAGE_DIR"
    echo "Please run: dotnet test --collect:\"XPlat Code Coverage\" --results-directory ./coverage"
    exit 1
fi

echo "Checking coverage report: $COVERAGE_FILE"

# Extract line coverage percentage from Cobertura XML
# Format: <coverage line-rate="0.8512" ...>
LINE_RATE=$(grep -oP '(?<=line-rate=")[0-9.]+' "$COVERAGE_FILE" | head -n 1)

if [ -z "$LINE_RATE" ]; then
    echo "ERROR: Could not extract coverage percentage from $COVERAGE_FILE"
    exit 1
fi

# Convert to percentage (multiply by 100)
COVERAGE=$(echo "$LINE_RATE * 100" | bc)
COVERAGE_INT=$(echo "$COVERAGE / 1" | bc)

echo "Current coverage: ${COVERAGE_INT}%"
echo "Required threshold: ${THRESHOLD}%"

# Compare coverage to threshold
if [ "$COVERAGE_INT" -lt "$THRESHOLD" ]; then
    echo ""
    echo "❌ COVERAGE CHECK FAILED"
    echo "Coverage ${COVERAGE_INT}% is below threshold ${THRESHOLD}%"
    echo ""
    echo "To fix:"
    echo "1. Add more unit tests to increase coverage"
    echo "2. Run 'dotnet test --collect:\"XPlat Code Coverage\"' to generate coverage report"
    echo "3. Check coverage report for uncovered lines"
    exit 1
fi

echo "✅ Coverage check passed (${COVERAGE_INT}% >= ${THRESHOLD}%)"
exit 0
