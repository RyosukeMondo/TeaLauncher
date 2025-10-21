# Tasks Document

## Phase 1: Workflow File Creation

- [x] 1. Create GitHub Actions release workflow file
  - File: .github/workflows/release.yml
  - Create workflow with trigger configuration (on.push.tags: v*.*.*)
  - Define workflow name and basic structure
  - Add workflow-level permissions (contents: write)
  - _Leverage: Existing ci.yml structure for reference_
  - _Requirements: 1_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: DevOps Engineer with expertise in GitHub Actions and CI/CD workflows | Task: Create `.github/workflows/release.yml` file following requirement 1 with trigger configuration for version tags (pattern: v*.*.*), workflow name "Release Build", and permissions set to `contents: write` for GitHub Release creation. Use existing `.github/workflows/ci.yml` as structural reference. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Do not add job definitions yet (skeleton only), ensure YAML syntax is valid, follow existing workflow naming conventions (lowercase, hyphenated), do not modify existing workflows (ci.yml, pr-validation.yml) | _Leverage: .github/workflows/ci.yml for workflow structure reference, GitHub Actions documentation for trigger syntax | _Requirements: Requirement 1 (Version Tag Triggered Releases) | Success: File created at `.github/workflows/release.yml`, triggers only on tags matching v*.*.*, YAML validates with `yamllint` or GitHub's validator, workflow appears in Actions tab_

- [x] 2. Implement quality-checks job in release workflow
  - File: .github/workflows/release.yml
  - Copy quality validation steps from ci.yml:
    - Checkout, setup .NET, cache NuGet
    - Restore dependencies, build, test
    - Coverage collection and validation (≥60%)
    - Metrics validation (≤50 lines/method)
    - Format validation (dotnet format --verify-no-changes)
  - Adapt steps for ubuntu-latest runner only (remove matrix)
  - _Leverage: ci.yml quality check steps (lines 18-84)_
  - _Requirements: 5_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: CI/CD Engineer with expertise in .NET build pipelines and quality gates | Task: Add quality-checks job to release.yml following requirement 5 by copying and adapting quality validation steps from ci.yml (checkout, .NET setup, restore, build, test, coverage, metrics, format). Run on ubuntu-latest only (no matrix), ensure all quality gates pass before allowing subsequent jobs. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Must include all quality checks from ci.yml (tests, coverage ≥60%, metrics, format), do not skip any validation steps, ensure job fails if any check fails, use same tool versions as ci.yml | _Leverage: .github/workflows/ci.yml lines 18-84 for quality check steps, scripts/check-coverage.sh for coverage validation, tools/MetricsChecker for metrics validation | _Requirements: Requirement 5 (Build Quality Validation) | Success: quality-checks job defined, includes all 10 validation steps (checkout, setup, cache, restore, build, test, coverage, coverage-check, metrics, format), job fails on any quality gate failure, runs on ubuntu-latest_

## Phase 2: Platform Build Jobs

- [x] 3. Implement Windows build job
  - File: .github/workflows/release.yml
  - Add build-windows job with dependency on quality-checks
  - Extract version from tag name using GitHub Actions expressions
  - Run dotnet publish for win-x64 runtime
  - Rename output executable to TeaLauncher-{version}-win-x64.exe
  - Upload artifact for release job
  - _Leverage: Project PublishSingleFile configuration in .csproj_
  - _Requirements: 2_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET Build Engineer with expertise in cross-platform compilation and GitHub Actions | Task: Add build-windows job to release.yml following requirement 2 that depends on quality-checks job passing. Extract version from `github.ref_name` (v1.0.0 → 1.0.0), publish self-contained single-file executable for win-x64 using `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`, rename output to `TeaLauncher-{version}-win-x64.exe`, and upload as workflow artifact. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Must use Release configuration, must be self-contained and single-file, must include version in assembly metadata (-p:AssemblyVersion), ensure executable is in publish output directory, set artifact retention to 1 day (ephemeral) | _Leverage: TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj PublishSingleFile and SelfContained settings, GitHub Actions expressions for version extraction (${{ github.ref_name }}), actions/upload-artifact@v4 for artifact upload | _Requirements: Requirement 2 (Windows Build Artifacts) | Success: build-windows job defined, depends on quality-checks, publishes ~40-50MB .exe named TeaLauncher-{version}-win-x64.exe, artifact uploaded successfully, job completes within 5 minutes_

