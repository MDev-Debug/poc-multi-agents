# Claude Code — Fluxo Multi-Agentes

## Visão geral

Este repositório usa **Claude Code Agent Teams** com seis agentes especializados que colaboram via artefatos de arquivo (planning, tasks, bugs, patches, dba).

## Agentes disponíveis

| Agente | Arquivo | Responsabilidade |
|---|---|---|
| Product Owner | `.claude/agents/product-owner.md` | Planning, breakdown de tasks, aprovação final e commit |
| Dev Backend | `.claude/agents/dev-backend.md` | Implementação .NET 10 + SQL Server |
| Dev Frontend | `.claude/agents/dev-frontend.md` | Implementação Angular + tema cyberpunk |
| QA | `.claude/agents/qa.md` | Testes, validação e registro de bugs |
| DBA | `.claude/agents/dba.md` | Análise e refatoração de schema SQL Server, nomes semânticos |
| Security | `.claude/agents/security.md` | Análise de vulnerabilidades e geração de patches de segurança |

## Fluxo obrigatório

```
Prompt de feature
      ↓
[PO] cria planning/sprint-N.md + tasks/sprint-N/task-*.md
      ↓
[Dev Backend] implementa tasks/sprint-N/task-backend-*.md
      ↓
[DBA] ← acionado se houver criação/alteração de migrations ou schema
      │  analisa schema, valida nomes semânticos
      │  gera dba/sprint-N/dba-report-N.md
      │  executa migration de refatoração se aprovado
      ↓
[Dev Frontend] implementa tasks/sprint-N/task-frontend-*.md
      ↓
[Security] ← acionado se houver dados sensíveis, criptografia ou feature de autenticação
      │  analisa código backend e frontend
      │  gera patches/sprint-N/patch-backend-*.md
      │  gera patches/sprint-N/patch-frontend-*.md
      │  gera patches/sprint-N/security-report-N.md
      ↓
[Dev Backend] aplica patches/sprint-N/patch-backend-*.md
[Dev Frontend] aplica patches/sprint-N/patch-frontend-*.md
      ↓
[QA] testa e cria bugs/sprint-N/bug-*.md se necessário
      ↓
[Dev Backend/Frontend] corrige bugs
      ↓
[QA] re-testa → QA OK
      ↓
[PO] "de acordo" → commit final
```

> **Nota sobre DBA e Security**: são acionados condicionalmente.
> - **DBA**: sempre que Dev Backend criar ou alterar migrations/entidades de domínio.
> - **Security**: sempre que a feature envolver dados sensíveis, autenticação, criptografia ou comunicação em tempo real. Patches de severidade Crítico ou Alto **bloqueiam** o commit final.

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

### Patches de segurança
- Pasta: `patches/sprint-N/`
- Backend: `patch-backend-1.md`, `patch-backend-2.md`, ...
- Frontend: `patch-frontend-1.md`, `patch-frontend-2.md`, ...
- Relatório: `patches/sprint-N/security-report-N.md`

### Relatórios DBA
- Pasta: `dba/sprint-N/` (por sprint) ou `dba/analysis/` (análise global)
- Relatório por sprint: `dba-report-1.md`, `dba-report-2.md`, ...
- Análise global: `dba/analysis/db-analysis.md`

## Stack técnica

- **Backend**: .NET 10 + SQL Server local
  - Connection string local: `Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;`
- **Frontend**: Angular (última versão) + tema cyberpunk (fundo escuro + acentos neon)
- **Criptografia de mensagens**: AES-256-GCM na camada de aplicação (chave via environment/secrets)

## Autonomia

Os agentes executam comandos e alterações **sem pedir confirmação**, exceto quando houver:
- Ambiguidade material ou alto impacto
- Credenciais/segredos externos
- Risco de compliance/segurança
- Migrations destrutivas de banco (DBA solicita aprovação do PO antes de executar)

## Commit final

Somente após **QA OK**, **todos os patches críticos/altos aplicados** e **aprovação do PO**:
```bash
git status
git add -A
git commit -m "<mensagem objetiva baseada nas tasks concluídas>"
```
