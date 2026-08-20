# Phase 2: Docker & Docs (Extra / Pós-MVP) - Pattern Map

**Mapped:** 2026-08-19
**Files analyzed:** 11 (6 new, 5 modified)
**Analogs found:** 6 / 11 (5 self-analogs for modified files + README.md as doc-style analog for ARCHITECTURE.md)

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `Dockerfile` | config | batch (build) | — (no Dockerfile exists) | none |
| `docker-compose.yml` | config | orchestration | — (no compose exists) | none |
| `.env.example` | config | — | `src/DotnetUserManagementApi.Api/appsettings.json` (config key source) | partial (same config surface) |
| `.github/workflows/ci-cd.yml` | config | batch (CI) | — (no workflow exists) | none |
| `ARCHITECTURE.md` | doc | — | `README.md` (style + PT-BR conventions) | role-match |
| `src/DotnetUserManagementApi.Infrastructure/DependencyInjection.cs` | config (DI registration) | config | itself (lines 13-23, `AddInfrastructure`) | exact (self) |
| `src/DotnetUserManagementApi.Infrastructure/DotnetUserManagementApi.Infrastructure.csproj` | config | — | itself + `Api.csproj` (PackageReference style) | exact (self) |
| `src/DotnetUserManagementApi.Api/Program.cs` | bootstrap | request-response | itself (lines 18-35 JWT, 103-107 Migrate) | exact (self) |
| `src/DotnetUserManagementApi.Api/appsettings.json` | config | — | itself + `appsettings.Development.json` (env-override pattern) | exact (self) |
| `.gitignore` | config | — | itself (lines 31-36, Local databases) | exact (self) |
| `README.md` | doc | — | itself (lines 71-85, decisions table + próximos passos) | exact (self) |

## Pattern Assignments

### `src/DotnetUserManagementApi.Infrastructure/DependencyInjection.cs` (config, DI registration — MODIFY)

**Analog:** itself — D-01/D-03 change point is the `UseSqlite` line (D-03 mandates provider selection happens here).

**Core pattern to preserve** (lines 13-23) — keep the `AddInfrastructure(IServiceCollection, IConfiguration)` signature and all service registrations; only the DbContext line branches:

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(configuration.GetConnectionString("Default")));   // <- D-01: replace with provider branch

    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
    services.AddSingleton<ITokenService, JwtTokenService>();

    return services;
}
```

**Change pattern (D-01/D-03):** branch on explicit config key (NOT prefix detection):
- read `configuration.GetConnectionString("Database")` (new key added to appsettings.json) → `"Sqlite"` | `"Postgres"`
- SQLite branch keeps the exact current call `options.UseSqlite(configuration.GetConnectionString("Default"))` (preserves D-04 zero-dependency local default)
- Postgres branch uses `options.UseNpgsql(configuration.GetConnectionString("Default"))` (needs `using Npgsql.EntityFrameworkCore.PostgreSQL;`)
- Default (unset/`Sqlite`) must preserve today's behavior (D-04)

**Test compatibility constraint (critical):** `tests/DotnetUserManagementApi.Tests/TestWebAppFactory.cs` lines 21-27 replace `DbContextOptions<AppDbContext>` with an in-memory SQLite connection. The default provider branch MUST remain `UseSqlite` so tests keep passing unchanged:

```csharp
var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
services.Remove(descriptor);
services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
```

### `src/DotnetUserManagementApi.Infrastructure/DotnetUserManagementApi.Infrastructure.csproj` (config — MODIFY)

**Analog:** itself (lines 7-11) — add Npgsql following the exact same PackageReference format (D-03):

```xml
<ItemGroup>
  <PackageReference Include="BCrypt.Net-Next" Version="4.2.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.21" />
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.1.2" />
  <!-- ADD: <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.x" /> -->