- [x] 4. Implement Linux build job
  - File: .github/workflows/release.yml
  - Add build-linux job with dependency on quality-checks (parallel to build-windows)
  - Extract version from tag name
  - Run dotnet publish for linux-x64 runtime
  - Rename output executable to TeaLauncher-{version}-linux-x64
  - Set execute permissions (chmod +x)
  - Upload artifact for release job
  - _Leverage: Project PublishSingleFile configuration in .csproj_
  - _Requirements: 3_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET Build Engineer with expertise in Linux builds and GitHub Actions | Task: Add build-linux job to release.yml following requirement 3 that depends on quality-checks and runs in parallel with build-windows. Extract version from tag, publish self-contained single-file executable for linux-x64, rename to `TeaLauncher-{version}-linux-x64` (no .exe extension), set execute permissions with `chmod +x`, and upload artifact. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Must use Release configuration, must be self-contained and single-file, executable must have +x permissions (chmod 755 or chmod +x), ensure no .exe extension, artifact retention 1 day | _Leverage: Same .csproj settings as Windows build, GitHub Actions bash shell for chmod command, actions/upload-artifact@v4 for artifact upload | _Requirements: Requirement 3 (Linux Build Artifacts) | Success: build-linux job defined, depends on quality-checks only (parallel to Windows), publishes ~50-60MB executable with execute permissions, artifact named TeaLauncher-{version}-linux-x64, job completes within 5 minutes_

## Phase 3: Release Creation

- [ ] 5. Implement create-release job
  - File: .github/workflows/release.yml
  - Add create-release job depending on both build-windows and build-linux
  - Download both platform artifacts
  - Generate release notes from commit history since previous tag
  - Create GitHub Release using softprops/action-gh-release@v1:
    - Release title: "TeaLauncher v{version}"
    - Body: Downloads section + installation instructions + changelog
    - Assets: Both Windows and Linux executables
    - Prerelease detection: Check tag for -alpha/-beta/-rc
  - _Leverage: softprops/action-gh-release GitHub Action_
  - _Requirements: 1, 4_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release Engineer with expertise in GitHub Releases and automated changelog generation | Task: Add create-release job to release.yml following requirements 1 and 4 that depends on both build-windows and build-linux jobs completing. Download artifacts using actions/download-artifact@v4, create GitHub Release using softprops/action-gh-release@v1 with title "TeaLauncher v{version}", body containing downloads section + installation instructions + auto-generated changelog, upload both platform executables as release assets, and mark as pre-release if tag contains -alpha/-beta/-rc. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Must wait for both build jobs to complete (needs: [build-windows, build-linux]), use GITHUB_TOKEN for authentication (built-in secret), include installation instructions for both platforms in release body, ensure idempotent (safe to re-run on same tag) | _Leverage: softprops/action-gh-release@v1 action for release creation, actions/download-artifact@v4 for artifact retrieval, GitHub's auto-generated release notes feature or manual changelog from git log | _Requirements: Requirement 1 (Version Tag Triggered Releases), Requirement 4 (Release Documentation) | Success: create-release job defined, depends on both build jobs, downloads both artifacts, creates GitHub Release with descriptive body, uploads 2 assets (Windows .exe + Linux executable), pre-release flag set correctly based on tag_

## Phase 4: Testing and Validation

