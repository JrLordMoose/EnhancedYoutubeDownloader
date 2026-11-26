# Context Learning Index

**Last Updated**: 2026-11-25 21:00 · **Total**: 5 · **Critical**: 3

---

## 🔴 Critical Learnings (Always Apply)

| Date | Title | Category | Key Takeaway |
|---|---|---|---|
| 2026-11-25 | [Auto-Update Must Prompt, Show Progress and Exit in 15s](November%202026/2026-11-25_Context_Session_39b.md) | build/ui | Onova LaunchUpdater waits ~15s for the app to exit, then installs nothing. Use UpdateCoordinator; test vN→vN+1 locally with EYD_UPDATE_SOURCE before every release. |
| 2026-11-25 | [Sign-in Walls, Settings Load Order, One Dialog at a Time](November%202026/2026-11-25_Context_Session_39.md) | auth/config/ui | YouTube bot check needs cookies.txt (Chromium cookies unreadable on Windows). Load settings before services read them. Never nest DialogManager dialogs. Marshal DownloadStatusChanged to the UI thread. |
| 2026-09-02 | [Visible Dialog, Broken Data Handoff](September%202026/2026-09-02_Context_Session_38.md) | ui | Test the full state transition, not merely whether a dialog opens. Wire required data, commands, selected subset, and downstream settings. |
| 2026-08-22 | [yt-dlp Requires a Bundled JavaScript Runtime](August%202026/2026-08-22_Context_Session_37.md) | build | A yt-dlp `403` is a **missing JS runtime** until proven otherwise. Run the bundled `yt-dlp.exe` manually and read stderr before touching C# code. Ship QuickJS alongside it. |

---

## 📅 Recent (Last 7 Days)

| Date | Title | Category | Key Takeaway |
|---|---|---|---|
| 2026-08-22 | [yt-dlp Requires a Bundled JavaScript Runtime](August%202026/2026-08-22_Context_Session_37.md) | build | Also: update-yt-dlp and add-JS-runtime must be tested as **separate variables**; updating alone produces a convincing false fix. |
| 2026-08-22 | [Download Latency — Measure Phases, Not Wall-Clock](August%202026/2026-08-22_Context_Session_37b.md) | process | First download ~12–16s, then ~6–8s. QuickJS is **~5.6s slower than Deno** at the solve, but it's cached per player version — keeping QuickJS. **Never compare implementations by wall-clock when network I/O dominates.** |

---

## 📁 By Category

### Build

- [2026-08-22 — yt-dlp Requires a Bundled JavaScript Runtime](August%202026/2026-08-22_Context_Session_37.md)
  - Bundle QuickJS (~2 MB), not Deno (~97 MB)
  - CSharpier formats `.csproj` too — keep `<None>` entries on one line
  - Repo is **CRLF**; LF writes fail the build
  - GitHub token lacks `workflow` scope — isolate `.github/workflows/**` changes
  - Never silent-install the built EXE to verify (shared `AppId` overwrites the real install)

### Process

- [2026-08-22 — Download Latency: Measure Phases, Not Wall-Clock](August%202026/2026-08-22_Context_Session_37b.md)
  - **Methodology**: never compare two implementations by total wall-clock when network I/O dominates — isolate the phase. A first attempt here concluded "QuickJS ≈ Deno" and was wrong by ~5.6s.
  - **Red flag**: if adding work makes something *faster*, the instrument is noise-dominated — change method, don't record the result.
  - QuickJS solve ~6.9s vs Deno ~1.3s, but **cached per player version** → occasional, not per-download
  - Keep QuickJS: Deno saves ~5.6s occasionally, costs +95 MB permanently
  - First download ~12–16s; steady state ~6–8s
  - Never pin `youtube:player_client` for speed — no gain, degrades quality
  - Only per-download lever left: ~1.9s PyInstaller startup

### UI

- [2026-09-02 - A Visible Dialog Can Still Have a Broken Data Handoff](September%202026/2026-09-02_Context_Session_38.md)
  - Verify resolved data reaches the dialog view model.
  - Exercise every command and confirm only selected items reach downstream work.

### API

_(none yet)_

### Auth

- See the **cross-reference warning** in the build learning: Session 34's cookie/auth 403 fix is a *different* bug from the Session 37 JS-runtime 403. Same symptom, unrelated cause.

### Database / Testing / Config

_(none yet)_

---

## 📁 Problem Areas

| Path | Known Issue |
|---|---|
| `src/Core/Services/YtDlpDownloadService.cs` | External runtime dependency (JS engine). Downloads fail app-wide with a misleading 403 if `qjs.exe` is missing from the output directory. |
| `src/Desktop/yt-dlp.exe` | Goes stale and breaks against YouTube within months. Startup self-update now mitigates this — do not remove it. |
| `.github/workflows/` | Pushes rejected - token lacks `workflow` scope. |
| `src/Desktop/ViewModels/Dialogs/DownloadMultipleSetupViewModel.cs` | Batch UI was historically scaffold-only; test full state handoff when changing playlist, channel, or search workflows. |

---

## 📈 Statistics

Most common categories: `build` (1), `process` (1) · Problem areas tracked: 3 · Last self-diagnosis: n/a
