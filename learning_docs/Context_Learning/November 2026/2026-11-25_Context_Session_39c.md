# Learning Session: Fixing a hidden UI exposes latent runtime crashes; test-update safety

**Date**: 2026-11-25
**Category**: ui / process
**Severity**: Critical
**Auto-Triggered**: Yes (a user hit a Fatal Error after installing v0.4.8)

## 📝 Summary
v0.4.8 fixed the SnackbarHost `x:DataType`, so notifications rendered for the first time. Their template had a `MultiBinding` with an `IValueConverter` (`SeverityToIconConverter`), and the first notification crashed the app with `InvalidCastException`. Because v0.4.8's update flow shows a notification *before* installing, v0.4.8 users can't auto-update and must run the installer. Fixed in v0.4.10.

## 💡 Lesson
- **Repairing UI that never rendered exposes every latent bug inside it.** When a fix makes something visible for the first time, render it (every variant) before release.
- **The XAML compiler doesn't check converters registered in code-behind** (`Resources["X"] = new ...`), so a wrong converter type only fails at runtime.
- **Never put a notification (or any UI that could fail) on the critical path of an update.** v0.4.9+'s "Update Now" path uses only the progress dialog.
- **Test updates must never be able to reach users:**
  - Onova reads *every* published GitHub release, including pre-releases.
  - The prepared-package cache (`%LocalAppData%\Onova\EnhancedYoutubeDownloader\<version>`) is shared with the real installed app. A leftover test package with a real version number would be installed *instead of* the real download.

## ✅ Correction (The Rule)
- **Always** manually trigger every notification severity and open every dialog in the built app before a release that touches UI templates or bindings.
- **Always** register converters in AXAML resources (compile-time checked) rather than code-behind. If code-behind is unavoidable, use `Binding` for `IValueConverter` and `MultiBinding` only for `IMultiValueConverter`.
- **Pseudo-update tests:**
  - local package folder only (`EYD_UPDATE_SOURCE`), never any GitHub release/draft/pre-release;
  - impossible version `99.0.0`;
  - env vars scoped to the test process only (never `setx`);
  - run on a throwaway copy, never the installed folder;
  - delete `%LocalAppData%\Onova\EnhancedYoutubeDownloader\99.0.0*` afterwards;
  - verify the installed app's version/hash didn't change.
- **CRITICAL REMINDER**: users on a broken version run the broken update code. Release notes must give them a manual path (the installer).

## 🔍 Context
- **File**: `src/Desktop/Controls/SnackbarHost.axaml` (icon `Kind` binding)
- **Error**: `System.InvalidCastException: Unable to cast object of type 'SeverityToIconConverter' to type 'Avalonia.Data.Converters.IMultiValueConverter'`
- **Verified**: local pseudo update v0.4.9 → v0.4.10. The Onova log showed an instant exit and "Update completed successfully"; the app reopened with no crash.

## 🔗 Related Learnings
- [Auto-update must prompt, show progress and exit in 15s](2026-11-25_Context_Session_39b.md)
- [Sign-in walls, settings load order, one dialog at a time](2026-11-25_Context_Session_39.md)
