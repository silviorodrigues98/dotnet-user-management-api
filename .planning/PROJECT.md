# dotnet-user-management-api

## What This Is

API de gerenciamento de usuários em .NET 8 (Clean Architecture) com cadastro, login seguro (JWT + BCrypt), listagem de usuários em endpoint protegido, containerização Docker (PostgreSQL 16), pipeline CI e documentação de arquitetura (ARCHITECTURE.md com Mermaid). MVP shipped v1.0.

## Core Value

O app precisa rodar: cadastro, login e listagem funcionando de ponta a ponta com autenticação JWT, localmente (SQLite, zero dependências) ou via Docker (PostgreSQL 16).

### Validated

- ✓ AUTH-01: Sign up with name, email and password — v1.0
- ✓ AUTH-02: Log in and receive JWT — v1.0
- ✓ AUTH-03: Protected endpoints reject invalid token (401) — v1.0
- ✓ USER-01: Authenticated user lists registered users — v1.0
- ✓ USER-02: Registered email is unique — v1.0
- ✓ QUAL-01: Passwords stored hashed (BCrypt, never plaintext) — v1.0
- ✓ QUAL-02: API returns structured RFC 7807 error responses — v1.0
- ✓ QUAL-03: Core paths covered by automated tests — v1.0
- ✓ Containerização: Dockerfile multi-stage + compose prod-like (PostgreSQL 16) — v1.0
- ✓ CI/CD pipeline: build + `dotnet test` (push main + PRs) — v1.0
- ✓ ARCHITECTURE.md: entregável do desafio (Mermaid, PT-BR, 4 diagramas) — v1.0

### Active

- (none — milestone complete, v1.0 shipped)

### Out of Scope

- Email verification / password reset — not required for basic scope
- OAuth / 2FA — out of scope
- Frontend framework — static HTML sufficient
- Refresh tokens / token revocation — v2 proposal (AUTH-06)
- User profile editing — v2 proposal (USER-03)

## Context

Desafio técnico de empresa de desenvolvimento (.NET). Ambiente WSL2 Ubuntu 26.04 sem libicu — projeto usa InvariantGlobalization, SDK .NET 8 instalado em `~/.dotnet`. Docker 29.7.2 + compose v5.5.0 instalado e verificado E2E. 73 commits, 2158 LOC C#, 46 testes verdes (xUnit), dual-provider (SQLite local | PostgreSQL via Docker), CI via GitHub Actions.

## Known Tech Debt (10 items)

| Item | Phase | Severity |
|------|-------|----------|
| JwtBearer 401 empty body (RFC 7807 inconsistency) | 01 | warning |
| Concurrent duplicate race → 500 (UserService.cs:32-33) | 01 | warning |
| README `dotnet test` path broken from repo root | 01 | warning |
| README line 55 claims 409 (current: uniform 201) | 01 | low |
| 02-01-SUMMARY.md stale Docker-absent claims | 02 | low |
| Migration snapshot drift after ValueConverter fix | 02 | warning |
| ConflictException dead code | 02 | low |
| launchSettings.json launchUrl leftover | 02 | low |
| POSTGRES_PASSWORD=changeme placeholder | 02 | low |
| api service lacks compose healthcheck | 02 | low |

## Constraints

- **Tech stack**: C#/.NET 8 LTS obrigatório — enunciado
- **Runtime**: SQLite local para rodar sem dependências externas
- **Security**: hash de senha BCrypt e auth por JWT — enunciado
- **Time**: MVP direto ao ponto, sem infraestrutura de produção

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Clean Architecture | Camadas claras, testável, alinhada ao desafio | ✓ Good |
| JWT (HS256) + BCrypt | API stateless, hash forte | ✓ Good |
| EF Core + SQLite (local) | Zero dependências para rodar | ✓ Good |
| InvariantGlobalization | Ambiente sem libicu | ✓ Good |
| Conventional commits | Padronização exigida | ✓ Good |
| Dual-provider (SQLite local \| PostgreSQL Docker) | Run local + compose prod-like | ✓ Good |
| Fail-fast JWT__KEY | Chave obrigatória em Production | ✓ Good |
| Docker compose prod-like | postgres:16, non-root, volume, healthcheck | ✓ Good |
| ARCHITECTURE.md (Mermaid, PT-BR) | Entregável do desafio | ✓ Good |
| CI build+test sem SonarQube | Pipeline pronto no repo | ✓ Good |
| UI single-file sem innerHTML | XSS-free (textContent-only) | ✓ Good |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

---
*Last updated: 2026-08-20 after v1.0 milestone close*
