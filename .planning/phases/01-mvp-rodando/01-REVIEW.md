---
phase: 01-mvp-rodando
reviewed: 2026-08-20T00:00:00Z
depth: standard
files_reviewed: 1
files_reviewed_list:
  - src/DotnetUserManagementApi.Api/wwwroot/index.html
findings:
  critical: 0
  warning: 6
  info: 5
  total: 11
status: issues_found
---

# Phase 01: Code Review Report

**Reviewed:** 2026-08-20T00:00:00Z
**Depth:** standard
**Files Reviewed:** 1
**Status:** issues_found

## Summary

Reviewed `src/DotnetUserManagementApi.Api/wwwroot/index.html` (277 lines — the entire frontend: markup, CSS, JS) after two gap-closure plans (01-01 blocker fixes, 01-02 UI polish). I cross-checked the file against the actual API contract (`AuthController`, `UsersController`, `UserService`, `ExceptionHandlingMiddleware`, `LoginResponse`, `UserDto`) to validate response-shape assumptions.

**Verified as correctly implemented (per plan 01-01/01-02):**
- `request()` content-type check is now `includes('json')` (line 158), so RFC 7807 `application/problem+json` bodies parse and `body.detail` surfaces ("E-mail ou senha inválidos." for 401, throttle messages for 429).
- Register success message is written to `#loginMessage` **after** `switchTab('login')` (lines 184-189) — ordering gate satisfied; the stale `#registerMessage` is reset.
- Mobile overflow fixes present: `.toolbar` has `flex-wrap: wrap; min-width: 0; gap: 0.5rem`, `.badge` has `max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap`, `.toolbar h2` truncation rule exists.
- A11y/UX polish present: `role="tablist"/"tab"/"tabpanel"`, `aria-selected` toggling, `:focus-visible` (no `outline: none` remains), `font-family: inherit`, `::placeholder` `#94a3b8`, product `<title>`, empty-state `colspan="3"` row, `btn.disabled = true` + `finally` re-enable in both submit handlers.
- **XSS surface is clean:** grep confirms zero `innerHTML`, `eval`, `localStorage`, `document.write`, `insertAdjacentHTML`. Every DOM write of user/server data uses `textContent` (lines 143, 187, 192, 219, 235, 243-247, 256-264). Token is held in memory only (line 139), never persisted. No injection vectors found — hence zero Critical findings.

**Key residual concerns:** stale login-form state after logout (misleading message + retained password), no focus management on view/tab switches despite the phase's a11y claims, incomplete tablist keyboard pattern, and unlocalized raw network errors. Details below.

## Warnings

### WR-01: Stale login feedback persists after logout / successful login

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:204` (and 214-216, 268-274)
**Issue:** `#loginMessage` is set to "Autenticando..." before the fetch (line 204) and is **never cleared on the success path**. `logout()` (lines 268-274) shows the auth card and calls `switchTab('login')` but does not reset the message. Trace: register success → loginMessage = "Conta criada! Faça login para continuar." → user logs in → that text still claims the account was *just* created → logout → the login form displays stale text ("Conta criada!..." or a leftover "Autenticando...") that is actively misleading. The only thing that clears it is typing into an input (`oninput="clearMessage(...)"`), which a user may never do before acting on the stale text.
**Fix:** Clear the message on login success and in `logout()`:
```js
// submitLogin success path, before showUsers():
message.className = 'message';
message.textContent = '';

// logout():
function logout() {
  state.token = null;
  state.email = null;
  const loginMsg = document.getElementById('loginMessage');
  loginMsg.className = 'message';
  loginMsg.textContent = '';
  document.getElementById('usersCard').classList.add('hidden');
  document.getElementById('authCard').classList.remove('hidden');
  switchTab('login');
}
```

### WR-02: Password (and email) retained in the DOM after logout

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:268-274`
**Issue:** `logout()` nulls the token state but leaves the login form fields populated — including the password, which remains readable in the DOM and pre-filled for the next user of a shared machine. Combined with WR-01, the form after logout looks "ready to submit" with the previous user's credentials still filled in.
**Fix:**
```js
function logout() {
  state.token = null;
  state.email = null;
  document.getElementById('loginEmail').value = '';
  document.getElementById('loginPassword').value = '';
  // ...rest unchanged
}
```

### WR-03: No focus management on tab/view switches — focus lands on hidden elements

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:147-154, 184, 216, 268-274`
**Issue:** `switchTab()` toggles classes/ARIA but never moves focus. On register success, `switchTab('login')` hides `#registerForm` while focus still sits on the register submit button — which is now `display:none` — so focus drops to `<body>`. Same on successful login (`showUsers()` hides the auth card while focus is on the login button) and on logout. Keyboard and screen-reader users lose their position and receive no pointer to the newly revealed form; the phase's a11y claims (per 01-02) do not cover focus management, which is the largest remaining a11y gap.
**Fix:** Move focus to the revealed form's first field after switching (and to a safe target after login/logout):
```js
// in submitRegister success branch, after switchTab('login'):
document.getElementById('loginEmail').focus();

// in showUsers(), after removing .hidden from usersCard:
document.getElementById('loggedUser').focus(); // add tabindex="-1" to #loggedUser
```
Also give `#loggedUser` `tabindex="-1"` so it can be focused programmatically without entering the tab order.

