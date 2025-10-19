# Task Completion Workflow

When you complete a task in this project, follow this workflow to ensure quality:

## Automatic Workflow (Recommended)
Simply commit your changes:
```bash
git add .
git commit -m "Descriptive commit message"
```

The pre-commit hooks (Husky.Net) will automatically run:
1. ✓ Build (dotnet build)
2. ✓ Tests (dotnet test)
3. ✓ Coverage check (≥80% threshold)
4. ✓ Metrics check (file ≤500, method ≤50, complexity ≤15)
5. ✓ Format check (dotnet format)

If any check fails, the commit is blocked and you'll see clear error messages.

## Manual Workflow (For Testing Before Commit)
Run these commands in order:

### 1. Build
```bash
dotnet build TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj --configuration Release --no-restore
```
**What it checks**: Code compiles without errors or warnings (warnings treated as errors)

### 2. Run Tests
```bash
dotnet test --no-build --verbosity minimal --configuration Release
```
**What it checks**: All unit, integration, and E2E tests pass

### 3. Generate Coverage
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage --configuration Release
```
**What it checks**: Collects code coverage data

### 4. Validate Coverage Threshold
```bash
./scripts/check-coverage.sh
```
**What it checks**: Line coverage ≥80%, Branch coverage ≥70%

### 5. Check Code Metrics
```bash
dotnet run --project tools/MetricsChecker/MetricsChecker.csproj -- TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj
```
**What it checks**:
- Files ≤500 lines
- Methods ≤50 lines (excluding comments/blank lines)
- Cyclomatic complexity ≤15 per method

### 6. Verify Code Formatting
```bash
dotnet format TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj --verify-no-changes
```
**What it checks**: Code follows .NET formatting conventions

### 7. Commit Changes
```bash
git add .
git commit -m "Your commit message"
git push
```

## If Pre-commit Hooks Fail

### Build Failures
- Fix compiler errors/warnings
- Ensure all using statements are present
- Check nullable reference type annotations

### Test Failures
- Read test output to identify failing test
- Fix the bug or update the test if requirements changed
- Re-run tests: `dotnet test`

### Coverage Below Threshold
```bash
# Generate HTML report to see what's not covered
reportgenerator -reports:./coverage/**/coverage.cobertura.xml \
  -targetdir:./coverage-report -reporttypes:Html
xdg-open ./coverage-report/index.html

# Add tests for uncovered code
# Focus on Application and Domain layers (need ≥90%)
```

### Metrics Violations
```
VIOLATION: File.cs - File has 520 lines (max: 500)
→ Refactor file into smaller files

VIOLATION: File.cs:45 - Method 'Foo' has 60 lines (max: 50)
→ Extract helper methods

VIOLATION: File.cs:100 - Method 'Bar' complexity 18 (max: 15)
→ Simplify method, reduce if/switch/loop nesting
```

### Format Violations
```bash
# Auto-format code (don't use --verify-no-changes)
dotnet format TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj

# Then commit
git add .
git commit -m "Your message"
```

## Emergency Bypass (Use Sparingly!)
If you need to commit WIP code on a feature branch:
```bash
git commit --no-verify -m "WIP: Feature in progress"
```

⚠️ **WARNING**: Never use `--no-verify` on main branch or before creating a PR!

## CI/CD Pipeline
After pushing to GitHub, the CI pipeline runs the same checks on:
- Linux (ubuntu-latest)
- Windows (windows-latest)

Pull requests require:
- All CI checks passing
- Code coverage ≥80%
- No metrics violations
- Proper formatting
- At least one approval

## Quick Reference
```bash
# Full manual workflow
dotnet build --configuration Release --no-restore && \
dotnet test --no-build --verbosity minimal && \
dotnet test --collect:"XPlat Code Coverage" && \
./scripts/check-coverage.sh && \
dotnet run --project tools/MetricsChecker/MetricsChecker.csproj -- TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj && \
dotnet format --verify-no-changes && \
echo "✓ All checks passed!"

# Or just commit and let hooks handle it
git commit -m "Your message"
```