</ItemGroup>
```

- All versions are pinned explicitly (no wildcards) — match this convention.
- EF Core packages all use `8.0.21` — align the Npgsql package with the EF Core 8.x line.
- PropertyGroup convention (lines 13-17): `net8.0` + `ImplicitUsings` + `Nullable` enabled.

**PackageReference reference pattern** also in `src/DotnetUserManagementApi.Api/DotnetUserManagementApi.Api.csproj` lines 9-14 (`Microsoft.EntityFrameworkCore.Design` 8.0.21 with `PrivateAssets=all` — needed by Npgsql migrations design-time; keep as-is).

### `src/DotnetUserManagementApi.Api/Program.cs` (bootstrap, request-response — MODIFY)

**Analog:** itself. Two change points.

**Change point 1 — JWT fail-fast (D-08),** lines 18-35. Current behavior (lines 21-26) GENERATES a random key when absent — keep for Development, but production must fail fast instead:

```csharp
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    jwtOptions.Key = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
    builder.Configuration["Jwt:Key"] = jwtOptions.Key;
    Console.WriteLine("[SECURITY] Jwt:Key não configurado. Chave aleatória gerada para esta execução (desenvolvimento local).");
}
```

- `JwtOptions.SectionName = "Jwt"` is defined in `src/DotnetUserManagementApi.Infrastructure/Security/JwtOptions.cs` line 5.
- D-08 pattern: branch on `app.Environment.IsDevelopment()` — random key only in Development; fail-fast (e.g. throw `InvalidOperationException` with PT-BR message) when `Jwt__Key` env var is absent in non-Development. The `builder.Configuration["Jwt:Key"] = ...` write-back line is the mechanism env overrides flow through (compose will pass `Jwt__Key`).
- Keep lines 28-35 `Configure<JwtOptions>` + `PostConfigure` block unchanged.

**Change point 2 — provider-aware DB init (D-02, D-07),** lines 103-107. Current block applies migrations unconditionally:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}
```

- D-02 pattern: branch on the same provider key — Postgres → `dbContext.Database.Migrate()` (existing migration `20260819220815_InitialCreate` applies; its explicit `type: "TEXT"` columns are valid in PostgreSQL via Npgsql, so no migration regeneration is expected — verify at implementation).
- SQLite → `dbContext.Database.EnsureCreated()` (D-02: no migrations needed locally).
- D-07 pattern: wrap the Postgres Migrate in a bounded retry loop (e.g. 5-10 attempts with backoff, catching `NpgsqlException`/`RetryableDbException`), since compose startup may race Postgres readiness despite the healthcheck. No external wait-for script (D-07 explicit).
- Keep the `using` scope disposal pattern as-is.

**Unchanged context to preserve:** lines 13-16 (`AddApplication`/`AddInfrastructure`), Swagger guarded by `IsDevelopment()` (lines 86-90) — compose is prod-like (D-05) so Swagger stays off; `ASPNETCORE_ENVIRONMENT` in compose must NOT be `Development`.

### `src/DotnetUserManagementApi.Api/appsettings.json` (config — MODIFY)

**Analog:** itself. Add the provider key and keep structure; env overrides (D-08) work natively via the `ConnectionStrings__*` / `Jwt__*` double-underscore convention:

```json
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": "Data Source=app.db",
    "Database": "Sqlite"
  },
  "Jwt": {
    "Issuer": "dotnet-user-management-api",
    "Audience": "dotnet-user-management-api",
    "ExpirationMinutes": 60
  }
}
```

- `appsettings.Development.json` (only Logging overrides) shows the env-override style to mirror in compose env vars.
- Do NOT put `Jwt:Key` in appsettings (security convention — README line 79: "Chave JWT gerada em runtime — nunca versionada").

### `.gitignore` (config — MODIFY)

**Analog:** itself. Add `.env` to the existing artifact sections (D-09):

```
# Local databases
*.db
*.db-journal
*.db-shm
*.db-wal
*.sqlite

# ADD: .env  (docker compose secrets — real .env never versioned, D-09)
```

- Pattern convention: grouped sections with comment headers.
- `postgres_data` is a named Docker volume (lives under `/var/lib/docker/volumes/`), so no gitignore entry is strictly needed unless a bind mount is chosen (discretion area) — if a bind mount is used, add the entry.

