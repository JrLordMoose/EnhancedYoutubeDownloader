# Learning Session: Sign-in walls, settings load order, and one-dialog-at-a-time

**Date**: 2026-11-25
**Category**: auth / config / ui
**Severity**: Critical
**Auto-Triggered**: Yes (the same "it's saved but does nothing" bug class showed up in 6+ places)

## 📝 Summary
Three root causes explained most of this session's bugs:
1. YouTube now blocks flagged IPs with "Sign in to confirm you're not a bot" (not HTTP 403), and on Windows yt-dlp can't read Chrome, Edge or Brave cookies.
2. Settings were loaded *after* the services that read them were constructed.
3. `DialogManager` allows one dialog at a time, so opening a dialog from inside a dialog waits forever.

## 💡 Lesson
- **A yt-dlp failure is classified from the `ERROR:` part only.** Sign-in walls appear as "confirm you're not a bot", "Account authentication is required" and "only works when logged-in". WARNING lines mention cookies too, so they must be ignored. See `YtDlpErrorClassifier`.
- **`--cookies-from-browser` on Windows:** Chrome 127+, Edge and Brave fail with "Failed to decrypt with DPAPI" (app-bound encryption); Brave also fails while running ("Could not copy Chrome cookie database"). Only Firefox works. The reliable path is a **cookies.txt** file passed with `--cookies`.
- **Exported YouTube cookies go stale** once the same browser session keeps browsing YouTube, and yt-dlp writes updates back into the file. Export from a **private window**, then close it without signing out.
- **The bot check is intermittent.** It blocked every player client at 19:05, yet a QA run at 20:05 downloaded YouTube with no cookies. Don't call YouTube "fixed" or "broken" from a single run.

## ✅ Correction (The Rule)
- **Always** call `SettingsService.Load()` before resolving any service that reads settings in its constructor (cache path, parallel limit). It now runs in the `App` constructor.
- **Never** open a dialog (`DialogManager.ShowDialogAsync`) from inside an open dialog. Close the first, show the second, then reopen the first (see `SettingsViewModel.ResetAllSettingsAsync`).
- **Always** marshal UI work triggered by `DownloadStatusChanged` with `Dispatcher.UIThread.Post`. Those events fire on thread-pool threads, and a fire-and-forget task swallows the Avalonia exception.
- **Never** bind `ComboBox.SelectedIndex` straight to an enum whose order differs from the items (the Theme bug), or `SelectedValue` to a string over plain `ComboBoxItem`s (the Browser bug). Use an index-mapping view-model property.
- **Always** check that a `DynamicResource` key exists in Material.Avalonia 3.9.2 (`MaterialPrimaryBrush` and `MaterialFilledTextBox` don't). A missing key fails silently.
- **CRITICAL REMINDER**: A setting is only "done" when it is: (1) in the UI, (2) read by the code that should use it, (3) restored by Cancel, (4) reset by Reset All, and (5) raising PropertyChanged.

## 🔍 Context
- **Files**:
  - `src/Desktop/App.axaml.cs` (load order, theme)
  - `src/Desktop/Services/SettingsService.cs` (all properties now notify)
  - `src/Desktop/ViewModels/Dialogs/SettingsViewModel.cs` (reflection snapshot, ThemeIndex/BrowserIndex)
  - `src/Core/Utils/YtDlpErrorClassifier.cs`
  - `src/Desktop/ViewModels/Components/DashboardViewModel.cs` (UI-thread marshalling)
- **Error Messages**: `Sign in to confirm you're not a bot`, `Failed to decrypt with DPAPI`, `Could not copy Chrome cookie database`
- **Solution**: cookies.txt setting plus an automatic sign-in prompt; settings load first; dialogs never nest.

## 🔗 Related Learnings
- [yt-dlp Requires a Bundled JavaScript Runtime](../August%202026/2026-08-22_Context_Session_37.md): a *different* 403 cause
- [Visible Dialog, Broken Data Handoff](../September%202026/2026-09-02_Context_Session_38.md): the same "looks wired, isn't" class
