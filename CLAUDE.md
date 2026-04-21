# Claude Code — Fluxo Multi-Agentes

## Visão geral

Este repositório usa **Claude Code Agent Teams** com quatro agentes especializados que colaboram via artefatos de arquivo (planning, tasks, bugs).

## Agentes disponíveis

| Agente | Arquivo | Responsabilidade |
|---|---|---|
| Product Owner | `.claude/agents/product-owner.md` | Planning, breakdown de tasks, aprovação final e commit |
| Dev Backend | `.claude/agents/dev-backend.md` | Implementação .NET 10 + SQL Server |
| Dev Frontend | `.claude/agents/dev-frontend.md` | Implementação Angular + tema cyberpunk |
| QA | `.claude/agents/qa.md` | Testes, validação e registro de bugs |

## Fluxo obrigatório

```
Prompt de feature
      ↓
[PO] cria planning/sprint-N.md + tasks/sprint-N/task-*.md
      ↓
[Dev Backend] implementa tasks/sprint-N/task-backend-*.md
      ↓
[Dev Frontend] implementa tasks/sprint-N/task-frontend-*.md
      ↓
[QA] testa e cria bugs/sprint-N/bug-*.md se necessário
      ↓
[Dev Backend/Frontend] corrige bugs
      ↓
[QA] re-testa → QA OK
      ↓
[PO] "de acordo" → commit final
```

## Convenções de arquivos

### Planning
- Pasta: `planning/`
- Nomes: `sprint-1.md`, `sprint-2.md`, ...

### Tasks
- Pasta: `tasks/sprint-N/`
- Backend: `task-backend-1.md`, `task-backend-2.md`, ...
- Frontend: `task-frontend-1.md`, `task-frontend-2.md`, ...

### Bugs
- Pasta: `bugs/sprint-N/`
- Backend: `bug-backend-1.md`, `bug-backend-2.md`, ...
- Frontend: `bug-frontend-1.md`, `bug-frontend-2.md`, ...

## Stack técnica

- **Backend**: .NET 10 + SQL Server local
  - Connection string local: `Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;`
- **Frontend**: Angular (última versão) + tema cyberpunk (fundo escuro + acentos neon)

## Autonomia

Os agentes executam comandos e alterações **sem pedir confirmação**, exceto quando houver:
- Ambiguidade material ou alto impacto
- Credenciais/segredos externos
- Risco de compliance/segurança

## Commit final

Somente após **QA OK** e **aprovação do PO**:
```bash
git status
git add -A
git commit -m "<mensagem objetiva baseada nas tasks concluídas>"
```
