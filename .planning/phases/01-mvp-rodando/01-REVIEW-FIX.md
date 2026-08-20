---
phase: 01-mvp-rodando
fixed_at: 2026-08-20T00:00:00Z
review_path: .planning/phases/01-mvp-rodando/01-REVIEW.md
iteration: 1
findings_in_scope: 6
fixed: 6
skipped: 0
status: all_fixed
---

# Phase 01: Code Review Fix Report

**Fixed at:** 2026-08-20T00:00:00Z
**Source review:** .planning/phases/01-mvp-rodando/01-REVIEW.md
**Iteration:** 1

**Summary:**
- Findings in scope: 6 (all Warning — WR-01..WR-06; Info findings out of scope per `critical_warning` fix scope)
- Fixed: 6
- Skipped: 0

## Fixed Issues

### WR-01: Stale login feedback persists after logout / successful login

**Files modified:** `src/DotnetUserManagementApi.Api/wwwroot/index.html`
**Commit:** d6b6733
**Applied fix:** Cleared `#loginMessage` (reset `className` to `'message'` and `textContent` to `''`) in the `submitLogin` success path just before `showUsers()`, and added the same reset in `logout()` so stale "Conta criada!..." / "Autenticando..." text no longer survives login success or logout.

### WR-02: Password (and email) retained in the DOM after logout

**Files modified:** `src/DotnetUserManagementApi.Api/wwwroot/index.html`
**Commit:** e634bc1
**Applied fix:** `logout()` now clears `#loginEmail` and `#loginPassword` (`.value = ''`) so credentials are not left readable in the DOM or pre-filled for the next user of a shared machine.

### WR-03: No focus management on tab/view switches — focus lands on hidden elements

**Files modified:** `src/DotnetUserManagementApi.Api/wwwroot/index.html`
**Commit:** c2263d9
**Applied fix:** Added `tabindex="-1"` to `#loggedUser` so it can be focused programmatically without entering the tab order. Focus now moves to `#loginEmail` after register success (post `switchTab('login')`) and after logout, and to `#loggedUser` in `showUsers()` after the auth card is hidden. No focus is left on `display:none` elements.

### WR-04: Tablist ARIA pattern incomplete — no roving tabindex / arrow-key navigation

**Files modified:** `src/DotnetUserManagementApi.Api/wwwroot/index.html`
**Commit:** e13cace
**Applied fix:** Implemented the WAI-ARIA tabs pattern: `switchTab()` now sets roving `tabindex` (`0` on the active tab, `-1` on the inactive one), the tab buttons start with `tabindex="0"`/`tabindex="-1"`, and a new `onTabKeydown(event, tab)` handler (wired via `onkeydown` on both tabs) switches tabs on ArrowLeft/ArrowRight and moves focus to the activated tab.

### WR-05: Network-level fetch failures surface raw English errors

**Files modified:** `src/DotnetUserManagementApi.Api/wwwroot/index.html`
**Commit:** fe6cea1
**Applied fix:** `request()` now wraps `fetch` in a try/catch that throws a localized `'Servidor indisponível. Verifique sua conexão e tente novamente.'` on network/CORS rejection, and wraps `res.json()` in a try/catch that throws `'Resposta inválida do servidor. Tente novamente.'` on a malformed/truncated JSON body. HTTP-level handling is unchanged.

### WR-06: No `aria-live` on message regions — async feedback is silent to screen readers

**Files modified:** `src/DotnetUserManagementApi.Api/wwwroot/index.html`
**Commit:** 917e082
**Applied fix:** Added `role="status" aria-live="polite"` to all three message regions (`#loginMessage`, `#registerMessage`, `#usersMessage`) so form-submission feedback is announced by assistive technology.

---

_Fixed: 2026-08-20T00:00:00Z_
_Fixer: the agent (gsd-code-fixer)_
_Iteration: 1_