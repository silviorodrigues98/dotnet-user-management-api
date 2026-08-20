---
phase: 02
slug: docker-docs-extra-p-s-mvp
status: verified
threats_open: 0
asvs_level: 1
created: 2026-08-20
---

# SECURITY.md — Phase 02: Docker, Docs & CI (dotnet-user-management-api)

**Phase:** 02 — docker-docs-extra-p-s-mvp
**Audit date:** 2026-08-20
**ASVS Level:** L1
**Block-on config:** `critical`
**Threats closed:** 9/9
**Open threats:** 0

## Threat Verification

| Threat ID | Category | Disposition | Status | Evidence |
|-----------|----------|-------------|--------|----------|
| T-02-01 | Spoofing / Tampering — chave de assinatura JWT (`JWT__KEY` / `Jwt:Key`) | mitigate | CLOSED | `Program.cs:22-42` fail-fast: fora de `IsDevelopment()` lança `InvalidOperationException` se chave vazia, contém `changeme` (placeholder versionado do `.env.example`) ou < 32 bytes UTF-8 (CR-01 fix `fd8274a`); `docker-compose.yml:34` `Jwt__Key: ${JWT__KEY:?}`; `.env.example:6` apenas placeholder; `appsettings.json:13-17` seção `Jwt` sem `Key`; `Dockerfile` sem ENV/ARG de segredo; `.gitignore:39` exclui `.env` (confirmado: `.env` não está no `git ls-files`) |
| T-02-02 | Information Disclosure — credenciais do PostgreSQL (`POSTGRES_PASSWORD`) | mitigate | CLOSED | `.env.example:10` apenas placeholder `changeme`; `docker-compose.yml:10,33` interpolação `${POSTGRES_PASSWORD:?}` (nunca hardcoded); `.dockerignore:2` exclui `.env` do build context; `.gitignore:39` exclui `.env` do versionamento; `.env` não rastreado por git |
| T-02-03 | Tampering — segredos em camadas da imagem Docker | mitigate | CLOSED | `.dockerignore:2-12` exclui `.env`, `.git`, `.planning`, `*.md`, `**/app.db`, `**/*.db` (WR-03 fix `5efb737`); `Dockerfile:1-29` sem nenhum `ENV`/`ARG` (config injetada em runtime via `environment:` do compose); `docker-compose.yml:27-34` |
| T-02-04 | Information Disclosure — porta do PostgreSQL exposta ao host | mitigate | CLOSED | `docker-compose.yml:23` comentário explícito "T-02-04: sem ports:" — serviço `db` sem bloco `ports:`; única porta publicada é `5290:8080` da API (`docker-compose.yml:36`) |
| T-02-05 | Tampering — dependência `Npgsql.EntityFrameworkCore.PostgreSQL` (supply chain) | accept | CLOSED (documented) | `DotnetUserManagementApi.Infrastructure.csproj:10` versão **8.0.11 pinada exata** (sem wildcard; maior 8.0.x existente no NuGet — 8.0.21 não existe). Registrada no accepted risks log abaixo |
| T-02-06 | Elevation of Privilege — bypass do fail-fast de JWT em ambientes não-Development | mitigate | CLOSED | `Program.cs:24,36` usa `builder.Environment.IsDevelopment()` (não config flag) em ambos os guards; o throw ocorre na construção do builder, antes de `app.Run()` — nenhuma requisição é servida em ambiente não-Development sem chave válida |
| T-02-07 | Tampering — supply chain das GitHub Actions | mitigate | CLOSED | `.github/workflows/ci-cd.yml:22,25` actions pinadas por major version (`checkout@v4`, `setup-dotnet@v4`); zero `${{ secrets.* }}`; sem passo de deploy (restore/build/test apenas, linhas 29-36); `permissions: contents: read` (WR-02 fix `0b1c795`, linhas 9-10) |
| T-02-08 | Information Disclosure — segredos vazando em ARCHITECTURE.md / README.md | mitigate | CLOSED | `ARCHITECTURE.md:149-167` e `README.md:81-95` citam apenas NOMES de variáveis (`JWT__KEY`, `POSTGRES_PASSWORD`) e o fluxo `cp .env.example .env` com instrução `openssl rand`; `git grep` por hex de 64 chars em `*.md/*.yml/*.json/*.cs` = 0; `changeme` presente apenas em `.env.example:6,10` (placeholders) e `Program.cs:37` (checagem de rejeição) |
| T-02-09 | Elevation of Privilege — execução de código não confiável em PRs (trigger `pull_request`) | accept | CLOSED (documented) | `.github/workflows/ci-cd.yml:29-36` job roda apenas restore/build/test do código do PR; sem secrets no ambiente do job; sem passos de publish/upload. Registrada no accepted risks log abaixo |

## Accepted Risks Log

| Threat ID | Risk | Rationale | Review trigger |
|-----------|------|-----------|----------------|
| T-02-05 | Pacote NuGet `Npgsql.EntityFrameworkCore.PostgreSQL` pode conter código comprometido (supply chain) | Pacote do ecossistema oficial Npgsql, restaurado de nuget.org, versão 8.0.11 pinada exatamente (convenção do repo, sem wildcard); restore sem segredos. Risco residual baixo. | Reavaliar em bump de versão; auditar caso o pacote seja removido/descontinuado no NuGet |
| T-02-09 | PRs abertos por terceiros executam código (restore/build/test) no runner `ubuntu-latest` | Job não usa `${{ secrets.* }}`, não faz publish/upload nem manipula tags (GITHUB_TOKEN com `contents: read`); build+test não manipulam material secreto. Risco residual baixo e aceito pelo projeto. | Reavaliar se o pipeline ganhar deploy, secrets ou passos de upload |

## Unregistered Flags

Nenhum. Os dois threat flags do `02-01-SUMMARY.md` mapeiam para threats existentes do register:

- `threat_flag: fail-fast-secret` (`Program.cs`) → T-02-01 / T-02-06 (comportamento intencional do threat_model)
- `threat_flag: env-secret-surface` (`docker-compose.yml`, `.env`, `.env.example`) → boundary "host filesystem → compose" do threat_model (T-02-02 / T-02-03)

O `02-02-SUMMARY.md` declara explicitamente nenhum novo surface além do threat_model do plano.

## Verification Notes

- **E2E do compose (human blocker, não é gap de mitigação):** Docker não está instalado neste ambiente; o fluxo E2E (`docker compose up --build`, curl vs Postgres, persistência, fail-fast do compose) permanece pendente de verificação humana. Todas as mitigações de ameaça são **artefato-level** e foram verificadas no conteúdo dos arquivos (greps + leitura integral). A prova do fail-fast do `Program.cs` foi executada em produção simulada sem `JWT__KEY` (exit 134 com a mensagem esperada, conforme 02-01-SUMMARY.md).
- Working tree limpo no momento da auditoria; os fixes CR-01 (`fd8274a`), WR-01 (`87db56f`), WR-02 (`0b1c795`), WR-03 (`5efb737`) estão presentes nos arquivos verificados.
- Correlação de código-review: CR-01, WR-01, WR-02, WR-03 verificados fechados nos artefatos; as findings IN-01..IN-05 (info) não integram o threat_model e ficam fora do escopo desta auditoria.

## Resultado

**9/9 threats CLOSED** — 7 mitigadas + 2 aceitas documentadas. Nenhum bloqueador (block_on: critical).