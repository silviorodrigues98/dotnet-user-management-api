---
phase: 02-docker-docs
plan: 02
subsystem: docs
tags: [architecture, mermaid, github-actions, ci, docker, documentation]

# Dependency graph
requires:
  - phase: 02-docker-docs
    plan: 01
    provides: Dockerfile multi-stage, docker-compose prod-like (postgres:16, 5290:8080, postgres_data), dual-provider SQLite|Postgres, fail-fast JWT__KEY — tudo documentado pelo ARCHITECTURE.md
provides:
  - ARCHITECTURE.md (PT-BR, 4 diagramas Mermaid) — entregável do desafio da vaga (D-14)
  - .github/workflows/ci-cd.yml — pipeline CI build + dotnet test (push main + PRs, sem SonarQube)
  - README.md com seção "Rodar com Docker (PostgreSQL)" e tabela de decisões dual-provider
affects: [verificação E2E (Docker), futuras fases que citem a arquitetura/CI do repo]

# Tech tracking
tech-stack:
  added: [GitHub Actions (checkout@v4, setup-dotnet@v4), Mermaid diagrams]
  patterns:
    - "Docs PT-BR com diagramas Mermaid (arquitetura, fluxo, deploy) e tabelas markdown de decisões"
    - "Workflow CI sem secrets e sem deploy — build+test apenas, steps PT-BR, actions pinadas por major version"

key-files:
  created: [ARCHITECTURE.md, .github/workflows/ci-cd.yml]
  modified: [README.md]

key-decisions:
  - "Workflow CI sem nenhum ${{ secrets.* }}: build+test não manipulam segredos (T-02-07/T-02-09)"
  - "Nomes de step do workflow em PT-BR, consistente com D-12 (docs em português)"
  - "Comentário do workflow evita a palavra 'SonarQube' para satisfazer o critério estrito grep -c = 0 (D-10)"
  - "README: seção 'Próximos passos' substituída por 'Rodar com Docker (PostgreSQL)' — marca a fase como entregue"

patterns-established:
  - "Entregável de arquitetura: diagramas Mermaid + justificativas por nível + mapeamento dos critérios verbatim do desafio"
  - "CI minimalista e seguro: triggers push main + pull_request, sem filtro de path, sem deploy"

requirements-completed: [extras]

# Metrics
duration: 4min
completed: 2026-08-20
---

# Phase 2 Plan 2: Docs & CI (ARCHITECTURE.md + pipeline) Summary

**ARCHITECTURE.md em PT-BR com 4 diagramas Mermaid (camadas, fluxo de autenticação, dual-provider, deploy compose) como entregável do desafio da vaga, pipeline CI build+test (push main + PRs, sem SonarQube) e README com seção Docker — documentando a topologia REAL criada no plan 02-01.**

## Performance

- **Duration:** 4 min
- **Started:** 2026-08-20T01:44:20Z
- **Completed:** 2026-08-20T01:48:32Z
- **Tasks:** 2 (2 commits)
- **Files modified:** 3 (1 criado em Task 1, 1 criado em Task 2, 1 modificado em Task 1)

## Accomplishments

- `.github/workflows/ci-cd.yml` — workflow `CI` com job `build-test` (ubuntu-latest): checkout@v4, setup-dotnet@v4 `8.0.x`, `dotnet restore` → `build -c Release --no-restore` → `test -c Release --no-build` contra `solution/DotnetUserManagementApi.sln`; triggers `push: branches: [main]` + `pull_request:`; **sem SonarQube** (D-10), sem secrets e sem deploy (T-02-07/T-02-09); steps em PT-BR (D-12)
- `ARCHITECTURE.md` — entregável do desafio (D-14): 4 blocos ```` ```mermaid ```` (flowchart das 4 camadas Clean Architecture + justificativas por camada, `sequenceDiagram` do fluxo register/login/users com justificativas JWT HS256/BCrypt, flowchart do dual-provider com tabela comparativa SQLite vs Postgres, graph do deploy compose com env vars), seções de segurança (fail-fast `JWT__KEY`, BCrypt, `.env` gitignored, Postgres sem porta exposta), CI/CD com exemplo conceitual do YAML e mapeamento dos 3 critérios verbatim do `<specifics>` (D-14)
- `README.md` — seção "Próximos passos" (com menção ao SonarQube, linha 84) substituída por "Rodar com Docker (PostgreSQL)" (`cp .env.example .env` + `docker compose up --build`, API em `http://localhost:5290`, PostgreSQL 16 + volume `postgres_data`, linha marcando Dockerfile/compose, pipeline CI e ARCHITECTURE.md como entregues); tabela de decisões com a linha `EF Core dual-provider (SQLite local / PostgreSQL Docker)` (D-01)

## Task Commits

Cada task foi commitada atomicamente:

1. **Task 1: Workflow CI/CD (build + dotnet test) e atualização do README** - `b0a6c12` (feat)
2. **Task 2: ARCHITECTURE.md — entregável do desafio (camadas, fluxo de auth, deploy compose)** - `93fdd07` (docs)

## Files Created/Modified

- `.github/workflows/ci-cd.yml` - Workflow CI build+test: triggers push main + PRs (D-11), actions pinadas por major version (T-02-07), sem SonarQube (D-10), sem secrets/deploy; comandos idênticos ao README, contra o sln
- `ARCHITECTURE.md` - Entregável do desafio: 4 diagramas Mermaid, justificativas por nível, dual-provider, segurança, CI/CD e mapeamento dos critérios verbatim (D-12/D-13/D-14); 207 linhas
- `README.md` - Seção "Rodar com Docker (PostgreSQL)" no lugar de "Próximos passos" (remove SonarQube, D-10); linha dual-provider na tabela de decisões

## Decisions Made

- **Workflow sem secrets** — nenhum `${{ secrets.* }}` no job: build+test não precisam de segredos, o que elimina material a vazar (alinhado a T-02-07/T-02-09)
- **Steps em PT-BR** — "Checkout do código", "Instalar .NET SDK", "Restaurar pacotes", "Build (Release)", "Testes" — consistente com D-12
- **Comentário do workflow sem a palavra "SonarQube"** — o critério de aceite exigia `grep -c "SonarQube" = 0` no YAML; o comentário inicial mencionava "Sem SonarQube (D-10)" e foi reescrito para "build + dotnet test apenas"
- **README marca a fase como entregue** — em vez de "Próximos passos" listando itens ainda por fazer, a seção vira "Rodar com Docker" e declara Dockerfile/compose, CI e ARCHITECTURE.md como entregues

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Comentário do workflow citava "SonarQube" e quebrava critério estrito**
- **Found during:** Task 1 (verificação de acceptance criteria)
- **Issue:** O cabeçalho comentado do ci-cd.yml dizia "Sem SonarQube (D-10)" — semanticamente correto, mas o critério de aceite `grep -c "SonarQube" .github/workflows/ci-cd.yml` exige **0** ocorrências (retornou 1)
- **Fix:** Comentário reescrito para "Escopo: build + dotnet test apenas, sem passos de deploy" — sem a palavra SonarQube; o workflow em si já não continha nenhum passo de análise estática
- **Files modified:** .github/workflows/ci-cd.yml
- **Verification:** `grep -c "SonarQube"` = 0 no workflow e no README; CI local reproduzido com exit 0
- **Committed in:** b0a6c12 (Task 1)

---

**Total deviations:** 1 auto-fixed (1 bug de conformidade com critério de aceite)
**Impact on plan:** Nenhum — ajuste de wording em comentário; o comportamento do workflow (sem SonarQube) nunca mudou.

## Issues Encountered

- Nenhum. O único ajuste (comentário do workflow) foi tratado como deviação menor, acima.

## Known Stubs

None — fase de documentação/CI, sem código de produto. Nenhum stub introduzido.

## User Setup Required

None - sem configuração de serviços externos. (O E2E do compose continua pendente de Docker instalado — blocker herdado do plan 02-01, sem impacto nesta fase.)

## Threat Flags

Nenhum novo surface além do threat_model do plano:
- T-02-07 (supply chain das Actions) — mitigado por ações pinadas por major version e workflow sem secrets
- T-02-08 (segredos em docs) — mitigado: docs citam apenas nomes de variáveis (`JWT__KEY`, `POSTGRES_PASSWORD`), nenhum valor real em fenced blocks
- T-02-09 (código não confiável em PRs) — aceito: job roda apenas restore/build/test, sem secrets no ambiente

## Self-Check: PASSED

- Arquivos verificados: `.github/workflows/ci-cd.yml`, `ARCHITECTURE.md`, `README.md` — todos presentes no disco
- Commits verificados: `b0a6c12` (Task 1) e `93fdd07` (Task 2) — ambos no git log
- Critérios de aceite re-executados: mermaid ≥ 3 (4), sequenceDiagram presente, postgres_data presente, JWT__KEY presente (sem valor real), ConnectionStrings__Database presente, SonarQube = 0 em workflow/README/ARCHITECTURE, `wc -l` = 207 ≥ 120, CI local (restore→build→test) exit 0 com 12 testes verdes

## Next Phase Readiness

- **Phase 2 completa** (2/2 plans): todos os 3 success criteria do ROADMAP atendidos — `docker compose up --build` documentado e configurado (E2E pendente de Docker), ARCHITECTURE.md entregue (SC-2) e ci-cd.yml pronto (SC-3)
- **Blocker herdado:** E2E do compose (plan 02-01) pendente de Docker instalado no ambiente — sem impacto nos artefatos desta fase
- Próximo passo: verificação E2E do compose quando o Docker estiver disponível e/ou fechamento do milestone

---
*Phase: 02-docker-docs*
*Completed: 2026-08-20*