---
phase: 01-mvp-rodando
plan: 01
subsystem: ui
tags: [html, css, javascript, static-frontend, responsive, problem-details]

# Dependency graph
requires:
  - phase: 01-mvp-rodando
    provides: 01-UI-REVIEW.md BLOCKER findings (error swallowing, invisible success message, mobile overflow)
provides:
  - request() parses RFC 7807 application/problem+json and surfaces body.detail / body.errors
  - Post-registration green success confirmation rendered on the visible login tab
  - No horizontal scroll at 375px with badge/heading/table-cell truncation
affects: [verify-work, future UI polish plans (placeholder contrast, tab semantics, empty state, font inheritance)]

# Tech tracking
tech-stack:
  added: [none — single static-file edit]
  patterns:
    - "Content-type detection uses includes('json') to match both application/json and application/problem+json"
    - "Flex overflow containment: min-width:0 + flex-wrap on container, max-width:100% + overflow:hidden + text-overflow:ellipsis + white-space:nowrap on flex children"
    - "overflow-wrap:anywhere on table cells for unbreakable email columns"

key-files:
  created: []
  modified:
    - src/DotnetUserManagementApi.Api/wwwroot/index.html

key-decisions:
  - "Content-type check matched with includes('json') so RFC 7807 ProblemDetails (application/problem+json) parse as JSON; 'Erro inesperado.' fallback retained only for non-JSON bodies (T-01-07 masks 500s server-side)"
  - "Register success message targeted #loginMessage after switchTab('login') — the now-visible form — and #registerMessage reset to empty"
  - "Mobile overflow fixed at 3 layers: .toolbar wraps (flex-wrap + min-width:0), .badge and .toolbar h2 truncate with ellipsis, table cells use overflow-wrap:anywhere"

patterns-established:
  - "Flex item truncation recipe: max-width:100%; overflow:hidden; text-overflow:ellipsis; white-space:nowrap"
  - "Message writes use textContent only (never innerHTML) — T-01-15 XSS mitigation preserved"

requirements-completed: [AUTH-01, AUTH-02, AUTH-03, USER-01, QUAL-02]

# Metrics
duration: 8min
completed: 2026-08-20
---

# Phase 01 Plan 01: Fix 3 UI BLOCKERs Summary

**Real API error messages surface in the login form (RFC 7807 problem+json parsing), the post-registration success confirmation renders visibly on the login tab, and the 375px viewport no longer scrolls horizontally (badge/heading/table truncation)**

## Performance

- **Duration:** 8 min
- **Started:** 2026-08-20T14:28:00Z
- **Completed:** 2026-08-20T14:36:00Z
- **Tasks:** 3
- **Files modified:** 1

## Accomplishments

- BLOCKER 1 closed: `request()` now matches `content-type` with `includes('json')`, so `application/problem+json` (RFC 7807) responses parse as JSON. Wrong-password login (401) renders **"E-mail ou senha inválidos."** in #loginMessage instead of the generic "Erro inesperado."; validation 400 messages surface; the generic fallback remains only for non-JSON bodies (masks 500s per T-01-07).
- BLOCKER 2 closed: `submitRegister()` calls `switchTab('login')` first, then writes the green **"Conta criada! Faça login para continuar."** to #loginMessage — the now-visible form — and resets #registerMessage so stale text does not reappear. Verified live: login tab active, message class `message success`, registerMessage empty.
- BLOCKER 3 closed: `.toolbar` wraps (`flex-wrap: wrap; min-width: 0; gap: 0.5rem`), `.badge` and `.toolbar h2` truncate with ellipsis (`max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap`). Live at 375x667 after login: `scrollWidth 375 <= 375` (was 472 pre-fix), badge truncated with ellipsis.
- No regressions: register → login → users list → logout flow re-verified live; `dotnet build` (solution/) 6 projects, 0 errors.

## Task Commits

Each task was committed atomically:

1. **Task 1: Surface real API error messages in request()** - `15022b6` (fix)
2. **Task 2: Render register success message on the login tab** - `41e05f2` (fix)
3. **Task 3: Eliminate mobile horizontal overflow (badge truncation)** - `3dc6c7e` (fix)

**Plan metadata:** `(pending)` (docs: complete plan)

## Files Created/Modified

