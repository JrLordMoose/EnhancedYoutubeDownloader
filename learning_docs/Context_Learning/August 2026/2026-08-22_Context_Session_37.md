# Learning Session: yt-dlp Requires a Bundled JavaScript Runtime

**Date**: 2026-08-22
**Category**: build
**Severity**: Critical
**Auto-Triggered**: Yes — total feature outage (every download failing) traced to an undocumented external dependency

## 📝 Summary

Every download in the app failed with `HTTP Error 403: Forbidden`, on every video and every supported site. The cause was not authentication, cookies, or URL parsing — yt-dlp now requires a **JavaScript runtime** to solve YouTube's EJS player challenges, and the app shipped without one.

## 💡 Lesson

yt-dlp is not a self-contained dependency. It has an **external runtime requirement** (a JS engine) that appeared after the version we bundled, and it degrades *silently* — it emits a warning, then falls back to player clients that produce URLs YouTube rejects. The visible symptom (403) points at auth; the actual cause is a missing binary.

Equally important: **a stale bundled binary is a time bomb.** Pinning yt-dlp at build time guarantees breakage within months, because YouTube changes its player constantly. Ours was 5 months old.

## ✅ Correction (The Rule)

- **Always ship a JavaScript runtime alongside yt-dlp.** The app bundles QuickJS (`qjs.exe`, ~2 MB) and registers it with `--js-runtimes quickjs:<path>` in `ApplyCompatibilityOptions()`.
- **Never diagnose a yt-dlp 403 as an auth problem before checking for a JS runtime warning.** Run the bundled binary directly and read stderr first.
- **Always test "update yt-dlp" and "add JS runtime" as separate variables.** Updating yt-dlp alone did *not* fix this and would have produced a confident, wrong fix.
- **Never bundle Deno for this purpose without weighing size.** Deno is ~97 MB and would have more than doubled the installer. QuickJS is ~2 MB and yt-dlp supports it natively.
- **CRITICAL REMINDER**: If downloads break app-wide again, run the bundled `yt-dlp.exe` manually first. The stderr warning names the true cause in one line. Do not start from the C# code.

## 🔍 Context

- **File**: `src/Core/Services/YtDlpDownloadService.cs` — `ApplyCompatibilityOptions()`
- **Error Message**: `ERROR: unable to download video data: HTTP Error 403: Forbidden`
- **Preceding warning (the real clue)**: `WARNING: [youtube] No supported JavaScript runtime could be found. Only deno is enabled by default; to use another runtime add --js-runtimes RUNTIME[:PATH]`
- **Secondary symptom**: after updating yt-dlp but still without a runtime, the error changes to `Requested format is not available` — a *different* misleading message with the *same* root cause.
- **Solution**:
  1. Bundle QuickJS via `src/Desktop/Download-JsRuntime.ps1` + `DownloadJsRuntime` MSBuild target
  2. Register it with `--js-runtimes quickjs:<path>`
  3. Update bundled yt-dlp to `2026.08.19`
  4. Wire up `YtDlpSelfUpdater` (previously dead code) to run `yt-dlp -U` on startup
- **Supported runtimes** (from yt-dlp): `deno`, `node`, `bun`, `quickjs`
- **Related Files**: `src/Desktop/Download-JsRuntime.ps1`, `src/Desktop/EnhancedYoutubeDownloader.csproj`, `src/Core/Services/YtDlpSelfUpdater.cs`, `src/Desktop/App.axaml.cs`

## 🧷 Secondary Learnings

- **CSharpier formats `.csproj`, not just `.cs`.** A multi-line `<None>` element failed the build with `error : Was not formatted`. Keep such entries on one line.
- **This repo is CRLF.** Writing files with LF fails the build with `"The file contained different line endings than formatting it would result in."` Normalize to CRLF after scripted edits.
- **The GitHub token has `repo` but not `workflow` scope.** Any commit touching `.github/workflows/**` will be rejected at push. Fix with `gh auth refresh -s workflow`, or keep workflow changes in a separate commit.
- **Never silent-install a built installer to verify contents.** It shares an `AppId` with the user's real install and would overwrite it. Verify via the Inno Setup compiler's file list or by inspecting the ZIP.

## 🔗 Related Learnings

- Session 34 (`Browser_Cookie_Auth_403_Auto_Enable`) fixed a **different** 403 — that one was genuinely cookie/auth related. **Same symptom, unrelated cause.** Do not assume the Session 34 fix applies to a new 403 report.
