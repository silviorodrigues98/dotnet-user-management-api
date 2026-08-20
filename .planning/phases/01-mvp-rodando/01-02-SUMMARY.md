---
phase: 01-mvp-rodando
plan: 02
subsystem: ui
tags: [html, css, javascript, accessibility, aria, focus-visible, contrast, empty-state]

# Dependency graph
requires:
  - phase: 01-mvp-rodando
    provides: 01-01 blocker fixes (request() RFC 7807 parsing, register success message, 375px overflow) — all kept intact as regression baseline
provides:
  - ARIA tab semantics: role=tablist/tab/tabpanel, aria-controls, aria-selected toggling in switchTab()
  - Keyboard focus always visible via :focus-visible outline (input/tab/button); outline:none removed
  - Form controls inherit system-ui font (no more Arial); ::placeholder slate-400 #94a3b8 (AA 4.5:1+)
  - Empty users list renders centered colspan=3 'Nenhum usuário cadastrado.' row
  - Register/login submit buttons disable in-flight and re-enable in finally (no double-submit)
  - Browser tab title 'Gerenciamento de Usuários' replaces repo slug
affects: [verify-work, future UI polish, accessibility audits]

# Tech tracking
tech-stack:
  added: [none — single static-file edit]
  patterns:
    - "ARIA tabs: role=tablist container, role=tab buttons with aria-controls pointing at role=tabpanel forms, aria-selected toggled via setAttribute in switchTab()"
    - "Focus visibility: outline: none removed from input:focus; :focus-visible outline added for input/tab/button (2px #60a5fa + offset 2px)"
    - "Disabled-in-flight submit: btn.disabled = true before await request(...), re-enabled in finally — covers success and error paths"
    - "XSS-safe DOM writes: every new node built with document.createElement + textContent (zero innerHTML in file, T-01-15/T-01-20)"

key-files:
  created: []
  modified:
    - src/DotnetUserManagementApi.Api/wwwroot/index.html

key-decisions:
  - "Empty-state colspan written via td.setAttribute('colspan','3') instead of td.colSpan = 3 — the plan's grep gate is case-sensitive ('colspan') and colSpan would not match; attribute is XSS-safe (T-01-20)"
  - "Existing tbody.innerHTML = '' replaced with tbody.textContent = '' — the plan's hard gate (! grep innerHTML) and its premise 'file has zero innerHTML usages' were factually wrong (one usage at old line 216); textContent clearing is behavior-identical and keeps the file innerHTML-free"
  - "Placeholder color #94a3b8 (slate-400) with opacity: 1 — ~7:1 contrast on #0f172a, exceeds AA 4.5:1; opacity 1 compensates the Firefox placeholder-opacity quirk"

requirements-completed: [AUTH-01, AUTH-02, USER-01, QUAL-02]

# Metrics
duration: 10min
completed: 2026-08-20
---

# Phase 01 Plan 02: UI Warning-Level Polish Summary

**ARIA tab semantics with aria-selected toggling, visible :focus-visible keyboard focus, system-ui font inheritance on form controls, AA-compliant slate-400 placeholders, an empty-state row for zero-user databases, disabled-in-flight submit buttons, and the product name as browser tab title — 7 of 10 UI-audit warnings closed in the single-file frontend without regressing the 3 plan-01 blocker fixes**

## Performance

- **Duration:** 10 min
- **Started:** 2026-08-20T14:41:21Z
- **Completed:** 2026-08-20T14:51:00Z
- **Tasks:** 3
- **Files modified:** 1

## Accomplishments

