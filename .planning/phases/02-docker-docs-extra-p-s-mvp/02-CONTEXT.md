# Phase 2: Docker & Docs (Extra / Pós-MVP) - Context

**Gathered:** 2026-08-19
**Status:** Ready for planning

<domain>
## Phase Boundary

Empacotar a API em Docker (Dockerfile multi-stage + docker-compose com PostgreSQL 16), documentar a arquitetura em `ARCHITECTURE.md` (entregável do teste da vaga) e deixar pipeline CI/CD pronto no repositório. O MVP (Phase 1) está completo e validado — esta fase não adiciona funcionalidades de produto.

</domain>

<decisions>
## Implementation Decisions

### Estratégia de banco (SQLite local vs PostgreSQL no Docker)
- **D-01:** Dual-provider selecionado por chave de config explícita (ex.: `ConnectionStrings:Database` = `Sqlite` ou `Postgres`), não por detecção de prefixo.
- **D-02:** SQLite (local) usa `EnsureCreated` no startup; PostgreSQL (Docker) aplica migrações EF Core (Npgsql) no startup.
- **D-03:** Adicionar pacote `Npgsql.EntityFrameworkCore.PostgreSQL`; a seleção do provider acontece em `AddInfrastructure` (`src/DotnetUserManagementApi.Infrastructure/DependencyInjection.cs`).
- **D-04:** Run local continua "zero dependências" com SQLite — comportamento atual preservado por padrão quando a config não indica Postgres.

### Topologia do docker-compose
- **D-05:** Compose orientado a produção (prod-like): sem Swagger, persistência real.
- **D-06:** PostgreSQL 16 com volume nomeado (`postgres_data`) para persistência.
- **D-07:** Aguardar banco via healthcheck (`pg_isready`) + `depends_on: condition: service_healthy` + retry de migração no startup da API (sem script wait-for externo).

### JWT no container
- **D-08:** `JWT__KEY` (ou `Jwt__Key`) via variável de ambiente **obrigatória** com fail-fast na inicialização se ausente em produção.
- **D-09:** `.env.example` versionado com placeholder; `.env` real gitignored.

### Escopo do CI/CD
- **D-10:** Pipeline **build + `dotnet test` apenas** — **sem SonarQube** (cancelado pelo usuário).
- **D-11:** Gatilhos: push para `main` + pull requests.

### ARCHITECTURE.md
- **D-12:** Escrito em **Português (PT-BR)**, consistente com README e código.
- **D-13:** Diagramas **Mermaid** cobrindo arquitetura, fluxo de autenticação e deployment do compose (API + Postgres).
- **D-14:** **Entregável do teste da vaga** — seguir os critérios de entrega fornecidos pelo usuário (ver `<specifics>`), incluindo justificativas das tecnologias/padrões em cada nível e exemplos conceituais de trechos críticos apenas se necessário.

### the agent's Discretion
- Base image do Dockerfile multi-stage (SDK vs runtime, distro, `InvariantGlobalization=true` permite imagem sem libicu) — sem preferência do usuário.
- Mapeamento de portas expostas, nomes de serviços no compose, estrutura interna do workflow YAML.
- Healthcheck HTTP da API (se houver) e detalhes do retry de migração.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Fase atual (ROADMAP)
- `.planning/ROADMAP.md` §Phase 2 — Goal e Success Criteria (3 critérios: `docker compose up --build` sobe API+Postgres; ARCHITECTURE.md documenta camadas/fluxo/decisões; `.github/workflows/ci-cd.yml` pronto).

### Requisitos e escopo
- `.planning/REQUIREMENTS.md` — v1 completo (8/8); "Out of Scope" marca CI/CD execution e containerização como extras pós-MVP.

### Decisões de arquitetura já feitas
- `README.md` — "Decisões técnicas principais" (Clean Architecture, JWT HS256, BCrypt work factor 12, EF Core + SQLite, chave JWT gerada em runtime — nunca versionada) e "Próximos passos" que motivam esta fase.
- `src/DotnetUserManagementApi.Infrastructure/DependencyInjection.cs` — `AddInfrastructure` com `UseSqlite` fixo (ponto de mudança do D-01).
- `src/DotnetUserManagementApi.Api/Program.cs` — `dbContext.Database.Migrate()` no startup (ponto de mudança do D-02) e geração de `Jwt:Key` em runtime quando ausente (base do D-08).
- `src/DotnetUserManagementApi.Api/appsettings.json` — `ConnectionStrings:Default` (SQLite) e bloco `Jwt` (Issuer/Audience/ExpirationMinutes).

### Entregável do teste da vaga (fornecido pelo usuário na discussão)
- Critérios de entrega do ARCHITECTURE.md (sem arquivo físico — capturados verbatim no `<specifics>`). Documentar no próprio `ARCHITECTURE.md` como guia.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `DependencyInjection.cs` (`AddInfrastructure`): único ponto de registro de DbContext — centralizar a seleção de provider aqui.
- `Program.cs` startup: bloco de migração já existe; adaptar para branch por provider + retry.
- `appsettings.json` + `appsettings.Development.json`: config por ambiente já estruturada (dotnet env override funciona nativamente com `Jwt__Key`).
- `solution/DotnetUserManagementApi.sln` + `Directory.Build.props`: ponto de referência para build no Dockerfile e CI.
- `.gitignore`: já ignora artefatos SQLite — adicionar `.env` e volume/bind de banco se necessário.

### Established Patterns
- Clean Architecture em 4 camadas (Api/Application/Domain/Infrastructure) + tests — Dockerfile multi-stage deve espelhar essa estrutura.
- `InvariantGlobalization=true` no csproj — permite base image leve sem dependência de libicu no runtime.
- Conventional commits (PT-BR) e docs do planejamento em PT-BR.

### Integration Points
- `DependencyInjection.cs` linha do `UseSqlite` — troca por seleção por config.
- `Program.cs` bloco `Database.Migrate()` — branch por provider + retry.
- `appsettings.json` — adicionar chave `ConnectionStrings:Database` e sobrescrever `ConnectionStrings:Default` por env no compose.
- `.gitignore` — adicionar `.env`.

</code_context>

<specifics>
## Specific Ideas

### Critérios de entrega do ARCHITECTURE.md (verbatim do usuário — teste da vaga)
1. Enviar um arquivo `ARCHITECTURE.md` (ou PDF bem estruturado).
2. O documento deve conter:
   - Diagramas (pode usar Mermaid.js, PlantUML ou descrição textual clara) da arquitetura e do fluxo de autenticação.
   - Justificativas claras para as tecnologias e padrões escolhidos em cada nível que decidir abordar.
   - Exemplos conceituais de trechos críticos de código ou scripts (ex.: configuração de pipeline, mapeamento de entidades ou middleware de segurança) apenas se julgar necessário para ilustrar a ideia.

</specifics>

<deferred>
## Deferred Ideas

None — discussão permaneceu dentro do escopo da fase.

</deferred>

---

*Phase: 2-Docker & Docs (Extra / Pós-MVP)*
*Context gathered: 2026-08-19*