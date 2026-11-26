# Learning Session: Auto-update must prompt, show progress, and EXIT within 15 seconds

**Date**: 2026-11-25
**Category**: build / ui (release channel)
**Severity**: Critical
**Auto-Triggered**: Yes (the user tested the v0.4.7 → v0.4.8 update and it silently failed)

## 📝 Summary
v0.4.8 was released with both files, but a real user on v0.4.7 couldn't update:
- nothing told them an update existed;
- after "Yes, Update" the app gave no feedback;
- the update never installed.

Onova's own log at `%LocalAppData%\Onova\EnhancedYoutubeDownloader\Log.txt` said:
`System.TimeoutException: Updatee did not exit in time.`

## 💡 Lesson
- **`LaunchUpdater` only *starts* the updater.** The updater waits about **15 seconds** for the app process to exit (so the DLLs are unlocked), then gives up and installs nothing. v0.4.7's Settings path called `FinalizeUpdate` and never exited, so it failed every time.
- **The startup "notification" was a toast that never rendered** (a SnackbarHost `x:DataType` mismatch), so users were never told about updates at all.
- **A running `yt-dlp.exe` (startup `-U` self-update, or downloads) is locked**, so the updater can't replace it.
- **"The release has both files" does not mean "updates work".** Before this, the update path had never been tested end to end.

## ✅ Correction (The Rule)
- **Always** route updates through `UpdateCoordinator` (`src/Desktop/Services/UpdateCoordinator.cs`). Its sequence:
  1. prompt dialog;
  2. progress dialog with % and "Installing to";
  3. kill yt-dlp/ffmpeg/qjs processes from the install folder;
  4. `FinalizeUpdate`;
  5. `Shutdown` immediately, with a 4-second `Environment.Exit` safety net.
- **Never** call `UpdateService.FinalizeUpdate` without exiting right after. Never announce an update with only a toast.
- **Always test the update end to end before each release.** Point a copy of the current build (labelled vN) at a local package (labelled vN+1) with `EYD_UPDATE_SOURCE=<folder containing EnhancedYoutubeDownloader-X.Y.Z.zip>`. Click Update Now and confirm it reopens as vN+1.
- **CRITICAL REMINDER**: users on an old version run the OLD update code. Fixes only help after they reach a fixed version, so release notes must tell old-version users how to get there (close the app within 15s after "Yes, Update", or run the installer).

## 🔍 Context
- **Files**:
  - `src/Desktop/Services/UpdateCoordinator.cs` (new)
  - `UpdateService.cs` (progress, `EYD_UPDATE_SOURCE`)
  - `ViewModels/Dialogs/UpdateProgressViewModel.cs` + `Views/Dialogs/UpdateProgressDialog.axaml` (new)
  - `MainViewModel.cs`, `SettingsViewModel.cs`
- **Error Message**: `System.TimeoutException: Updatee did not exit in time.` (Onova Updater v2.6.13)
- **Solution**: one coordinated flow, verified locally: v0.4.8 → v0.4.9 test package installed and reopened.

## 🔗 Related Learnings
- [Sign-in walls, settings load order, one dialog at a time](2026-11-25_Context_Session_39.md)
