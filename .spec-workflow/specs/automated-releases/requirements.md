# Requirements Document

## Introduction

This specification defines an automated release pipeline using GitHub Actions to build and publish TeaLauncher binaries for Windows and Linux platforms when version tags are created. The system will automate the manual release process, ensuring consistent builds, reducing release overhead, and making it easier for users to download platform-specific executables.

The value to the project includes:
- **Automated Distribution**: Instant release creation when tags are pushed (e.g., `v1.0.0`)
- **Multi-Platform Support**: Cross-platform builds (Windows x64, Linux x64) from a single workflow
- **User Convenience**: Pre-built executables available for download without requiring users to build from source
- **Release Consistency**: Standardized build process eliminating manual errors
- **Community Growth**: Lower barrier to adoption with ready-to-run binaries

## Alignment with Product Vision

This feature directly supports TeaLauncher's business objectives and product principles:

**Business Objectives Alignment:**
- **Foster Community Adoption**: Pre-built binaries reduce friction for new users who want to try TeaLauncher
- **Open Source Tool**: Transparent, automated builds demonstrate commitment to open development
- **Minimal Dependencies**: Release artifacts include self-contained executables reducing user setup burden

**Product Principles Alignment:**
- **Simplicity First**: Users can download and run without installing build tools or dependencies
- **Fast Feedback**: Automated releases provide immediate availability of new features to users
- **Unobtrusive**: Release process runs automatically in GitHub Actions without developer intervention

**Future Vision Support:**
- **Cross-Platform Support**: Aligns with ".NET Core port for Linux and macOS compatibility" vision
- **Community Collaboration**: Makes it easy for contributors to test releases on multiple platforms

## Requirements

### Requirement 1: Version Tag Triggered Releases

**User Story:** As a project maintainer, I want releases to be automatically created when I push a version tag, so that I don't have to manually build and upload binaries for each release.

#### Acceptance Criteria

1. WHEN a git tag matching pattern `v*.*.*` (e.g., `v1.0.0`, `v2.1.3-beta`) is pushed THEN the GitHub Actions workflow SHALL trigger automatically
2. WHEN the tag is pushed THEN the system SHALL extract the version number from the tag name (e.g., `v1.2.3` → `1.2.3`)
3. WHEN a non-version tag is pushed (e.g., `docs-update`, `test-tag`) THEN the workflow SHALL NOT trigger
4. IF a tag is deleted and re-pushed THEN the workflow SHALL re-run and update the existing release
5. WHEN the workflow completes successfully THEN a GitHub Release SHALL be created with the tag as the release version

### Requirement 2: Windows Build Artifacts

**User Story:** As a Windows user, I want to download a pre-built Windows executable, so that I can run TeaLauncher without installing .NET SDK or build tools.

#### Acceptance Criteria

1. WHEN the release workflow runs THEN the system SHALL build TeaLauncher for Windows x64 (win-x64) runtime
2. WHEN building for Windows THEN the system SHALL use Release configuration for optimized binaries
3. WHEN building for Windows THEN the system SHALL create a self-contained, single-file executable including the .NET runtime
4. WHEN the Windows build completes THEN the executable SHALL be named `TeaLauncher-{version}-win-x64.exe` (e.g., `TeaLauncher-1.0.0-win-x64.exe`)
5. WHEN the Windows build completes THEN the executable SHALL be uploaded as a release asset to the GitHub Release
6. IF the Windows build fails THEN the workflow SHALL fail and NOT create a release

### Requirement 3: Linux Build Artifacts

**User Story:** As a Linux user, I want to download a pre-built Linux executable, so that I can run TeaLauncher on my Linux desktop without manual compilation.

#### Acceptance Criteria

1. WHEN the release workflow runs THEN the system SHALL build TeaLauncher for Linux x64 (linux-x64) runtime
2. WHEN building for Linux THEN the system SHALL use Release configuration for optimized binaries
3. WHEN building for Linux THEN the system SHALL create a self-contained, single-file executable including the .NET runtime
4. WHEN the Linux build completes THEN the executable SHALL be named `TeaLauncher-{version}-linux-x64` (no .exe extension)
5. WHEN the Linux build completes THEN the executable SHALL have execute permissions set (`chmod +x`)
6. WHEN the Linux build completes THEN the executable SHALL be uploaded as a release asset to the GitHub Release
7. IF the Linux build fails THEN the workflow SHALL fail and NOT create a release

### Requirement 4: Release Documentation

