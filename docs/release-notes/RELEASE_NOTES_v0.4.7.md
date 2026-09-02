# Release Notes v0.4.7

**Date:** 2026-09-02

## YouTube Playlist Downloads Fixed

- Fixed the playlist dialog opening with `0 of 0 videos selected` even when YouTube returned playlist entries.
- Playlist title and all resolved videos are now passed into the batch-download dialog.
- Implemented functional per-video checkboxes plus **Select All** and **Select None** controls.
- Implemented the **Browse**, **Cancel**, and **Download Selected** actions.
- The selection counter now updates immediately and only selected videos are queued.
- Quality, format, save folder, subtitle, and metadata preferences now apply to playlist downloads.
- Filenames are sanitized and kept unique within a batch.
- YouTube `watch?v=...&list=...` links now open the playlist workflow rather than downloading only one video.
- Fixed Cancel closing the visual dialog without completing its underlying task.

## Verification

- Confirmed the reported playlist `PLJbmuQXBZ2mo` is available and contains 12 entries.
- Release build succeeds with zero warnings and zero errors.
- All 56 automated tests pass.

The GitHub release must include both the installer EXE and ZIP package so automatic updates continue to work.
