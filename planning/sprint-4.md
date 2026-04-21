# Sprint 4 — Dashboard: Sidebar esquerda + Chat com usuários online à direita

## Prompt inicial (original)
"o dashboard não está conforme o esperado, é necessário um menu lateral na direita (sidebar) com os menus home, chat e logout. A home por enquanto não será implementada, ao entrar no menu de chat devemos ver os usuarios online em um menu lateral na direita por enquanto sem mais ações apenas isso"

## Ajuste acordado (refinamento do prompt)
- Sidebar de navegação (Home/Chat/Logout): **na esquerda**.
- Ao entrar no menu **Chat**: exibir um painel **na direita** com a lista de **usuários online**.
- Home: não implementar conteúdo (placeholder).
- Usuários online: por enquanto apenas exibição (sem ações/sem chat real).

## Objetivo
- Ajustar o layout do dashboard para o fluxo esperado.

## Escopo
- Inclui:
  - Refatorar layout do dashboard para sidebar de navegação à esquerda.
  - Criar view de Chat com painel à direita listando usuários online (dados reais via SignalR).
- Não inclui:
  - Chat funcional e mensagens.

## Dependências
- Frontend Angular existente.

## Tasks
- `tasks/sprint-4/task-frontend-1.md`
- `tasks/sprint-4/task-backend-1.md`
