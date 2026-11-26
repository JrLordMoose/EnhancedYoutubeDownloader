# 🛡️ Security Audit 1: Baseline after ThreatLens install

| | |
|---|---|
| **Date** | November 25, 2026 (2026-11-25) |
| **Time** | 19:15–19:25 EST |
| **Requested as** | "add universal session learner and threatlens if not already added" → Flow A (install) + Flow B baseline |
| **Profile** | `.claude/security/profile.md` (2026-11-25, initial) |
| **Mode of action** | Audit + fixes for Critical/High only (owner choice: "Install + baseline audit") |
| **How we worked** | 1 — myself (single pass) |
| **Live testing** | Code only, plus a local argv probe (scratch console app, no network) |
| **Fix flow** | Critical/High only, rest to checklist |
| **Git** | branch `main` at `af42487` + Session 39 working-tree changes; previous audit: none |

**Scope — files audited**
- `src/Core/Services/*.cs`: download pipeline, yt-dlp argv, ffmpeg processes, model download, updaters
- `src/Desktop/ViewModels/**`, `src/Desktop/Services/*.cs`, `src/Desktop/Framework/DialogManager.cs`: input handling, process launches, settings
- `src/Desktop/Download-*.ps1`, `build-installer.ps1`, `.github/workflows/ci.yml`: supply chain and CI
- Tracked files + full git history: secret scan

**Why these files:** first audit of the project. A desktop downloader's real risks are code execution (argv injection, update channel, bundled binaries) and users' site cookies.

> **TL;DR** — No Critical or High findings. The most important result is a **clean** one: a pasted "URL" can't inject yt-dlp options. YoutubeDLSharp puts `--` before the URL, and the probe showed a quote-breakout still landing after it. Supply chain is the weak area. There are 2 Medium items (build-time binary downloads aren't pinned or hash-checked; auto-updates aren't signature-checked) and 2 Low items (the Whisper model isn't hash-checked; CI/agent-config hygiene). All four went to the checklist; none were fixed this run, by design.

| Severity | Confirmed | Plausible |
|---|---|---|
| Critical | 0 | 0 |
| High | 0 | 0 |
| Medium | 1 | 1 |
| Low/Info | 2 | 0 |

## Findings

### TL-20261125-01 · Medium · Confirmed · SC — Build-time binary downloads aren't pinned or hash-verified
- **Where:** `src/Desktop/Download-JsRuntime.ps1:56`, `src/Desktop/Download-FFmpeg.ps1:38`, `:100`
- **Evidence:**
  ```
  $downloadUrl = "https://github.com/quickjs-ng/quickjs/releases/latest/download/$asset"
  $downloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
  $downloadUrl = "https://github.com/FFmpegBin/FFmpegBin/releases/download/7.1.1/ffmpeg-$platformInfo.zip"
  ```
  `grep -niE "hash|sha256|checksum" src/Desktop/Download-*.ps1` returns nothing.
- **Scenario:** If an upstream (or the third-party `FFmpegBin` mirror for non-Windows) is compromised, or `latest` changes, the next clean build silently bundles that binary into the signed-off installer for every user.
- **Proposed fix:** Pin exact versions and verify a SHA-256 for each asset in the scripts; fail the build on mismatch. (`yt-dlp.exe` and `qjs.exe` are committed, which already freezes them between deliberate updates.)
- **Verified by:** reading the scripts; the grep above shows no integrity check exists.
- **Deterministic guard:** proposed: hash check inside each script.
- **Status:** Open

### TL-20261125-02 · Medium · Plausible · SC — Auto-update packages aren't signature-verified
- **Where:** `src/Desktop/Services/UpdateService.cs:17-22` (`GithubPackageResolver` + `ZipPackageExtractor`)
- **Scenario:** Anyone who can publish a release on `JrLordMoose/EnhancedYoutubeDownloader` (for example after an account takeover) can push code that every installed copy runs on its next restart.
- **Proposed fix:** Turn on 2FA and a hardware key for the publishing account. Longer term, publish a signed SHA-256 manifest and check it before `FinalizeUpdate`, or code-sign the binaries.
- **Verified by:** code read. Plausible, because exploiting it requires account compromise first.
- **Status:** Open

