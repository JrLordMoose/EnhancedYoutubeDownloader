# Release Notes v0.4.5

**Date:** 2026-05-21

## Maintenance & Fixes

- Updated bundled **yt-dlp** to 2026.03.17 (from 2025.09.26)
- Updated **YoutubeExplode** to 6.6.0
- Fixed failing Windows drive-letter subtitle path unit test
- Replaced debug `Console.WriteLine` with Release-safe `TraceLog` (Debug builds only)
- Fixed landing page SEO schema version metadata (0.4.5)
- Settings reopens after update check dialogs (no more dead-end flow)
- Auth dialog now directs users to browser cookie authentication in Settings
- Added GitHub Actions CI (build + test on Windows)
- Removed accidental `NUL` artifact; added to `.gitignore`

## Notes

Upload **both** `EnhancedYoutubeDownloader-Setup-v0.4.5.exe` and `EnhancedYoutubeDownloader-0.4.5.zip` to GitHub Releases for auto-update support.
