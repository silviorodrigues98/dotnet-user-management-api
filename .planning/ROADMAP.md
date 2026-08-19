# Roadmap: dotnet-user-management-api

**Created:** 2026-08-19
**Mode:** yolo | coarse | sequential

## Phase 1: MVP Rodando

**Goal:** API .NET 8 rodando com cadastro, login (JWT + BCrypt) e listagem protegida, com testes e documentação básica.

**Success Criteria:**
1. `dotnet run` inicia a API; `POST /api/auth/register` cria usuário
2. `POST /api/auth/login` retorna JWT válido
3. `GET /api/users` retorna 401 sem token e 200 com token
4. `dotnet test` passa; senha nunca armazenada em texto puro

**Requirements:** AUTH-01, AUTH-02, AUTH-03, USER-01, USER-02, QUAL-01, QUAL-02, QUAL-03

**Mode:** mvp

---

## Phase 2 (Extra / Pós-MVP): Docker & Docs

**Goal:** Empacotamento (Dockerfile multi-stage + docker-compose com PostgreSQL) e documentação de arquitetura para o desafio.

**Success Criteria:**
1. `docker compose up --build` sobe API + PostgreSQL com um comando
2. `ARCHITECTURE.md` documenta camadas, fluxo de autenticação e decisões
3. `.github/workflows/ci-cd.yml` pronto no repositório

**Requirements:** (extras — executados apenas se o MVP 100% validado)

---

**Coverage:** Todos os 8 requisitos v1 mapeados à Phase 1 ✓

---
*Last updated: 2026-08-19 after initial creation*