### TL-20261125-03 · Low · Confirmed · SC — Whisper model downloaded without an integrity check
- **Where:** `src/Core/Services/SubtitleGenerationService.cs` (`DownloadModelAsync`, `resolve/main/ggml-base.bin`)
- **Scenario:** A changed or tampered model on the `main` branch gets loaded by native whisper.cpp code. This only matters when the user turns on AI subtitles.
- **Proposed fix:** Pin a commit revision in the URL and verify a SHA-256 before the move. (This session already added temp-file-then-move, so a partial download can't corrupt the model.)
- **Status:** Open

### TL-20261125-04 · Low · Confirmed · CF/AI — CI actions pinned to tags; personal agent permissions file is tracked
- **Where:** `.github/workflows/ci.yml` (`actions/checkout@v4`, `actions/setup-dotnet@v4`); `.claude/settings.local.json` is tracked in git
- **Scenario:** A moved tag runs different action code in CI. The tracked local settings publish one developer's permission allowlist. No secrets were found in it.
- **Proposed fix:** Pin the actions to commit SHAs. Untrack `.claude/settings.local.json` and gitignore it.
- **Status:** Open

## Checked and clean
| Check | Where | How this check could have failed |
|---|---|---|
| IN — yt-dlp option injection via pasted URL | YoutubeDLSharp 1.1.2 via `RunVideoDownload` | Probe app recorded the argv for 3 inputs (spaces, leading `--exec=`, embedded `"`). Case 3 **did** split into extra args, proving the probe can see injection, but all of them landed after `--`, so yt-dlp treats them as URLs. |
| IN — ffmpeg argv | `SubtitleBurnInService.RunFfmpegAsync` (new) | Uses `ArgumentList`, so each value stays one argv entry. |
| IN — shell-execute of URLs | `DashboardViewModel.OpenUrl` callers, `App.axaml.cs:309` | Every caller passes a constant URL or the `DependencyValidator` download URL. No site- or user-controlled value reaches them. |
| IN — explorer `/select,"path"` | `DashboardViewModel.OpenDownloadFolder` | Path comes from `SanitizeFileName` (`GetInvalidFileNameChars` includes `"`). |
| SE — secrets in tracked files and history | whole repo | Pattern grep for `sk-ant`, `AKIA…`, `ghp_…`, PEM, Slack, Google keys. Positive control: the same regex matched a planted sample string. 0 hits in tree and history. |
| DA — cookie contents | `ApplyCookieOptions`, `BrowseCookiesFile` | grep shows no code reads or copies the cookies file. Only its path goes to yt-dlp. |
| DA — logging | `src/*/Utils/TraceLog.cs` | `[Conditional("DEBUG")]`, so Release builds log nothing. |

## Dropped candidates
1 candidate didn't survive: "queued-download cancel releases a semaphore slot it never acquired." That's a real **correctness** bug (it could throw `SemaphoreFullException` or exceed the limit), but it isn't a security issue. It was fixed in Session 39 as a bug.

## Untrusted instructions encountered
None.

## Live probes performed
- Local only: `argprobe` scratch console app, YoutubeDLSharp pointed at itself to echo argv. No network requests.

## ThreatLens changes proposed
- Initial profile written (approved via the install choice).
- Offered, not applied: a ThreatLens section in `CLAUDE.md` (needs a separate yes).

## Recommended next steps
1. Add pinned versions + SHA-256 checks to `Download-FFmpeg.ps1` / `Download-JsRuntime.ps1` (TL-01).
2. Confirm 2FA/hardware key on the release-publishing GitHub account (TL-02).
3. Pin CI actions to SHAs and untrack `.claude/settings.local.json` (TL-04).
