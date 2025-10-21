# Initialization Mode Testing Guide

This document describes how to test the new initialization mode feature.

## Feature Overview

The initialization mode provides a first-time setup experience that:
1. Detects if a configuration file exists in the current directory
2. Shows a welcome window with setup instructions
3. Generates a sample `commands.yaml` configuration file
4. Allows users to select their preferred keyboard shortcut (Ctrl+Space or Alt+Space)
5. Saves the shortcut preference to `.tealauncher-settings.json`

## Test Cases

### Test Case 1: First Run (No Config File)

**Steps:**
1. Delete `commands.yaml` and `.tealauncher-settings.json` if they exist
2. Run `TeaLauncher-2.0.0-win-x64.exe`
3. Verify that the initialization window appears
4. Read the welcome message and verify all information is clear
5. Note the config file path displayed
6. Select either "Ctrl + Space" or "Alt + Space" hotkey
7. Click "Start TeaLauncher"
8. Verify that:
   - The initialization window closes
   - The `commands.yaml` file is created at the displayed path
   - The `.tealauncher-settings.json` file is created
   - TeaLauncher starts in the background (no window visible)

**Expected Results:**
- Initialization window appears with clear instructions
- Config file is created successfully
- Settings file is created with selected hotkey
- Application starts hidden

### Test Case 2: Subsequent Runs (Config File Exists)

**Steps:**
1. Ensure `commands.yaml` exists in the current directory
2. Run `TeaLauncher-2.0.0-win-x64.exe`
3. Verify that the initialization window does NOT appear
4. Verify that TeaLauncher starts in the background

**Expected Results:**
- No initialization window
- Application starts normally in background mode

### Test Case 3: Hotkey Registration

**Steps:**
1. Complete Test Case 1 with "Ctrl + Space" selected
2. Press `Ctrl + Space`
3. Verify that the TeaLauncher main window appears
4. Press `Ctrl + Space` again
5. Verify that the window hides
6. Delete `commands.yaml` and `.tealauncher-settings.json`
7. Run the application again and select "Alt + Space"
8. Press `Alt + Space`
9. Verify that the TeaLauncher main window appears

**Expected Results:**
- Selected hotkey activates/hides the launcher window
- Hotkey preference is persisted and used on subsequent runs

### Test Case 4: Config File Generation

**Steps:**
1. Delete `commands.yaml` if it exists
2. Run the initialization flow
3. Open the generated `commands.yaml` file
4. Verify that it contains sample commands for:
   - Web URLs (google, github)
   - Applications (notepad, cmd, explorer)
   - Config editing (edit_config)
   - Special commands (reload, version, exit)

**Expected Results:**
- Config file contains helpful sample commands
- File is properly formatted YAML
- Comments explain the syntax

### Test Case 5: Settings File

**Steps:**
1. Complete initialization with "Ctrl + Space"
2. Open `.tealauncher-settings.json`
3. Verify that `HotkeyModifier` is set to `"Control"`
4. Delete the files and re-run with "Alt + Space"
5. Verify that `HotkeyModifier` is set to `"Alt"`

**Expected Results:**
- Settings file is valid JSON
- Hotkey preference is correctly stored

### Test Case 6: Initialization Cancellation

**Steps:**
1. Delete `commands.yaml`
2. Run the application
3. When the initialization window appears, close it using the X button
4. Verify that the application exits

**Expected Results:**
- Closing the initialization window without clicking "Start" exits the app
- No files are created

## Manual Testing Commands

```bash
# Clean up existing files
rm -f commands.yaml .tealauncher-settings.json

# Build and publish the application (on Linux)
dotnet publish TeaLauncher.Avalonia/TeaLauncher.Avalonia.csproj \
    -c Release \
    -r win-x64 \
    --self-contained \
    -p:PublishSingleFile=true \
    -o ./publish

# Copy to Windows machine and run
# (copy ./publish/TeaLauncher.Avalonia.exe to Windows)
```

## Files Created by Initialization

- `commands.yaml` - Main configuration file with sample commands
- `.tealauncher-settings.json` - Settings file with hotkey preference

Example `.tealauncher-settings.json`:
```json
{
  "HotkeyModifier": "Control"
}
```

## Troubleshooting

### Issue: Nothing happens when I run the exe

**Cause:** The application is designed to run in the background. It only shows when you press the hotkey.

**Solution:**
1. Check if the initialization window appeared (first run only)
2. Press the configured hotkey (Ctrl+Space or Alt+Space)
3. Look in Task Manager to verify the process is running

### Issue: Initialization window doesn't appear

**Possible Causes:**
1. A `commands.yaml` file already exists in the current directory
2. The application is using a different config file path

**Solution:**
1. Verify that `commands.yaml` doesn't exist in the current directory
2. Try deleting `commands.yaml` and running again
3. Check if a custom config file path was passed as a command-line argument

### Issue: Hotkey doesn't work

**Possible Causes:**
1. Another application is using the same hotkey
2. Settings file was corrupted or deleted

**Solution:**
1. Try the alternative hotkey (Alt+Space or Ctrl+Space)
2. Delete `.tealauncher-settings.json` and run initialization again
3. Check Task Manager to ensure TeaLauncher is running

## Notes for Developers

- Initialization service: `TeaLauncher.Avalonia/Infrastructure/Initialization/InitializationService.cs`
- Settings service: `TeaLauncher.Avalonia/Infrastructure/Settings/SettingsService.cs`
- Initialization window: `TeaLauncher.Avalonia/Views/InitializationWindow.axaml`
- Application startup: `TeaLauncher.Avalonia/App.axaml.cs`
