# Sprint 1 — Autenticação (Cadastro + Login) + Redirecionamento

## Prompt inicial (original)
"Na sprint-1 inicialmente quero apenas criar uma tela inicial com um sistema de autenticação com cadastro e login de usuarios, após fazer o login ou cadastro o usuario irá pra uma tela de dashboard onde será desenvolvida na sprint-2."

## Objetivo e valor
- Objetivo: permitir que usuários criem conta e façam login.
- Para quem: usuários finais do sistema.
- Valor esperado: habilitar acesso autenticado e preparar base para evoluir a experiência na Sprint 2 (dashboard).

## Escopo
- Inclui:
  - Backend: API de autenticação (cadastro e login), persistência em SQL Server, emissão de token.
  - Frontend: tela inicial de autenticação (login + cadastro) com tema cyberpunk (fundo escuro + acentos neon).
  - Navegação: após sucesso (login/cadastro) redirecionar para `/dashboard`.
  - Dashboard: **apenas placeholder** (conteúdo real na Sprint 2).
- Não inclui:
  - Funcionalidades do dashboard (Sprint 2).
  - Recuperação de senha, social login, 2FA, e-mail de confirmação.

## Assunções e dependências
- Desenvolvimento local.
- Backend em .NET 10 e banco SQL Server local.
- Connection string local (DEV):
  - `"DefaultConnection": "Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"`

## Contrato de UX/UI (aprovado pelo PO)
- Tema: cyberpunk (fundo escuro + acentos neon).
- Fluxo principal:
  1) Usuário abre app e vê tela de autenticação.
  2) Usuário alterna entre abas/estado de Login e Cadastro.
  3) Em sucesso, app redireciona para `/dashboard`.
- Estados:
  - Carregando: durante requisição.
  - Erro: credenciais inválidas, validação de campos, falha de rede.
  - Sucesso: confirmação visual breve e redirecionamento.
- Validações e mensagens:
  - Campos obrigatórios e mensagens claras.
  - Erros de API exibidos de forma amigável.
- Acessibilidade:
  - Labels explícitas, navegação por teclado, foco visível.

## Plano de execução
- Tasks backend:
  - `tasks/task-backend-1.md`
  - `tasks/task-backend-2.md`
- Tasks frontend:
  - `tasks/task-frontend-1.md`
  - `tasks/task-frontend-2.md`
- Estratégia de QA:
  - Validar critérios Given/When/Then das tasks.
  - Se houver falhas, abrir bug em `bugs/bug-*-N.md` com evidências.
