# Tasks Document

## Migration Tasks

- [x] 1. Convert CommandLauncher.csproj to SDK-style format
  - File: CommandLauncher/CommandLauncher.csproj
  - Replace legacy .csproj with SDK-style format targeting net8.0-windows
  - Configure Windows Forms support, assembly metadata, and build properties
  - Purpose: Modernize project file for .NET 8 compatibility
  - _Leverage: CommandLauncher/CommandLauncher.csproj (current file for reference)_
  - _Requirements: 2.1, 2.2_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET Build Engineer with expertise in SDK-style projects and MSBuild | Task: Convert CommandLauncher/CommandLauncher.csproj from legacy format to SDK-style targeting net8.0-windows following requirements 2.1 and 2.2. Reference the current file to extract assembly name, namespace, and build settings. The new format should use Sdk="Microsoft.NET.Sdk", set UseWindowsForms=true, TargetFramework=net8.0-windows, and move assembly metadata (title, description, company, product, copyright, version) from AssemblyInfo.cs to .csproj properties. Set TreatWarningsAsErrors=true, Platforms to x64 and ARM64, and OutputType to WinExe. | Restrictions: Do not change assembly name or namespace, maintain exact version from AssemblyInfo.cs, preserve TreatWarningsAsErrors setting, do not add unnecessary NuGet packages yet | Success: Project file uses SDK-style format, targets net8.0-windows, Windows Forms enabled, all assembly metadata migrated from AssemblyInfo.cs, compiles without warnings when you run 'dotnet build -r win-x64', Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [x] 2. Convert TestCommandLauncher.csproj to SDK-style format
  - File: TestCommandLauncher/TestCommandLauncher.csproj
  - Replace legacy test project with SDK-style format targeting net8.0-windows
  - Add NUnit 4.x package references
  - Purpose: Modernize test project for .NET 8 compatibility
  - _Leverage: TestCommandLauncher/TestCommandLauncher.csproj (current file), CommandLauncher/CommandLauncher.csproj (from task 1)_
  - _Requirements: 2.2, 3.1_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET Test Engineer with expertise in NUnit and SDK-style test projects | Task: Convert TestCommandLauncher/TestCommandLauncher.csproj to SDK-style format targeting net8.0-windows following requirements 2.2 and 3.1. Reference the current file and the updated CommandLauncher.csproj from task 1. Add PackageReference for NUnit 4.2.2, NUnit3TestAdapter 4.6.0, and Microsoft.NET.Test.Sdk 17.11.1. Set OutputType to Library, reference CommandLauncher project using ProjectReference. | Restrictions: Must target net8.0-windows (not net8.0), do not change test framework from NUnit, maintain project reference to CommandLauncher, do not modify test code yet | Success: Test project uses SDK-style format, NUnit 4.x packages added via PackageReference, targets net8.0-windows, project reference to CommandLauncher works, compiles without errors when you run 'dotnet build', Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [x] 3. Update CommandLauncher.sln solution file format
  - File: CommandLauncher.sln
  - Verify solution file compatibility with .NET 8 SDK
  - Update format version if needed
  - Purpose: Ensure solution works with modern tooling
  - _Leverage: CommandLauncher.sln (current file)_
  - _Requirements: 2.2_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET Solution Architect with expertise in Visual Studio solution files | Task: Review and update CommandLauncher.sln following requirement 2.2 to ensure compatibility with .NET 8 SDK and Visual Studio 2022. Verify the solution file format is compatible (Visual Studio 2022 uses Format Version 12.00, Visual Studio Version 17). If the current format is older, update the header while preserving project references and configurations. | Restrictions: Do not change project GUIDs, maintain all existing project references, preserve solution configurations (Debug/Release) | Success: Solution opens in Visual Studio 2022 without warnings, 'dotnet sln list' shows both projects correctly, 'dotnet build' at solution level succeeds, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [x] 4. Update ConfigLoader.cs to use Dictionary instead of Hashtable
  - File: CommandLauncher/ConfigLoader.cs
  - Replace Hashtable with Dictionary<string, Dictionary<string, string>>
  - Remove casts and update method signatures
  - Purpose: Use type-safe modern collections
  - _Leverage: CommandLauncher/ConfigLoader.cs (current implementation)_
  - _Requirements: 4.1, 4.2_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# Developer with expertise in .NET collections and generics | Task: Update CommandLauncher/ConfigLoader.cs following requirements 4.1 and 4.2 to replace Hashtable with Dictionary<string, Dictionary<string, string>>. Change the private field 'm_Conf' from 'Hashtable' to 'Dictionary<string, Dictionary<string, string>> m_Conf = new();'. Update GetConfig method to return 'Dictionary<string, string>' instead of 'Hashtable' and remove the cast. In ParseConfig method where 'm_Conf[section] = new Hashtable()' is used, change it to 'm_Conf[section] = new Dictionary<string, string>()'. Update the variable 'ht' from 'Hashtable' to 'Dictionary<string, string>'. Preserve all existing logic, parsing algorithms, and exception handling. | Restrictions: Do not change parsing logic, maintain all existing exceptions (ConfigLoaderNotExistsSectionException, etc.), preserve method names and public API, do not modify file I/O code | Success: ConfigLoader uses Dictionary instead of Hashtable, all casts removed, method signatures updated, existing functionality preserved, compiles without warnings, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 5. Delete AssemblyInfo.cs files
  - Files: CommandLauncher/Properties/AssemblyInfo.cs, TestCommandLauncher/Properties/AssemblyInfo.cs
  - Remove AssemblyInfo.cs files (metadata now in .csproj)
  - Purpose: Eliminate redundant assembly metadata files
  - _Leverage: CommandLauncher.csproj and TestCommandLauncher.csproj (tasks 1-2)_
  - _Requirements: 2.2_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET Build Engineer with expertise in SDK-style project conventions | Task: Delete AssemblyInfo.cs files following requirement 2.2 after verifying that all assembly metadata has been migrated to .csproj files in tasks 1 and 2. Remove CommandLauncher/Properties/AssemblyInfo.cs and TestCommandLauncher/Properties/AssemblyInfo.cs. Verify that assembly title, description, company, product, copyright, and version are all defined in the respective .csproj files. | Restrictions: Only delete AssemblyInfo.cs files after confirming metadata is in .csproj, do not delete other files in Properties folder (Resources.Designer.cs, Settings.Designer.cs should remain), do not modify .csproj files in this task | Success: AssemblyInfo.cs files deleted, assembly metadata remains accessible via .csproj properties, build succeeds with 'dotnet build', assembly version shows correctly in output, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 6. Update NUnit test code for compatibility
  - Files: TestCommandLauncher/Test_AutoCompleteMachine.cs, TestCommandLauncher/Test_ConfigLoader.cs
  - Verify test code works with NUnit 4.x
  - Update any deprecated NUnit attributes if needed
  - Purpose: Ensure tests run on .NET 8 with NUnit 4
  - _Leverage: TestCommandLauncher/Test_AutoCompleteMachine.cs, TestCommandLauncher/Test_ConfigLoader.cs (current tests)_
  - _Requirements: 3.1, 8.1_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Engineer with expertise in NUnit and unit testing | Task: Review and update test files following requirements 3.1 and 8.1 for NUnit 4.x compatibility. Read TestCommandLauncher/Test_AutoCompleteMachine.cs and TestCommandLauncher/Test_ConfigLoader.cs to check if any NUnit attributes or assertions need updates. NUnit 4.x maintains backward compatibility with most NUnit 3.x code, so verify [TestFixture], [Test], Assert.AreEqual, Assert.IsTrue, etc. still work. Only make changes if there are compilation errors related to NUnit. If tests reference ConfigLoader, verify they work with Dictionary instead of Hashtable. | Restrictions: Preserve all test logic and assertions, do not change test coverage, maintain test names and structure, only update if NUnit 4.x requires it | Success: All tests compile without errors, test attributes recognized by NUnit 4.x, 'dotnet test' discovers and runs all tests, ConfigLoader tests work with Dictionary type, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 7. Verify P/Invoke compatibility with .NET 8
  - File: CommandLauncher/Hotkey.cs
  - Review P/Invoke signatures for .NET 8 interop compatibility
  - Test RegisterHotKey/UnregisterHotKey functionality
  - Purpose: Ensure Windows API interop works on .NET 8
  - _Leverage: CommandLauncher/Hotkey.cs (current P/Invoke code)_
  - _Requirements: 5.4_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Windows Interop Specialist with expertise in P/Invoke and .NET runtime | Task: Review CommandLauncher/Hotkey.cs following requirement 5.4 to verify P/Invoke declarations are compatible with .NET 8. Check DllImport attributes for user32.dll RegisterHotKey and UnregisterHotKey. Verify IntPtr, MOD_KEY enum, Keys enum, and WM_HOTKEY constant are correctly declared. .NET 8 maintains backward compatibility with .NET Framework P/Invoke, so changes should only be needed if there are compilation errors. The code should work as-is, but verify marshalling of types is correct. | Restrictions: Do not change P/Invoke signatures unless compilation fails, maintain exact Windows API calling convention, preserve MOD_KEY enum values, do not modify hotkey registration logic | Success: Hotkey.cs compiles without P/Invoke warnings, no marshalling errors, code structure unchanged unless .NET 8 requires it, ready for runtime testing on Windows, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 8. Build verification on Linux
  - Run full build on Linux development environment
  - Verify Windows executable is produced
  - Check build output and warnings
  - Purpose: Confirm Linux cross-compilation works
  - _Leverage: All updated project files_
  - _Requirements: 2.2, 2.5_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: DevOps Engineer with expertise in .NET SDK and cross-platform builds | Task: Execute build verification on Linux following requirements 2.2 and 2.5 to confirm cross-compilation to Windows works. Run 'dotnet build -r win-x64' from the solution directory. Verify that CommandLauncher.exe is produced in bin/Debug/net8.0-windows/win-x64/ directory. Check that there are zero compilation warnings (TreatWarningsAsErrors is enabled). Verify the executable is a Windows PE file (use 'file' command to check). | Restrictions: Must build on Linux (not Windows), use 'dotnet build -r win-x64' command, do not suppress warnings, do not modify code to fix build errors in this task (go back to previous tasks if needed) | Success: 'dotnet build -r win-x64' succeeds with zero warnings, CommandLauncher.exe created in correct output directory, file type is 'PE32+ executable (console) x86-64, for MS Windows', build time is reasonable (< 30 seconds clean build), Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 9. Run unit tests on Linux
  - Execute 'dotnet test' on Linux
  - Verify non-UI tests pass
  - Review test output for any failures
  - Purpose: Confirm logic tests work cross-platform
  - _Leverage: Updated test project_
  - _Requirements: 8.1, 8.2_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Automation Engineer with expertise in .NET testing and CI/CD | Task: Execute unit tests on Linux following requirements 8.1 and 8.2 to verify that logic-based tests run successfully. Run 'dotnet test' from the solution directory. Verify that Test_AutoCompleteMachine and Test_ConfigLoader tests are discovered and executed. Check that all tests pass. Note: UI tests and P/Invoke tests will not run on Linux (Windows Forms and user32.dll are Windows-specific), so expect those to be skipped or fail if they exist. Focus on AutoCompleteMachine and ConfigLoader tests passing. | Restrictions: Run on Linux only, use 'dotnet test' command, do not modify test code to make them pass, do not skip tests manually | Success: 'dotnet test' discovers all test classes, AutoCompleteMachine tests pass (prefix matching, word registration), ConfigLoader tests pass (configuration parsing with Dictionary), test summary shows pass rate, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 10. Update README with .NET 8 build instructions
  - File: README
  - Add .NET 8 SDK installation instructions
  - Document build commands for Linux and Windows
  - Add deployment instructions
  - Purpose: Help developers set up .NET 8 environment
  - _Leverage: README (current content)_
  - _Requirements: All_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical Writer with expertise in developer documentation | Task: Update README file covering all requirements to provide clear .NET 8 build instructions for both Linux and Windows developers. Add a section "= Building with .NET 8 =" that includes: (1) Installation instructions for .NET 8 SDK on Linux (wget script) and Windows (download link), (2) Build commands on Linux ('dotnet build -r win-x64') and Windows ('dotnet build'), (3) Test commands ('dotnet test'), (4) Publish commands for framework-dependent and self-contained deployments. Update the existing "== Usage ==" section to note that .NET 8 Desktop Runtime is required for framework-dependent builds. Keep the existing format style (= for headers, indentation with spaces). | Restrictions: Preserve existing README content and format, use same text style (simple ASCII), do not change usage instructions for end users, keep it concise (KISS principle) | Success: README includes .NET 8 build instructions, Linux and Windows commands documented, SDK installation steps clear, deployment options explained, existing content preserved, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 11. Create framework-dependent publish configuration
  - Test framework-dependent deployment build
  - Verify output size and runtime requirements
  - Document publish command
  - Purpose: Enable lightweight deployment option
  - _Leverage: CommandLauncher.csproj (from task 1)_
  - _Requirements: 7.1, 7.4_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release Engineer with expertise in .NET deployment and packaging | Task: Test framework-dependent deployment following requirements 7.1 and 7.4 to verify that the publish process produces a lightweight executable. Run 'dotnet publish -c Release -r win-x64 CommandLauncher/CommandLauncher.csproj' and verify the output in CommandLauncher/bin/Release/net8.0-windows/win-x64/publish/. Check that CommandLauncher.exe is created, the total output size is small (< 5MB for framework-dependent), and the publish folder contains minimal dependencies. The executable should require .NET 8 Desktop Runtime to run on Windows. Document this command in a comment or note. | Restrictions: Use Release configuration only, target win-x64 runtime, do not use --self-contained flag (framework-dependent is the default), do not modify .csproj file | Success: 'dotnet publish' succeeds for framework-dependent deployment, executable size is reasonable (< 5MB), output directory structure is clean, CommandLauncher.exe present in publish folder, command documented, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 12. Create self-contained publish configuration
  - Test self-contained single-file deployment
  - Verify output size and runtime bundling
  - Document publish command
  - Purpose: Enable zero-dependency deployment option
  - _Leverage: CommandLauncher.csproj (from task 1)_
  - _Requirements: 7.2, 7.3_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release Engineer with expertise in .NET deployment and self-contained applications | Task: Test self-contained deployment following requirements 7.2 and 7.3 to verify single-file executable with embedded runtime. Run 'dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true CommandLauncher/CommandLauncher.csproj' and verify output in publish folder. Check that a single CommandLauncher.exe is produced (approximately 60-80MB including .NET 8 runtime). The executable should run on Windows without requiring .NET installation. Test that the file count in publish directory is minimal (ideally just the .exe). Document this command. | Restrictions: Must use --self-contained flag, use PublishSingleFile=true, target win-x64, do not use PublishTrimmed (can break Windows Forms), Release configuration only | Success: Single-file .exe created, file size is 60-80MB (includes runtime), no separate runtime files required, command documented, executable is standalone, Edit tasks.md to mark this task [-] when starting and [x] when complete_

