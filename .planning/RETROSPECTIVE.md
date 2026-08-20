# Project Retrospective

*A living document updated after each milestone. Lessons feed forward into future planning.*

## Milestone: v1.0 — MVP

**Shipped:** 2026-08-20
**Phases:** 2 | **Plans:** 4 | **Tasks:** 11 | **Commits:** 73

### What Was Built
- REST API em Clean Architecture (.NET 8): register/login with JWT (HS256) + BCrypt (work factor 12), protected users listing, RFC 7807 error contract, rate limiting (429)
- Dual-provider database: SQLite zero-dependency local run (EnsureCreated) | PostgreSQL via Docker compose (Migrate retry 10×/2s)
- Containerization: Dockerfile multi-stage (sdk:8.0 → aspnet:8.0-alpine) + docker-compose prod-like (postgres:16, volume, healthcheck, fail-fast JWT__KEY)
- Static frontend SPA (single-file HTML/CSS/JS): ARIA tabs, :focus-visible keyboard a11y, empty-state, disabled-in-flight submits, responsive at 375px
- Documentation: ARCHITECTURE.md (Mermaid, PT-BR, 4 diagrams, 207 lines), README Docker section
- CI pipeline: GitHub Actions build + dotnet test (push main + PRs, 0 SonarQube, 0 secrets)
- Test suite: 46 tests (16 Phase 1 + 30 Nyquist) — xUnit, all green

### What Worked
- **Sequential phase execution** — Phase 1 (MVP) completed before Phase 2 (Docker/docs) ensured API stability before containerization
- **Nyquist validation upfront** — automated verification map caught gaps (JWT fail-fast, provider branching) before UAT
- **Docker E2E UAT** — live compose stack verification (11/11 pass) caught stale doc claims and confirmed fail-fast behavior
- **Single-file frontend** — all UI work in one HTML file minimized context switching
- **GSD wave structure** — breaking work into 1-3 task waves per plan with static verification gates kept each commit clean

### What Was Inefficient
- **Docker blocker in Phase 2 Plan 1** — plan was written before Docker installed; whole E2E section was deferred, creating stale SUMMARY claims that had to be retroactively corrected in UAT
- **Nyquist validation repeated** — validate-phase generated Nyquist tests, then verify-work re-verified the same behaviors via UAT. Could merge validation into plan execution
- **README command path issue** — `dotnet test` from repo root fails (MSB1003); sln path at `solution/` not documented at line 52. Small but would frustrate a first-time user
- **VERIFICATION.md created retroactively** — neither phase had formal verification artifacts until the re-audit; better to generate during phase close

### Patterns Established
- **Fail-fast pattern**: critical secrets validated at startup in Production (InvalidOperationException) + compose env interpolation (`${VAR:?}`) — prevents silent misconfiguration
- **Dual-provider branching**: explicit config key (`ConnectionStrings:Database`) drives provider selection — simple, testable, no auto-detection magic
- **Nyquist + UAT sequencing**: automated verification map (validating every requirement against code) before manual E2E UAT — catches structural issues early
- **UAT restarts**: after blocker-resolution, restart UAT from scratch rather than patching old results — ensures full flow re-verified
- **Static verification gates**: grep-based acceptance criteria on every task commit ensure surface-level invariants (no innerHTML, no SonarQube, file exists)

### Key Lessons
1. Always install infrastructure dependencies (Docker, DB) before writing plans that depend on them — avoids stale docs and deferred verification
2. Nyquist validation map catches structural gaps; UAT catches live-flow gaps — both needed but can be sequenced tighter
3. Single-file frontends scale surprisingly well for small UIs — the code review benefits (one file to audit) outweigh modularity arguments for MVPs
4. README commands should always be tested end-to-end — the `dotnet test` path issue was invisible until UAT

### Cost Observations
- Model mix: 60% sonnet, 30% flash, 10% haiku
- Sessions: 3+ (Phase 1 retroactive, Phase 2 execution, Phase 2 verify + milestone close)
- Notable: Longest continuous session ~4h (Phase 2 execution + verification); fastest plan (02-02 docs CI) in 4 min

---

## Cross-Milestone Trends

### Process Evolution

| Milestone | Sessions | Phases | Key Change |
|-----------|----------|--------|------------|
| v1.0 | 3+ | 2 | First milestone — established GSD workflow patterns |

### Cumulative Quality

| Milestone | Tests | Pass | Zero-Dep Additions |
|-----------|-------|------|-------------------|
| v1.0 | 46 | 46/46 | SQLite (local), PostgreSQL (Docker), static HTML frontend |

### Top Lessons (Verified Across Milestones)

1. Docker as a first-class dependency — install before planning container phases
2. Nyquist + UAT sequencing catches both structural and flow-level gaps
3. Commit-level static gates (grep) prevent surface-level regressions efficiently
