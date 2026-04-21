# Backend — Correções de bugs (Auth/Refresh) (Sprint 3)

## Objetivo/escopo
Corrigir bugs identificados pelo QA/PO nos endpoints e regras de autenticação/refresh, mantendo o escopo **somente bugfix**.

## Dependências
- Bugs abertos em `bugs/bug-backend-*.md`.

## Passos de implementação (alto nível)
- Reproduzir cada bug em ambiente local.
- Ajustar API/validações/respostas conforme critério de correção.
- Garantir build ok.

## Critérios de aceitação (Given/When/Then)
- Given os bugs `bug-backend-*.md`, When eu aplico as correções, Then cada bug fica não reproduzível seguindo seu critério de correção.
- Given o script de smoke, When eu executo `scripts/qa-smoke-auth-refresh.ps1`, Then ele finaliza com `SMOKE_OK`.

## Definição de pronto (DoD)
- Bugs fechados com evidência de reprodução/validação
- Build ok