- [ ] 6. Test workflow with test tag
  - Create and push test tag: v0.0.1-test
  - Monitor workflow execution in GitHub Actions
  - Verify all 4 jobs execute successfully
  - Download artifacts from GitHub Release
  - Test executables locally on Windows and Linux
  - Delete test tag and release after validation
  - _Leverage: GitHub Actions UI, local test environments_
  - _Requirements: All_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Engineer with expertise in CI/CD validation and end-to-end testing | Task: Validate the complete release workflow following all requirements by creating test tag `v0.0.1-test`, pushing to GitHub, monitoring workflow execution, verifying all jobs pass, downloading release assets, testing executables on Windows and Linux platforms, and cleaning up test artifacts. Document any issues found. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Use test tag with -test suffix (not production version), test on real Windows and Linux systems if available (or use VMs), verify file sizes are reasonable (~40-50MB Windows, ~50-60MB Linux), confirm executables launch successfully, delete test release after validation to avoid confusion | _Leverage: GitHub Actions web UI for workflow monitoring, GitHub Releases page for asset download, Windows and Linux test environments for execution validation | _Requirements: All requirements (end-to-end validation) | Success: Test tag triggers workflow successfully, all 4 jobs pass (quality-checks, build-windows, build-linux, create-release), GitHub Release created with 2 assets, Windows .exe runs on Windows 10/11, Linux executable runs on Ubuntu 22.04+, test artifacts cleaned up, workflow completes within 10 minutes_

- [ ] 7. Add workflow timeout and error handling
  - File: .github/workflows/release.yml
  - Add timeout-minutes: 15 to each job
  - Add continue-on-error: false to critical steps
  - Ensure fail-fast behavior (default)
  - Add workflow summary output for failed jobs
  - _Leverage: GitHub Actions timeout and error handling features_
  - _Requirements: 6_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: DevOps Engineer with expertise in workflow reliability and error handling | Task: Enhance release.yml following requirement 6 by adding `timeout-minutes: 15` to all jobs to prevent hanging builds, ensure critical steps have `continue-on-error: false` (default), verify fail-fast behavior prevents wasted execution, and add workflow summary outputs for debugging. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Timeout should be reasonable (15 minutes allows for occasional slowness but prevents infinite hangs), do not set continue-on-error: true unless explicitly needed for non-critical steps, ensure quality checks fail immediately on error | _Leverage: GitHub Actions timeout-minutes property, continue-on-error property, workflow run summaries for error reporting | _Requirements: Requirement 6 (Workflow Performance) | Success: All jobs have timeout-minutes: 15 configured, workflow fails fast on any job failure, error messages are clear and actionable, no hanging builds observed in testing_

## Phase 5: Documentation

