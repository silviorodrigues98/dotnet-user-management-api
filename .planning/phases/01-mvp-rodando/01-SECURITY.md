---
phase: 01
slug: mvp-rodando
status: verified
threats_open: 0
asvs_level: 1
created: 2026-08-20
---

# Phase 01 — Security

> Per-phase security contract: threat register, accepted risks, and audit trail.
> Fase executada antes da adoção do fluxo formal GSD — register construído retroativamente (retroactive-STRIDE) a partir dos arquivos de implementação em 2026-08-20.

---

## Trust Boundaries

| Boundary | Description | Data Crossing |
|----------|-------------|---------------|
| Cliente ↔ API | Requisições HTTP públicas (register/login) e autenticadas (users) | Credenciais (e-mail/senha), tokens JWT |
| API ↔ Banco SQLite (local) / PostgreSQL (Docker) | Persistência de usuários e hashes | User + PasswordHash (BCrypt) |
| API ↔ Chave JWT | Chave simétrica de assinatura (HS256) | Segredo — nunca versionado |

---

## Threat Register

| Threat ID | Category | Component | Disposition | Mitigation | Status |
|-----------|----------|-----------|-------------|------------|--------|
| T-01-01 | Spoofing — JWT forjado | JwtTokenService / Program.cs | mitigate | HS256 com ValidateIssuer, ValidateAudience, ValidateLifetime e ValidateIssuerSigningKey | closed |
| T-01-02 | Spoofing — senha fraca/comprometida | BcryptPasswordHasher | mitigate | BCrypt work factor 12 + senha mínima de 8 caracteres | closed |
| T-01-03 | Tampering — claims do JWT alteradas | JwtTokenService | mitigate | Verificação de assinatura na validação do token | closed |
| T-01-04 | Tampering — hash de senha na DB | BcryptPasswordHasher | mitigate | Hash com salt aleatório; qualquer alteração quebra a verificação | closed |
| T-01-05 | Repudiation — ausência de trilha de auditoria | UserService | mitigate | Logging estruturado de registro, login bem-sucedido e tentativas falhas | closed |
| T-01-06 | Information Disclosure — hash de senha em respostas | UserDto / UsersController | mitigate | UserDto não expõe PasswordHash; [Authorize] no GET /api/users | closed |
| T-01-07 | Information Disclosure — detalhes de erro | ExceptionHandlingMiddleware | mitigate | Resposta genérica 500; detalhes de domínio apenas para 400/401/409; RFC 7807 | closed |
| T-01-08 | Information Disclosure — chave JWT versionada | Program.cs | mitigate | Chave aleatória em dev; fail-fast (JWT__KEY obrigatória ≥32 bytes, sem placeholder) em produção; nada versionado | closed |
| T-01-09 | Information Disclosure — Swagger em produção | Program.cs | mitigate | Swagger habilitado apenas em ambiente Development | closed |
| T-01-10 | Information Disclosure — enumeração de e-mail no register | UserService / AuthController | mitigate | Resposta uniforme 201 para e-mail novo e já existente (sem 409); mensagem genérica | closed |
| T-01-11 | DoS — brute force no login | AuthController / LoginThrottle | mitigate | Rate limiting em memória: 5 falhas / 15 min por IP → 429 Too Many Requests | closed |
| T-01-12 | Tampering — SQL injection | UserRepository | mitigate | EF Core com consultas parametrizadas | closed |
| T-01-13 | EoP — acesso não autenticado à listagem | UsersController | mitigate | [Authorize] + UseAuthentication/UseAuthorization | closed |
| T-01-14 | EoP — escalada de privilégio via claims | JwtTokenService | mitigate | Token carrega apenas sub/email/name; sem roles ou claims de elevação | closed |
| T-01-15 | Tampering — XSS na página web | wwwroot/index.html | mitigate | Dados renderizados via textContent (não innerHTML) | closed |
| T-01-16 | Spoofing — CSRF | wwwroot/index.html | mitigate | Autenticação via Bearer header (não cookies) | closed |

*Status: open · closed*
*Disposition: mitigate (implementation required) · accept (documented risk) · transfer (third-party)*

---

## Accepted Risks Log

| Risk ID | Threat Ref | Rationale | Accepted By | Date |
|---------|------------|-----------|-------------|------|
| R-01-01 | T-01-11 | Rate limiting em memória (in-process) — reinicia com o processo e não é compartilhado entre instâncias; adequado para MVP single-instance | Silvi | 2026-08-20 |

*Accepted risks do not resurface in future audit runs.*

---

## Security Audit Trail

| Audit Date | Threats Total | Closed | Open | Run By |
|------------|---------------|--------|------|--------|
| 2026-08-20 | 16 | 16 | 0 | opencode (retroactive-STRIDE) |

### Auditoria 2026-08-20 — mitigations implementadas nesta auditoria

| Threat | Mitigation adicionada |
|--------|------------------------|
| T-01-05 | Logging estruturado em UserService: `User registered`, `Failed login attempt`, `User logged in`, `Registration attempt for existing email` |
| T-01-10 | Register passa a retornar 201 uniforme com `{ message: "Conta criada." }` — removido o 409 de e-mail duplicado |
| T-01-11 | InMemoryLoginThrottle (5 falhas/15 min/IP) + resposta 429 com ProblemDetails |

---

## Sign-Off

- [x] All threats have a disposition (mitigate / accept / transfer)
- [x] Accepted risks documented in Accepted Risks Log
- [x] `threats_open: 0` confirmed
- [x] `status: verified` set in frontmatter

**Approval:** verified 2026-08-20