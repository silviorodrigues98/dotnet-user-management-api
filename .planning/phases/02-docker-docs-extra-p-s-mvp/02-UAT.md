---
status: complete
phase: 02-docker-docs-extra-p-s-mvp
source: 02-01-SUMMARY.md, 02-02-SUMMARY.md
started: 2026-08-20T15:17:06Z
updated: 2026-08-20T15:33:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Clear ephemeral state (temp DBs, caches, lock files). Start the application from scratch (`docker compose up --build -d`). Server boots without errors, migration completes (postgres:16 healthy), and a primary query (GET / on port 5290) returns live data.
result: pass

### 2. Docker Compose Up
expected: `docker compose up --build -d` brings up `db` (postgres:16, healthcheck pg_isready → healthy) and `api` (Production, ConnectionStrings__Database=Postgres, port 5290:8080). `docker compose ps` shows both up and healthy; `docker compose config --quiet` passes.
result: pass

### 3. Docker E2E Flow
expected: Against http://localhost:5290: POST register → 201; duplicate register → 201 (anti-enumeração T-01-10 — resposta uniforme, não 409); POST login → 200 with token; GET /api/users without token → 401; GET /api/users with token → 200; GET / → 200.
result: pass
note: "Duplicate returned 201 'Conta criada.' — expected 409 per SUMMARY E2E block, but code comment confirms T-01-10 anti-enumeration (uniform 201) is intentional design. SUMMARY/PLAN documentation is stale; no code defect."
expected: `docker compose restart api` (PostgreSQL persists in postgres_data volume). Logging in again with the same user works and the registered user is still listed — data survived the restart.
result: pass

### 5. Fail-fast JWT__KEY (compose config)
expected: Rename `.env` away (`mv .env .env.bak`) then `docker compose config` fails citing `JWT__KEY`. Restore `.env` and `docker compose config --quiet` passes again.
result: pass

### 6. Fail-fast JWT__KEY (Production run)
expected: Running the API outside Development without `JWT__KEY` (e.g. ASPNETCORE_ENVIRONMENT=Production, no Jwt__Key) exits with an `InvalidOperationException` citing `JWT__KEY` (Jwt:Key) — a clear, immediate error instead of a runtime auth failure.
result: pass

### 7. Local Zero-Dependency Run (SQLite)
expected: `dotnet run` in Development (default ConnectionStrings:Database=Sqlite) boots with SQLite via EnsureCreated — no Postgres/Docker needed. GET /api/users → 401, register/login work locally.
result: pass

### 8. Dockerfile Multi-stage Build
expected: `docker build .` succeeds with the multi-stage Dockerfile (sdk 8.0 → aspnet 8.0-alpine, EXPOSE 8080, no secrets/ENV of credentials baked in). Image builds without errors.
result: pass

### 9. ARCHITECTURE.md Deliverable
expected: ARCHITECTURE.md exists in PT-BR with 4 Mermaid diagrams (layer flowchart, auth sequenceDiagram, dual-provider flowchart, deploy graph), security section (fail-fast JWT__KEY, BCrypt, .env gitignored, Postgres no exposed port), CI/CD section, and mapping of the challenge criteria.
result: pass

### 10. CI Pipeline Config
expected: `.github/workflows/ci-cd.yml` exists with job build-test: checkout@v4, setup-dotnet@v4 8.0.x, restore → build Release → test Release; triggers push on main + pull_request; steps in PT-BR; no SonarQube, no secrets, no deploy steps.
result: pass

### 11. README Docker Section
expected: README.md has a "Rodar com Docker (PostgreSQL)" section with `cp .env.example .env` + `docker compose up --build`, API at http://localhost:5290, PostgreSQL 16 + volume postgres_data, and the decision table includes the dual-provider (SQLite/PostgreSQL) row.
result: pass

## Summary

total: 11
passed: 11
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

- truth: "SUMMARY 02-01 E2E block documents 'duplicado 409' for duplicate registration, but implemented behavior is uniform 201 (anti-enumeração T-01-10) — documentation is stale"
  status: resolved
  reason: "UAT found duplicate register returns 201 'Conta criada.'; UserService.cs:36-38 confirms T-01-10 anti-enumeration is intentional design. Docs (SUMMARY/PLAN E2E flow) should say 201, not 409."
  severity: minor
  test: 3
  resolution: "Fixed directly on 2026-08-20: updated 02-01-PLAN.md (3 lines), 02-01-SUMMARY.md, and 02-SECURITY.md to reflect uniform 201 (anti-enumeração T-01-10)."
  artifacts:
    - path: ".planning/phases/02-docker-docs-extra-p-s-mvp/02-01-SUMMARY.md"
      issue: "E2E flow documents 'duplicado 409' but design returns uniform 201"
  missing:
    - "Update SUMMARY/PLAN E2E documentation to reflect uniform 201 (T-01-10 anti-enumeration)"