- [ ] 8. Update README.md with release information
  - File: README.md
  - Add "Installation" section with download links to latest release
  - Add platform-specific installation instructions
  - Add GitHub Actions release workflow badge
  - Document manual release process (create tag, push tag)
  - _Leverage: Existing README.md structure_
  - _Requirements: 4_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical Writer with expertise in developer documentation and user guides | Task: Update README.md following requirement 4 by adding "Installation" section with links to GitHub Releases page, platform-specific download and run instructions for Windows and Linux, GitHub Actions workflow status badge for release.yml, and brief documentation of the release process for maintainers. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Keep documentation concise and user-friendly, use clear step-by-step instructions for each platform, include badge from GitHub Actions (shields.io format), do not duplicate information already in release notes | _Leverage: Existing README.md structure and formatting, GitHub Actions badge syntax (https://github.com/{owner}/{repo}/actions/workflows/release.yml/badge.svg) | _Requirements: Requirement 4 (Release Documentation) | Success: README.md updated with Installation section, includes download links to latest release, platform-specific instructions are clear (Windows: download .exe and run; Linux: download, chmod +x, run), workflow badge added and displays correctly_

- [ ] 9. Document release workflow in TESTING.md or CONTRIBUTING.md
  - File: TESTING.md (or create CONTRIBUTING.md if doesn't exist)
  - Document release workflow validation steps
  - Add troubleshooting guide for common workflow failures
  - Document tag naming convention (v{major}.{minor}.{patch}[-{prerelease}])
  - Add section on testing releases before production
  - _Leverage: Existing TESTING.md structure_
  - _Requirements: 4_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical Writer with expertise in developer onboarding and troubleshooting documentation | Task: Document the release workflow in TESTING.md (or CONTRIBUTING.md) following requirement 4 by adding sections on release workflow validation, troubleshooting common failures (quality checks fail, build errors, release creation errors), tag naming conventions, and testing process for releases. Include examples and clear steps. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Documentation should be actionable and example-driven, include real error messages and solutions, follow existing documentation style in TESTING.md, provide clear tag naming examples (v1.0.0, v2.1.3-beta, v0.5.0-rc1) | _Leverage: TESTING.md existing structure if available, GitHub Actions workflow logs for real error examples, troubleshooting guide from design document | _Requirements: Requirement 4 (Release Documentation) | Success: TESTING.md or CONTRIBUTING.md updated with "Release Process" section, includes tag naming convention documentation, troubleshooting guide covers common failures (quality gates, build errors, release creation), testing workflow documented with test tag examples_

## Phase 6: Production Release

- [ ] 10. Create first production release (v2.0.0)
  - Create git tag: v2.0.0 (matches AssemblyVersion in .csproj)
  - Push tag to GitHub
  - Monitor workflow execution
  - Verify GitHub Release created successfully
  - Download and test both platform binaries
  - Announce release to users (if applicable)
  - _Leverage: Completed release workflow, GitHub Releases_
  - _Requirements: All_
  - _Prompt: Implement the task for spec automated-releases, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release Manager with expertise in production deployments and release management | Task: Create the first production release v2.0.0 following all requirements by creating and pushing git tag, monitoring workflow execution, verifying GitHub Release is published correctly, testing downloaded binaries on Windows and Linux, and optionally announcing release to users. Document the release in project changelog or release notes. Edit tasks.md to mark this task as in-progress [-] when you start, then as completed [x] when finished. | Restrictions: Use production-ready version number (v2.0.0 matches AssemblyVersion in TeaLauncher.Avalonia.csproj), ensure tag is pushed to main repository (not fork), verify all quality gates pass before release is created, test downloaded binaries thoroughly before announcing, do not delete or modify release after publication | _Leverage: Completed release.yml workflow, GitHub CLI for tag creation (gh release create) or git commands, Windows and Linux test environments for validation | _Requirements: All requirements (production validation) | Success: Tag v2.0.0 created and pushed, workflow completes successfully within 10 minutes, GitHub Release published with title "TeaLauncher v2.0.0", 2 assets uploaded (Windows .exe ~40-50MB, Linux executable ~50-60MB), binaries tested and work correctly on respective platforms, release is publicly visible and downloadable_

## Summary

**Total Tasks**: 10
**Estimated Effort**:
- Phase 1 (Workflow Creation): 2 hours
- Phase 2 (Platform Builds): 2 hours
- Phase 3 (Release Creation): 1.5 hours
- Phase 4 (Testing): 2 hours
- Phase 5 (Documentation): 1.5 hours
- Phase 6 (Production Release): 1 hour

**Dependencies**:
- Task 2 depends on Task 1 (workflow file must exist)
- Tasks 3-4 depend on Task 2 (quality-checks job must exist)
- Task 5 depends on Tasks 3-4 (both build jobs must exist)
- Task 6 depends on Tasks 1-5 (complete workflow for testing)
- Task 7 depends on Tasks 1-5 (workflow exists for enhancement)
- Tasks 8-9 can be done in parallel (independent documentation)
- Task 10 depends on all previous tasks (production release requires complete, tested workflow)

**Success Criteria**:
- All 10 tasks marked as completed [x]
- release.yml workflow created with 4 jobs (quality-checks, build-windows, build-linux, create-release)
- Workflow triggers on version tags (v*.*.*)
- All quality gates enforced (tests, coverage ≥60%, metrics, format)
- Windows and Linux binaries built successfully (~40-50MB and ~50-60MB respectively)
- GitHub Release created automatically with both platform assets
- Documentation updated (README.md, TESTING.md/CONTRIBUTING.md)
- Production release v2.0.0 published and validated
- Workflow completes within 10 minutes target (5-7 minutes typical)
