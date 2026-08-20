# Phase 2: Docker & Docs (Extra / Pós-MVP) - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-19
**Phase:** 2-Docker & Docs (Extra / Pós-MVP)
**Areas discussed:** Estratégia de banco, Topologia compose, JWT no container, Escopo do CI/CD, ARCHITECTURE.md

---

## Estratégia de banco

| Option | Description | Selected |
|--------|-------------|----------|
| Dual-provider por config | Npgsql + provider escolhido por config; local SQLite, Docker Postgres | ✓ |
| Trocar tudo para PostgreSQL | Postgres único provider; local precisa de Postgres | |
| SQLite no container | Manter SQLite no container; só empacotar | |

| Option | Description | Selected |
|--------|-------------|----------|
| Chave de config explícita | `ConnectionStrings:Database=Postgres/Sqlite` no appsettings/compose | ✓ |
| Detecção por prefixo | Detectar pelo prefixo da connection string | |

| Option | Description | Selected |
|--------|-------------|----------|
| Migrações por provider | Duas pastas de migração (SQLite + Npgsql) | |
| SQLite migra / Postgres EnsureCreated | SQLite com Migrate, Postgres com EnsureCreated | |
| Postgres migra / SQLite EnsureCreated | Npgsql com Migrate; SQLite local com EnsureCreated | ✓ |

**User's choice:** Dual-provider por chave de config explícita; PostgreSQL (Docker) usa migrações Npgsql; SQLite local usa EnsureCreated.
**Notes:** Adicionar pacote Npgsql.EntityFrameworkCore.PostgreSQL; seleção em AddInfrastructure.

---

## Topologia compose

| Option | Description | Selected |
|--------|-------------|----------|
| Compose prod-like | Postgres com volume nomeado, healthcheck, sem Swagger | ✓ |
| Compose dev com Swagger | Swagger ativo, código montado | |
| Dois composes (dev/prod) | Dev + prod separados | |

| Option | Description | Selected |
|--------|-------------|----------|
| PostgreSQL 16 | LTS, compatível com EF Core 8 + Npgsql | ✓ |
| PostgreSQL 17 | Mais recente, menos adoção | |
| PostgreSQL 15 | Conservador, fim de ciclo | |

| Option | Description | Selected |
|--------|-------------|----------|
| Healthcheck + retry no app | pg_isready + service_healthy + retry de migração | ✓ |
| Script wait-for-db | Entrypoint aguardando pg_isready | |
| Sem healthcheck | API tenta migrar e reinicia | |

| Option | Description | Selected |
|--------|-------------|----------|
| Volume nomeado | `postgres_data` para persistência | ✓ |
| Bind mount | `./data` acoplado ao host | |
| Sem persistência | Dados descartados a cada recreate | |

**User's choice:** Compose prod-like; PostgreSQL 16; healthcheck + retry no app; volume nomeado.

---

## JWT no container

| Option | Description | Selected |
|--------|-------------|----------|
| Env obrigatória, fail-fast | `JWT__KEY` obrigatória; API falha sem ela | ✓ |
| Env com fallback dev | Fallback para geração aleatória em Development | |
| Docker secrets | Chave via arquivo montado | |

| Option | Description | Selected |
|--------|-------------|----------|
| .env.example + .gitignore | Placeholder versionado; .env real gitignored | ✓ |
| Interpolação ${VAR:?} | Exigência via interpolação do compose | |
| .env commitado | Chave dev versionada (anti-pattern) | |

**User's choice:** Env obrigatória com fail-fast; `.env.example` versionado, `.env` gitignored.

---

## Escopo do CI/CD

| Option | Description | Selected |
|--------|-------------|----------|
| Build+test+SonarQube | Com análise estática | ✓ (inicialmente) |
| Build+test só | Sem análise estática | ✓ (final) |
| Com build/push de imagem | Inclui push da imagem Docker | |

| Option | Description | Selected |
|--------|-------------|----------|
| main + PRs | Push para main + pull requests | ✓ |
| Só main | Apenas push em main | |
| Só PRs | Apenas pull requests | |

| Option | Description | Selected |
|--------|-------------|----------|
| SonarCloud | sonarcloud.io com SONAR_TOKEN | |
| SonarQube self-hosted | SONAR_HOST_URL + secrets | |
| Deixar para depois | Sem integração agora | |

**User's choice:** Build + `dotnet test` apenas. Sem SonarQube — o usuário cancelou a análise estática ("cancele, sem sonar qube").
**Notes:** Remover menção a SonarQube do escopo/README do pipeline. Gatilhos: push para main + PRs.

---

## ARCHITECTURE.md

| Option | Description | Selected |
|--------|-------------|----------|
| Mermaid + decisões | Diagramas Mermaid + camadas + decisões | ✓ |
| Texto corrido | Sem diagramas | |
| Mermaid + ASCII | Diagramas + fallback ASCII | |

| Option | Description | Selected |
|--------|-------------|----------|
| Português | Consistente com README e código | ✓ |
| Inglês | Padrão para artefatos técnicos | |
| Híbrido | PT-BR com jargão técnico | |

| Option | Description | Selected |
|--------|-------------|----------|
| Incluir deployment Docker | Diagrama de deployment do compose | ✓ |
| Escopo mínimo do roadmap | Só camadas, fluxo e decisões | |

**User's choice:** Mermaid + decisões, em Português, incluindo deployment do compose. ARCHITECTURE.md é **entregável do teste da vaga** — usuário forneceu os critérios de entrega (diagramas da arquitetura e fluxo de auth; justificativas por nível; exemplos conceituais de trechos críticos se necessário).

---

## the agent's Discretion

- Base image do Dockerfile multi-stage (SDK vs runtime, distro, `InvariantGlobalization=true` permite imagem sem libicu).
- Mapeamento de portas expostas, nomes de serviços no compose, estrutura interna do workflow YAML.
- Healthcheck HTTP da API e detalhes do retry de migração.

## Deferred Ideas

Nenhuma — discussão permaneceu dentro do escopo da fase.