- **Task 1 — ARIA + focus:** `.tabs` is now `role="tablist"`; both tab buttons carry `role="tab"`, `aria-controls` (loginForm/registerForm) and initial `aria-selected` (true/false); both forms are `role="tabpanel"`. `switchTab()` toggles `aria-selected` via `setAttribute` alongside the existing class toggles. `outline: none` was removed from `input:focus` and a `:focus-visible` rule (2px `#60a5fa`, offset 2px) added for input/tab/button. **Live:** Tab-key focus on #tabLogin computed `outline: 2px solid rgb(96,165,250)` with `matches(':focus-visible') === true`; aria-selected flips correctly when switching tabs.
- **Task 2 — Typography/contrast:** `input, button, textarea, select { font-family: inherit }` kills the UA-default Arial mismatch; `::placeholder { color: #94a3b8; opacity: 1 }` lifts placeholder contrast from ~3.5:1 to ~7:1 (AA). **Live:** computed font-family of `#loginEmail` and the primary button both read `system-ui, -apple-system, "Segoe UI", Roboto, sans-serif`; computed `::placeholder` color is `rgb(148, 163, 184)`.
- **Task 3 — Copy/empty/disabled:** `<title>` reads "Gerenciamento de Usuários"; `loadUsers()` renders a centered `colspan=3` row with `textContent = 'Nenhum usuário cadastrado.'` when the API returns `[]`; `submitRegister()`/`submitLogin()` disable the submit button before the fetch and re-enable in `finally`; new `button:disabled { opacity: 0.6; cursor: not-allowed }` rule. **Live:** with the Users table emptied, `loadUsers()` against the real API rendered the empty-state row (colspan=3, centered); during a wrong-password login the button was `disabled=true` in-flight and `false` after the 401; register button likewise disabled in-flight and re-enabled after success.
- **No regressions:** BLOCKER 1 (wrong-password login shows "E-mail ou senha inválidos." — verified live), BLOCKER 2 (register success message "Conta criada! Faça login para continuar." on the visible login tab, class `message success` — verified live), BLOCKER 3 (375px viewport `scrollWidth 375 <= 375` with a long email badge — verified live). `dotnet build` (solution/): 6 projects, 0 errors, 0 warnings.

## Task Commits

Each task was committed atomically:

1. **Task 1: Add tab ARIA semantics and :focus-visible styling** - `a619361` (feat)
2. **Task 2: Fix form-control font inheritance and placeholder contrast** - `062affe` (feat)
3. **Task 3: Copy, empty state, and disabled submit state** - `c9e88b6` (feat)

**Plan metadata:** `(pending)` (docs: complete plan)

## Files Created/Modified

- `src/DotnetUserManagementApi.Api/wwwroot/index.html` - the entire frontend (HTML/CSS/JS); all polish fixes applied here:
  - `.tabs` gains `role="tablist"`; tabs gain `role="tab"` + `aria-controls` + initial `aria-selected`; forms gain `role="tabpanel"`; `switchTab()` toggles `aria-selected` (lines ~31, ~98-101, ~105, ~152-153)
  - `input:focus` loses `outline: none`; new `input:focus-visible, .tab:focus-visible, button:focus-visible` rule (lines ~54-55)
  - New `input, button, textarea, select { font-family: inherit }` and `::placeholder { color: #94a3b8; opacity: 1 }` after the body rule (lines ~20-21)
  - New `button:disabled { opacity: 0.6; cursor: not-allowed }` (line ~68)
  - `<title>Gerenciamento de Usuários</title>` (line 6)
  - `submitRegister()`/`submitLogin()`: `btn.disabled = true` before the request, `finally { btn.disabled = false }` (lines ~169-170, ~193-195, ~200-201, ~220-222)
  - `loadUsers()`: `tbody.textContent = ''` (was `innerHTML = ''`) + empty-array branch with `setAttribute('colspan','3')` + `textContent` (lines ~229-239)

## Decisions Made

- **setAttribute('colspan', '3') over td.colSpan = 3:** the plan's automated gate `grep -q 'colspan'` is case-sensitive; the JS property `colSpan` would not match. The attribute call produces the identical rendered attribute and satisfies both the grep gate and the createElement/textContent XSS constraint (T-01-20).
- **tbody.textContent = '' over tbody.innerHTML = '':** the plan states "the file currently has zero innerHTML usages and must keep it that way" but the file had `tbody.innerHTML = ''` (old line 216); the hard gate `! grep -q 'innerHTML'` could not pass with it retained. `textContent = ''` clears child rows identically with no XSS surface.
- **Placeholder color value:** slate-400 `#94a3b8` per plan — ~7:1 on the `#0f172a` input background, comfortably above AA 4.5:1; `opacity: 1` per plan for Firefox.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Replaced existing `tbody.innerHTML = ''` to satisfy the plan's own zero-innerHTML hard gate**
- **Found during:** Task 3 (Copy, empty state, and disabled submit state)
- **Issue:** The plan's Task 3 action instructs "after `tbody.innerHTML = ''`, add ..." and its threat model asserts "the file currently has zero innerHTML usages and must keep it that way" — but the file actually contained one innerHTML usage (`tbody.innerHTML = ''` in `loadUsers()`). Retaining it would fail the plan's hard gate `! grep -q 'innerHTML'` (part of the Task 3 acceptance criteria and the plan-level verification).
- **Fix:** Replaced `tbody.innerHTML = '';` with `tbody.textContent = '';` — clears all child rows identically, keeps the file innerHTML-free, and preserves the T-01-15/T-01-20 XSS posture.
- **Files modified:** src/DotnetUserManagementApi.Api/wwwroot/index.html (`loadUsers()`)
- **Verification:** `grep -c 'innerHTML'` returns 0; live users list rendered 6 rows correctly before the empty-state test.
- **Committed in:** c9e88b6 (Task 3 commit)