### `README.md` (doc — MODIFY)

**Analog:** itself. Update "Próximos passos" (lines 81-85) — remove/replace the SonarQube mention on line 84 (D-10 canceled SonarQube) and mark the Docker/docs items as delivered.

**Style to preserve for ARCHITECTURE.md** (analog for the new doc):
- PT-BR throughout (D-12), `##`-level sections, fenced bash code blocks, markdown tables for decisions (lines 71-79):

```markdown
| Decisão | Por quê |
|---------|---------|
| Clean Architecture | Separação clara de responsabilidades, testável e auditável |
```

- README line 3 gives the one-line project description style ARCHITECTURE.md should open with.

### `Dockerfile` (config, batch — NEW, no analog)

**No Dockerfile exists.** Use these repo facts as source patterns:
- **Base image freedom** (discretion): `InvariantGlobalization=true` (Api.csproj line 21) means the runtime stage needs NO libicu — enables alpine/distroless non-root images. `SatelliteResourceLanguages=en` (Directory.Build.props line 3) further trims.
- **Build entry:** solution at `solution/DotnetUserManagementApi.sln` (5 projects); `dotnet restore`/`build`/`publish` from repo root or via the sln. Publish target: `src/DotnetUserManagementApi.Api/DotnetUserManagementApi.Api.csproj`.
- **Multi-stage shape:** SDK `8.0` image → restore/build → publish `--no-restore` → runtime image. Mirror the 4-layer Clean Architecture (Api/Application/Domain/Infrastructure + tests) — the sln is the single reference point.
- **App assets:** Program.cs lines 92-93 (`UseDefaultFiles`/`UseStaticFiles`) and lines 100-101 serve `favicon.svg` from `wwwroot` — `dotnet publish` includes wwwroot automatically; ensure the publish command does not exclude it.

### `docker-compose.yml` (config, orchestration — NEW, no analog)

**No compose exists.** The config surface comes from existing files:
- **API service env vars** (dotnet double-underscore convention — `builder.Configuration` reads them natively):
  - `ASPNETCORE_ENVIRONMENT=Production` (Swagger off — D-05)
  - `ConnectionStrings__Database=Postgres` (D-01)
  - `ConnectionStrings__Default=Host=db;Port=5432;Database=dotnet_user_management;Username=...;Password=...` (overrides appsettings value)
  - `Jwt__Key=${JWT__KEY:?}` — fail-fast via compose env requirement (D-08); `Jwt__Issuer`/`Jwt__Audience`/`Jwt__ExpirationMinutes` optional overrides matching appsettings.json lines 12-16
- **Port mapping:** API listens on `http://localhost:5290` (launchSettings.json line 17) — map `5290:8080` or set `ASPNETCORE_URLS=http://+:8080` in the container (discretion).
- **db service:** image `postgres:16` (D-06), named volume `postgres_data` (D-06), healthcheck `pg_isready -U <user> -d <db>` (D-07), `depends_on: condition: service_healthy` (D-07). Compose env substitution from `.env` (D-09) — `.env` holds `POSTGRES_PASSWORD`, `JWT__KEY`, etc.

### `.env.example` (config — NEW, no analog)

**Source of keys:** appsettings.json (Jwt block) + compose needs. Placeholder values only (D-09), e.g. `JWT__KEY=changeme-in-prod`, `POSTGRES_PASSWORD=changeme`, `POSTGRES_USER=app`, `POSTGRES_DB=dotnet_user_management`. The real `.env` is gitignored.

### `.github/workflows/ci-cd.yml` (config, batch CI — NEW, no analog)

