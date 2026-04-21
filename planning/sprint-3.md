# Sprint 3 — Ajustes (somente bugs) em UI + Autenticação

## Prompt inicial (original)
"Vamos começar a sprint 3, eu como especialista no sistema não estou satisfeito com a interface e sistema de autenticação, a interface está ruim e o cadastro e login também. Preciso que arrume que o QA e PO analisem e criem os bugs necessários para os desenvolvedores atuarem. A sprint 3 será apenas ajuste de bugs."

## Contexto e objetivo
- Contexto: Sprints 1/2 entregaram cadastro/login + dashboard cyberpunk, guard de rota e refresh token no backend.
- Objetivo da Sprint 3: **identificar e registrar bugs** (UI/UX e fluxo de autenticação) para correção pelos devs.

## Escopo
- Inclui:
  - QA + PO: revisão do fluxo de autenticação (cadastro/login/logout) e aderência ao contrato de UX/UI.
  - QA: criação de bugs em `bugs/` com passos, evidências e critérios de correção.
- Não inclui:
  - Novas features (ex.: chat real, home real, 2FA, recuperação de senha, etc.).

## Assunções e dependências
- Execução local.
- Backend: .NET 10 + SQL Server local.
- Frontend: Angular (última versão do repo).

## Contrato (referência)
- Sprint 1: UI cyberpunk na autenticação; estados de loading/erro/sucesso; redirect para `/dashboard` em sucesso.
- Sprint 2: dashboard mais elegante; sidebar com hamburger; `/dashboard` protegido; logout redireciona para `/auth`; refresh token no backend.

## Plano de execução
- PO registra tasks de correção (referenciando bugs) em:
  - `tasks/sprint-3/task-frontend-1.md`
  - `tasks/sprint-3/task-backend-1.md`
- QA executa smoke + validações e abre bugs em `bugs/`.
