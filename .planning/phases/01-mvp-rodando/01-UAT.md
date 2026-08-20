---
status: complete
phase: 01-mvp-rodando
source: .planning/phases/01-mvp-rodando/README.md (fase documentada retroativamente; sem SUMMARY)
started: 2026-08-20T10:26:40Z
updated: 2026-08-20T10:40:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: |
  Kill any running server. Start the application from scratch with `dotnet run` (from src/DotnetUserManagementApi.Api).
  Server boots without errors and the page at http://localhost:5290/ loads.
result: pass
verified: |
  Automated via Playwright + dotnet. Fresh `dotnet run` (port 5291) booted cleanly in ~13s, SQLite initialized,
  page http://localhost:5291/ served 200 with the "Gerenciamento de Usuários" UI.
  Nota: um processo dotnet legado de outro UID ocupava a porta 5290 (não terminável) — o cold start foi validado em porta livre.

### 2. Register a New User (web)
expected: |
  Open http://localhost:5290/ and click the "Cadastrar" tab. Fill in Nome, E-mail and Senha (min 8 chars),
  click "Cadastrar". You should see a green "Conta criada! Faça login para continuar." message and the form switches back to "Entrar".
result: pass
verified: |
  Automated via Playwright: filled Nome/E-mail/Senha, clicked Cadastrar → message "Conta criada! Faça login para continuar."
  and auto-switch to the "Entrar" tab. Persistence confirmed via 409 on duplicate email.

### 3. Log In (web)
expected: |
  On the "Entrar" tab, type the email and password just registered and click "Entrar".
  You should land on the "Usuários cadastrados" card with a blue badge showing your logged-in email.
result: pass
verified: |
  Automated via Playwright: login with ana@example.com/senha12345 → "Usuários cadastrados" card shown with badge "ana@example.com".

### 4. List Users (web)
expected: |
  After login, the "Usuários cadastrados" table lists users with Nome, E-mail and Cadastrado em columns,
  including the user you just registered.
result: pass
verified: |
  Automated via Playwright: table rendered columns Nome/E-mail/Cadastrado em and row "Ana Souza | ana@example.com | 20/08/2026, 07:32:31".

### 5. Register API behaviors
expected: |
  POST /api/auth/register: valid payload → 201 with the new user; duplicate email → 409; invalid payload → 400 (RFC 7807 problem details).
result: pass
verified: |
  curl: valid → 201 + user JSON (id/name/email/createdAtUtc); duplicate ana@example.com → 409 "Já existe um usuário cadastrado com este e-mail.";
  invalid (empty name, bad email, short password) → 400 "Nome é obrigatório."

### 6. Login API behaviors
expected: |
  POST /api/auth/login: correct credentials → 200 with a JWT token; wrong password → 401; malformed payload → 400.
result: pass
verified: |
  curl: correct → 200 + JWT (HS256, exp 3599s, tokenType Bearer); wrong password → 401 "E-mail ou senha inválidos.";
  missing password → 400 validation errors.

### 7. Users endpoint protection
expected: |
  GET /api/users: returns 401 without a token; returns 200 with the user list when a valid Bearer token is sent.
result: pass
verified: |
  curl: no token → 401; with Bearer token from login → 200 with list of both registered users.

### 8. Password stored hashed (BCrypt)
expected: |
  The stored password in the database is a BCrypt hash (starts with $2a$/$2b$), never the plaintext value.
result: pass
verified: |
  SQLite (app.db): both users' PasswordHash are $2a$12$... (60 chars) — BCrypt work factor 12, no plaintext.

### 9. Automated Tests Green
expected: |
  `dotnet test` from the repository root completes with 12 tests passing (register 201, duplicate 409, validations 400,
  login 200/401, users 401/200, BCrypt hashing).
result: pass
verified: |
  dotnet test solution → Passed: 12, Failed: 0, Skipped: 0.

### 10. MVP Coverage — full loop end to end
expected: |
  From a fresh start a brand-new user can register, log in, and see the user list — the complete
  cadastro → login (JWT) → listagem protegida loop works without manual DB setup or environment files.
result: pass
verified: |
  Full loop exercised in a single fresh server: cadastro (web + API 201) → login (JWT) → listagem protegida (401 sem token / 200 com token)
  → visualização na web. Banco SQLite auto-criado (EnsureCreated), Jwt:Key auto-gerada em Development. Nenhum setup manual necessário.

## Summary

total: 10
passed: 10
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none]