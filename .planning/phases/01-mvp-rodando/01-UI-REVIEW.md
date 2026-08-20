# Phase 1 — UI Review (MVP Rodando)

**Audited:** 2026-08-20
**Baseline:** Abstract 6-pillar standards (no UI-SPEC.md — phase retroactively documented)
**Screenshots:** captured (5) — `.planning/ui-reviews/01-20260820-110446/` (desktop-auth, mobile-auth, tablet-auth, desktop-users, mobile-users); live app at http://localhost:5291/

---

## Pillar Scores

| Pillar | Score | Key Finding |
|--------|-------|-------------|
| 1. Copywriting | 2/4 | Well-written pt-BR copy, but the two most important feedback strings are invisible or generic |
| 2. Visuals | 3/4 | Coherent dark theme with clear hierarchy; horizontal overflow on mobile breaks layout |
| 3. Color | 3/4 | Disciplined 60/30/10 accent usage, AA-compliant surfaces; placeholder contrast fails AA |
| 4. Typography | 2/4 | Form controls render in a different font (Arial vs system-ui); ad-hoc 7-size/3-weight scale |
| 5. Spacing | 3/4 | Consistent 0.25rem-based scale; off-grid outliers and no truncation guard |
| 6. Experience Design | 2/4 | Two BLOCKERs: all API error messages swallowed; register success message never visible |

**Overall: 15/24**

---

## Top 3 Priority Fixes

1. **Fix `request()` content-type check (`index.html:151`)** — `content-type?.includes('application/json')` is false for the API's RFC 7807 `application/problem+json` responses, so every server error message is discarded and the user sees only "Erro inesperado.". **User impact:** a failed login (401) shows a generic error instead of "E-mail ou senha inválidos."; validation errors never reach the user — recovery is guesswork. **Fix:** match `includes('json')` or explicitly check `includes('application/problem+json') || includes('application/json')`, then prefer `body.detail`.

2. **Make the register success message visible (`index.html:175-177`)** — `submitRegister` sets "Conta criada! Faça login para continuar." on `#registerMessage` and *then* calls `switchTab('login')`, which hides the register form. The message is written into a `display:none` element — verified live: DOM contains the text, the user sees nothing. **User impact:** registration confirmation is the app's key success feedback; users are left unsure whether the account was created. **Fix:** set the text on `#loginMessage` after the tab switch, or re-render it post-switch.

3. **Fix mobile horizontal overflow (`index.html:84-85`)** — at 375px the page scrolls horizontally (scrollWidth 472 > 375) because the logged-in email `.badge` (222px, no truncation) overflows the `.toolbar` and viewport. **User impact:** on phones the layout breaks and the user's own email badge is cut off. **Fix:** add `max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap` to `.badge` and `min-width: 0`/`flex-wrap: wrap` to `.toolbar`.

---

## Detailed Findings

### Pillar 1: Copywriting (2/4)

All UI copy is specific, contextual pt-BR — no generic "Submit/Click Here/OK". CTAs ("Entrar", "Cadastrar", "Sair"), labels ("E-mail", "Senha", "Nome", "Cadastrado em"), placeholders ("voce@exemplo.com", "Seu nome") and loading states ("Cadastrando...", "Autenticando...", "Carregando...") are all well-worded. Failures:

- **BLOCKER — success confirmation invisible** (`index.html:175-177`): "Conta criada! Faça login para continuar." is written to `#registerMessage` inside the register form, then `switchTab('login')` hides that form. Verified live: `registerMessageText: "Conta criada!..."`, `registerFormVisible: false` — the copy never renders on screen. The UAT (check 2) validated the DOM text, not visual rendering.
- **BLOCKER — generic error copy** (`index.html:154`): fallback "Erro inesperado." is what users actually see for a 401 wrong-password login, because the API's specific "E-mail ou senha inválidos." (RFC 7807 `detail`) never parses (see Pillar 6). Verified live.
- **Missing empty state** (`index.html:211-224`): a zero-user database renders a header-only table with no "Nenhum usuário cadastrado." message — the usersMessage paragraph is cleared to empty on success.
- **`<title>` is the repo slug** (`index.html:6`): browser tab shows "dotnet-user-management-api" instead of the product name "Gerenciamento de Usuários".

### Pillar 2: Visuals (3/4)

Clear focal point on each view (auth card → users card), strong hierarchy via size + weight (h1 1.6rem/700 vs body 1rem/400), consistent card/tab/input styling, coherent dark theme. Failures:

- **WARNING — mobile layout breakage** (`index.html:84-85`, verified at 375px): `.toolbar` is `flex; justify-content: space-between` with no wrap/truncation; the email badge (222px for `ui.audit.20260820@example.com`) overflows the toolbar right edge (384.9 > 334) and the viewport — `document.documentElement.scrollWidth = 472` vs 375 viewport. Horizontal scroll on every phone.
- **WARNING — color-only active-tab cue** (`index.html:43`): the only active-tab differentiator is background color (blue vs slate-700). Luminance difference mitigates color-blind risk, but no underline/icon/weight cue exists.
- **WARNING — no tab semantics** (`index.html:31-43, 92-95`): `.tabs` lacks `role="tablist"`, tabs lack `role="tab"`, `aria-selected`, and `aria-controls`. Screen readers announce plain buttons, losing tab state.

