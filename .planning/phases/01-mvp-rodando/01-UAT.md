---
status: complete
phase: 01-mvp-rodando
source: .planning/phases/01-mvp-rodando/README.md (fase documentada retroativamente; sem SUMMARY)
started: 2026-08-20T11:00:00Z
updated: 2026-08-20T11:25:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: |
  Mate qualquer servidor em execução. Inicie a aplicação do zero com `dotnet run` (a partir de src/DotnetUserManagementApi.Api).
  O servidor sobe sem erros e a página em http://localhost:5290/ carrega com a interface "Gerenciamento de Usuários".
result: pass
verified: |
  Automated: app.db removido (cold start limpo). `dotnet run --no-launch-profile` em Development (porta 5291) bootou em ~13s,
  SQLite recriado via EnsureCreated, GET / → 200 com UI "Gerenciamento de Usuários".
  Nota: um processo dotnet legado de outro UID ocupa a porta 5290 (não terminável) — cold start validado em porta livre.

### 2. Register a New User (web)
expected: |
  Abra http://localhost:5290/ e clique na aba "Cadastrar". Preencha Nome, E-mail e Senha (min 8 caracteres),
  clique em "Cadastrar". Você deve ver a mensagem verde "Conta criada! Faça login para continuar." e o formulário volta para a aba "Entrar".
result: pass
verified: |
  Automated via Playwright: Nome "Maria UAT", maria.uat@example.com / senha12345 → POST /api/auth/register 201
  ({"message":"Conta criada."}); formulário auto-mudou para aba "Entrar" com parágrafo "Conta criada! Faça login para continuar."

### 3. Log In (web)
expected: |
  Na aba "Entrar", digite o e-mail e a senha recém-cadastrados e clique em "Entrar".
  Você deve cair no card "Usuários cadastrados" com um badge azul mostrando seu e-mail logado.
result: pass
verified: |
  Automated via Playwright: login maria.uat@example.com/senha12345 → card "Usuários cadastrados" exibido com badge "maria.uat@example.com" e botão "Sair".

### 4. List Users (web)
expected: |
  Após o login, a tabela "Usuários cadastrados" lista usuários com as colunas Nome, E-mail e Cadastrado em,
  incluindo o usuário que você acabou de cadastrar.
result: pass
verified: |
  Automated via Playwright: tabela renderizada com cabeçalhos Nome/E-mail/Cadastrado em e linha
  "Maria UAT | maria.uat@example.com | 20/08/2026, 10:50:38".

### 5. Register API behaviors
expected: |
  POST /api/auth/register: payload válido → 201 com o novo usuário; e-mail duplicado → 201 uniforme "Conta criada."
  (anti-enumeração T-01-10 — NÃO cria linha duplicada); payload inválido → 400 (RFC 7807 problem details).
result: pass
verified: |
  Automated via curl: válido → 201 + persistido; duplicado → 201 "Conta criada." (sem linha duplicada no banco —
  listagem GET /api/users mostra 1 único registro por e-mail); inválido (nome vazio, e-mail ruim, senha curta)
  → 400 {"title":"Erro de Validação","status":400,"detail":"Nome é obrigatório."} RFC 7807.

### 6. Login API behaviors
expected: |
  POST /api/auth/login: credenciais corretas → 200 com token JWT; senha errada → 401; payload malformado → 400.
result: pass
verified: |
  Automated via curl: correto → 200 + JWT (HS256, exp 3599s, tokenType Bearer); senha errada → 401 "E-mail ou senha inválidos.";
  payload sem senha → 400 validation errors; e-mail inexistente → 401 (mensagem uniforme, anti-enumeração).

### 7. Users endpoint protection
expected: |
  GET /api/users: retorna 401 sem token; retorna 200 com a lista de usuários quando um Bearer token válido é enviado.
result: pass
verified: |
  Automated via curl: sem token → 401; token inválido → 401; Bearer token válido (login) → 200 com lista de usuários.

### 8. Password stored hashed (BCrypt)
expected: |
  A senha armazenada no banco é um hash BCrypt (começa com $2a$/$2b$), nunca o valor em texto puro.
result: pass
verified: |
  Automated via python3+sqlite: ambas as linhas têm PasswordHash de 60 chars com prefixo $2a$ (BCrypt work factor 12);
  nenhum registro contém o valor em texto puro.

### 9. Automated Tests Green
expected: |
  `dotnet test` a partir da raiz do repositório completa com todos os testes passando
  (register 201, duplicado 201 uniforme, validações 400, login 200/401, users 401/200, hashing BCrypt, rate limit).
result: pass
verified: |
  Automated via dotnet test (solution/): Passed: 16, Failed: 0, Skipped: 0 (a suíte cresceu de 12 para 16 testes
  desde a UAT anterior — inclui anti-enumeração e rate limiting).

### 10. MVP Coverage — full loop end to end
expected: |
  A partir de um estado limpo, um usuário novo consegue cadastrar, logar e ver a lista de usuários — o loop completo
  cadastro → login (JWT) → listagem protegida funciona sem setup manual de banco ou arquivos de ambiente.
result: pass
verified: |
  Automated: loop completo exercido em servidor único com cold start limpo (app.db removido). Cadastro web (Maria UAT)
  + API 201 → login JWT 200 → GET /api/users 401 sem token / 200 com token → visualização web na tabela.
  SQLite auto-criado (EnsureCreated), Jwt:Key auto-gerada em Development. Nenhum setup manual necessário.

## Summary

total: 10
passed: 10
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none]
