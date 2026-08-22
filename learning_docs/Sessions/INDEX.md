# Sessions Index

**Last Updated**: 2026-08-22 19:10 · **Total Logged Here**: 1

> Full session history (Sessions 1–37) lives in [`guides-and-instructions/chats/`](../../guides-and-instructions/chats/) and its [SESSION_INDEX.md](../../guides-and-instructions/chats/SESSION_INDEX.md). This index tracks sessions logged through the universal-session-learner workflow.

---

| Date | # | Title | Status | Summary | Link |
|---|---|---|---|---|---|
| 2026-08-22 | 37 | Critical 403 Fix — Bundled JS Runtime & Release v0.4.6 | ✅ Complete | Every download app-wide failed with HTTP 403 because yt-dlp now needs a JavaScript runtime to solve YouTube's EJS player challenges. Bundled QuickJS (2 MB, chosen over Deno's 97 MB), updated yt-dlp to 2026.08.19, wired up startup self-update, released v0.4.6. User-confirmed working. | [Session 37](../../guides-and-instructions/chats/Session_37_Critical_403_Fix_JS_Runtime_Release_v0.4.6.md) |

---

## Open Follow-Ups

Carried from the most recent session:

1. `.github/workflows/ci.yml` is untracked — needs `gh auth refresh -s workflow` before it can be committed.
2. CLAUDE.md release checklist references a hardcoded version at `SettingsDialog.axaml:406` that no longer exists (now a dynamic binding).
3. `docs/index.html:326` has a stale `<!-- NEW FEATURES (v0.4.1) -->` comment.
4. Non-YouTube platforms not yet explicitly verified against the v0.4.6 fix.
