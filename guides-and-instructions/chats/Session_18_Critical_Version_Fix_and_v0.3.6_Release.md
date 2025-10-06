# Session 18: Critical Version Fix and v0.3.6 Release

**Date**: 2025-10-06
**Version**: v0.3.5 → v0.3.6
**Status**: ✅ Complete

## Overview

This session uncovered and fixed a **critical bug** that affected all releases from v0.3.2 to v0.3.5: the application version was hardcoded to v0.3.1 in `Directory.Build.props`, meaning the installer version didn't match the actual application version. This caused all previous bug fixes to not be compiled into the binary.

## Critical Discovery

### The Problem
User reported installing v0.3.5 but title bar still showed **"EnhancedYoutubeDownloader v0.3.1"**. Investigation revealed:

**Root Cause**: `Directory.Build.props` line 4 had `<Version>0.3.1</Version>` hardcoded.

**Impact**:
- Installer versions: 0.3.2, 0.3.3, 0.3.4, 0.3.5 (correct)
- Application binary version: 0.3.1 (WRONG!)
- All bug fixes in v0.3.2-v0.3.5 were **not compiled** into the application

**What This Broke**:
1. ❌ URL validation (added in v0.3.4) - not active
2. ❌ File size display fix (added in v0.3.4) - not active
3. ❌ Download folder cleanup (added in v0.3.5) - installer worked but app version wrong

### Files Involved
- `Directory.Build.props` - **The actual application version** (MSBuild reads this)
- `setup.iss` - Installer version only (for package metadata)
- `build-installer.ps1` - Build script version only

**Lesson Learned**: There are THREE version locations that must all be updated!

## Issues Fixed in v0.3.6

### 1. Application Version Mismatch ✅
**Problem**: Title bar showed v0.3.1 even after installing v0.3.5

**Root Cause**: `Directory.Build.props` line 4 never updated from 0.3.1

**Solution**: Updated `Directory.Build.props` version to 0.3.6

**File**: `Directory.Build.props` line 4
```xml
<!-- Before -->
<Version>0.3.1</Version>

<!-- After -->
<Version>0.3.6</Version>
```

### 2. Uninstaller Crash ✅
**Problem**: Uninstaller showed error "Internal error: Unknown constant 'uservideos'" (see ytscreenshot31.png)

**Root Cause**: `{uservideos}` is not a valid Inno Setup constant

**Solution**: Changed to `{userprofile}\Videos\Downloads`

**File**: `setup.iss` line 122
```inno
<!-- Before -->
DefaultDownloadPath := ExpandConstant('{uservideos}\Downloads');

<!-- After -->
DefaultDownloadPath := ExpandConstant('{userprofile}\Videos\Downloads');
```

### 3. Empty Dialog with 0 Videos ✅
**Problem**: Invalid queries like "ttt", "ddd", "rrrr" showed empty frozen dialog with "0 of 0 videos selected" (see ytscreenshot29.png, ytscreenshot30.png, ytscreenshot32.png)

**Root Cause**: Check for empty videos happened AFTER dialog was created

**Solution**: Moved empty check before `ShowDialogAsync()` call

**File**: `src/Desktop/ViewModels/Components/DashboardViewModel.cs` lines 323-328
```csharp
// Check if we have any videos BEFORE showing the dialog
if (result.Videos == null || !result.Videos.Any())
{
    _snackbarManager.NotifyError("No videos found for this query");
    return;
}
```

### 4. Cancel Button Not Working ✅
**Problem**: Cancel button visible but not clickable, dialog frozen (see ytscreenshot32.png)

**Root Cause**: No event handler wired up for Cancel button

**Solution**: Added Click handler to close dialog with false result

**File**: `src/Desktop/Views/Dialogs/DownloadMultipleSetupDialog.axaml.cs` lines 14-31
```csharp
public DownloadMultipleSetupDialog()
{
    InitializeComponent();

    // Wire up Cancel button
    var cancelButton = this.FindControl<Button>("CancelButton");
    if (cancelButton != null)
    {
        cancelButton.Click += OnCancelClicked;
    }
}

private void OnCancelClicked(object? sender, RoutedEventArgs e)
{
    // Close the dialog with false result
    DialogHost.Close("RootDialog", false);
}
```

