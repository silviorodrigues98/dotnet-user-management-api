---
phase: 02-docker-docs
plan: 01
subsystem: infra
tags: [docker, dockerfile, docker-compose, postgres, npgsql, ef-core, sqlite, jwt]

# Dependency graph
requires:
  - phase: 01-mvp
    provides: API .NET 8 funcional (register/login/users + JWT), AppDbContext com migração SQLite, TestWebAppFactory (UseSqlite)
provides:
  - Dockerfile multi-stage (sdk 8.0 → aspnet 8.0-alpine) da API
  - docker-compose prod-like: db postgres:16 + volume postgres_data + healthcheck, api Production na porta 5290:8080
  - Seleção dual-provider por ConnectionStrings:Database (Sqlite local | Postgres Docker) em AddInfrastructure e no startup
  - Fail-fast de Jwt:Key fora de IsDevelopment() (Program.cs) + ${JWT__KEY:?} no compose
  - .env.example com placeholders; .env gitignored; .dockerignore exclui segredos do build context
affects: [02-02 (docs/CI — README/ARCHITECTURE.md citarão o compose), verificação E2E pendente de Docker]

# Tech tracking
tech-stack:
  added: [Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11]
  patterns:
    - "Provider dual por chave explícita ConnectionStrings:Database (sem detecção de prefixo)"
    - "Init de banco por provider: Postgres → Migrate() com retry 10×/2s; local → EnsureCreated()"
    - "Fail-fast de segredos obrigatórios: Program.cs (InvalidOperationException) + compose (${VAR:?})"
    - "Config via env double-underscore (ConnectionStrings__X, Jwt__Key) — padrão dotnet nativo"

key-files:
  created: [Dockerfile, docker-compose.yml, .env.example, .dockerignore]
  modified: [src/DotnetUserManagementApi.Infrastructure/DotnetUserManagementApi.Infrastructure.csproj, src/DotnetUserManagementApi.Infrastructure/DependencyInjection.cs, src/DotnetUserManagementApi.Api/appsettings.json, src/DotnetUserManagementApi.Api/Program.cs, .gitignore]

key-decisions:
  - "Npgsql.EntityFrameworkCore.PostgreSQL pinado em 8.0.11 — 8.0.21 (linha dos demais pacotes EF) não existe no NuGet; 8.0.11 é a maior 8.0.x disponível"
  - "Base image final aspnet:8.0-alpine — permitida pois InvariantGlobalization=true dispensa libicu"
  - "db sem ports: publicados — PostgreSQL apenas na rede interna do compose (T-02-04); única porta exposta 5290:8080"
  - "Migrações no startup com retry 10×/2s capturando NpgsqlException — sem script wait-for externo (D-07)"

patterns-established:
  - "Provider branching explícito: uma chave de config dirige AddDbContext e o init de banco; default preserva SQLite zero-dependência (TestWebAppFactory intocado, 12 verdes)"
  - "Compose prod-like: ASPNETCORE_ENVIRONMENT=Production (Swagger off), persistência real, segredos via .env gitignored"

requirements-completed: [extras]

# Metrics
duration: 8min
completed: 2026-08-20
---

# Phase 2 Plan 1: Containerização (Docker + Compose + PostgreSQL 16) Summary

**Dual-provider de banco (SQLite local | PostgreSQL via Docker) com Dockerfile multi-stage, docker-compose prod-like postgres:16 e fail-fast de JWT__KEY — mantendo run local zero-dependência e os 12 testes verdes.**

## Performance

- **Duration:** 8 min
- **Started:** 2026-08-20T01:27:56Z
- **Completed:** 2026-08-20T01:35:36Z
- **Tasks:** 3 (2 com commits; Task 3 de verificação parcial — blocker de Docker documentado)
- **Files modified:** 9 (4 em Task 1, 5 em Task 2)

## Accomplishments