**No workflow exists.** Repo facts to encode:
- **Triggers (D-11):** `push: branches: [main]` + `pull_request:` (no path filters — whole repo).
- **Build + test only (D-10):** no SonarQube step. Use `actions/checkout@v4` + `actions/setup-dotnet@v4` with `dotnet-version: '8.0.x'` (matches `TargetFramework net8.0` everywhere and README prereq `dotnet --version` → `8.0.x`).
- **Commands match README lines 50-53:** `dotnet restore` (or implicit), `dotnet build --no-restore` against `solution/DotnetUserManagementApi.sln`, `dotnet test --no-build` (xunit project `tests/DotnetUserManagementApi.Tests` is `IsTestProject=true`, csproj lines 8-9).
- **Linux runner** is safe: tests use in-memory SQLite (`TestWebAppFactory.cs`), no Windows-specific paths; sln paths are `..\src\...` which `dotnet` normalizes cross-platform.

### `ARCHITECTURE.md` (doc — NEW, role-match analog)

**Analog:** `README.md` (only doc in repo). Follow README's structure conventions: PT-BR, `##`-level sections, markdown table for decisions, bash fenced blocks.
- D-13: Mermaid diagrams (architecture layers, auth flow, compose deployment API+Postgres).
- D-14 (job-test deliverable): include technology/pattern justifications per layer and conceptual code snippets only where needed.
- Content sources: README "Estrutura" (lines 57-69) for the layer layout; `DependencyInjection.cs` + `Program.cs` for auth/DB wiring descriptions; the 3 Mermaid diagrams per D-13.

## Shared Patterns

### Configuration via environment override (dotnet double-underscore convention)
**Source:** `Program.cs` lines 18-24 + `appsettings.json`
**Apply to:** docker-compose.yml, .env.example, Program.cs modifications
**Pattern:** `builder.Configuration` merges env vars natively — `ConnectionStrings__Default`, `Jwt__Key`, `ASPNETCORE_ENVIRONMENT`. Compose passes config exclusively via env (D-05 prod-like; no secrets in appsettings).

### Fail-fast for required production secrets
**Source:** `Program.cs` lines 21-26 (current random-key fallback)
**Apply to:** Program.cs JWT change (D-08), docker-compose (`${JWT__KEY:?}`)
**Pattern:** Development keeps the random-key fallback; Production throws if `Jwt__Key` is missing. Compose also hard-requires the var via `${JWT__KEY:?}` so `docker compose up` fails early.

### Explicit provider branching (no prefix detection)
**Source:** `DependencyInjection.cs` line 16 (current single-provider call)
**Apply to:** DependencyInjection.cs (D-01), Program.cs DB init (D-02), appsettings.json (new `Database` key)
**Pattern:** a single explicit config key (`ConnectionStrings:Database` = `"Sqlite"` | `"Postgres"`) drives both the DbContext registration and the startup DB-init branch. SQLite default keeps today's zero-dependency behavior (D-04) and keeps tests green (TestWebAppFactory overrides with `UseSqlite`).

### Pinned package versions
**Source:** Infrastructure.csproj lines 7-11, Api.csproj lines 9-14
**Apply to:** Infrastructure.csproj (Npgsql addition)
**Pattern:** exact versions, no wildcards; EF Core ecosystem pinned to `8.0.21`.

### PT-BR documentation and code comments
**Source:** README.md, Program.cs line 25, appsettings
**Apply to:** ARCHITECTURE.md, README.md, new code comments/messages in Program.cs and DependencyInjection.cs
**Pattern:** all user-facing docs, log messages, and code comments in PT-BR (D-12, established convention).

## No Analog Found

Files with no close match in the codebase (planner should use RESEARCH.md patterns / official docs):

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| `Dockerfile` | config | batch (build) | No containerization exists in repo yet |
| `docker-compose.yml` | config | orchestration | No compose/orchestration exists yet |
| `.env.example` | config | — | No env-secrets convention exists yet |
| `.github/workflows/ci-cd.yml` | config | batch (CI) | No CI exists (Phase 1 explicitly deferred it) |

## Metadata

**Analog search scope:** repo root (src/, tests/, solution/, .gitignore, README.md, Directory.Build.props, .planning/)
**Files scanned:** 16 source/config/doc files
**Pattern extraction date:** 2026-08-19
**Key constraint flagged:** TestWebAppFactory.cs (lines 21-27) pins `UseSqlite` — the default provider branch must stay SQLite for tests to pass unchanged.