---
phase: 02-docker-docs-extra-p-s-mvp
reviewed: 2026-08-20T02:01:19Z
depth: standard
files_reviewed: 12
files_reviewed_list:
  - .dockerignore
  - .env.example
  - .github/workflows/ci-cd.yml
  - .gitignore
  - ARCHITECTURE.md
  - Dockerfile
  - README.md
  - docker-compose.yml
  - src/DotnetUserManagementApi.Api/Program.cs
  - src/DotnetUserManagementApi.Api/appsettings.json
  - src/DotnetUserManagementApi.Infrastructure/DependencyInjection.cs
  - src/DotnetUserManagementApi.Infrastructure/DotnetUserManagementApi.Infrastructure.csproj
findings:
  critical: 1
  warning: 3
  info: 5
  total: 9
status: issues_found
---

# Phase 2: Code Review Report

**Reviewed:** 2026-08-20T02:01:19Z
**Depth:** standard
**Files Reviewed:** 12
**Status:** issues_found

## Summary

Reviewed the containerization, CI, documentation, and dual-provider persistence work of phase 02: `Dockerfile`, `docker-compose.yml`, `.dockerignore`, `.env.example`, CI workflow, `ARCHITECTURE.md`, `README.md`, and the Postgres/SQLite provider selection changes in `Program.cs`, `DependencyInjection.cs`, and `appsettings.json`.

The overall structure is sound: multi-stage Dockerfile is correct (layout-preserving COPYs, `--no-restore` publish, alpine runtime consistent with `InvariantGlobalization=true` in the Api csproj), the compose fail-fast pattern (`${VAR:?}`) works, the EF Core startup path correctly branches on `ConnectionStrings:Database`, and the CI pipeline builds and tests the full solution. Cross-checks performed: migrations exist in Infrastructure (so `Migrate()` works), `wwwroot/favicon.svg` exists (so the `/favicon.ico` route will not NRE), all NuGet package versions exist, `JwtOptions`/`JwtTokenService`/`BcryptPasswordHasher` are consistent with claims in docs, and the test factory's Development default satisfies the JWT key generation path.

**Key concern:** the fail-fast JWT key guard only checks *presence*, not *strength* — the publicly-versioned placeholder key from `.env.example` is accepted in Production, which enables trivial token forgery in the documented deployment flow (CR-01).

## Critical Issues

### CR-01: Placeholder/default JWT signing key accepted in Production — token forgery

**File:** `src/DotnetUserManagementApi.Api/Program.cs:22-35` (also `.env.example:6`, `docker-compose.yml:34`)
**Issue:** The only guard on `Jwt:Key` outside Development is `string.IsNullOrWhiteSpace`. The placeholder value documented in the repo — `JWT__KEY=changeme-troque-por-uma-chave-aleatoria-de-ao-menos-32-bytes` (`.env.example:6`) — is non-empty and therefore passes both the compose fail-fast (`${JWT__KEY:?}`, `docker-compose.yml:34`) and the startup check. A user following the documented flow (`cp .env.example .env` in `README.md:85`) without replacing the value starts the API in `Production` signing JWTs with a key that is **public in the repository**. Any attacker can forge a valid HS256 token for any user (arbitrary `sub`/`email`/`name`, arbitrary expiry) and access protected endpoints such as `GET /api/users`. This defeats the security guarantee claimed in `ARCHITECTURE.md:164` ("impossível subir o ambiente prod-like sem a chave"). Additionally, no minimum key length is enforced — a 1-byte key passes the guard and produces a trivially brute-forceable HMAC key. Same defect class applies to the `POSTGRES_PASSWORD=changeme` placeholder (`.env.example:10`) — acceptable only because the db port is not published, but it silently disables the security model if copied unchanged.

**Fix:** Validate key strength (and reject the known placeholder) before configuring authentication, e.g.:

```csharp
if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtOptions.Key = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        builder.Configuration["Jwt:Key"] = jwtOptions.Key;
        Console.WriteLine("[SECURITY] Jwt:Key não configurado. Chave aleatória gerada para esta execução (desenvolvimento local).");
    }
    else
    {
        throw new InvalidOperationException("JWT__KEY (Jwt:Key) é obrigatório em produção. Defina a variável de ambiente JWT__KEY antes de iniciar a API.");
    }
}
else if (!builder.Environment.IsDevelopment()
         && (jwtOptions.Key.Contains("changeme", StringComparison.OrdinalIgnoreCase)
             || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32))
{
    // Rejeita o placeholder versionado e chaves fracas — D-08: fail-fast real
    throw new InvalidOperationException("JWT__KEY (Jwt:Key) deve ter ao menos 32 bytes e não pode ser o valor de exemplo do .env.example.");
}
```

