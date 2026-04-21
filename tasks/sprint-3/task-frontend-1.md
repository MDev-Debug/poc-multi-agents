# Frontend — Correções de bugs (UI + Auth) (Sprint 3)

## Objetivo/escopo
Corrigir bugs identificados pelo QA/PO na interface e no fluxo de autenticação (cadastro/login/logout), mantendo o escopo **somente bugfix**.

## Dependências
- Bugs abertos em `bugs/bug-frontend-*.md`.

## Passos de implementação (alto nível)
- Reproduzir cada bug em ambiente local.
- Ajustar UI/fluxo conforme critérios de correção em cada bug.
- Garantir que build do Angular segue ok.

## Critérios de aceitação (Given/When/Then)
- Given os bugs `bug-frontend-*.md`, When eu aplico as correções, Then cada bug fica não reproduzível seguindo seu critério de correção.
- Given o app, When eu rodo `npm run build`, Then o build finaliza com sucesso.

## Definição de pronto (DoD)
- Bugs fechados com evidência de reprodução/validação
- Build ok