### WR-04: Tablist ARIA pattern incomplete — no roving tabindex / arrow-key navigation

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:97-99, 147-154`
**Issue:** `role="tablist"`/`role="tab"`/`aria-selected` were added, but the WAI-ARIA tabs pattern also requires: (1) only the active tab in the sequential tab order (`tabindex="0"` on active, `tabindex="-1"` on inactive), and (2) Left/Right arrow keys to switch tabs. As implemented, both tab buttons are always tabbable and arrow keys do nothing — assistive tech announces a tablist that does not behave like one. Also, `switchTab()` doesn't move focus to the activated tab.
**Fix:**
```js
function switchTab(tab) {
  // ...existing classList/aria-selected toggles...
  document.getElementById('tabLogin').setAttribute('tabindex', tab === 'login' ? '0' : '-1');
  document.getElementById('tabRegister').setAttribute('tabindex', tab === 'register' ? '0' : '-1');
}

function onTabKeydown(event, tab) {
  if (event.key !== 'ArrowRight' && event.key !== 'ArrowLeft') return;
  event.preventDefault();
  const next = tab === 'login' ? 'register' : 'login';
  switchTab(next);
  document.getElementById(next === 'login' ? 'tabLogin' : 'tabRegister').focus();
}
// wire: onclick="switchTab('login')" + onkeydown="onTabKeydown(event, 'login')"
```

### WR-05: Network-level fetch failures surface raw English errors

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:157, 162`
**Issue:** `request()` only handles HTTP-level errors. If `fetch` itself rejects (server down, network drop, CORS failure), the raw `TypeError` propagates and `error.message` — "Failed to fetch" / "NetworkError when attempting to fetch resource" — is rendered verbatim in pt-BR UI copy. The plan's "Erro inesperado." fallback only covers non-JSON *responses*, not thrown exceptions. Same family: if `res.json()` throws (JSON content-type with empty/truncated body), the raw `SyntaxError` message leaks to the UI.
**Fix:**
```js
async function request(path, options) {
  let res;
  try {
    res = await fetch(path, options);
  } catch {
    throw new Error('Servidor indisponível. Verifique sua conexão e tente novamente.');
  }
  const isJson = res.headers.get('content-type')?.includes('json');
  let body = null;
  if (isJson) {
    try {
      body = await res.json();
    } catch {
      throw new Error('Resposta inválida do servidor. Tente novamente.');
    }
  }
  if (!res.ok) {
    const detail = body?.detail || (body?.errors ? Object.values(body.errors).flat().join(' ') : 'Erro inesperado.');
    throw new Error(detail);
  }
  return body;
}
```

### WR-06: No `aria-live` on message regions — async feedback is silent to screen readers

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:108, 119, 135`
**Issue:** All user feedback — login errors, register success, users-list errors — is written to `<p class="message">` elements with no `aria-live`/`role` attribute. Screen reader users submit the form and hear nothing; the entire error/success feedback loop of the app is invisible to assistive tech. Given 01-02 explicitly targeted a11y, this is a notable omission.
**Fix:** Add live-region semantics to the three message paragraphs:
```html
<p id="loginMessage" class="message" role="status" aria-live="polite"></p>
<p id="registerMessage" class="message" role="status" aria-live="polite"></p>
<p id="usersMessage" class="message" role="status" aria-live="polite"></p>
```
(Errors can additionally be flagged by setting `role="alert"` when the `error` class is applied.)

## Info

### IN-01: Register form fields not reset after successful registration

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:167-196`
**Issue:** After a successful register, name/email/password stay populated in the hidden register form; switching back to the "Cadastrar" tab shows stale credentials, and re-submitting without changes just re-attempts the same account (which returns a uniform 201 due to anti-enumeration, but the UI implication is confusing).
**Fix:** Clear the three fields in the success branch: `document.getElementById('regName').value = '';` (and `regEmail`, `regPassword`).

### IN-02: `data.token` / `data.user` accessed without null guard

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:214-215`
**Issue:** `state.token = data.token; state.email = data.user.email;` assumes the 200 body is always the full `LoginResponse`. A shape change (or a 200 with a null/missing body) throws `TypeError`, surfaced raw in the catch branch instead of a meaningful message.
**Fix:** Guard: `if (!data?.token || !data?.user) throw new Error('Resposta de login inválida.');` before assigning.

### IN-03: Empty `errors`/`detail` object yields an invisible error message

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:161`
**Issue:** If a problem+json body has `detail: null` and `errors: {}` (e.g., a validation response whose `errors` map is empty), `Object.values({}).flat().join(' ')` produces `''`, so `request()` throws `new Error('')` and the user sees an empty message area — a silent failure with no fallback.
**Fix:** Normalize the fallback chain: `const detail = body?.detail || (body?.errors && Object.values(body.errors).flat().join(' ').trim()) || 'Erro inesperado.';`

### IN-04: No Content-Security-Policy

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:8, 138`
**Issue:** The page uses inline `<style>`/`<script>` and inline `onclick`/`oninput` handlers, so a strict CSP is not possible without refactoring — but there is no CSP meta tag at all, leaving the page with default permissive behavior. Acceptable for a static single-file demo page; worth noting if this page ever grows third-party assets.
**Fix:** Add a `Content-Security-Policy` meta allowing only `default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'` (inline handlers force `'unsafe-inline'`), or refactor to external listeners and tighten from there.

### IN-05: `showUsers()` called without `await`

**File:** `src/DotnetUserManagementApi.Api/wwwroot/index.html:216`
**Issue:** `showUsers()` is async and internally swallows its own errors, so no unhandled rejection occurs today — but any future exception outside its inner try/catch (e.g., a null `getElementById`) would become a silent unhandled rejection.
**Fix:** `await showUsers();` inside the try block (harmless, keeps error path deterministic).

---

_Reviewed: 2026-08-20T00:00:00Z_
_Reviewer: the agent (gsd-code-reviewer)_
_Depth: standard_
