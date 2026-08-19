# Requirements: dotnet-user-management-api

**Defined:** 2026-08-19
**Core Value:** O app precisa rodar: cadastro, login e listagem funcionando de ponta a ponta com autenticação JWT.

## v1 Requirements

### Authentication

- [ ] **AUTH-01**: User can sign up with name, email and password
- [ ] **AUTH-02**: User can log in with email/password and receive a JWT token
- [ ] **AUTH-03**: Protected endpoints reject requests without a valid token (401)

### Users

- [ ] **USER-01**: Authenticated user can list registered users (name and email)
- [ ] **USER-02**: Registered email is unique

### Quality

- [ ] **QUAL-01**: Passwords are stored hashed (never plaintext)
- [ ] **QUAL-02**: API returns structured, consistent error responses
- [ ] **QUAL-03**: Core paths covered by automated tests

## v2 Requirements

- **AUTH-04**: Email verification after signup
- **AUTH-05**: Password reset
- **AUTH-06**: Refresh tokens / token revocation
- **USER-03**: User profile editing

## Out of Scope

| Feature | Reason |
|---------|--------|
| CI/CD execution | Config no repo, execução posterior |
| Containerização | Extra depois do MVP funcionar |
| Email verification | Fora do escopo básico |
| OAuth / 2FA | Fora do escopo básico |
| Frontend framework | Tela HTML estática suficiente |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| AUTH-01 | Phase 1 | Pending |
| AUTH-02 | Phase 1 | Pending |
| AUTH-03 | Phase 1 | Pending |
| USER-01 | Phase 1 | Pending |
| USER-02 | Phase 1 | Pending |
| QUAL-01 | Phase 1 | Pending |
| QUAL-02 | Phase 1 | Pending |
| QUAL-03 | Phase 1 | Pending |

**Coverage:**
- v1 requirements: 8 total
- Mapped to phases: 8
- Unmapped: 0 ✓

---
*Requirements defined: 2026-08-19*
*Last updated: 2026-08-19 after initial definition*