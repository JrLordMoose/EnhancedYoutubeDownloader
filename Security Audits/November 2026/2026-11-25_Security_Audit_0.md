# 🛡️ Security Audit 0: Install & baseline profile

| | |
|---|---|
| **Date** | November 25, 2026 (2026-11-25) |
| **Time** | 19:10–19:25 EST |
| **Requested as** | "add universal session learner and threatlens if not already added" → Flow A |

## Choices the owner made
- **Setup:** "Install + baseline audit (Recommended)": profile plus an audit folder matching the
  `learning_docs` convention, then a one-pass, code-only baseline that fixes only Critical/High.
- **Audit folder:** `Security Audits/<Month YYYY>/YYYY-MM-DD_Security_Audit_<N>.md` (matches
  `learning_docs/Sessions/<Month YYYY>/YYYY-MM-DD_Session_<N>.md`).
- **Deep research:** not requested. Two stack facts were reproduced locally and recorded in profile §10.
- **Deterministic tooling / CI jobs:** not installed this run (nothing added without a yes).
- **Secret management:** not needed; the app has no secrets (profile §7b).
- **CLAUDE.md section:** offered, not applied.

## Files written
- `.claude/security/profile.md`
- `Security Audits/INDEX.md`, `Security Audits/CHECKLIST.md`
- `Security Audits/November 2026/2026-11-25_Security_Audit_0.md` (this file), `…_Audit_1.md` (baseline)
