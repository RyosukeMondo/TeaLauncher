# TeaLauncher Development Commands

## Building
```bash
# Restore dependencies
dotnet restore TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj

# Build main application (Release)
dotnet build TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj --configuration Release --no-restore

# Build for Windows (from any OS)
dotnet build TeaLauncher.Avalonia -c Release -r win-x64

# Create single-file executable
dotnet publish TeaLauncher.Avalonia -c Release -r win-x64 \
  --self-contained true -p:PublishSingleFile=true
```

## Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run specific test category
dotnet test --filter FullyQualifiedName~Application.Services  # Unit tests
dotnet test --filter FullyQualifiedName~Integration           # Integration tests
dotnet test --filter FullyQualifiedName~EndToEnd              # E2E tests

# Watch mode (auto-rerun on changes)
dotnet watch test

# Generate HTML coverage report
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:./coverage/**/coverage.cobertura.xml \
  -targetdir:./coverage-report -reporttypes:Html

# Open coverage report
xdg-open ./coverage-report/index.html  # Linux
start ./coverage-report/index.html      # Windows
```

## Quality Checks
```bash
# Check code coverage threshold (≥80%)
./scripts/check-coverage.sh

# Check code metrics (file ≤500 lines, method ≤50 lines, complexity ≤15)
dotnet run --project tools/MetricsChecker/MetricsChecker.csproj \
  -- TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj

# Verify code formatting
dotnet format --verify-no-changes

# Run all pre-commit checks manually
dotnet husky run
```

## Pre-commit Hooks
```bash
# Install Husky.Net hooks (one-time setup)
dotnet tool install Husky
dotnet husky install

# Bypass hooks (emergency only)
git commit --no-verify -m "WIP: Feature in progress"
```

## Git Workflow
```bash
# Standard commands
git status
git add <files>
git commit -m "message"  # Runs pre-commit hooks automatically
git push
git pull

# Create branch
git checkout -b feature/my-feature

# View logs
git log --oneline
git log -1 --format='%an %ae'  # Check authorship
```

## Development Utilities
```bash
# List files/directories
ls -la
find <directory> -name "*.cs"

# Search code
grep -r "pattern" TeaLauncher.Avalonia/
grep -n "class.*Service" TeaLauncher.Avalonia/Application/Services/

# View files
cat <file>
head -n 20 <file>
tail -n 20 <file>
```

## Task Completion Workflow
When a task is completed, run these commands in order:
1. **Build**: `dotnet build --configuration Release --no-restore`
2. **Test**: `dotnet test --no-build --verbosity minimal`
3. **Coverage**: `dotnet test --collect:"XPlat Code Coverage"`
4. **Check Coverage**: `./scripts/check-coverage.sh`
5. **Metrics**: `dotnet run --project tools/MetricsChecker/MetricsChecker.csproj -- TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj`
6. **Format**: `dotnet format --verify-no-changes`
7. **Commit**: `git add . && git commit -m "Descriptive message"`

Or simply run `git commit` and let pre-commit hooks handle steps 1-6 automatically.
