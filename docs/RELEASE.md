# Release Process

This document describes how to create releases for TeaLauncher using GitHub Actions.

## Automated Release Workflow

The release workflow automatically builds TeaLauncher for both Windows and Linux platforms.

### Supported Platforms

- **Windows** (win-x64): Self-contained single executable for Windows 10/11
- **Linux** (linux-x64): Self-contained single executable for Linux distributions

### Triggering a Release

#### Option 1: Tag-based Release (Recommended)

Create and push a version tag to automatically trigger a release:

```bash
# Create a version tag
git tag v2.0.0

# Push the tag to GitHub
git push origin v2.0.0
```

This will:
1. Build the application for Windows and Linux
2. Run all tests
3. Create release packages (`.zip` for Windows, `.tar.gz` for Linux)
4. Create a GitHub Release with the artifacts attached
5. Generate release notes automatically

#### Option 2: Manual Workflow Trigger

You can manually trigger the workflow from GitHub:

1. Go to the "Actions" tab in your GitHub repository
2. Select "Build and Release" workflow
3. Click "Run workflow"
4. Optionally specify a version (e.g., `v2.0.0` or `manual-build`)
5. Click "Run workflow"

Manual builds create artifacts but **do not** create GitHub releases.

### Release Artifacts

Each release creates the following artifacts:

#### Windows
- `TeaLauncher-windows-x64-vX.X.X.zip`
  - Contains: `TeaLauncher.exe` (self-contained, no .NET installation required)
  - Size: ~60-80 MB

#### Linux
- `TeaLauncher-linux-x64-vX.X.X.tar.gz`
  - Contains: `TeaLauncher` (self-contained, no .NET installation required)
  - Size: ~60-80 MB

### Version Numbering

Follow semantic versioning (SemVer):

- **Major version** (v2.0.0): Breaking changes
- **Minor version** (v2.1.0): New features, backwards compatible
- **Patch version** (v2.0.1): Bug fixes, backwards compatible

### Release Checklist

Before creating a release:

1. ✅ All tests pass locally
2. ✅ CI builds are green
3. ✅ Version number updated in `.csproj` files if needed
4. ✅ CHANGELOG.md updated (if you maintain one)
5. ✅ Code reviewed and merged to main branch
6. ✅ Create and push version tag

### Testing Release Builds

You can test release builds without creating a GitHub release:

#### Manually trigger workflow
1. Go to Actions → Build and Release → Run workflow
2. Download artifacts from the workflow run
3. Test on target platforms

#### Build locally

**Windows:**
```bash
dotnet publish TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

**Linux:**
```bash
dotnet publish TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

### Troubleshooting

#### Build fails on Linux
- Check that the project doesn't have Windows-specific dependencies
- Ensure Avalonia is properly configured for cross-platform builds

#### Build fails on Windows
- Verify .NET 8 SDK is correctly installed
- Check that all NuGet packages are restored

#### GitHub Release not created
- Ensure the workflow was triggered by a tag push (not manual trigger)
- Verify repository permissions allow creating releases
- Check that `GITHUB_TOKEN` has sufficient permissions

### Advanced: Pre-release Builds

To create a pre-release:

```bash
# Create a pre-release tag
git tag v2.0.0-beta.1

# Push the tag
git push origin v2.0.0-beta.1
```

The workflow will detect the pre-release tag and mark the GitHub release accordingly.

### Build Times

Typical build times on GitHub Actions:
- Windows build: ~5-10 minutes
- Linux build: ~5-10 minutes
- Total (parallel): ~10-15 minutes

### Artifact Retention

Build artifacts are retained for **30 days** by default. GitHub releases are permanent unless manually deleted.
