# dotnet-user-management-api

## What This Is

API de gerenciamento de usuários em .NET 8 (Clean Architecture) com cadastro, login seguro (JWT + BCrypt) e listagem de usuários em endpoint protegido. MVP direto ao ponto: rodar localmente com um comando.

## Core Value

O app precisa rodar: cadastro, login e listagem funcionando de ponta a ponta com autenticação JWT.

**Current State:** Phase 1 complete — MVP rodando com UI auditada e corrigida (3 blockers + polish a11y resolvidos via gap closure).

### Validated

- [x] **AUTH-01**: User can sign up with name, email and password — Validated in Phase 1
- [x] **AUTH-02**: User can log in with email/password and receive a JWT — Validated in Phase 1
- [x] **USER-01**: Authenticated user can list registered users — Validated in Phase 1
- [x] **QUAL-01**: Password hashing and error handling follow best practices — Validated in Phase 1

### Active

- (none)

### Out of Scope

- CI/CD execution — config pronta no repo, execução posterior
- Containerização — Dockerfile/Compose como extra depois do MVP funcionar
- Email de verificação / reset de senha — não exigidos no escopo básico
- OAuth / 2FA — fora do escopo
- Frontend framework — tela simples HTML estática

## Context

Desafio técnico de empresa de desenvolvimento (.NET). Ambiente WSL2 Ubuntu 26.04 sem slick de libicu, então o projeto usa InvariantGlobalization e o SDK é instalado local em `~/.dotnet`. Banco padrão local: SQLite (zero dependências).

## Constraints

- **Tech stack**: C#/.NET 8 LTS obrigatório — enunciado
- **Runtime**: SQLite local para rodar sem dependências externas — decisão de MVP
- **Security**: hash de senha com algoritmo forte e auth por JWT — enunciado
- **Time**: MVP direto ao ponto, sem infraestrutura de produção

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Clean Architecture | Camadas claras, testável, alinhada ao desafio | — Implemented |
| JWT (HS256) + BCrypt | API stateless, hash forte | — Implemented |
| EF Core + SQLite (local) | Zero dependências para rodar | — Implemented |
| InvariantGlobalization | Ambiente sem libicu; execução garantida | — Implemented |
| Conventional commits | Padronização exigida | — Implemented |
| Dual-provider (SQLite local \| PostgreSQL Docker) | Run local zero-dependência + compose prod-like com Postgres 16 | — Implemented (Phase 2) |
| Fail-fast JWT__KEY | Chave obrigatória fora de Development; placeholder/fraca rejeitada | — Implemented (Phase 2) |
| Docker compose prod-like | postgres:16 + volume + healthcheck, API non-root na porta 5290:8080 | — Implemented (Phase 2) |
| ARCHITECTURE.md (Mermaid, PT-BR) | Entregável do desafio | — Implemented (Phase 2) |
| CI build+test sem SonarQube | Pipeline pronto no repo (push main + PRs) | — Implemented (Phase 2) |
| UI single-file sem innerHTML | Sem XSS (textContent-only) — confirmado em code review | — Implemented (Phase 1) |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

---
*Last updated: 2026-08-20 after Phase 1 gap closure*