# Sessions Index

**Last Updated**: 2026-11-25 20:25 · **Total Logged Here**: 3

> Full session history (Sessions 1–37) lives in [`guides-and-instructions/chats/`](../../guides-and-instructions/chats/) and its [SESSION_INDEX.md](../../guides-and-instructions/chats/SESSION_INDEX.md). This index tracks sessions logged through the universal-session-learner workflow.

---

| Date | # | Title | Status | Summary | Link |
|---|---|---|---|---|---|
| 2026-11-25 | 39 | QC Sweep, Sign-in Support, Settings Repair, Release v0.4.8 | ✅ Complete | cookies.txt sign-in + prompt; every setting wired; 4 QC subagents (design, settings, downloads, bug hunt) with fixes; H.264 MP4; pause/collision fixes; auto-update dialog; ThreatLens installed; 95 tests. | [Session 39](November%202026/2026-11-25_Session_39.md) |
| 2026-09-02 | 38 | Repair and Release YouTube Playlist Downloads | Complete | Fixed the empty playlist dialog and completed selection, settings, folder, cancel, and selected-only queue behavior; released v0.4.7. | [Session 38](September%202026/2026-09-02_Session_38.md) |
| 2026-08-22 | 37 | Critical 403 Fix — Bundled JS Runtime & Release v0.4.6 | ✅ Complete | Every download app-wide failed with HTTP 403 because yt-dlp now needs a JavaScript runtime to solve YouTube's EJS player challenges. Bundled QuickJS (2 MB, chosen over Deno's 97 MB), updated yt-dlp to 2026.08.19, wired up startup self-update, released v0.4.6. User-confirmed working. Also benchmarked download latency: first download ~12–16s, then ~6–8s. QuickJS is ~5.6s slower than Deno at the challenge solve (an initial wall-clock benchmark wrongly concluded they were equal), but the solve is cached per player version, so QuickJS stays. | [Session 37](../../guides-and-instructions/chats/Session_37_Critical_403_Fix_JS_Runtime_Release_v0.4.6.md) |

---

## Open Follow-Ups

Carried from the most recent session:

1. `.github/workflows/ci.yml` is still untracked: needs `gh auth refresh -s workflow` on JrLordMoose before it can be pushed.
2. ~~CLAUDE.md SettingsDialog:406 reference~~: fixed in Session 39.
3. ~~Stale v0.4.1 comment in docs/index.html~~: fixed in Session 39.
4. ~~Non-YouTube platforms unverified~~: Session 39 QA verified MP4+MP3 on 9 platforms (Instagram/Reddit/Vimeo need login).
5. Real pause/resume (.part continuation) and bundling ffprobe.exe.
6. ThreatLens open items TL-20261125-01..04 (`Security Audits/CHECKLIST.md`).
7. Optional perf: yt-dlp as a Python library to cut ~1.9s PyInstaller startup.
