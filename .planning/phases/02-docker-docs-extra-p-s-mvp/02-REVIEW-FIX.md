---
phase: 02-docker-docs-extra-p-s-mvp
fixed_at: 2026-08-20T02:20:00Z
review_path: .planning/phases/02-docker-docs-extra-p-s-mvp/02-REVIEW.md
iteration: 1
findings_in_scope: 4
fixed: 4
skipped: 0
status: all_fixed
---

# Phase 2: Code Review Fix Report

**Fixed at:** 2026-08-20T02:20:00Z
**Source review:** `.planning/phases/02-docker-docs-extra-p-s-mvp/02-REVIEW.md`
**Iteration:** 1

**Summary:**
- Findings in scope: 4 (fix_scope: critical_warning)
- Fixed: 4
- Skipped: 0

## Fixed Issues

### CR-01: Placeholder/default JWT signing key accepted in Production — token forgery

**Files modified:** `src/DotnetUserManagementApi.Api/Program.cs`
**Commit:** fd8274a
**Applied fix:** Added a fail-fast `else if` branch to the JWT key validation in `Program.cs` (following the reviewer's suggested code). Outside Development, the startup now rejects the publicly-versioned placeholder key (`.env.example` value, matched by `Contains("changeme", OrdinalIgnoreCase)`) and any key shorter than 32 UTF-8 bytes (`Encoding.UTF8.GetByteCount < 32`), throwing `InvalidOperationException` before authentication is configured. Development keeps the existing random-key generation path, and the placeholder remains usable for local testing.

**Status:** `fixed: requires human verification` — introduces new conditional startup logic (key-strength + placeholder rejection); logic confirmed by full build (`0 errors, 0 warnings`) and all 12 tests passing, but the security semantics warrant a human confirmation before the phase is verified.

**Note:** The reviewer's optional "consider" to also reject `POSTGRES_PASSWORD=changeme` in compose/docs was left as follow-up documentation work — the db port is not published (internal network only), so the practical exposure is lower; the primary CR-01 token-forgery vector is closed by the JWT key guard.

### WR-01: Docker final image runs as root

**Files modified:** `Dockerfile`
**Commit:** 87db56f
**Applied fix:** Added `USER $APP_UID` before the `ENTRYPOINT` in the final stage. The `aspnet:8.0-alpine` base image defines `APP_UID` (UID 1654, non-root `app` user), so the API now runs as a non-root user, reducing the blast radius of a container compromise.

### WR-02: CI workflow runs with overprivileged default GITHUB_TOKEN

**Files modified:** `.github/workflows/ci-cd.yml`
**Commit:** 0b1c795
**Applied fix:** Added workflow-level `permissions: contents: read` (with a comment referencing T-02-09), so the build-only job's `GITHUB_TOKEN` carries the minimum scope it needs and cannot push commits or tamper with tags if a step is compromised. YAML validated.

### WR-03: `.dockerignore` does not exclude the local SQLite DB — dev PII enters build context

**Files modified:** `.dockerignore`
**Commit:** 5efb737
**Applied fix:** Added `**/app.db` and `**/*.db` to `.dockerignore` with a comment matching the file's security intent (T-02-03), so the gitignored local development database (user emails + BCrypt hashes) no longer enters the Docker build context or build-stage layers.

---

_Fixed: 2026-08-20T02:20:00Z_
_Fixer: the agent (gsd-code-fixer)_
_Iteration: 1_