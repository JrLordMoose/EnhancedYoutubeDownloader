# Learning Session: A Visible Dialog Can Still Have a Broken Data Handoff

**Date**: 2026-09-02
**Category**: ui
**Severity**: High
**Auto-Triggered**: Yes - playlist dialog opened successfully but displayed `0 of 0 videos selected`

## Summary

The playlist resolver returned videos, but `DashboardViewModel.ProcessMultipleVideosAsync` created the batch dialog without assigning those videos or the playlist title. The visible dialog made the upstream resolution look broken even though the actual failure was an unfinished view-model handoff.

## Lesson

Opening a dialog is not proof that its workflow is connected. Batch dialogs must be tested as an end-to-end state transition: resolved items enter the view model, selection controls mutate per-item state, confirmation returns the selected subset, and downstream queue creation applies the chosen settings.

## Correction (The Rule)

Always test dialog data handoff and every action binding, not just whether the dialog renders. Never leave placeholder comments at a production call site where required view-model properties should be assigned.

## Context

- **File**: `src/Desktop/ViewModels/Components/DashboardViewModel.cs`
- **Error Message**: UI displayed `0 of 0 videos selected`
- **Solution**: Populate the dialog with resolved videos/title/defaults; implement per-video selection, counter, commands, chosen settings, and selected-only queueing.
- **Related Files**: `src/Desktop/ViewModels/Dialogs/DownloadMultipleSetupViewModel.cs`, `src/Desktop/Views/Dialogs/DownloadMultipleSetupDialog.axaml`, `src/Desktop/Views/Dialogs/DownloadMultipleSetupDialog.axaml.cs`

## Related Learnings

- [2026-08-22 - yt-dlp Requires a Bundled JavaScript Runtime](../August%202026/2026-08-22_Context_Session_37.md)
