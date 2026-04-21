# task-frontend-3 — Redesenhar Dashboard + corrigir bug usuários online

## Contexto
O dashboard atual tem layout funcional mas design ruim e o painel de usuários online não funciona. Redesenhar completamente e corrigir o bug de SignalR.

## Bug a corrigir: usuários online não aparecem ao logar
- Root cause suspeito: o `PresenceHubService.connect()` é chamado em `ngOnInit` do dashboard, mas pode haver race condition entre a navegação e a disponibilidade do token, ou a invocação `GetOnlineUsers` falha silenciosamente.
- Correção: garantir que ao entrar no dashboard, o hub conecte e a lista de usuários online seja exibida imediatamente. O usuário logado deve aparecer em sua própria lista.

## Escopo
- Inclui:
  - Redesign completo do Dashboard (ts + html + scss)
  - Implementar SidebarComponent, TopbarComponent, OnlineUsersComponent
  - Corrigir PresenceHubService para conexão confiável
  - Navegação: Home (placeholder) e Chat (com painel de usuários online à direita)
  - Usuários online devem aparecer ao entrar no Chat view
- Não inclui:
  - Chat funcional (mensagens)

## Requisitos UX/UI

### Layout geral do Dashboard
```
┌─────────────────────────────────────────────────────┐
│  SIDEBAR (240px fixo) │  MAIN AREA (flex: 1)        │
│  ─────────────────────┤─────────────────────────────│
│  [Avatar] user@...    │  TOPBAR                     │
│  ─────────────────────│  ─────────────────────────  │
│  🏠 Home              │                             │
│  💬 Chat  ← ativo     │  CONTEÚDO DA VIEW           │
│                       │                             │
│  ─────────────────────│                             │
│  ⏻  Logout            │                             │
└─────────────────────────────────────────────────────┘
```

### Chat View Layout (quando Chat ativo)
```
┌──────────────────────────────────────┬────────────────┐
│  Área central                        │ ONLINE (3)     │
│  "Selecione um usuário"              │ ──────────────│
│  (placeholder elegante)              │ ● user1@...   │
│                                      │   há 2 min    │
│                                      │ ● user2@...   │
│                                      │   há 5 min    │
└──────────────────────────────────────┴────────────────┘
```

### SidebarComponent
- Fundo: `--bg-panel`, borda direita 1px `--border`
- Topo: avatar circular com inicial do email + email truncado
- Nav items: ícone + texto, padding 12px 20px
- Item ativo: background `rgba(0, 246, 255, 0.08)`, borda esquerda 3px `--neon-cyan`, texto `--neon-cyan`
- Hover: background `rgba(0, 246, 255, 0.04)`
- Rodapé: botão logout, texto `--danger`, hover fundo danger com opacity

### TopbarComponent
- Fundo: `--bg-panel`, border-bottom 1px `--border`
- Altura: 60px
- Esquerda: título da seção atual
- Direita: indicador de conexão SignalR (● verde "Conectado" / ● vermelho "Desconectado")

### OnlineUsersComponent (painel direito, Chat view)
- Largura: 280px, fundo `--bg-panel`, borda esquerda 1px `--border`
- Header: "ONLINE" badge + contador neon
- Lista de usuários:
  - Indicador ● verde pulsante (CSS animation)
  - Email (truncado com ellipsis)
  - LastSeenAt como "há X min" (via pipe ou função)
- Estado vazio: "Nenhum usuário online"
- Estado loading: skeleton animado

### PresenceHubService — correção obrigatória
1. Adicionar tratamento de erro no `connect()` com `try/catch`
2. Logar no console erros de conexão
3. Após `connection.start()`, invocar `GetOnlineUsers` e aguardar resposta
4. Implementar `reconnected` callback para re-invocar `GetOnlineUsers` após reconexão automática
5. Expor `connectionState$` como Observable<boolean> para o TopbarComponent

### DashboardComponent
- Injetar `AuthService` para obter email do usuário logado
- Injetar `PresenceHubService` e conectar em `ngOnInit`
- Expor `view: 'home' | 'chat'` controlado pelo sidebar
- Ao mudar para 'chat': garantir que a lista de usuários online está visível
- Em `ngOnDestroy`: desconectar hub + unsubscribe observables

## Critérios de aceitação

### Bug fix
- Given: usuário faz login com credenciais válidas
- When: redirecionado para /dashboard (view Chat por padrão)
- Then: lista de usuários online aparece em ≤ 3 segundos (incluindo o próprio usuário)

### Design
- Given: usuário no dashboard
- When: visualização inicial
- Then: layout sidebar + main + painel direito conforme contrato, sem overflow

### Navegação
- Given: usuário clica em "Home"
- When: view muda
- Then: placeholder de home visível, topbar mostra "Home"

- Given: usuário clica em "Chat"
- When: view muda
- Then: painel de usuários online visível à direita

### Logout
- Given: usuário clica em Logout
- When: confirmação (ou direto)
- Then: hub desconectado, tokens limpos, redireciona para /auth

## Definição de pronto (DoD)
- [ ] Layout conforme contrato visual
- [ ] Usuários online aparecem ao entrar no dashboard
- [ ] SidebarComponent funcionando com navegação
- [ ] TopbarComponent com indicador de conexão
- [ ] OnlineUsersComponent com todos os estados
- [ ] Logout funcional
- [ ] `ng build` passa sem erros
