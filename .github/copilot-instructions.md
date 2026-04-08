# Copilot Workspace Instructions — Fluxo Automatizado (PO → Dev → QA → PO)

## Objetivo
Este repositório segue um fluxo totalmente automatizado, iniciado por **um único prompt** de feature. O sistema deve operar com **autonomia total**, executando comandos e fazendo mudanças no repo **sem pedir confirmação**, exceto quando estiver genuinamente bloqueado.

## Autonomia (regra geral)
- Se o pedido é acionável, **execute** (crie/edite arquivos, rode comandos, valide) antes de perguntar.
- Perguntas são exceção: faça no máximo **1–3** apenas quando houver ambiguidade material/alto impacto/segredos/risco de compliance.

## Workflow obrigatório
1) **PO** detalha o prompt inicial e grava em `planning/sprint-<N>.md`.
2) **PO** quebra o prompt em tasks e grava em `tasks/`.
3) **Dev Backend** implementa as tasks `task-backend-*.md` usando **.NET 10** e **SQL Server**.
4) **Dev Frontend** implementa as tasks `task-frontend-*.md` usando **Angular (última versão)**.
5) **QA** testa backend e frontend. Se houver bugs, cria arquivos em `bugs/`.
6) **Devs** corrigem bugs.
7) **PO** dá o “de acordo”, faz o **commit final** com a mensagem do que foi desenvolvido e segue para a release.

## Git (commit final)
- Somente após **QA OK** e **aprovação do PO** ("de acordo"), o PO deve:
  - Rodar `git status` para revisar mudanças.
  - Rodar `git add -A`.
  - Rodar `git commit -m "<mensagem descrevendo o que foi desenvolvido>"`.
- A mensagem do commit deve ser objetiva e baseada nas tasks concluídas (ex.: "feat(auth): cadastro e login + endpoints + validações").
- Evite commits intermediários automáticos: o commit é parte do encerramento do fluxo.

## Convenções de arquivos (contrato entre agentes)
### Planning
- Pasta: `planning/`
- Nomes: `sprint-1.md`, `sprint-2.md`, ...
- Cada sprint deve conter:
  - Prompt inicial (original)
  - Contexto e objetivo
  - Escopo (inclui / não inclui)
  - Assunções e dependências
  - Contrato de UX/UI (fluxos, estados e tema)
  - Lista de tasks planejadas (referenciando os arquivos em `tasks/`)

### Tasks
- Pasta: `tasks/`
- Nomes:
  - Backend: `task-backend-1.md`, `task-backend-2.md`, ...
  - Frontend: `task-frontend-1.md`, `task-frontend-2.md`, ...
- Cada task deve conter:
  - Objetivo/escopo
  - Dependências
  - Passos de implementação
  - Critérios de aceitação (Given/When/Then)
  - Definição de pronto (DoD)

### Bugs
- Pasta: `bugs/`
- Nomes:
  - Backend: `bug-backend-1.md`, `bug-backend-2.md`, ...
  - Frontend: `bug-frontend-1.md`, `bug-frontend-2.md`, ...
- Cada bug deve conter:
  - Como reproduzir
  - Resultado esperado vs atual
  - Evidência (logs/stacktrace)
  - Critério de correção

## Backend: stack e banco
- Framework: **.NET 10** (API)
- Banco: **SQL Server local**
- Connection string a usar em ambiente local:
  - `"DefaultConnection": "Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"`

Observação: manter segredos apenas para uso local (preferir `appsettings.Development.json`/user-secrets quando aplicável).

## Frontend: UX/UI e tema
- Framework: **Angular (última versão)**
- Tema: **cyberpunk** com **fundos escuros** e **acentos neon**.
- O design e decisões de UX/UI devem ser **aprovados pelo PO**:
  - O PO define o contrato visual (componentes, tokens/variáveis e estados).
  - Dev Frontend implementa exatamente o especificado.

## Execução local
- Tudo deve rodar localmente.
- Agentes podem e devem executar comandos (restore/build/test/run/migrations) sem pedir confirmação.

## Escopo
- Não inventar features “nice to have” fora das tasks.
- Mudanças devem ser mínimas e focadas para cumprir as tasks e corrigir bugs.
