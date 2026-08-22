# Learning Session: ~10s Download Startup Is Inherent to yt-dlp, Not a Bug

**Date**: 2026-08-22
**Category**: process
**Severity**: Medium
**Auto-Triggered**: Yes — user reported a perceived slowdown; benchmarked before accepting or "fixing" it

## 📝 Summary

After the v0.4.6 JS-runtime fix, downloads take **10–20 seconds to start**. This is expected and inherent to yt-dlp's current YouTube extraction pipeline. It is **not** a regression, and **not** a cost of bundling QuickJS instead of Deno.

## 💡 Lesson

The app previously felt instant because it was **instantly failing** with a 403. Once extraction actually works, the mandatory JS-challenge pipeline costs ~10s. A latency increase that appears alongside a bug fix is not automatically caused by that fix — measure before attributing.

Critically: **QuickJS and Deno were within ~50ms of each other.** The 2 MB runtime is not slower than the 97 MB one. Had this been assumed rather than measured, the obvious "fix" would have been to swap in Deno — adding 95 MB to the installer for zero gain.

## ✅ Correction (The Rule)

- **Never treat ~10s download startup as a bug.** Benchmark before investigating; the baseline is ~10.5–11s.
- **Never swap QuickJS for Deno to chase speed.** Measured difference is ~50ms. The only thing Deno buys is 95 MB of installer.
- **Never pin `youtube:player_client` for performance.** Measured: no speed gain, and it actively degrades output (`web` → 360p, `web_safari` → single HLS format, `tv` → hard error).
- **Always measure before attributing a slowdown to a recent change** — especially when the change turned a fast failure into a slower success.

## 🔍 Context

- **Benchmarked on**: `dQw4w9WgXcQ`, format `bv*+ba/b`, yt-dlp `2026.08.19`

| Scenario | Time |
|---|---|
| yt-dlp bare startup (`--version`) | 1.9s |
| Full extraction — QuickJS, cold cache | 10.74s |
| Full extraction — Deno, cold cache | 10.79s |
| Full extraction — warm cache | 10.5s |
| Actual file transfer (11 MB) | <1s |

- **Where the time goes** (sequential yt-dlp round-trips):
  1. `Downloading webpage`
  2. `Downloading visionos player API JSON`
  3. `Downloading m3u8 information`
  4. `Downloading player <id>-main` (~2 MB of player JS)
  5. `Solving JS challenges using quickjs`
  - Plus ~1.9s PyInstaller self-extraction per `yt-dlp.exe` invocation.
- **Challenge solving re-runs per video.** The `youtube-sigfuncs` cache does not eliminate it — confirmed by identical cold vs warm timings.
- **Rejected optimisation**: `--extractor-args youtube:skip=hls` saved ~0.9s (near noise) but adds a knob that risks live-stream and subtitle edge cases. Not shipped.
- **Only real lever if this ever matters**: eliminate the ~1.9s PyInstaller startup by invoking yt-dlp as a Python library rather than the one-file exe. Swapping the JS runtime will not help.

## 🔗 Related Learnings

- [yt-dlp Requires a Bundled JavaScript Runtime](2026-08-22_Context_Session_37.md) — the fix that introduced this (worthwhile) latency.
