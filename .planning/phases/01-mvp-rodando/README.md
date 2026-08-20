# Phase 1 — MVP Rodando

**Status:** Complete (executada antes do padrão GSD formal de artefatos por fase)

**Data:** 2026-08-19 a 2026-08-20

**Entregue:**
- API .NET 8 (Clean Architecture) com `POST /api/auth/register`, `POST /api/auth/login` (JWT + BCrypt) e `GET /api/users` (protegido)
- Banco SQLite local (zero dependências, `InvariantGlobalization`)
- 12 testes automatizados verdes
- Smoke E2E via curl validado (201/409/400/200/401/200)

**Requisitos validados:** AUTH-01, AUTH-02, AUTH-03, USER-01, USER-02, QUAL-01, QUAL-02, QUAL-03

> Este diretório foi criado retroativamente em 2026-08-20 para sanar o W006 do health check (fase no ROADMAP sem diretório). Os artefatos formais (PLAN/SUMMARY) não existem porque a fase foi executada antes da adoção do fluxo GSD completo.