## Files Modified

### Directory.Build.props
- **Line 4**: Version 0.3.1 → 0.3.6 ✅ **CRITICAL FIX**

### setup.iss
- **Line 5**: Version 0.3.5 → 0.3.6
- **Line 122**: Fixed {uservideos} constant to {userprofile}\Videos\Downloads

### build-installer.ps1
- **Line 6**: Default version 0.3.5 → 0.3.6

### README.md
- **Lines 47, 140**: Updated download links to v0.3.6

### src/Desktop/ViewModels/Components/DashboardViewModel.cs
- **Lines 323-328**: Moved empty video check before dialog creation
- **Line 326**: Changed from `Notify` to `NotifyError` for better visibility

### src/Desktop/Views/Dialogs/DownloadMultipleSetupDialog.axaml.cs
- **Lines 1-32**: Added Cancel button handler and DialogHostAvalonia namespace

## Build & Release

**Installer**: `EnhancedYoutubeDownloader-Setup-v0.3.6.exe` (79.72 MB)

**Release**: https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/tag/v0.3.6

**Direct Download**: [EnhancedYoutubeDownloader-Setup-v0.3.6.exe](https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/download/v0.3.6/EnhancedYoutubeDownloader-Setup-v0.3.6.exe)

## Commits

1. **a82bb48** - Add user-specific folders to .gitignore
2. **6ca3736** - Release v0.3.6: Critical version fix and dialog improvements

## User Feedback Journey

1. **First Report**: "downloaded the latest release but its still showing the previous version"
2. **Second Report**: Showed screenshots (ytscreenshot29, ytscreenshot30) of v0.3.1 in title bar
3. **Third Report**: "after uninstall i got this error" (ytscreenshot31 - uservideos constant error)
4. **Fourth Report**: "did a fresh install as an administrator and it still happened and showing the wrong version number"
5. **Investigation**: "check over your work again" - Led to discovery of Directory.Build.props issue

## Technical Insights

### Version Management in .NET Projects
1. **Directory.Build.props** - THE source of truth for .NET assembly version
   - Read by MSBuild during compilation
   - Sets `Assembly.GetName().Version`
   - Used by `Program.cs` for title bar display
2. **setup.iss** - Installer package metadata only
3. **build-installer.ps1** - Script default parameter only

**Critical Lesson**: Always update Directory.Build.props first, then installer files!

### Inno Setup Constants
- `{userprofile}` = `C:\Users\Username`
- `{userdocs}` = `C:\Users\Username\Documents`
- `{localappdata}` = `C:\Users\Username\AppData\Local`
- ❌ `{uservideos}` = **DOES NOT EXIST**

### Dialog Flow Issues
**Anti-Pattern**: Showing dialog before validating data
```csharp
// ❌ BAD
var dialog = CreateDialog();
if (videos.Count == 0) return;
await ShowDialog(dialog);

// ✅ GOOD
if (videos.Count == 0) return;
var dialog = CreateDialog();
await ShowDialog(dialog);
```

### Event Handler Wiring in Avalonia
**Pattern**: Wire up events in constructor after InitializeComponent()
```csharp
public MyControl()
{
    InitializeComponent();

    var button = this.FindControl<Button>("ButtonName");
    if (button != null)
    {
        button.Click += OnButtonClicked;
    }
}
```

## Future Improvements Noted

1. **Version sync script** - Automated script to update all three version locations
2. **Pre-build version check** - Fail build if versions don't match
3. **Release checklist** - Document requiring Directory.Build.props update first
4. **Direct download links in release notes** - User requested clickable installer links

## Next Steps

User requested:
1. Add direct installer download links to release notes markdown ✅ (Done in v0.3.6 release)
2. Update CLAUDE.md with instruction to always include download links in releases (Pending)

## Lessons Learned

1. **Always verify title bar version** after installing new release
2. **Directory.Build.props is the source of truth** for .NET application version
3. **Test uninstaller** before releasing (caught uservideos constant error)
4. **Validate before showing dialogs** prevents frozen UI states
5. **Event handlers need explicit wiring** in Avalonia code-behind
6. **User screenshots are invaluable** for debugging - saw exact version mismatch
7. **Fresh installs don't always work** - version was baked into binary at build time
