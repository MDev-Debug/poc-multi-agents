# Sprint 6 — Chat privado: filtro de presença + mensagens + notificações

## Prompt inicial (original)
1. Fix: O usuário logado não deve aparecer na própria lista de usuários online (atualmente ele se vê na lista).
2. Feature: Ao clicar em um usuário online, deve abrir uma conversa de mensagens privadas entre os dois usuários.
3. Feature: Notificação com ícone de mensagem recebida no usuário que enviou a mensagem — exibir apenas quando a conversa NÃO estiver aberta (se o usuário já estiver na conversa, não mostrar notificação).

Stack:
- Backend: .NET 10 + SignalR + SQL Server
  - Connection string: `Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;`
- Frontend: Angular + tema cyberpunk (fundo escuro + acentos neon)

## Objetivo e valor
- Objetivo: Implementar o núcleo do chat privado — filtragem do próprio usuário na presença, envio/recebimento de mensagens em tempo real via SignalR e notificações de mensagens não lidas.
- Para quem: Usuários autenticados que desejam se comunicar de forma privada em tempo real.
- Valor esperado: Experiência de chat funcional e responsiva, com feedback visual imediato de novas mensagens quando o usuário não está na conversa.

## Escopo

### Inclui:
- **Fix presença**: filtrar o próprio usuário logado da lista de usuários online no frontend e/ou backend.
- **Entidade Message**: nova tabela no banco com SenderId, ReceiverId, Content, SentAt.
- **Hub de chat (ChatHub)**: método SendPrivateMessage para troca de mensagens em tempo real entre dois usuários via SignalR.
- **Histórico de mensagens**: endpoint REST GET `/api/messages/{otherUserId}` retornando as últimas N mensagens entre os dois usuários, paginado.
- **UI — conversa privada**: ao clicar num usuário online, abre painel de chat na área central com histórico + campo de envio.
- **UI — notificação de mensagem não lida**: exibir badge/ícone na entrada do usuário remetente na lista de presença quando há mensagem nova e a conversa correspondente NÃO está aberta.
- **Estado de conversa ativa**: controle no frontend de qual conversa está aberta para suprimir notificação corretamente.

### Não inclui:
- Chat em grupo / canais
- Upload de arquivos ou imagens
- Notificações push (browser notifications / service workers)
- Leitura persistida no banco (read receipts)
- Paginação com scroll infinito (somente carga inicial das últimas 50 mensagens)
- Deploy / CI-CD

## Assunções e dependências
- AuthService já salva `userId` via `AuthResponse.userId` — o frontend deve persistir o `userId` em sessionStorage para uso nas chamadas de chat.
- O JWT já contém `sub` (userId) e `email` — o backend pode extrair o remetente direto do contexto do hub sem precisar de parâmetro extra.
- O SignalR Hub de presença (`/hubs/presence`) permanece inalterado em seu path; o novo hub de chat será `/hubs/chat`.
- A tabela `Messages` será criada via EF Core Migration.
- O frontend já possui `PresenceHubService`; será criado `ChatHubService` análogo.
- `userId` deve ser salvo no `sessionStorage` pelo AuthService ao logar/registrar (ajuste mínimo no frontend).

## Contrato de UX/UI (aprovado pelo PO)

### Tema cyberpunk — tokens SCSS em vigor (herdados da Sprint 5)
```
--bg-deep, --bg-panel, --bg-surface, --border, --text, --text-muted,
--neon-cyan, --neon-pink, --neon-green, --danger,
--glow-cyan, --glow-pink
```

### Fix — lista de usuários online
- Filtrar o usuário logado da lista antes de renderizar (frontend, usando o `userId` ou `email` do usuário autenticado).
- O contador "ONLINE" deve refletir apenas os outros usuários.

### Fluxo principal — conversa privada
1. Usuário vê a lista de presença (usuários online) no painel direito.
2. Clica em um usuário — o painel central ("chat-center") substitui o placeholder por um componente `PrivateChatComponent`.
3. `PrivateChatComponent` exibe:
   - Header: avatar/inicial + email do interlocutor + botão fechar (volta ao placeholder).
   - Área de mensagens (scroll para baixo automaticamente no carregamento e a cada nova mensagem).
   - Cada mensagem mostra: conteúdo, horário (HH:mm) e alinhamento diferente para remetente vs. receptor.
   - Input de texto + botão Enviar (Enter também envia).
4. Ao fechar a conversa, volta ao placeholder padrão.