- Seleção dual-provider em `AddInfrastructure` guiada por `ConnectionStrings:Database` (`"Postgres"` → `UseNpgsql`; default → `UseSqlite`), com `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11 pinado (D-01/D-03/D-04)
- Fail-fast de `Jwt:Key`: branch por `IsDevelopment()` — Development mantém a chave aleatória atual; qualquer outro ambiente lança `InvalidOperationException` citando `JWT__KEY` (D-08, T-02-01, T-02-06)
- Init de banco por provider: Postgres → `Migrate()` com retry 10×/2s capturando `NpgsqlException`; local → `EnsureCreated()` (D-02/D-07)
- Dockerfile multi-stage (sdk 8.0 → aspnet 8.0-alpine) sem segredos (T-02-03); compose com db postgres:16 + volume nomeado `postgres_data` + healthcheck `pg_isready` + `depends_on: service_healthy` e api prod-like na porta 5290:8080 (D-05/D-06/D-07, T-02-04)
- `.env.example` versionado com placeholders (D-09); `.gitignore` com seção secrets; `.dockerignore` exclui `.env`/`.git`/`.planning` do build context (T-02-02/T-02-03)

## Task Commits

Cada task foi commitada atomicamente:

1. **Task 1: Dual-provider (SQLite local | PostgreSQL) no registro e no startup** - `87c9982` (feat)
2. **Task 2: Dockerfile multi-stage + docker-compose prod-like + .env.example + .dockerignore** - `b8f97e6` (feat)
3. **Task 3: Verificação E2E** - sem commit (task de verificação; contingência de migração não aplicável sem Docker; `.env` gitignored por design)

**Plan metadata:** `docs(02-01): complete containerization plan` (commit final de metadados)

## Files Created/Modified

- `Dockerfile` - Multi-stage: sdk 8.0 (restore/publish via solution/) → aspnet 8.0-alpine (sem libicu, `InvariantGlobalization`); EXPOSE 8080; sem ENV/ARG de segredo
- `docker-compose.yml` - Serviços `db` (postgres:16, volume `postgres_data`, healthcheck pg_isready) e `api` (build ., Production, `ConnectionStrings__Database=Postgres`, `Jwt__Key: ${JWT__KEY:?}`, porta 5290:8080, depends_on service_healthy)
- `.env.example` - Placeholders `JWT__KEY`/`POSTGRES_PASSWORD` (changeme) + instruções `cp .env.example .env`
- `.dockerignore` - Exclui `.env`, `.git`, `**/bin`, `**/obj`, `.planning`, `*.md`, `.playwright-mcp`
- `src/DotnetUserManagementApi.Infrastructure/DotnetUserManagementApi.Infrastructure.csproj` - PackageReference `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11 pinado
- `src/DotnetUserManagementApi.Infrastructure/DependencyInjection.cs` - Branch de provider (UseNpgsql/UseSqlite) + `using Npgsql.EntityFrameworkCore.PostgreSQL;`
- `src/DotnetUserManagementApi.Api/appsettings.json` - Chave `"Database": "Sqlite"` em ConnectionStrings
- `src/DotnetUserManagementApi.Api/Program.cs` - Fail-fast Jwt:Key (D-08) + init de banco por provider com retry (D-02/D-07); `using Npgsql;`
- `.gitignore` - Seção `# Docker compose secrets` com `.env`

## Decisions Made