## Testing and Verification Tasks

- [ ] 13. Manual testing on Windows - Application startup
  - Test: Transfer published executable to Windows machine
  - Test: Run CommandLauncher.exe with conf/my.conf
  - Verify: Application starts in < 300ms
  - Verify: System tray icon appears
  - Purpose: Confirm basic startup functionality on .NET 8
  - _Leverage: Published executables from tasks 11-12_
  - _Requirements: 5.1, 8.1_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Tester with expertise in Windows desktop applications | Task: Perform manual startup testing on Windows following requirements 5.1 and 8.1 to verify basic application functionality. Copy CommandLauncher.exe from Linux build to a Windows 10 or Windows 11 machine. Also copy the resource/conf/my.conf file. Run 'CommandLauncher.exe resource\conf\my.conf' from command prompt. Verify that the application starts without errors, a system tray icon appears (TeaLauncher icon in notification area), and startup is fast (feels instant, ideally < 300ms). Check for any error dialogs or console errors. | Restrictions: Test on Windows 10 version 1607+ or Windows 11, use .NET 8 Desktop Runtime (install if needed for framework-dependent build), test both framework-dependent and self-contained builds | Success: Application starts without errors on Windows, system tray icon visible, startup time feels fast (< 300ms subjectively), no error messages or crashes, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 14. Manual testing on Windows - Hotkey and UI interaction
  - Test: Press Ctrl+Space to show input window
  - Test: Type command and verify auto-completion (Tab key)
  - Test: Press Escape to hide window
  - Purpose: Verify Windows Forms UI and P/Invoke hotkey work on .NET 8
  - _Leverage: Running application from task 13_
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Tester with expertise in UI testing and keyboard interactions | Task: Perform manual UI and hotkey testing on Windows following requirements 5.1 through 5.5 to verify Windows Forms and P/Invoke functionality. With TeaLauncher running, press Ctrl+Space from any application (e.g., Notepad, browser). Verify the input window appears quickly (< 100ms). Type a partial command name and press Tab to test auto-completion. Verify that matching commands appear. Press Escape to hide the window. Verify window disappears immediately. Test that you can show the window again with Ctrl+Space. Check that focus returns to the textbox when window appears. | Restrictions: Test on real Windows hardware (not Wine or VM if possible for accurate hotkey testing), verify both show and hide behavior, test auto-completion with commands from my.conf | Success: Ctrl+Space shows input window in < 100ms, auto-completion works with Tab key, Escape hides window immediately, window can be shown/hidden repeatedly, textbox receives focus correctly, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 15. Manual testing on Windows - Command execution
  - Test: Execute URL command (http://, https://, ftp://)
  - Test: Execute file path command
  - Test: Execute special commands (!reload, !exit, !version)
  - Purpose: Verify Process.Start and command execution on .NET 8
  - _Leverage: Running application from task 13, resource/conf/my.conf_
  - _Requirements: 5.6, 6.1, 6.2, 6.3_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Tester with expertise in application functionality testing | Task: Perform command execution testing on Windows following requirements 5.6 and 6.1-6.3 to verify Process.Start and configuration handling work correctly. Test URL execution: Enter a URL command (e.g., if my.conf has [github] linkto=http://github.com, type 'github' and press Enter). Verify the default browser opens the URL. Test file execution: If my.conf has a file path command, execute it and verify the file/application opens. Test special commands: Type '!version' and press Enter, verify version dialog appears. Type '!reload' to test configuration reload. Type '!exit' to quit the application gracefully. | Restrictions: Use commands defined in resource/conf/my.conf, test on Windows with default browser configured, verify each command type works | Success: URL commands open in browser correctly, file path commands execute applications, !version shows version info dialog, !reload refreshes configuration without errors, !exit closes application cleanly, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 16. Manual testing on Windows - Configuration file parsing
  - Test: Modify resource/conf/my.conf to add new command
  - Test: Execute !reload command
  - Verify: New command is available via auto-completion
  - Purpose: Confirm ConfigLoader works with Dictionary on .NET 8
  - _Leverage: Running application from task 13, updated ConfigLoader from task 4_
  - _Requirements: 6.1, 6.2, 8.3_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Tester with expertise in configuration and data handling | Task: Perform configuration testing on Windows following requirements 6.1, 6.2, and 8.3 to verify ConfigLoader with Dictionary works correctly. While TeaLauncher is running, open resource/conf/my.conf in a text editor. Add a new command section like '[test_command]\nlinkto = http://example.com'. Save the file. In TeaLauncher, type '!reload' and press Enter. Verify no errors occur. Type 'test' and press Tab to trigger auto-completion. Verify 'test_command' appears as an option. Execute the command to verify it works. This confirms that Dictionary-based ConfigLoader parses and loads configuration correctly. | Restrictions: Use the [section] and key=value format from existing my.conf, do not introduce syntax errors intentionally, test reload functionality specifically | Success: Configuration file modifications are detected via !reload, new commands are loaded and available, auto-completion includes newly added commands, Dictionary-based ConfigLoader works identically to Hashtable version, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 17. Performance and memory verification
  - Measure: Application startup time (target: ≤ 300ms)
  - Measure: Hotkey response time (target: < 100ms)
  - Measure: Memory footprint after 60s idle (target: ≤ 20MB)
  - Purpose: Verify .NET 8 meets or exceeds performance targets
  - _Leverage: Running application from task 13_
  - _Requirements: All performance requirements_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Performance Engineer with expertise in application profiling and benchmarking | Task: Perform performance measurements on Windows to verify that all performance requirements are met or exceeded. Measure startup time: Use a stopwatch or Measure-Command in PowerShell to time from 'dotnet run' (or .exe launch) to when the system tray icon appears. Target is ≤ 300ms. Measure hotkey response: Use a stopwatch to measure from Ctrl+Space keypress to when the input window becomes visible. Target is < 100ms. Measure memory: Open Task Manager, wait 60 seconds after launch with no activity, check Private Working Set memory for CommandLauncher.exe process. Target is ≤ 20MB (ideally ≤ 15MB). Record all measurements. | Restrictions: Measure on real Windows hardware, close other applications for accurate measurements, average 3 runs for each measurement, use Release build for accurate performance | Success: Startup time ≤ 300ms (ideally faster), hotkey response < 100ms, memory ≤ 20MB after idle, performance meets or exceeds .NET Framework 3.5 baseline, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 18. Final regression testing
  - Execute complete regression test checklist
  - Verify all functionality from requirements
  - Document any issues found
  - Purpose: Ensure zero regressions from .NET Framework version
  - _Leverage: All previous tests, requirements.md checklist_
  - _Requirements: All_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA Lead with expertise in regression testing and quality assurance | Task: Execute comprehensive regression testing covering all requirements to ensure the .NET 8 migration has zero functional regressions. Go through the regression testing checklist from design.md: Application starts without errors, Hotkey triggers input window, Auto-completion functions correctly, URL commands open browser, File path commands execute, Special commands work (!reload, !exit, !version), Configuration file parsing unchanged, System tray integration works, Window hide/show behavior correct, Memory usage < 20MB idle, No crashes during normal operation, All unit tests pass. Document results in a comment or test log. If any issues found, note them but do not mark task complete until resolved. | Restrictions: Test on Windows 10 or 11, use both framework-dependent and self-contained builds, compare behavior to documented .NET Framework behavior if possible | Success: All regression checklist items pass, no functional regressions found, behavior matches requirements, application is stable and reliable, Edit tasks.md to mark this task [-] when starting and [x] when complete_

## Documentation Tasks

- [ ] 19. Create build and deployment documentation
  - Document: Linux development setup and workflow
  - Document: Windows testing workflow
  - Document: Deployment options comparison
  - Purpose: Help developers and users understand .NET 8 requirements
  - _Leverage: README updates from task 10, build commands from tasks 11-12_
  - _Requirements: All_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical Documentation Specialist with expertise in developer guides | Task: Create comprehensive build and deployment documentation covering all requirements for .NET 8 migration. Expand the README updates from task 10 into detailed documentation. Include: (1) Linux development setup (installing .NET 8 SDK, git clone, dotnet build -r win-x64), (2) Windows testing workflow (transferring .exe, installing .NET 8 Desktop Runtime, running tests), (3) Deployment options comparison table (framework-dependent vs self-contained: size, runtime requirement, use cases). Keep documentation concise following KISS principle but thorough enough for new developers. Use simple text format matching README style. | Restrictions: Use existing README format, keep language simple and direct, do not add unnecessary complexity, focus on practical instructions | Success: Documentation is clear and complete, covers Linux development and Windows testing, deployment options well-explained, new developers can follow instructions successfully, Edit tasks.md to mark this task [-] when starting and [x] when complete_

- [ ] 20. Migration completion and cleanup
  - Verify: All migration tasks completed successfully
  - Verify: No legacy .NET Framework references remain
  - Verify: Build and test succeed on both Linux and Windows
  - Document: Migration completion and next steps
  - Purpose: Confirm migration is complete and production-ready
  - _Leverage: All completed tasks, requirements.md, design.md_
  - _Requirements: All_
  - _Prompt: Implement the task for spec migrate-to-dotnet8, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Senior Software Engineer with expertise in project delivery and quality assurance | Task: Perform final migration verification covering all requirements to ensure complete and successful .NET 8 migration. Review all 19 previous tasks and verify each is marked [x] completed. Run final verification: 'dotnet build' succeeds on Linux and Windows, 'dotnet test' passes all tests, published executables work on Windows, no .NET Framework references in .csproj files, AssemblyInfo.cs files deleted, all code uses modern collections (Dictionary not Hashtable). Check that TeaLauncher runs correctly on Windows 10 and 11. Verify README and documentation are updated. Create a summary comment documenting migration completion, any issues encountered and resolved, and recommendations for future work (cross-platform support spec). | Restrictions: Do not mark complete unless all previous tasks are verified working, ensure no regressions exist, verify both deployment options work | Success: All 19 tasks completed and verified, migration is production-ready, no .NET Framework dependencies remain, application works on .NET 8, documentation complete, Edit tasks.md to mark this task [-] when starting and [x] when complete_