### Notificação de mensagem não lida
- Quando uma mensagem chega via SignalR e a conversa com aquele remetente NÃO está aberta: exibir um badge neon-pink com ícone de mensagem (ou número de não lidas) ao lado do email do usuário na lista de presença.
- Quando o usuário abre a conversa com aquele remetente: o badge é limpo automaticamente (zero não lidas).
- O badge deve ter animação de pulse (semelhante ao dot verde de presença).

### Estados obrigatórios — PrivateChatComponent
- Carregando histórico: spinner neon centralizado.
- Histórico vazio: placeholder "Nenhuma mensagem ainda. Diga olá!".
- Erro ao carregar: mensagem de erro com botão "Tentar novamente".
- Enviando mensagem: botão desabilitado + spinner mini no botão.
- Erro ao enviar: toast/inline "Falha ao enviar. Tente novamente."

### Acessibilidade
- Campo de texto: `aria-label="Mensagem para <email>"`.
- Botão Enviar: `aria-label="Enviar mensagem"`.
- Lista de mensagens: `role="log"` + `aria-live="polite"`.
- Badges de notificação: `aria-label="X mensagem(ns) não lida(s) de <email>"`.
- Foco retorna ao campo de texto após envio.

## Plano de execução

### Tasks backend
- `tasks/sprint-6/task-backend-1.md` — Entidade Message + Migration + ChatHub (SignalR) + endpoint GET mensagens

### Tasks frontend
- `tasks/sprint-6/task-frontend-1.md` — Fix filtro de presença + salvar userId no AuthService
- `tasks/sprint-6/task-frontend-2.md` — ChatHubService + PrivateChatComponent + integração no Dashboard
- `tasks/sprint-6/task-frontend-3.md` — Notificações de mensagem não lida na lista de presença

### Estratégia de QA
- QA abre `bugs/sprint-6/` para cada defeito encontrado.
- Cenários mínimos a validar:
  1. Usuário logado não aparece na própria lista de presença.
  2. Dois usuários em abas diferentes conseguem trocar mensagens em tempo real.
  3. Histórico de mensagens é carregado ao abrir a conversa.
  4. Badge de notificação aparece somente quando a conversa não está aberta.
  5. Badge some ao abrir a conversa.
  6. Estados de loading e erro são exibidos corretamente.

## Encerramento
Após QA OK: registrar "de acordo" e executar commit final.

---

## Resultado de QA

- Data: 2026-04-21
- Tasks testadas: 3 (`task-backend-1`, `task-frontend-2`, `task-frontend-3`)
- Bugs encontrados: 2
- Bugs corrigidos: 2
- Bugs abertos: 0

### Re-teste — 2026-04-21

#### bug-frontend-1 — Mensagem duplicada (Fechado)
- `private-chat.component.ts` linha 57: filtro é `msg.senderId === this.otherUser.userId` exclusivamente.
- Condição `|| msg.senderId === this.myUserId` removida — eliminando a duplicacao via `messages$`.
- Optimistic update permanece funcional na linha 110.
- `ng build` sem erros.
- Criterio Given/When/Then satisfeito.

#### bug-frontend-2 — Sintaxe mista *ngIf/*ngFor (Fechado)
- `online-users.component.html` usa exclusivamente `@if`, `@for` e `@else` — zero ocorrencias de `*ngIf` ou `*ngFor`.
- `online-users.component.ts`: `imports: []` — `CommonModule` removido.
- `ng build` sem erros.
- Criterio Given/When/Then satisfeito.

### Critérios verificados

#### Backend — task-backend-1

| Critério | Status |
|---|---|
| `dotnet build` limpo (0 erros, 0 warnings) | OK |
| Entidade `Message` com Id, SenderId, ReceiverId, Content, SentAt | OK |
| `DbSet<Message>` no `ChatDbContext` com FK Restrict e índice composto | OK |
| Migration `AddMessages` gerada e aplicada | OK |
| `MessageDto` e `SendMessageRequest` corretos | OK |
| `IMessageService` com `SaveAsync` e `GetHistoryAsync` | OK |
| `MessageService.SaveAsync` salva e retorna DTO com email | OK |
| `MessageService.GetHistoryAsync` busca bidirecional, ordena por SentAt ASC, paginação correta | OK |
| `IUserConnectionTracker` com AddConnection/RemoveConnection/GetConnectionIds | OK |
| `UserConnectionTracker` usa ConcurrentDictionary com lock | OK |
| `ChatHub` decorado com `[Authorize]` | OK |
| `ChatHub.OnConnectedAsync` chama `AddConnection` | OK |
| `ChatHub.OnDisconnectedAsync` chama `RemoveConnection` | OK |
| `SendPrivateMessage` valida content não vazio e <= 4000 chars | OK |
| `SendPrivateMessage` envia `ReceiveMessage` ao receptor | OK |
| `SendPrivateMessage` envia `ReceiveMessage` às outras conexões do remetente | OK |
| `MessagesController` com `[Authorize]`, extrai userId do JWT | OK |
| `MessagesController` valida pageSize <= 100 | OK |
| `Program.cs`: `AddScoped<IMessageService>`, `AddSingleton<IUserConnectionTracker>` | OK |
| `Program.cs`: `MapHub<ChatHub>("/hubs/chat")` | OK |
| CORS inclui `/hubs/chat` via query string `access_token` | OK |