- `src/DotnetUserManagementApi.Api/wwwroot/index.html` - The entire frontend (HTML/CSS/JS); all 3 blocker fixes applied here:
  - `request()` content-type check: `includes('application/json')` → `includes('json')` (line ~151)
  - `submitRegister()` success branch: switchTab first, write to #loginMessage, reset #registerMessage (lines ~175-180)
  - `.toolbar` gains `gap: 0.5rem; flex-wrap: wrap; min-width: 0;`; `.badge` gains truncation properties; new `.toolbar h2` truncation rule; `th, td` gains `overflow-wrap: anywhere` (lines ~84-86, ~81)

## Decisions Made

- **Content-type matching:** `includes('json')` over an explicit dual check — matches both `application/json` and `application/problem+json` in one expression; the API returns ProblemDetails for all 4xx/5xx (verified via curl: 401 → `Content-Type: application/problem+json`, `detail: "E-mail ou senha inválidos."`).
- **Success message placement:** written to #loginMessage after the tab switch because the register form is `display: none` after switching — the pre-fix code wrote to a hidden element (audit-verified: DOM text present, user saw nothing).
- **Truncation over scrolling:** badge/heading/table cells truncate or wrap instead of expanding the page — preserves single-column mobile layout.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Table-cell overflow containment added for the users table**
- **Found during:** Task 3 (Eliminate mobile horizontal overflow)
- **Issue:** The plan's specified CSS (`.toolbar` wrap + `.badge`/`.toolbar h2` truncation) fixed the badge but the acceptance criterion `scrollWidth <= 375` still failed: the users `<table>` rendered at 591px at a 375px viewport because the long unbreakable email in the E-mail column forces the table's min-content width beyond the card. Live measurement: `scrollWidth 632 > 375` with the toolbar already fixed. The plan's "Do NOT change other CSS" constraint was impossible to satisfy alongside its own no-horizontal-scroll acceptance criterion.
- **Fix:** Added `overflow-wrap: anywhere;` to the existing `th, td` rule — long emails wrap within the fixed table width instead of expanding it. Minimal single-property change; no other CSS touched.
- **Files modified:** src/DotnetUserManagementApi.Api/wwwroot/index.html (`th, td` rule)
- **Verification:** Live at 375x667 after login with a 42-char email: `scrollWidth 375 <= 375` (no horizontal scroll), table width 293px (fits card), badge still truncated with ellipsis. Static grep gates unchanged.
- **Committed in:** 3dc6c7e (Task 3 commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** The auto-fix was required to satisfy the plan's own acceptance criterion (no horizontal scroll at 375px). No scope creep — single CSS property on an existing rule.

## Issues Encountered

- **Stale browser cache during live verification:** the first wrong-password login test after Task 1 showed "Erro inesperado." because the browser served the pre-edit page from cache. Confirmed the server was serving the new file via fetch of `/` (contains `includes('json')`), then re-ran the test with a cache-busted URL (`?v=2`) — message correctly showed "E-mail ou senha inválidos.".
- **Plan's Task 2 verify command is not function-scoped:** the awk ordering gate matches *any* `switchTab('login')` occurrence (3 in the file: HTML onclick, submitRegister, logout), so it compares line 249 (logout) against line 177 and always fails. A function-scoped awk (scoped to `submitRegister`) proves the real gate: `switchTab('login')` at line 175 < `loginMessage.className = 'message success'` at line 177 — PASS. The plan's intended ordering is correctly implemented; the verify script needs scoping if reused.
- **Pre-existing cosmetic quirk (out of scope):** after logout, #loginMessage retains stale text ("Autenticando..." from the last login attempt) because logout() does not clear it. Not flagged by the audit, not part of this plan's 3 blockers — logged for future UI polish.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All 3 BLOCKERs from 01-UI-REVIEW.md closed; UAT flows re-verified live with no regressions (register, login, users list, logout).
- Ready for plan 02 of phase 01-mvp-rodando.
- Future UI polish backlog (from audit, not in this plan): placeholder `::placeholder` contrast, tab semantics (`role=tablist/tab`/`aria-selected`), empty-state message for zero users, `font-family: inherit` on form controls, focus-visible styling, disabled/submitting state on submit buttons.

---
*Phase: 01-mvp-rodando*
*Completed: 2026-08-20*

## Self-Check: PASSED

- File exists: `.planning/phases/01-mvp-rodando/01-01-SUMMARY.md`
- Commits exist: `15022b6` (Task 1), `41e05f2` (Task 2), `3dc6c7e` (Task 3)
- All 3 static gates pass (grep counts / scoped awk ordering)
- Live re-test passed: 401 message, register confirmation, 375px no-horizontal-scroll, logout regression
- `dotnet build` (solution/): 6 projects, 0 errors, 0 warnings