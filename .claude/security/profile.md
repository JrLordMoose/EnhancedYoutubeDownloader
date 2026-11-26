# ThreatLens profile — Enhanced YouTube Downloader

**Created:** 2026-11-25 · **Last approved change:** 2026-11-25 — initial install (owner chose "Install + baseline audit") · **Engine:** ~/.claude/skills/threatlens

This file is ThreatLens's project-specific brain. It is loaded first on every run. Changes are
proposed as diffs and applied only with the owner's approval.

## 1. What this system is
- **Stack:** Windows desktop app. .NET 9, Avalonia 11.3 + Material.Avalonia, CommunityToolkit.Mvvm.
  Downloads via bundled `yt-dlp.exe` (driven by YoutubeDLSharp 1.1.2), `ffmpeg.exe`, QuickJS `qjs.exe`
  (yt-dlp JS challenge solver). Metadata via YoutubeExplode 6.6. SQLite (Microsoft.Data.Sqlite) for
  cache + download state. Whisper.net 1.8.1 for optional AI subtitles. Onova 2.6 auto-update from
  GitHub Releases. Installer: Inno Setup, per-user, no admin.
- **Users:** the general public on their own PCs. No accounts, no server, no payments, no telemetry.
- **Assets worth protecting (ranked):**
  1. Code execution on users' PCs — via the update channel, bundled binaries, or yt-dlp argument injection.
  2. Users' site sessions — `cookies.txt` files / browser cookies passed to yt-dlp for sign-in.
  3. Users' files — downloads write to user-chosen folders; post-processing replaces files in place.
- **Trust boundaries:** pasted URL/query (untrusted) → QueryResolver → yt-dlp argv; remote sites →
  yt-dlp output/metadata (titles become file names); GitHub Releases → Onova updater → executed
  binaries; build-time download scripts → binaries shipped in the installer; Hugging Face → Whisper model.
- **Source of truth for rules:** CLAUDE.md (release checklist) → this profile for security rules.

## 2. Project rules ThreatLens enforces (verify these first, every audit)
| Rule | Where it lives | How to verify |
|---|---|---|
| User input reaches yt-dlp only as a positional URL after `--` | YoutubeDLSharp `RunVideoDownload` (adds `--`); `YtDlpDownloadService.ProcessDownloadAsync` | argv probe (Audit 1): quote-breakout lands after `--`. Re-run if YoutubeDLSharp is upgraded or replaced. |
| External processes launched with `ArgumentList`, not concatenated strings, when any part is user/site-controlled | `SubtitleBurnInService.RunFfmpegAsync`, `YtDlpSelfUpdater` | grep `Arguments =` near user data |
| File names derived from titles are sanitized | `DashboardViewModel.SanitizeFileName` (uses `GetInvalidFileNameChars`) | read |
| Cookie **contents** are never logged, copied, or stored — only the file path | `YtDlpDownloadService.ApplyCookieOptions`, `SettingsViewModel.BrowseCookiesFile` | grep for reads of the cookies file |
| Logging is debug-only (`[Conditional("DEBUG")]`) | `src/*/Utils/TraceLog.cs` | read both files |
| Release must ship both EXE + ZIP (update channel integrity depends on it) | CLAUDE.md | `gh release view` |

## 3. Critical files (always in scope when touched)
- `src/Core/Services/YtDlpDownloadService.cs` — builds yt-dlp argv, cookies, post-processing file moves.
- `src/Core/Services/QueryResolver.cs` — classifies untrusted input.
- `src/Core/Services/SubtitleBurnInService.cs`, `SubtitleGenerationService.cs` — ffmpeg processes, model download.
- `src/Core/Services/YtDlpSelfUpdater.cs`, `src/Desktop/Services/UpdateService.cs` — code-update channels.
- `src/Desktop/Download-*.ps1`, `build-installer.ps1`, `setup.iss` — what ends up in the installer.
- `.github/workflows/*.yml` — CI.

## 4. Known-good controls (verify, don't re-flag)
- `--` end-of-options before the URL — YoutubeDLSharp 1.1.2 — proven by argv probe in Audit 1.
- yt-dlp `-U` verifies release checksums itself — upstream behavior; startup self-update in `YtDlpSelfUpdater`.
- Debug-only logging — `TraceLog` is `[Conditional("DEBUG")]` in Core and Desktop.
- Installer is per-user (no admin) — `setup.iss`.

## 5. Tailored check catalog
- **IN** (input/injection) — applies: argv injection into yt-dlp/ffmpeg, path traversal via titles, shell-execute of URLs.
- **SC** (supply chain) — applies strongly: bundled binaries, build-time downloads, Onova updates, NuGet.
- **SE** (secrets) — applies: repo/history scan; cookies handling; no app secrets exist.
- **DA** (data) — light: cookies.txt path; no PII collected.
- **CF** — light: CI workflow pinning only. No HTTP server, so headers/CORS do not apply.
- **AC/AU** — do not apply (no accounts, no server). **AI** — repo agent config only (`.claude/`).
- **RL/OP** — not applicable beyond download concurrency.

## 6. Recommended defaults for intake questions
| Question | Recommended | Why for this project |
|---|---|---|
| Mode of action | Audit + fixes (Critical/High only) | Small codebase; supply-chain items need owner decisions |
| How to work (1/2/3) | 1 — myself | ~9k lines of C#; a single pass covers it |
| Live testing | Code only + argv probe | No server to probe; the argv probe is local and harmless |
| Fix flow | Critical/High only, rest to checklist | |
| Session-check scope | Changes since last audit | Unless Critical files above were touched |

## 7. Tooling
- Approved scanners and commands: argv probe project (rebuild on demand in scratchpad); `git grep` secret patterns with positive control.
- CI security jobs: none yet (CI builds + tests only).
- Local dev server: n/a (desktop app).

## 7b. Secret management
- Manager: NONE needed — the app has no API keys or server credentials.
- User credentials: users' own `cookies.txt` / browser cookies, referenced by path in user settings, never bundled.
- Secrets known to have been committed: none found (Audit 1 scan of tracked files + history).

## 8. Audit folder
- Location and naming: `Security Audits/<Month YYYY>/YYYY-MM-DD_Security_Audit_<N>.md`
- Main build checklist to cross-link Critical/High items: none (CLAUDE.md release checklist only)
- Session logging for Flow C: `universal-session-learner` → `learning_docs/Sessions/`

## 9. Accepted risks and exceptions
- none yet

## 10. Research notes
- 2026-11-25 — Chromium browsers on Windows (Chrome 127+, Edge, Brave) use app-bound cookie encryption; yt-dlp `--cookies-from-browser` fails with "Failed to decrypt with DPAPI" (yt-dlp issue #10927). Reproduced locally with Edge.
- 2026-11-25 — YouTube "Sign in to confirm you're not a bot" is IP-reputation based; reproduced with yt-dlp 2026.11.23 across tv, mweb, web_safari, tv_simply, android_vr, web_embedded clients.
