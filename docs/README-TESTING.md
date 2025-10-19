# TeaLauncher .NET 8 Migration - Testing Documentation

This directory contains testing documentation and scripts for the .NET 8 migration.

## Manual Testing Tasks

### Task 13: Application Startup Testing
- **Test Plan**: [manual-test-task13.md](manual-test-task13.md)
- **Automated Script**: [test-startup.ps1](test-startup.ps1)
- **Status**: Ready for execution on Windows
- **Requirements**: Windows 10/11, .NET 8 Desktop Runtime (for framework-dependent build)

## How to Execute Tests

### On Windows

1. **Transfer files from Linux build**:
   - Copy `CommandLauncher/bin/Release/net8.0-windows/win-x64/publish/CommandLauncher.exe` (self-contained)
   - OR copy `CommandLauncher/bin/Release/net8.0-windows/win-x64/CommandLauncher.exe` + DLLs (framework-dependent)
   - Copy `resource/conf/my.conf` maintaining directory structure

2. **Run automated test**:
   ```powershell
   .\test-startup.ps1
   ```

3. **Manual verification**:
   - Follow checklist in `manual-test-task13.md`
   - Document results in the test plan

## Test Files Location

All test documentation is in the `docs/` directory:
- Test plans: `manual-test-*.md`
- Test scripts: `test-*.ps1`
- Test results: Document in respective test plan files

## Notes

- Manual tests require a real Windows machine (not Wine/VM recommended)
- Self-contained builds work without .NET 8 installation
- Framework-dependent builds require .NET 8 Desktop Runtime
- All tests should be executed on both build types when possible
