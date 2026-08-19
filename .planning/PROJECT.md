# dotnet-user-management-api

## What This Is

API de gerenciamento de usuários em .NET 8 (Clean Architecture) com cadastro, login seguro (JWT + BCrypt) e listagem de usuários em endpoint protegido. MVP direto ao ponto: rodar localmente com um comando.

## Core Value

O app precisa rodar: cadastro, login e listagem funcionando de ponta a ponta com autenticação JWT.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] **AUTH-01**: User can sign up with name, email and password
- [ ] **AUTH-02**: User can log in with email/password and receive a JWT
- [ ] **USER-01**: Authenticated user can list registered users
- [ ] **QUAL-01**: Password hashing and error handling follow best practices

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
| Clean Architecture | Camadas claras, testável, alinhada ao desafio | — Pending |
| JWT (HS256) + BCrypt | API stateless, hash forte | — Pending |
| EF Core + SQLite (local) | Zero dependências para rodar | — Pending |
| InvariantGlobalization | Ambiente sem libicu; execução garantida | — Pending |
| Conventional commits | Padronização exigida | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

---
*Last updated: 2026-08-19 after initialization*