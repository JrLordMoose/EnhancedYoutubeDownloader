# Learning Session: Download Startup Latency — Measure Phases, Not Wall-Clock

**Date**: 2026-08-22 (revised same day after a measurement error)
**Category**: process
**Severity**: Medium
**Auto-Triggered**: Yes — user reported downloads felt slow; first benchmark reached a wrong conclusion and had to be redone

## 📝 Summary

After the v0.4.6 JS-runtime fix, the first download takes ~12–16s before transfer begins. Investigated, got it **wrong the first time**, then re-measured correctly.

**Corrected finding:** QuickJS is **~5.6 seconds slower than Deno** at solving the YouTube JS challenge (~6.9s vs ~1.3s, consistent across 3 alternating runs). However, the solve is **cached per player version**, not run per video — so this cost is paid only on the first download after YouTube rotates its player (every few days/weeks), not on every download.

**Decision: keep QuickJS.** Deno would save ~5.6s occasionally in exchange for +95 MB of installer permanently. Bad trade.

## ⚠️ The Measurement Error (the real lesson)

**First (wrong) method**: timed total wall-clock of two `--simulate` runs. Got QuickJS 10.74s vs Deno 10.79s and concluded they were equivalent (~50ms apart). This was written up as a rule: *"never swap runtimes to chase speed."*

**Why it was wrong**: total wall-clock is dominated by variable network round-trips. The ~5.6s runtime difference was buried inside ~10s of noisy network time. Two single runs that happened to land near each other looked like equivalence.

**Correct method**: timestamp yt-dlp's own progress lines and measure the interval between `Solving JS challenges` and `Downloading N format(s)`. Repeat, alternating runtimes, with a cleared cache each iteration.

| Run | QuickJS solve | Deno solve |
|---|---|---|
| 1 | 6776 ms | 1267 ms |
| 2 | 7031 ms | 1214 ms |
| 3 | 6806 ms | 1328 ms |

Corroborating signal that should have prompted suspicion earlier: a run with subtitles and tags **enabled** (16.9s) finished *faster* than one with them disabled (22.9s). When an added workload appears to speed things up, the measurement is noise-dominated — stop and change methodology.

## ✅ Correction (The Rule)

- **Never compare two implementations by total wall-clock when network I/O dominates.** Isolate the phase under test. Here: timestamp log lines and diff the relevant interval.
- **Treat a nonsensical result as a methodology failure, not a finding.** "Adding work made it faster" means the instrument is wrong.
- **Run ≥3 alternating iterations** before concluding anything about a difference smaller than observed run-to-run variance.
- **Keep QuickJS despite being ~5.6s slower.** The solve is cached per player version, so the cost is occasional; Deno costs +95 MB permanently. Revisit only if YouTube starts rotating players daily.
- **Do not "fix" steady-state download speed by swapping the JS runtime** — in steady state the solver does not run at all.

## 🔍 Context — Real Phase Breakdown

Timestamped, full app flag set, cold cache:

| Phase | Elapsed | Duration |
|---|---|---|
| yt-dlp PyInstaller startup | 0 → 1945 ms | **1.9s** |
| Webpage + player API + m3u8 | 1945 → 3553 ms | 1.6s |
| **JS challenge solve (QuickJS)** | 3553 → 11536 ms | **8.0s** |
| Subtitle + format selection | 11536 → 11831 ms | 0.3s |
| File transfer (29 MB + 3 MB) | 11831 → 15754 ms | 3.9s |
| Merge + embed subs/metadata | 15886 → 16245 ms | 0.4s |

**Cache behaviour** (the key mitigating fact):

| Video | Total | Solver invoked? |
|---|---|---|
| 1st (cold) | 12271 ms | ✅ yes |
| 2nd | 8351 ms | ❌ no |
| 3rd | 5862 ms | ❌ no |

Cache is persistent at `~/.cache/yt-dlp` (the app passes no `--cache-dir`), so this survives app restarts.

**Practical user experience:**
- First download after a YouTube player rotation: **~12–16s**
- Every download after that: **~6–8s**

**Not causes** (ruled out by measurement):
- Browser cookies — user's setting is `UseBrowserCookies: false`
- Subtitles/tags/thumbnail embedding — measured within noise
- Player-client pinning — no speed gain, and degrades quality (`web` → 360p, `tv` → error)

**Only remaining lever worth considering**: the ~1.9s PyInstaller startup, paid on *every* download (unlike the solve). Would require invoking yt-dlp as a Python library instead of the one-file exe.

## 🔗 Related Learnings

- [yt-dlp Requires a Bundled JavaScript Runtime](2026-08-22_Context_Session_37.md) — the fix that introduced this latency.