**2. [Rule 1 - Bug] `colspan` written via `setAttribute` instead of the `colSpan` property**
- **Found during:** Task 3 (Copy, empty state, and disabled submit state)
- **Issue:** The plan's Task 3 action says create a `<td>` with `colspan = 3`. Written as the JS property `td.colSpan = 3`, the lowercase grep gate `grep -q 'colspan'` (Task 3 automated verify + must_haves pattern) would not match — case-sensitive grep.
- **Fix:** Used `td.setAttribute('colspan', '3')` — same rendered attribute, matches the grep gate, still createElement/textContent only (no innerHTML, T-01-20).
- **Files modified:** src/DotnetUserManagementApi.Api/wwwroot/index.html (`loadUsers()` empty-state branch)
- **Verification:** `grep -q 'colspan'` passes; live empty-state row rendered with `colspan="3"`.
- **Committed in:** c9e88b6 (Task 3 commit)

---

**Total deviations:** 2 auto-fixed (2 bugs — both plan-internal gate/implementation mismatches)
**Impact on plan:** Both deviations were required to satisfy the plan's own acceptance criteria and hard gates. Behavior identical to the plan's intent. No scope creep.

## Issues Encountered

- **Stale browser page after edits:** the first Playwright navigation after Task 1 served the pre-edit page (title still "dotnet-user-management-api") while `curl` confirmed the server was already serving the new file — static files are served from disk per request; a cache-busted navigation (`/?v=2`) loaded the new DOM. Same class of issue as 01-01's stale-cache note.
- **Empty-state live test required DB surgery:** with users in the DB the empty state cannot appear, and the dev JWT key is random per process (Program.cs line 26, T-01-08), so restarting the server invalidates the in-memory token — a "delete app.db then login" sequence cannot reach a zero-user state (login needs a user). **Approach:** backed up `app.db`, emptied the `Users` table in-place via `python3 sqlite3` (same inode — the server's pooled connections see the emptied table while the valid token still works), verified the empty-state row end-to-end against the real API, then restored the backup and restarted the server. The wave2 test user created during verification lived in the SQLite WAL and was lost in the backup/restore cycle — acceptable, since it was throwaway test data; the 5 original users (maria.uat, carlos.teste, ui.audit.20260820, reg.tarefa2.20260820, ui.audit.long.email...) were fully preserved (verified: 5 rows after cleanup, register/login round-trip OK on the restored DB).
- **Server restart environment:** a bare `dotnet run --no-launch-profile --urls http://localhost:5291` starts in Production and trips the JWT__KEY fail-fast (Program.cs line 33). The original server ran with `ASPNETCORE_ENVIRONMENT=Development` (random dev key). Restarted with that env var; health check 200 and `[SECURITY] Jwt:Key não configurado. Chave aleatória gerada` logged.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All 7 naturally-fitting WARNING-level UI-REVIEW findings closed; 3 explicitly deferred by the plan with rationale (spacing scale drift, color-only active-tab cue, design tokens).
- All 3 plan-01 blocker fixes re-verified live with no regressions; `dotnet build` clean (6 projects, 0 errors, 0 warnings).
- This was the last plan of phase 01 — phase complete, ready for the next step (UAT re-run / phase verification).

---
*Phase: 01-mvp-rodando*
*Completed: 2026-08-20*

## Self-Check: PASSED

- File exists: `.planning/phases/01-mvp-rodando/01-02-SUMMARY.md`
- Commits exist: `a619361` (Task 1), `062affe` (Task 2), `c9e88b6` (Task 3)
- All 3 static gate suites pass (T1: tablist/aria-selected/setAttribute/:focus-visible, no outline:none; T2: font-family inherit/::placeholder/#94a3b8; T3: title/empty-state/colspan/button:disabled/disabled=true, no innerHTML)
- Live re-test passed: focus ring via keyboard, aria-selected toggle, system-ui fonts, placeholder rgb(148,163,184), empty-state row vs real API, disabled-in-flight + re-enable, title, 375px no-horizontal-scroll regression, error/success message regressions
- `dotnet build` (solution/): 6 projects, 0 errors, 0 warnings