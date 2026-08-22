# Session 37: Critical 403 Fix — Bundled JS Runtime & Release v0.4.6

**Date:** 2026-08-22
**Type:** Critical Bug Fix + Release
**Status:** ✅ Complete (user-confirmed working)
**Version:** v0.4.6
**Commit:** `7bb68b7`
**Release:** https://github.com/JrLordMoose/EnhancedYoutubeDownloader/releases/tag/v0.4.6

---

## ⚡ Quick Resume (Start Here)

> App was **completely non-functional** — every download on every site failed with HTTP 403. Cause was a missing JavaScript runtime (a new yt-dlp requirement), not a bad URL or cookie problem. Fixed by bundling QuickJS, updating yt-dlp, and shipping v0.4.6. Verified working end-to-end and confirmed by the user.

| Item | Value |
|---|---|
| Blocking Issues | None |
| Branch | `main` (pushed) |
| Last Action | Verified live landing page serves v0.4.6 |
| Release Assets | EXE 83.6 MB + ZIP 108.5 MB (both uploaded & verified 200) |
| Tests | 56/56 passing |
| Known Timing | ~10–20s before a download starts — **expected**, benchmarked, not a regression ([details](#️-performance-1020s-per-download-is-expected)) |

### Immediate Next Actions

1. [ ] Add `.github/workflows/ci.yml` — still **untracked on disk**; blocked because the GitHub token lacks `workflow` scope. Run `gh auth refresh -s workflow`, then commit separately.
2. [ ] Fix stale CLAUDE.md checklist item — it says update a hardcoded version at `SettingsDialog.axaml:406`, but that now binds dynamically to `CurrentVersion`. Nothing to edit there anymore.
3. [ ] Relabel the `<!-- NEW FEATURES (v0.4.1) -->` comment in `docs/index.html:326` — the section now covers features well beyond v0.4.1.
4. [ ] Confirm non-YouTube platforms also download correctly — only YouTube was explicitly verified this session.

---

## 🐛 The Problem

Every download failed regardless of video or source site:

```
ERROR: unable to download video data: HTTP Error 403: Forbidden
```

Reproduced directly against the bundled binary, which revealed the real cause:

```
WARNING: [youtube] No supported JavaScript runtime could be found.
Only deno is enabled by default; to use another runtime add --js-runtimes RUNTIME[:PATH]
```

### Root Cause

yt-dlp now requires a **JavaScript runtime** to solve YouTube's EJS player challenges — the step that produces signed media URLs. The app shipped no runtime, so yt-dlp fell back to degraded player clients (`android vr`, `visionos`) whose URLs YouTube rejects with 403.

Compounding factor: the bundled yt-dlp was **5 months stale** (`2026.03.17`).

### Critical Diagnostic Insight

**Updating yt-dlp alone does NOT fix this.** Tested explicitly — latest yt-dlp (`2026.08.19`) without a JS runtime still only exposed degraded HLS formats and failed with *"Requested format is not available."* The JS runtime was the actual blocker. Anyone debugging a future 403 should test these two variables **separately**.

---

## 🎯 Accomplishments

- [x] Reproduced and isolated the true root cause (JS runtime, not cookies/auth)
- [x] Proved a yt-dlp update alone is insufficient — avoided shipping a false fix
- [x] Benchmarked QuickJS vs Deno and chose the 2 MB option over the 97 MB one
- [x] `Download-JsRuntime.ps1`: new cross-platform QuickJS fetch script
- [x] `EnhancedYoutubeDownloader.csproj`: `DownloadJsRuntime` target + output copy
- [x] `YtDlpDownloadService.cs`: rewrote `ApplyCompatibilityOptions()` to register bundled QuickJS
- [x] `YtDlpSelfUpdater.cs`: hardened and **actually wired up** (was dead code)
- [x] `App.axaml.cs`: fire-and-forget `yt-dlp -U` on startup
- [x] Updated bundled yt-dlp to `2026.08.19`
- [x] Version bumped to 0.4.6 across all locations; built, released, verified

---

## 🔍 Key Decisions

### Bundle QuickJS (2 MB) instead of Deno (97 MB)

- **Why**: Deno is yt-dlp's recommended default, but bundling it would grow the installer from 83 MB to ~180 MB. QuickJS is officially supported by yt-dlp and tested clean.
- **Validated across**: standard video, music video, YouTube Shorts, and a merged video+audio download.
- **Not a lock-in**: yt-dlp still auto-detects and *prefers* a system Deno/Node/Bun when one exists — verified that precedence works. QuickJS is only the bundled fallback.
- **Result**: installer grew just ~0.5 MB (83.1 → 83.6 MB).

### Wire up yt-dlp self-update on startup

- **Why**: this outage happened *because* a frozen binary went stale. A one-time binary refresh fixes today but guarantees a repeat later.
- **Safety**: non-blocking, 2-minute timeout, both stdout/stderr drained concurrently to avoid pipe-buffer deadlock, all exceptions swallowed and logged. A failed update can never break startup.

### Dropped `--extractor-args youtube:player_client=default,web_embedded` from WIP

- **Why**: A/B tested it — identical format selection with and without. It added no benefit while risking breakage whenever yt-dlp renames player clients.

### Did NOT silent-install the built EXE to verify packaging

- **Why**: the installer shares an `AppId` with the user's existing install, so a test install would have **upgraded/overwritten their real v0.4.5**.
- **Instead**: verified `qjs.exe` via the Inno Setup compiler's own file list plus direct ZIP inspection — same confidence, zero risk.

---

## 🔄 Code Changes

**New**

- `src/Core/Services/YtDlpSelfUpdater.cs` — hardened background yt-dlp updater
- `src/Desktop/Download-JsRuntime.ps1` — QuickJS fetch (win/linux/osx, x64/arm64/x86)
- `src/Desktop/qjs.exe` — bundled QuickJS binary (2.1 MB)

**Modified**

- `src/Core/Services/YtDlpDownloadService.cs` — `ApplyCompatibilityOptions()` registers QuickJS; added `--retries`/`--fragment-retries`
- `src/Desktop/App.axaml.cs` — invoke `YtDlpSelfUpdater.TryUpdateAsync()` at startup
- `src/Desktop/EnhancedYoutubeDownloader.csproj` — `DownloadJsRuntime` target, qjs output copy
- `src/Core/Services/QueryResolver.cs` — yt-dlp metadata fallback when YoutubeExplode fails
- `src/Core/Models/GenericVideo.cs` — accept real video ID; pad short IDs safely
- `src/Core/Utils/Http.cs` — browser User-Agent
- `src/Desktop/yt-dlp.exe` — 2026.03.17 → 2026.08.19
- Version bump: `Directory.Build.props`, `setup.iss`, `build-installer.ps1`, `README.md`, `docs/index.html`

---

## 🐛 Issues & Solutions

| Error | Cause | Solution |
|---|---|---|
| `HTTP Error 403: Forbidden` on all downloads | No JS runtime for EJS challenges | Bundle QuickJS, register via `--js-runtimes` |
| `Requested format is not available` | Degraded fallback clients when no JS runtime | Same fix — not a format-selection bug |
| `error : Was not formatted` (build) | CSharpier enforces formatting on `.csproj` too | Put `<None>` entries on one line |
| `error : Was not formatted` — *"different line endings"* | Wrote files with LF; repo is CRLF | Normalized all touched files to CRLF |
| `remote rejected ... without workflow scope` | Token has `repo` but not `workflow`; commit included `.github/workflows/ci.yml` | Removed the unrelated CI file from the commit; left it on disk |

---

## ⏱️ Performance: ~10–20s Per Download Is Expected

User reported downloads now take **10–20 seconds to start**. Benchmarked this rather than guessing — it is **expected behaviour, not a regression**, and **not** a cost of choosing QuickJS.

### Measurements (same video, `bv*+ba/b`)

| Scenario | Time |
|---|---|
| yt-dlp bare startup (`--version`, no network) | **1.9s** |
| Full extraction — **QuickJS**, cold cache | **10.74s** |
| Full extraction — **Deno**, cold cache | **10.79s** |
| Full extraction — warm cache | **10.5s** |
| Actual file transfer (11 MB) | **<1s** |

**QuickJS vs Deno differ by ~50ms.** The 97 MB runtime would buy nothing. The size/speed tradeoff was free.

### Where the time actually goes

Sequential network round-trips in yt-dlp's YouTube pipeline, none of which are ours:

1. `Downloading webpage`
2. `Downloading visionos player API JSON`
3. `Downloading m3u8 information`
4. `Downloading player 2574220e-main` (~2 MB of player JS)
5. `Solving JS challenges using quickjs`

Plus ~1.9s of PyInstaller self-extraction on every `yt-dlp.exe` invocation.

The challenge solve **re-runs per video** — the `youtube-sigfuncs` cache does not eliminate it, confirmed by an identical warm-cache timing.

### Why it feels new

Before v0.4.6 the app returned a 403 almost instantly. It was fast because it was **instantly failing**. The 10s is the cost of the now-mandatory challenge pipeline that makes downloads work at all.

### Optimisations evaluated and rejected

| Attempt | Result |
|---|---|
| Pin `player_client=web_safari` | No speed gain; **degrades** to a single HLS format |
| Pin `player_client=web` | No speed gain; **drops to 360p** |
| Pin `player_client=tv` | **Errors**: "The page needs to be reloaded" |
| `--extractor-args youtube:skip=hls` | ~0.9s saved (near noise); adds a knob that risks live/subtitle edge cases — **not shipped** |

This is a second, independent confirmation that dropping the `player_client` pin from the prior WIP was correct: it gives no speed benefit **and** actively degrades quality.

**Conclusion:** no code change. If this ever needs to improve, the real lever is avoiding the ~1.9s PyInstaller startup (invoke yt-dlp as a Python library instead of the one-file exe), not swapping the JS runtime.

---

## 🧠 Learnings This Session

- [2026-08-22 — yt-dlp requires a bundled JS runtime](../../learning_docs/Context_Learning/August%202026/2026-08-22_Context_Session_37.md)

---

## ❓ Knowledge Gaps

- Only YouTube was verified end-to-end. The other supported platforms share the yt-dlp path so they *should* be fixed, but this is unconfirmed.
- QuickJS held up on every video tested, but Deno remains yt-dlp's reference runtime. If an exotic challenge ever fails under QuickJS, the fallback is to bundle Deno or instruct users to install it (the code already prefers a system runtime automatically).

---

## 💭 Notes for Next Session

- **The 403 in Session 34 was a different bug.** That one was cookie/auth-related and fixed with browser-cookie auto-enable. This one is the JS-runtime requirement. Same symptom, unrelated cause — don't let the old fix mislead the diagnosis next time.
- `ProductVersion` on the shipped DLL reads `0.4.6+f75c8ff`, referencing the pre-amend commit hash (final is `7bb68b7`). Cosmetic only — the amend just dropped an unrelated CI file; compiled code is identical.
- The `Session Details` section of `SESSION_INDEX.md` still stops at Session 35 and is drifting from the `Sessions by Date` list.
