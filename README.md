# dotnet-user-management-api

API de gerenciamento de usuários em **.NET 8** seguindo **Clean Architecture**, com cadastro, autenticação via **JWT + BCrypt** e listagem de usuários em endpoint protegido.

## Funcionalidades

- **Cadastro** (`POST /api/auth/register`) — Nome, E-mail e Senha (hash BCrypt)
- **Login** (`POST /api/auth/login`) — retorna JWT (HS256)
- **Listagem** (`GET /api/users`) — endpoint protegido com `[Authorize]` (401 sem token)
- **Tratamento de erros** — middleware global retornando RFC 7807 (Problem Details)
- **Swagger** em `/swagger` com suporte a Bearer token
- **Página web** em `/` — cadastro, login e listagem sem precisar de frontend separado

## Pré-requisitos

- **.NET 8 SDK** (`dotnet --version` → `8.0.x`)
- Sem dependências externas: banco SQLite local gerado na primeira execução

> Ambiente sem `libicu` (ex.: WSL minimalista): o projeto já define `InvariantGlobalization=true`.

## Como rodar

```bash
# do diretório raiz do repositório
cd src/DotnetUserManagementApi.Api
dotnet run
```

A API sobe em `http://localhost:5290` (ajuste em `Properties/launchSettings.json`).

Fluxo rápido:

```bash
# cadastro
curl -X POST http://localhost:5290/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Ana Souza","email":"ana@example.com","password":"senha12345"}'

# login → retorna o token
curl -X POST http://localhost:5290/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"ana@example.com","password":"senha12345"}'

# listagem (troque <token> pelo token retornado no login)
curl http://localhost:5290/api/users -H "Authorization: Bearer <token>"
```

## Executar os testes

```bash
cd <raiz-do-repositorio>
dotnet test
```

Cobrem: cadastro (201), e-mail duplicado (409), validações (400), login correto (200) e inválido (401), acesso não autorizado (401) e autorizado (200) e o hashing BCrypt.

## Estrutura

```
src/
  DotnetUserManagementApi.Api/          # Controllers, middleware, Swagger, página web
  DotnetUserManagementApi.Application/  # Use cases, DTOs, contratos (serviços)
  DotnetUserManagementApi.Domain/       # Entidades e regras de domínio (User, Email)
  DotnetUserManagementApi.Infrastructure/  # EF Core, migrações, BCrypt, emissão de JWT
tests/
  DotnetUserManagementApi.Tests/        # Testes de integração (WebApplicationFactory) e unidade
solution/
  DotnetUserManagementApi.sln
```

## Decisões técnicas principais

| Decisão | Por quê |
|---------|---------|
| Clean Architecture | Separação clara de responsabilidades, testável e auditável |
| JWT (HS256) | API stateless; o token carrega a identidade do usuário |
| BCrypt (work factor 12) | Hash de senha com salt aleatório, resistente a brute force |
| EF Core dual-provider (SQLite local / PostgreSQL Docker) | Zero configuração local e Postgres prod-like via chave `ConnectionStrings:Database` (D-01) |
| Chave JWT gerada em runtime | Nenhuma chave de assinatura é versionada no repositório |

## Rodar com Docker (PostgreSQL)

```bash
# do diretório raiz do repositório
cp .env.example .env        # e preencha JWT__KEY e POSTGRES_PASSWORD (ex.: openssl rand -hex 32 / -hex 16)
docker compose up --build
```

A API sobe em `http://localhost:5290` com **PostgreSQL 16** em container (volume `postgres_data` para persistência) e ambiente `Production` (Swagger desligado, `JWT__KEY` obrigatória).

Nesta fase também foram entregues:

- Dockerfile multi-stage + `docker-compose` com PostgreSQL
- Pipeline CI (GitHub Actions) com build e testes — disparado em push para `main` e pull requests
- Documento `ARCHITECTURE.md` com diagramas e justificativas completas

Planejamento e decisões registrados em `.planning/`.