# Frontend — Dashboard: Sidebar esquerda + Chat com usuários online à direita (Sprint 4)

## Objetivo/escopo
Adequar o dashboard ao layout esperado:
- Sidebar de navegação à esquerda com menus: Home, Chat, Logout.
- Ao selecionar Chat, exibir um painel à direita com usuários online (somente visualização).

## Dependências
- Backend: PresenceHub SignalR (`/hubs/presence`).

## Passos de implementação
- Refatorar layout do dashboard mantendo tema cyberpunk existente.
- Garantir que `Home` continue como placeholder.
- Em `Chat`, conectar no SignalR Hub autenticado e renderizar lista de usuários online em painel lateral direito (dados reais).

## Critérios de aceitação (Given/When/Then)
- Given que estou autenticado, When eu acesso `/dashboard`, Then vejo a sidebar de navegação à esquerda com Home/Chat/Logout.
- Given o dashboard, When eu clico em Home, Then vejo apenas conteúdo placeholder (sem painel de usuários online).
- Given o dashboard, When eu clico em Chat, Then vejo um painel à direita com uma lista de usuários online (sem ações).
- Given o dashboard, When eu clico em Logout, Then tokens são limpos e sou redirecionado para `/auth`.

## Definição de pronto (DoD)
- Build ok (`npm run build`)
- Layout aderente aos critérios acima