**User Story:** As a user downloading a release, I want to see release notes and installation instructions, so that I understand what's new and how to install TeaLauncher.

#### Acceptance Criteria

1. WHEN a GitHub Release is created THEN it SHALL include auto-generated release notes from commits since the previous tag
2. WHEN a GitHub Release is created THEN it SHALL include a download section listing all platform-specific binaries
3. WHEN a GitHub Release is created THEN it SHALL include installation instructions for each platform:
   - Windows: Download .exe, run directly
   - Linux: Download executable, `chmod +x`, run
4. IF no previous tag exists THEN the release notes SHALL include all commits from repository history
5. WHEN the release is a pre-release (tag contains `-alpha`, `-beta`, `-rc`) THEN the GitHub Release SHALL be marked as "Pre-release"

### Requirement 5: Build Quality Validation

**User Story:** As a project maintainer, I want releases to only be created from code that passes all quality gates, so that users don't download broken or non-compliant binaries.

#### Acceptance Criteria

1. WHEN the release workflow starts THEN it SHALL run all quality checks (tests, coverage, metrics, format) before building binaries
2. IF any quality check fails (tests, coverage < 60%, metrics violations, format violations) THEN the workflow SHALL fail and NOT create a release
3. WHEN quality checks pass THEN the workflow SHALL proceed to build platform-specific binaries
4. WHEN building binaries THEN all 305+ tests SHALL pass in the build environment
5. IF binary creation fails for any platform THEN the entire workflow SHALL fail and the release SHALL NOT be published

### Requirement 6: Workflow Performance

**User Story:** As a project maintainer, I want the release workflow to complete quickly, so that I can iterate on releases without long wait times.

#### Acceptance Criteria

1. WHEN the release workflow runs THEN it SHALL complete all steps (quality checks + builds + release creation) within 10 minutes
2. WHEN building for multiple platforms THEN builds SHALL run in parallel to minimize total execution time
3. WHEN the workflow completes successfully THEN the GitHub Release SHALL be published immediately without manual approval
4. IF the workflow takes longer than 15 minutes THEN it SHALL timeout and fail to prevent hanging builds

## Non-Functional Requirements

### Code Architecture and Modularity
- **Single Responsibility Principle**: GitHub Actions workflow file should have clear, focused jobs (quality-check, build-windows, build-linux, create-release)
- **Modular Design**: Reusable workflow steps for common operations (checkout, setup .NET, restore dependencies)
- **Dependency Management**: Workflow should cache NuGet packages to speed up builds
- **Clear Interfaces**: Job outputs should be well-defined for passing artifacts between jobs

### Performance
- **Workflow Execution Time**: Complete release process within 10 minutes (target: 5-7 minutes)
- **Binary Size**: Single-file executables should be <50MB for Windows, <60MB for Linux (includes .NET runtime)
- **Parallel Execution**: Windows and Linux builds should run concurrently
- **Cache Efficiency**: NuGet package cache should reduce dependency restore time to <30 seconds

### Security
- **Permissions**: GitHub Actions should use minimum required permissions (contents: write, actions: read)
- **Token Usage**: Use GitHub's built-in `GITHUB_TOKEN` for authentication (no custom PATs)
- **Build Isolation**: Each platform build runs in isolated environment (ubuntu-latest runner for all builds)
- **No Secrets in Logs**: Ensure no sensitive information is leaked in build logs

### Reliability
- **Idempotent Releases**: Re-pushing the same tag should update the existing release, not fail
- **Build Reproducibility**: Same tag should always produce identical binaries (deterministic builds)
- **Error Handling**: Clear error messages when builds fail (e.g., "Windows build failed: compilation error at line X")
- **Retry Logic**: Transient failures (network issues downloading dependencies) should trigger automatic retry

### Usability
- **Clear Release Names**: GitHub Release title should be "TeaLauncher v{version}" (e.g., "TeaLauncher v1.0.0")
- **Download Instructions**: Release description should include platform-specific download and run instructions
- **Binary Naming**: Asset names should clearly indicate platform and version (no ambiguity)
- **Status Visibility**: GitHub Actions workflow status should be visible in repository badges and commit status checks

### Maintainability
- **Workflow Documentation**: Comments in YAML explaining each step's purpose
- **Version Flexibility**: Workflow should support semantic versioning with optional pre-release suffixes
- **Easy Updates**: Changing .NET version or build configuration should require minimal YAML changes
- **Debugging Support**: Workflow should enable debug logging via GitHub Actions secrets (`ACTIONS_STEP_DEBUG`)
