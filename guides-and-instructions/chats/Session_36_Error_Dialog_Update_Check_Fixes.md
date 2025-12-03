# Session 36: Error Dialog & Update Check Fixes (Release v0.4.4)

**Date:** 2025-12-03
**Starting Commit:** d5d41e7 (Update Session Index with Session 35 entry)
**Ending Commit:** 4777a19 (Fix update check dialog not appearing in Settings)
**Version:** v0.4.4
**Focus Areas:** Error dialog button fixes, update check dialog deadlock fix, feedback options

---

## Quick Resume

- Fixed Error Dialog buttons not responding (Close, Retry, Copy, etc.) - root cause was custom `Close` property shadowing inherited `Close(bool)` method
- Fixed "Check for Updates" dialog not appearing in Settings - root cause was SemaphoreSlim deadlock from nested dialogs
- Added "Submit Feedback" (Google Form) and "Report Issue" (GitHub) buttons to error dialogs
- Released v0.4.4 with all fixes, uploaded both EXE and ZIP to GitHub
- Next: Test auto-update flow from v0.4.3 to v0.4.4

---

## Key Accomplishments

### 1. Fixed Error Dialog Buttons Not Working

**Problem:** When network errors occurred, the Error Dialog appeared but all buttons (Toggle Details, Check Connection, Retry, Copy, Close) were unresponsive.

**Root Cause:** `ErrorDialogViewModel` had a custom `public Action? Close { get; set; }` property that **shadowed** the inherited `Close(bool)` method from `DialogViewModelBase`. When buttons called `Close?.Invoke()`:
1. The custom `Close` action was null (never set)
2. The TaskCompletionSource in base class never signaled
3. DialogManager's `WaitForCloseAsync()` never completed
4. Dialog stayed open indefinitely

**Fix:** Removed custom Close property and changed button commands to call `Close(true)` directly (the inherited method).

**Files Modified:**
- `src/Desktop/ViewModels/Dialogs/ErrorDialogViewModel.cs`
  - Line 96: `Close?.Invoke()` → `Close(true)`
  - Line 102: `Close?.Invoke()` → `Close(true)`
  - Line 105: Removed `public Action? Close { get; set; }`

### 2. Fixed "Check for Updates" Dialog Not Appearing

**Problem:** When clicking "Check for Updates" in Settings:
1. Button showed "Checking..." and grayed out
2. Update detected (showed "Update available: v0.4.4")
3. But no dialog appeared to prompt download/install
4. Button stayed stuck on "Checking..."

**Root Cause:** SemaphoreSlim deadlock in DialogManager:
1. User opens Settings → `ShowDialogAsync(settingsDialog)` acquires `_dialogLock`
2. User clicks "Check for Updates" → `CheckForUpdatesAsync()` runs
3. Update found → tries `ShowDialogAsync(updatePrompt)`
4. **DEADLOCK**: `_dialogLock.WaitAsync()` blocks forever (already held by Settings)

**Fix:** Close Settings dialog before showing any nested dialogs to release the lock.

**Files Modified:**
- `src/Desktop/ViewModels/Dialogs/SettingsViewModel.cs`
  - Line 318: Added `Close(false)` before "No updates" dialog
  - Line 332: Added `Close(false)` before "Update available" dialog
  - Line 380: Added `Close(false)` before "Error" dialog

### 3. Added Feedback Options to Error Dialog

**New Features:**
- **Submit Feedback** button - Opens Google Form for general users
- **Report Issue** button - Opens GitHub Issues for developers

**Files Modified:**
- `src/Desktop/ViewModels/Components/DashboardViewModel.cs`
  - Lines 678-691: Added new error actions with `feedback_form` and `report_github` action keys
  - Lines 766-784: Added handlers to open URLs in default browser
  - Lines 1-8: Added `System.Diagnostics` and `System.Runtime.InteropServices` imports
  - Lines 831-853: Added cross-platform `OpenUrl()` helper method

### 4. Released v0.4.4

**Version Locations Updated:**
1. `Directory.Build.props` (line 4): 0.4.3 → 0.4.4
2. `setup.iss` (line 5): 0.4.3 → 0.4.4
3. `build-installer.ps1` (line 6): 0.4.3 → 0.4.4
4. `README.md`: Updated download links
5. `docs/index.html`: Updated landing page download buttons

**GitHub Release:**
- URL: https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/tag/v0.4.4
- Files: EnhancedYoutubeDownloader-Setup-v0.4.4.exe (~83 MB), EnhancedYoutubeDownloader-0.4.4.zip (~107 MB)

---

## Technical Details

### DialogManager Lock Pattern

```csharp
// DialogManager.cs - The lock that caused the deadlock
private readonly SemaphoreSlim _dialogLock = new(1, 1);

public async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialog)
{
    await _dialogLock.WaitAsync();  // Blocks if lock already held
    try
    {
        await DialogHost.Show(...);
        await dialog.WaitForCloseAsync();  // Waits for Close(T) call
        ...
    }
    finally
    {
        _dialogLock.Release();
    }
}
```

### Comparison of Dialog Close Patterns

| Dialog | Pattern Used | Works? |
|--------|-------------|--------|
| MessageBoxViewModel | `Close(true)` (inherited) | Yes |
| SettingsViewModel | `Close(true)` (inherited) | Yes |
| ErrorDialogViewModel (before) | `Close?.Invoke()` (custom) | No |
| ErrorDialogViewModel (after) | `Close(true)` (inherited) | Yes |

---

## Commits This Session

1. `cba3074` - Release v0.4.4: Fix Error Dialog buttons & add feedback options
2. `4777a19` - Fix update check dialog not appearing in Settings

---

## Files Changed

| File | Changes |
|------|---------|
| `src/Desktop/ViewModels/Dialogs/ErrorDialogViewModel.cs` | Fixed Close method shadowing |
| `src/Desktop/ViewModels/Dialogs/SettingsViewModel.cs` | Fixed nested dialog deadlock |
| `src/Desktop/ViewModels/Components/DashboardViewModel.cs` | Added feedback options & OpenUrl |
| `Directory.Build.props` | Version 0.4.4 |
| `setup.iss` | Version 0.4.4 |
| `build-installer.ps1` | Version 0.4.4 |
| `README.md` | Updated download links |
| `docs/index.html` | Updated landing page |

---

## Next Session Priorities

1. Test auto-update flow from v0.4.3 to v0.4.4 to verify Onova works correctly
2. Consider making the Settings dialog behavior more user-friendly (maybe reopen after update check)
3. Add more detailed error logging for debugging future issues

---

## Keywords

`v0.4.4` `error-dialog` `close-button` `dialog-deadlock` `semaphore` `nested-dialog` `update-check` `feedback-form` `github-issues` `report-issue` `dialogviewmodelbase` `close-method` `method-shadowing` `release`
