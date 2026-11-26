# 🗓️ Health Check: November 25, 2026 (baseline)

> **✅ All passed (baseline)**
> Everything that should work does: 17 of 17 MP4/MP3 checks passed on 9 platforms right after the v0.4.8 release. This is the reference point future monthly runs compare against.

| | |
|---|---|
| **Ran** | 2026-11-25 20:35–20:40 EST (~5 min, run by hand to create the baseline) |
| **App version tested** | v0.4.8 (commit `9c9e4a5`) |
| **yt-dlp** | already current: 2026.11.23 |
| **Cookies file** | present, exported 0 days ago |
| **Pushed to GitHub?** | Log only (this baseline + the scheduled-log setup) |
| **Release** | none (v0.4.8 was released earlier the same day) |
| **Session log** | [Session 39](../../learning_docs/Sessions/November%202026/2026-11-25_Session_39.md) |

## 1. Build & tests
- Build: 0 errors, 0 warnings
- Unit tests: 95/95 passed (first run, no previous count)

## 2. Downloads (MP4 + MP3)
**17 passed · 0 failed · 3 site-limited** (yt-dlp 2026.11.23, cookies: yes)

| Result | Platform | Format | Codecs | Duration | Size | Time |
|---|---|---|---|---|---|---|
| ✅ PASS | YouTube (watch) | MP4 | h264 + aac | 0:19 | 929 KB | 12.2s |
| ✅ PASS | YouTube (watch) | MP3 | mp3 | 0:19 | 524 KB | 12.9s |
| ✅ PASS | YouTube Short | MP4 | h264 + aac | 0:01 | 360 KB | 11.6s |
| ✅ PASS | YouTube Short | MP3 | mp3 | 0:01 | 314 KB | 12.2s |
| ✅ PASS | TikTok | MP4 | hevc + aac | 0:10 | 1,816 KB | 3.5s |
| ✅ PASS | TikTok | MP3 | mp3 | 0:11 | 478 KB | 3.6s |
| ✅ PASS | X / Twitter | MP4 | h264 + aac | 1:54 | 10,923 KB | 4.1s |
| ✅ PASS | X / Twitter | MP3 | mp3 | 1:54 | 3,266 KB | 8.1s |
| ✅ PASS | Facebook | MP4 | h264 + aac | 1:14 | 5,262 KB | 4.0s |
| ✅ PASS | Facebook | MP3 | mp3 | 1:14 | 2,133 KB | 5.1s |
| ✅ PASS | Dailymotion | MP4 | h264 + aac | 1:27 | 21,903 KB | 5.6s |
| ✅ PASS | Dailymotion | MP3 | mp3 | 1:27 | 2,696 KB | 6.6s |
| ✅ PASS | SoundCloud | MP3 | mp3 | 3:34 | 6,652 KB | 8.6s |
| ✅ PASS | Twitch clip | MP4 | h264 + aac | 0:32 | 16,108 KB | 3.5s |
| ✅ PASS | Twitch clip | MP3 | mp3 | 0:32 | 1,126 KB | 4.1s |
| ✅ PASS | Direct .mp4 URL | MP4 | h264 + aac | 0:06 | 2,781 KB | 3.5s |
| ✅ PASS | Direct .mp4 URL | MP3 | mp3 | 0:06 | 139 KB | 4.1s |
| ⚠️ SITE-LIMITED | Instagram reel | MP4 | | | | 5.1s |
| ⚠️ SITE-LIMITED | Reddit | MP4 | | | | 2.5s |
| ⚠️ SITE-LIMITED | Vimeo | MP4 | | | | 2.0s |

Raw data: [2026-11-25_results.json](2026-11-25_results.json)

**Compared with last month:** first run, no previous data. YouTube now saves **H.264** (it was AV1 before v0.4.8).

## 3. Settings audit
All 21 settings verified in Session 39: each is shown in the dialog (or is intentionally internal), used by the code, restored by Cancel and cleared by Reset All.

## 4. Release channel
- Latest release `v0.4.8`: EXE ✅ (93,438,816 bytes), ZIP ✅ (119,553,265 bytes). Both return HTTP 200.

## 5. Fixes made
None in this run. Session 39 shipped the fixes in v0.4.8.

## 6. Site-side limits (not app bugs)
- **Instagram, Reddit:** require a signed-in account. The app shows "Sign-in required" and offers a cookies.txt for that site.
- **Vimeo:** the web client requires login, and logged-out playback is DRM-only.
- **Bilibili:** blocks this network (403/567), so it isn't in the test list.

## 7. Action needed from you
- Nothing right now.
- Optional: test the auto-update from your installed v0.4.7 via **Settings → Advanced → Check for updates now**.

## 8. Ideas for future development
- **TikTok saves HEVC (H.265)**, because TikTok doesn't always offer H.264. Windows may need the free HEVC extension to play it. Consider an optional "re-encode to H.264 for compatibility" setting.
- **Real pause/resume:** today, Resume restarts the download from 0%. Continuing yt-dlp's `.part` files would make long downloads resumable.
- **Bundle `ffprobe.exe`:** yt-dlp warns when it's missing (it's used for some metadata/thumbnail work).
- **Per-site cookies:** the cookies setting holds one file. Instagram and Reddit need their own sites' cookies, so allow several files or a merged export.
- **Release channel:** sign the update ZIP or publish a SHA-256 (ThreatLens TL-20261125-02).