Consider also validating `POSTGRES_PASSWORD` against the `changeme` placeholder in the compose file or docs to make the "prod-like" deployment fail loudly instead of silently running with known credentials.

## Warnings

### WR-01: Docker final image runs as root

**File:** `Dockerfile:23-27`
**Issue:** The final stage has no `USER` directive, so the API process runs as root inside the container. The `aspnet:8.0-alpine` image ships a non-root `app` user (UID 1654) precisely for this purpose. Running as root amplifies the blast radius of any container compromise (RCE → host-level access via container escape vectors).
**Fix:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "DotnetUserManagementApi.Api.dll"]
```

### WR-02: CI workflow runs with overprivileged default GITHUB_TOKEN

**File:** `.github/workflows/ci-cd.yml:13-32`
**Issue:** The workflow declares no `permissions:`. On `push` to `main`, the default `GITHUB_TOKEN` carries read/write scopes for `contents`, which this build-only job never needs. A compromised dependency/step could then push commits or tamper with tags. The workflow correctly avoids repo secrets, but the token scope is still wider than necessary.
**Fix:** Add at workflow level:
```yaml
permissions:
  contents: read
```

### WR-03: `.dockerignore` does not exclude the local SQLite DB — dev PII enters build context

**File:** `.dockerignore:2-8` (with `Dockerfile:11`)
**Issue:** `.dockerignore` excludes `.env` with the explicit threat-model comment "segredos nunca entram nas camadas da imagem" (T-02-03), but `src/DotnetUserManagementApi.Api/app.db` — the gitignored local development database containing user emails and BCrypt password hashes — is not excluded and is copied into the build stage by `COPY src/ ./src/` (`Dockerfile:11`). It does not reach the final image (only the publish output is copied), but it persists in build-context and build-stage layers and is inconsistent with the file's own stated security intent.
**Fix:** Add the SQLite artifact to `.dockerignore`:
```
**/app.db
**/*.db
```

## Info

### IN-01: Template leftover `launchUrl: "weatherforecast"` in launchSettings

**File:** `README.md:29` (referenced file: `src/DotnetUserManagementApi.Api/Properties/launchSettings.json:16,26,34`)
**Issue:** All three launch profiles retain the default template `launchUrl: "weatherforecast"` and `launchBrowser: true`. This API has no `weatherforecast` route, so `dotnet run` opens the browser on a 404.
**Fix:** Remove `launchUrl` (or point it at `/swagger`).

### IN-02: `AllowedHosts: "*"` in Production

**File:** `src/DotnetUserManagementApi.Api/appsettings.json:8`
**Issue:** The compose deployment runs in `Production` but accepts any Host header. No absolute-URI generation was observed, so impact is limited, but the setting is unnecessarily permissive for a prod-like environment.
**Fix:** Restrict hosts, e.g. add `AllowedHosts__: "localhost"` (or a real domain) to the `api` service environment in `docker-compose.yml`.

### IN-03: Unused using directive

**File:** `src/DotnetUserManagementApi.Infrastructure/DependencyInjection.cs:8`
**Issue:** `using Npgsql.EntityFrameworkCore.PostgreSQL;` is unused — the `UseNpgsql` extension method lives in the `Microsoft.EntityFrameworkCore` namespace (already imported via `using Microsoft.EntityFrameworkCore;`).
**Fix:** Remove the directive.

### IN-04: Workflow named "ci-cd" but contains no CD; no job timeout

**File:** `.github/workflows/ci-cd.yml:1,14`
**Issue:** The file is named `ci-cd.yml` and the header comment says "CI — build + testes apenas, sem passos de deploy", but there is no CD stage — the name is misleading. Also, the job has no `timeout-minutes`, so a hung test run consumes the default 360-minute allowance.
**Fix:** Rename to `ci.yml` (or add the CD stage later); add `timeout-minutes: 15` to the job.

### IN-05: API service lacks a healthcheck in compose

**File:** `docker-compose.yml:25-40`
**Issue:** Only `db` has a healthcheck; the `api` service has none. `restart: unless-stopped` cannot react to an unresponsive API, and orchestration has no readiness signal for the app.
**Fix:** Add a healthcheck to the `api` service, e.g. `test: ["CMD", "wget", "-q", "--spider", "http://localhost:8080/favicon.ico"]` (or a dedicated `/health` endpoint).

---

_Reviewed: 2026-08-20T02:01:19Z_
_Reviewer: the agent (gsd-code-reviewer)_
_Depth: standard_