#### Frontend — task-frontend-2

| Critério | Status |
|---|---|
| `ng build` limpo (0 erros) | OK |
| `ChatHubService` com `messages$` Subject e `connected$` BehaviorSubject | OK |
| `ChatHubService.connect()` com `withAutomaticReconnect()` e `accessTokenFactory` | OK |
| `ChatHubService.disconnect()` limpa estado | OK |
| `ChatHubService.sendMessage()` invoca `SendPrivateMessage` | OK |
| `MessageApiService.getHistory()` faz GET tipado | OK |
| `OnlineUsersComponent` emite `userSelected` Output | OK |
| `OnlineUsersComponent` com `tabindex="0"` e `keydown.enter` | OK |
| `OnlineUsersComponent` com `aria-label` por item | OK |
| `DashboardComponent.activeConversationUser` signal | OK |
| `DashboardComponent.openConversation` / `closeConversation` | OK |
| `DashboardComponent` chama `chatHub.connect()` no `ngOnInit` | OK |
| `DashboardComponent` chama `chatHub.disconnect()` no `logout()` e `ngOnDestroy()` | OK |
| Dashboard template condiciona `PrivateChatComponent` vs placeholder | OK |
| `PrivateChatComponent` states: loading, erro, vazio | OK |
| `PrivateChatComponent` auto-scroll via AfterViewChecked | OK |
| `PrivateChatComponent` optimistic update ao enviar | OK |
| `PrivateChatComponent` mensagens próprias à direita, outras à esquerda | OK |
| `PrivateChatComponent` ARIA: `role="log"`, `aria-live="polite"`, `aria-label` no input | OK |
| Botão Enviar com `type="submit"` e `aria-label` | OK |
| Botão Fechar com `aria-label="Fechar conversa"` | OK |
| Tema cyberpunk aplicado (tokens CSS corretos) | OK |
| **Duplicação de mensagem ao enviar** (optimistic + echo do hub via `msg.senderId === myUserId`) | **BUG** — `bugs/sprint-6/bug-frontend-1.md` |

#### Frontend — task-frontend-3

| Critério | Status |
|---|---|
| `UnreadMessagesService` com `increment`, `clear`, `counts$` | OK |
| `DashboardComponent` assina `messages$` e incrementa só quando conversa fechada e remetente != próprio usuário | OK |
| `DashboardComponent.openConversation` chama `unreadMessages.clear` | OK |
| `OnlineUsersComponent` recebe `unreadCounts` Input | OK |
| Badge renderizado condicionalmente com `@if` | OK |
| Badge exibe "99+" para count > 99 | OK |
| Badge com `aria-label` descritivo e `role="status"` | OK |
| Animação pulse neon-pink | OK |
| `prefers-reduced-motion` respeitado (animation dentro de `@media (prefers-reduced-motion: no-preference)`) | OK |
| **Sintaxe mista `*ngIf`/`*ngFor` e `@if` no mesmo template** | **BUG** — `bugs/sprint-6/bug-frontend-2.md` |

### Bugs abertos

Nenhum.

### Bugs fechados

| Ref | Título | Severidade | Status |
|---|---|---|---|
| `bugs/sprint-6/bug-frontend-1.md` | Mensagem duplicada ao enviar (optimistic update + echo do hub) | Alto | Fechado |
| `bugs/sprint-6/bug-frontend-2.md` | Sintaxe de template mista (*ngIf/*ngFor e @if/@for) em OnlineUsersComponent | Médio | Fechado |

### Veredicto
- [x] QA OK — aprovado para commit final
- [ ] QA NOK — bugs em aberto
