# HealthCheck: download smoke test

A small console app that runs the app's **real** `YtDlpDownloadService` against the platforms in
[`platforms.json`](platforms.json). It downloads **MP4** and **MP3** for each one and checks every
file with the bundled ffmpeg.

The monthly scheduled task uses it (see [`Scheduled Logs/README.md`](../../Scheduled%20Logs/README.md)).
You can also run it yourself after any change to the download pipeline.

## Run

```powershell
dotnet run -c Release --project tools/HealthCheck -- --out "$env:TEMP\eyd-hc"
```

| Option | Meaning |
|---|---|
| `--out <folder>` | Where `results.json` and `results.md` are written (media files are deleted afterwards) |
| `--cookies <path>` | Optional cookies.txt for sign-in walls (only the path is passed to yt-dlp; contents are never read) |
| `--only <id>` | Test one platform by its `id` in platforms.json |
| `--timeout-minutes <n>` | Per-download timeout (default 5) |

Exit code `0` means every `"expected": "pass"` platform passed, `1` means at least one regression, and `2` means a setup problem.

## Reading the results

| Outcome | Meaning |
|---|---|
| ✅ PASS | Downloaded, and ffmpeg confirmed video+audio (MP4) or MP3 audio |
| ❌ FAIL | A platform that should work failed: an **app or yt-dlp problem to fix** |
| ⚠️ SITE-LIMITED | Expected limit (login wall, DRM, geo block): **report only, don't "fix"** |

## Maintaining it

- A test URL was deleted? Swap in another public video from the same platform. Don't mark the platform failed.
- A platform starts requiring login? Change `expected` to `site-limited` and note why in the findings.
- It's not part of `EnhancedYoutubeDownloader.sln` on purpose, so it can't break the app build or CI.
