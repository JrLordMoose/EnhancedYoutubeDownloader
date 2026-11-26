# 🗓️ Scheduled Logs

Findings from the **monthly health check**, an automated Claude task that runs on the **last day of
every month at 9:00 AM** (task `eyd-monthly-health-check`, under **Scheduled** in the Claude app).

**Start here:** [INDEX.md](INDEX.md) has one line per run, newest first.

## What each run does
1. Updates the bundled yt-dlp if a newer one is out, builds the app and runs every unit test.
2. Runs [`tools/HealthCheck`](../tools/HealthCheck/README.md): real MP4 and MP3 downloads on every supported platform, each file checked with ffmpeg.
3. Audits every setting: shown in the dialog, actually used, restored by Cancel, cleared by Reset All.
4. Checks the release channel: the latest GitHub release has both the installer EXE and the auto-update ZIP, and the landing-page download links work.
5. **If something is broken:** subagents fix it. It releases a patch (`vX.Y.Z+1`) to `JrLordMoose/EnhancedYoutubeDownloader` **only** when the build, all tests and the re-run health check pass.
6. Writes a findings document (below), adds a line to INDEX.md, writes a session log when anything changed, then notifies you in Claude and emails team@thiinkmediagraphics.com.

## Folder layout
```
Scheduled Logs/
├── README.md                         ← this file
├── INDEX.md                          ← one row per run (newest first)
├── TEMPLATE_Findings.md              ← the shape every findings document follows
└── <YYYY>/
    ├── YYYY-MM-DD_Health_Check.md     ← human-readable findings for that run
    └── YYYY-MM-DD_results.json        ← raw HealthCheck output (for trends and diffs)
```

## Outcomes
| Badge | Meaning | Pushed? |
|---|---|---|
| ✅ All passed | Nothing broken; only this log was committed | Log only |
| 🔧 Fixed & released | Something broke, was fixed, verified and shipped as a new patch | Code + release |
| ⚠️ Needs attention | Something broke and couldn't be fixed safely, so nothing was released | Log only |
| ⏭️ Skipped | Run aborted early (dirty working tree, offline, GitHub sign-in missing) | Log only, if possible |

## Things the check never does
- Release unless the fix is verified, or weaken or delete tests to make them pass.
- "Fix" site-side limits (Instagram/Reddit/Vimeo login walls, DRM, geo blocks). These are reported only.
- Read, print or commit your `cookies temp/` file (only its path goes to yt-dlp), or push `.github/workflows`.

## Related
- Session logs: [`learning_docs/Sessions/INDEX.md`](../learning_docs/Sessions/INDEX.md)
- Lessons learned: [`learning_docs/Context_Learning/INDEX.md`](../learning_docs/Context_Learning/INDEX.md)
- Security items: [`Security Audits/CHECKLIST.md`](../Security%20Audits/CHECKLIST.md)
