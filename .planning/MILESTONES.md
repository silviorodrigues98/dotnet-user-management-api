# Milestones

## v1.0 MVP (Shipped: 2026-08-20)

**Phases completed:** 2 phases, 4 plans, 11 tasks

**Key accomplishments:**

- Real API error messages surface in the login form (RFC 7807 problem+json parsing), the post-registration success confirmation renders visibly on the login tab, and the 375px viewport no longer scrolls horizontally (badge/heading/table truncation)
- ARIA tab semantics with aria-selected toggling, visible :focus-visible keyboard focus, system-ui font inheritance on form controls, AA-compliant slate-400 placeholders, an empty-state row for zero-user databases, disabled-in-flight submit buttons, and the product name as browser tab title — 7 of 10 UI-audit warnings closed in the single-file frontend without regressing the 3 plan-01 blocker fixes
- Dual-provider de banco (SQLite local | PostgreSQL via Docker) com Dockerfile multi-stage, docker-compose prod-like postgres:16 e fail-fast de JWT__KEY — mantendo run local zero-dependência e os 12 testes verdes.
- ARCHITECTURE.md em PT-BR com 4 diagramas Mermaid (camadas, fluxo de autenticação, dual-provider, deploy compose) como entregável do desafio da vaga, pipeline CI build+test (push main + PRs, sem SonarQube) e README com seção Docker — documentando a topologia REAL criada no plan 02-01.

---
