# Context Learning Index

**Last Updated**: 2026-08-22 19:10 · **Total**: 1 · **Critical**: 1

---

## 🔴 Critical Learnings (Always Apply)

| Date | Title | Category | Key Takeaway |
|---|---|---|---|
| 2026-08-22 | [yt-dlp Requires a Bundled JavaScript Runtime](August%202026/2026-08-22_Context_Session_37.md) | build | A yt-dlp `403` is a **missing JS runtime** until proven otherwise. Run the bundled `yt-dlp.exe` manually and read stderr before touching C# code. Ship QuickJS alongside it. |

---

## 📅 Recent (Last 7 Days)

| Date | Title | Category | Key Takeaway |
|---|---|---|---|
| 2026-08-22 | [yt-dlp Requires a Bundled JavaScript Runtime](August%202026/2026-08-22_Context_Session_37.md) | build | Also: update-yt-dlp and add-JS-runtime must be tested as **separate variables**; updating alone produces a convincing false fix. |

---

## 📁 By Category

### Build

- [2026-08-22 — yt-dlp Requires a Bundled JavaScript Runtime](August%202026/2026-08-22_Context_Session_37.md)
  - Bundle QuickJS (~2 MB), not Deno (~97 MB)
  - CSharpier formats `.csproj` too — keep `<None>` entries on one line
  - Repo is **CRLF**; LF writes fail the build
  - GitHub token lacks `workflow` scope — isolate `.github/workflows/**` changes
  - Never silent-install the built EXE to verify (shared `AppId` overwrites the real install)

### API

_(none yet)_

### Auth

- See the **cross-reference warning** in the build learning: Session 34's cookie/auth 403 fix is a *different* bug from the Session 37 JS-runtime 403. Same symptom, unrelated cause.

### Database / UI / Testing / Config / Process

_(none yet)_

---

## 📁 Problem Areas

| Path | Known Issue |
|---|---|
| `src/Core/Services/YtDlpDownloadService.cs` | External runtime dependency (JS engine). Downloads fail app-wide with a misleading 403 if `qjs.exe` is missing from the output directory. |
| `src/Desktop/yt-dlp.exe` | Goes stale and breaks against YouTube within months. Startup self-update now mitigates this — do not remove it. |
| `.github/workflows/` | Pushes rejected — token lacks `workflow` scope. |

---

## 📈 Statistics

Most common category: `build` (1) · Problem areas tracked: 3 · Last self-diagnosis: n/a
