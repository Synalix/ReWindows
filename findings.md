# Findings — Commit vs Current (0.1.3)

## Bugs Fixed

### 1. Hardcoded Wrong Version (HIGH) — `Views/MainWindow.axaml:233`
UI displayed `v0.1.0` instead of `v0.1.2` (now `0.1.3`).
**Fix:** Updated to `v0.1.3`.

### 2. Classic Context Menu Check Used null Wrong (MEDIUM) — `ViewModels/MainWindowViewModel.cs:410`
`GetString(..., "", null)` was invalid C# — `null` can't be a default for a non-nullable parameter. Result was always the default `""`, so the check was broken.
**Fix:** Replaced with `GetStringOrNull` + `IsNullOrEmpty` check.

### 3. Hibernation FlyoutMenuSettings Path Doesn't Exist (MEDIUM) — `ViewModels/MainWindowViewModel.cs:164-176`
`HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings` is only present on Windows 11 24H2+. On older builds the key doesn't exist — writes silently fail, `GetDword` returns default, `CheckAction` reports wrong state.
**Fix:** Wrapped FlyoutMenuSettings in `KeyExists` checks for both Run and Check actions.

### 4. ExecuteApply/ExecuteRevert Swallow Fatal Exceptions (LOW) — `ViewModels/MainWindowViewModel.cs:544-551,589-596`
`catch { }` swallowed all exceptions including `StackOverflowException` and `OutOfMemoryException`, which could crash the app.
**Fix:** Filtered with `catch (Exception ex) when (ex is not StackOverflowException and not OutOfMemoryException)`.

## Other Notes

### Copilot Tweak Uses Non-Standard Registry Paths (INFO)
`HKEY_*\Software\Policies\Microsoft\Windows\WindowsAI` is not a standard Windows path. The policies won't apply on most systems. This is a known limitation of the Windows Recall / AI data controls which Microsoft hasn't exposed via standard policy keys yet. Not a crash, just ineffective.

### Explorer Home Tweak Uses HKLM (INFO)
`explorerhomegallery` writes to `HKEY_LOCAL_MACHINE` which requires admin. May fail on locked-down systems. That's by design for this tweak.

### Build: 0 warnings, 0 errors