- **Npgsql 8.0.11** — o plano pedia 8.0.21 (alinhado aos demais pacotes EF 8.0.21), mas essa versão não existe no NuGet para `Npgsql.EntityFrameworkCore.PostgreSQL`; usada a maior 8.0.x disponível, conforme instrução do próprio plano (verificado via api.nuget.org/v3-flatcontainer)
- **Base alpine** — `aspnet:8.0-alpine` escolhida (discretion do plano) porque `InvariantGlobalization=true` dispensa libicu
- **db sem porta publicada** — T-02-04: PostgreSQL restrito à rede interna do compose
- **Migração única preservada** — nenhuma regeneração de migração; a contingência `.HasColumnType("TEXT")` ficou documentada no plano e só seria aplicada se o fluxo Postgres falhasse por tipo de coluna (não testável sem Docker)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Versão Npgsql 8.0.21 inexistente no NuGet**
- **Found during:** Task 1 (Dual-provider)
- **Issue:** O plano mandava pinar `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.21, mas a versão não existe no feed (verificado com `dotnet package search` e api.nuget.org — 8.0.x máx. = 8.0.11)
- **Fix:** Pinação em **8.0.11** (maior 8.0.x disponível), conforme a própria contingência do plano ("use a versão 8.0.x mais alta disponível")
- **Files modified:** src/DotnetUserManagementApi.Infrastructure/DotnetUserManagementApi.Infrastructure.csproj
- **Verification:** `dotnet build` 0 erros; 12 testes verdes; `grep -c "Npgsql.EntityFrameworkCore.PostgreSQL"` = 1
- **Committed in:** 87c9982 (Task 1)

**2. [Rule 3 - Blocking] Docker ausente no ambiente — E2E do compose não executável**
- **Found during:** Task 3 (Verificação E2E), step 1 (preflight)
- **Issue:** `docker --version` e `docker compose version` falham — binário docker não instalado (nem systemd unit) no WSL2. O user_setup do plano prevê exatamente este caso: "Se ausente, reportar como blocker no SUMMARY"
- **Fix:** Executadas todas as verificações não-dependentes de Docker: `.env` real criado (openssl rand, valores nunca impressos), regressão local (`dotnet build` + `dotnet test` 12 verdes), smoke local (`dotnet run` Development → `GET /api/users` = 401, EnsureCreated no branch SQLite), prova do fail-fast (Production sem `JWT__KEY` → `InvalidOperationException: JWT__KEY (Jwt:Key) é obrigatório em produção...`, exit 134). Blocker registrado para o E2E (compose up, fluxo curl vs Postgres, persistência, `docker compose config` fail-fast)
- **Files modified:** .env (gitignored — nunca commitado)
- **Verification:** 12 testes verdes; smoke 401; fail-fast comprovado por log; `git status` sem `.env`
- **Committed in:** sem commit (verificação)

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** A pinação 8.0.11 é uma contingência prevista pelo próprio plano (sem impacto). O blocker de Docker impede apenas a validação E2E do compose — todo o código e a configuração estão entregues e verificados estaticamente (greps de aceite passando, YAML validado). O E2E fica como passo manual documentado no SUMMARY quando o Docker estiver disponível.

## Issues Encountered

- **Docker não instalado** (blocker): `command -v docker` vazio, `systemctl is-active docker` = inactive, unit inexistente. Por isso `docker compose config --quiet` (verify da Task 2) e todo o fluxo E2E da Task 3 (steps 4-8) não puderam ser executados. O YAML do compose foi validado com parser alternativo (python3/yaml: services db+api, volume postgres_data, db sem ports, api 5290:8080). Quando o Docker estiver disponível, rodar:

```bash
cp .env.example .env   # e preencher com openssl rand -hex 32 / -hex 16
docker compose up --build -d
# fluxo esperado: register 201 / duplicado 409 / login 200 / users 401 sem token / users 200 / / 200
docker compose restart api && <login novamente>  # persistência em postgres_data
mv .env .env.bak && docker compose config   # deve falhar citando JWT__KEY
mv .env.bak .env && docker compose config --quiet   # volta a passar
docker compose down   # sem -v, preserva o volume
```

## Known Stubs

None — nenhum stub introduzido. A contingência `.HasColumnType("TEXT")` em `AppDbContext.cs` **não** foi aplicada (só se o Postgres rejeitasse o tipo das colunas — não testável sem Docker); a migração única permanece válida para os dois providers.

## User Setup Required

**Docker é pré-requisito do E2E.** O user_setup do plano lista `docker` (compose funcional) como serviço necessário. Este ambiente não tem Docker instalado — a instalação (ex.: `sudo apt install docker.io` + daemon ativo) fica a cargo do usuário. Todos os arquivos já estão prontos: `cp .env.example .env` + `docker compose up --build` devem funcionar direto.

## Threat Flags

| Flag | File | Description |
|------|------|-------------|
| threat_flag: fail-fast-secret | src/DotnetUserManagementApi.Api/Program.cs | Novo caminho de throw `InvalidOperationException` fora de IsDevelopment() (T-02-01/T-02-06) — comportamento intencional, parte do threat_model do plano |
| threat_flag: env-secret-surface | docker-compose.yml, .env, .env.example | Interpolação `${JWT__KEY:?}`/`${POSTGRES_PASSWORD:?}` do .env do host para env vars de container (boundary "host filesystem → compose" do threat_model; mitigado por .gitignore + .dockerignore) |

## Self-Check: PASSED

- Arquivos verificados: Dockerfile, docker-compose.yml, .dockerignore, .env.example, Program.cs, SUMMARY.md — todos presentes
- Commits verificados: `87c9982` (Task 1) e `b8f97e6` (Task 2) — ambos no git log

## Next Phase Readiness

- Ready para **02-02** (docs + CI): README/ARCHITECTURE.md podem citar o compose (`docker compose up --build`, porta 5290) e o fluxo dual-provider; o CI (build + test) é independente de Docker
- **Blocker:** E2E do compose pendente de Docker instalado (ver Issues Encountered) — sem impacto no código entregue
- Contingência de migração `.HasColumnType("TEXT")` documentada no plano e no SUMMARY para o caso de erro de tipo de coluna no Postgres

---
*Phase: 02-docker-docs*
*Completed: 2026-08-20*