# 🛡️ ThreatLens Checklist — Enhanced YouTube Downloader

**Last updated:** 2026-11-25 after Security Audit 1 · Open: 4 (Critical 0, High 0, Medium 2, Low 2)

Legend: `[ ]` open · `[~]` in progress · `[x]` fixed and re-verified · `[!]` accepted risk (user's
words recorded) · `[?]` needs a decision. Never delete an item; move it to Done.

## 🔴 Critical
_(none)_

## 🟠 High
_(none)_

## 🟡 Medium
- [ ] **TL-20261125-01**: Build-time binary downloads aren't pinned or hash-verified · `src/Desktop/Download-JsRuntime.ps1:56`, `Download-FFmpeg.ps1:38,100` · [audit](November%202026/2026-11-25_Security_Audit_1.md) · proposed: pin versions + SHA-256, fail build on mismatch
- [ ] **TL-20261125-02**: Auto-update packages aren't signature-verified · `src/Desktop/Services/UpdateService.cs:17-22` · [audit](November%202026/2026-11-25_Security_Audit_1.md) · proposed: 2FA/hardware key on the publishing account; signed SHA-256 manifest

## 🟢 Low / hardening
- [ ] **TL-20261125-03**: Whisper model has no integrity check · `src/Core/Services/SubtitleGenerationService.cs` (`DownloadModelAsync`) · proposed: pin HF revision + SHA-256
- [ ] **TL-20261125-04**: CI actions pinned to tags; `.claude/settings.local.json` tracked · `.github/workflows/ci.yml` · proposed: pin to SHAs, untrack + gitignore

## 🧰 Deterministic guards to add
- [ ] SHA-256 verification inside each `Download-*.ps1`: prevents silently bundling a changed binary · from TL-01

## [?] Decisions needed
- TL-02: owner to confirm 2FA/hardware key on the release-publishing account.

## [!] Accepted risks
_(none)_

## ✅ Done
_(none yet)_