### Pillar 3: Color (3/4)

Coherent slate/blue palette; 60/30/10 distribution respected (slate-900 bg 60%, slate-800/700 surfaces 30%, blue-600 accent ~10%). Accent `#2563eb` on exactly 5 elements (active tab, input focus border, primary buttons, badge, favicon) — under the >10 overuse flag. All surface contrast measured and passing: body 14.48:1, labels on card 5.71:1, inactive tab 6.97:1, white-on-blue-600 5.17:1. Failures:

- **WARNING — placeholder contrast fails AA** (`index.html:45-53`, measured): placeholder renders at UA default `rgb(117,117,117)` on `#0f172a` input bg ≈ 3.5:1 — below the 4.5:1 minimum for the placeholder text users read to understand field purpose. No explicit `::placeholder` color is set.
- **WARNING — no design tokens**: 14 hardcoded hex values in one `<style>` block; changing the palette means editing every rule. Acceptable for a single-file app, but no token discipline for future growth.

### Pillar 4: Typography (2/4)

- **WARNING — form controls use a different font** (`index.html:11` vs 45-66, measured): `font-family` is declared only on `body` (system-ui); `input`, `button` (tabs, primary, secondary) fall back to the UA default and compute to **Arial**. The entire form area renders in a visibly different typeface from headings/body. Fix: `input, button, textarea { font-family: inherit; }`.
- **WARNING — ad-hoc type scale**: 7 distinct sizes (0.75 / 0.85 / 0.9 / 0.95 / 1rem / 1.1 / 1.6rem) and 3 weights (400/600/700) in a 250-line file — above the abstract flag threshold (>4 sizes, >2 weights). The ramp is coherent but undocumented, and **no `line-height` is set anywhere**, leaving defaults to vary per element.
- Positive: weights map consistently to roles (600 for tabs/table headers, 700 for buttons/headings); h1→h2→body hierarchy is clear.

### Pillar 5: Spacing (3/4)

- **WARNING — scale drift**: most values sit on a 0.25rem grid (0.2–2.0rem), but 0.55rem (`th, td` padding, line 81), 0.85rem (label margin, line 44), 0.9rem (message margin, line 77) fall off it — subtle, but inconsistent with the rest of the file.
- **WARNING — no overflow containment**: spacing/width interplay causes the badge overflow (Pillar 2) — no `max-width`, `overflow`, or `min-width: 0` discipline on flex children.
- Positive: no arbitrary px values; element-type-consistent padding (inputs 0.6rem, tabs 0.6rem, primary button 0.7rem); `.message { min-height: 1.2em }` (line 77) reserves space and prevents layout shift on async feedback.

### Pillar 6: Experience Design (2/4)

- **BLOCKER — all API error messages swallowed** (`index.html:151`): `res.headers.get('content-type')?.includes('application/json')` returns false for `Content-Type: application/problem+json` (verified via curl: the 401 body carries `detail: "E-mail ou senha inválidos."`). Every non-2xx response falls through to `body = null` → generic "Erro inesperado.". This affects login 401, users 401 (expired token), and any validation 400 that reaches the UI. Users cannot distinguish wrong credentials from server failures.
- **BLOCKER — success feedback invisible** (same root as Pillar 1; `index.html:175-177`): verified live — the only confirmation of a successful registration never renders.
- **WARNING — no empty state** (`index.html:211-224`): empty `users` array → headers with zero rows and a cleared message; no guidance.
- **WARNING — no disabled/submitting state** (`index.html:160-205`): "Cadastrando..."/"Autenticando..." text appears but the submit button stays enabled — double-click fires duplicate requests.
- **WARNING — no focus-visible styling**: only `input:focus` (line 54) is styled; tabs and buttons rely on the UA outline, and `outline: none` is applied to inputs without a `:focus-visible` counterpart.
- Positive: loading text states for all three async operations; try/catch with inline message rendering on every fetch path; logout cleanly resets state; native HTML5 validation (required/minlength/type=email) pre-empts most invalid register submissions.

---

## Files Audited

- `src/DotnetUserManagementApi.Api/wwwroot/index.html` (250 lines — the entire frontend: markup, CSS, JS)
- `src/DotnetUserManagementApi.Api/wwwroot/favicon.svg` (5 lines)
- `.planning/phases/01-mvp-rodando/README.md` (phase context)
- `.planning/phases/01-mvp-rodando/01-UAT.md` (UAT results — noted: UAT checks 2–4 verified DOM presence, not visual rendering; the invisible success message passed UAT)
- Live app exercised at http://localhost:5291/ (register, login, wrong-password error, logout, users table; viewports 1440/768/375)

**Registry audit:** skipped — no `components.json` (vanilla single-file HTML app, no shadcn/third-party registries).

**Screenshot evidence:** 5 captures in `.planning/ui-reviews/01-20260820-110446/` (git-ignored via `.planning/ui-reviews/.gitignore`).