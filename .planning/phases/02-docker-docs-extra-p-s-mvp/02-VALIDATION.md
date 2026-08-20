---
phase: 02
slug: docker-docs-extra-p-s-mvp
status: compliant
nyquist_compliant: true
wave_0_complete: true
created: 2026-08-20
---

# Phase 2 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.5.3 |
| **Config file** | tests/DotnetUserManagementApi.Tests/DotnetUserManagementApi.Tests.csproj |
| **Quick run command** | `dotnet test --filter {Class}` |
| **Full suite command** | `dotnet test solution/DotnetUserManagementApi.sln` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test --filter ProviderSelectionTests|JwtFailFastTests|ContainerArtifactsTests|DocumentationArtifactsTests`
- **After every plan wave:** Run `dotnet test solution/DotnetUserManagementApi.sln`
- **Before `/gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 10 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 02-01-01 | 01 | 1 | extras (D-01/D-03/D-04) | T-02-01 | `ConnectionStrings:Database="Postgres"` (OrdinalIgnoreCase) → UseNpgsql; ausente/`Sqlite`/qualquer outro → UseSqlite (default local preservado) | unit | `dotnet test --filter ProviderSelectionTests` | ✅ | ✅ green |
| 02-01-01 | 01 | 1 | extras (D-08) | T-02-01 / T-02-06 | Fail-fast: fora de Development, `Jwt:Key` ausente/placeholder (`changeme`)/<32 bytes → `InvalidOperationException` citando `JWT__KEY`; Development gera chave aleatória 64-byte | integration | `dotnet test --filter JwtFailFastTests` | ✅ | ✅ green |
| 02-01-01 | 01 | 1 | extras (D-02/D-07) | — | Init de banco por provider: Postgres → `Migrate()` com retry 10×/2s; local → `EnsureCreated()` | manual | — | — | 🟡 manual-only (requer PostgreSQL/Docker) |
| 02-01-02 | 01 | 1 | extras (D-05/D-06/D-09) | T-02-02 / T-02-03 / T-02-04 | Artefatos Docker: multi-stage sdk:8.0→aspnet:8.0-alpine, EXPOSE 8080, sem ENV/ARG de segredo; compose postgres:16 + volume postgres_data + pg_isready + service_healthy + Production + `${JWT__KEY:?}` + db sem porta publicada; `.env.example` só placeholders; `.dockerignore`/`.gitignore` excluem `.env` | unit | `dotnet test --filter ContainerArtifactsTests` | ✅ | ✅ green |
| 02-01-03 | 01 | 1 | SC-1 ROADMAP (compose sobe API+PG) | T-02-01 / T-02-04 | E2E real: `docker compose up --build -d`, fluxo curl register/login/users, persistência pós-restart, fail-fast `docker compose config` | manual | — | — | 🟡 manual-only (requer Docker daemon) |
| 02-02-01 | 02 | 2 | extras (D-10/D-11) | T-02-07 / T-02-09 | Workflow CI: push main + pull_request, setup-dotnet@v4 `8.0.x`, `dotnet test --no-build`; sem SonarQube, sem `${{ secrets.* }}`; README seção "Rodar com Docker" + dual-provider na tabela, sem SonarQube | unit | `dotnet test --filter DocumentationArtifactsTests` | ✅ | ✅ green |
| 02-02-02 | 02 | 2 | extras (D-12/D-13/D-14) | T-02-08 | ARCHITECTURE.md: ≥3 blocos ```mermaid, sequenceDiagram do fluxo de auth, postgres_data, JWT__KEY/ConnectionStrings__Database, ≥120 linhas, sem valor real de segredo | unit | `dotnet test --filter DocumentationArtifactsTests` | ✅ | ✅ green |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky · 🟡 manual-only*

---

## Wave 0 Requirements

- [x] `tests/DotnetUserManagementApi.Tests/ProviderSelectionTests.cs` — dual-provider (5 testes)
- [x] `tests/DotnetUserManagementApi.Tests/JwtFailFastTests.cs` — fail-fast Jwt:Key (4 testes)
- [x] `tests/DotnetUserManagementApi.Tests/ContainerArtifactsTests.cs` — invariantes Docker (12 testes)
- [x] `tests/DotnetUserManagementApi.Tests/DocumentationArtifactsTests.cs` — CI + docs (9 testes)
- [x] `tests/DotnetUserManagementApi.Tests/RepoPath.cs` — helper de resolução da raiz do repo

*Wave 0 completa: 30 testes novos + 16 existentes = 46 verdes.*

---

## Manual-Only Verifications

[none — both items verified live via Docker compose stack at 2026-08-20]

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 10s
- [x] `nyquist_compliant: true` set in frontmatter (both manual-only items verified via Docker E2E UAT at 2026-08-20)

**Approval:** approved 2026-08-20