# Frontend — ChatHubService + PrivateChatComponent + integração no Dashboard

## Contexto
Com o backend de chat pronto (`ChatHub` em `/hubs/chat` e endpoint `GET /api/messages/{otherUserId}`), esta task implementa o serviço Angular de conexão ao hub, o componente de conversa privada e a integração no layout do dashboard — substituindo o placeholder central ao clicar num usuário online.

## Escopo

### Inclui:
- `ChatHubService` em `src/app/core/services/chat-hub.service.ts`.
- `PrivateChatComponent` em `src/app/features/dashboard/components/private-chat/`.
- Tornar `OnlineUsersComponent` clicável (emitir evento com o usuário selecionado).
- Integração no `DashboardComponent`: gerenciar o usuário de conversa ativa e passar para `PrivateChatComponent`.
- Serviço `MessageApiService` (ou método no `ChatHubService`) para `GET /api/messages/{otherUserId}`.

### Não inclui:
- Notificações de mensagem não lida (task-frontend-3).
- Upload de arquivos.
- Paginação infinita.

## Dependências
- task-backend-1 concluída (hub `/hubs/chat` + endpoint `GET /api/messages/{otherUserId}` disponíveis).
- task-frontend-1 concluída (`AuthService.getUserId()` disponível).
- `AuthService` em `src/app/core/services/auth.service.ts`.

## Requisitos funcionais

### 1. `ChatHubService` — `src/app/core/services/chat-hub.service.ts`

Interface de mensagem recebida:
```typescript
export interface ChatMessage {
  messageId: string;
  senderId: string;
  senderEmail: string;
  content: string;
  sentAt: string; // ISO 8601
}
```

Responsabilidades:
- Gerenciar conexão SignalR para `http://localhost:5000/hubs/chat` com `accessTokenFactory`.
- Expor `messages$: Subject<ChatMessage>` — stream de mensagens recebidas via evento `ReceiveMessage`.
- Expor `connected$: BehaviorSubject<boolean>`.
- Método `connect(): Promise<void>` — conecta se ainda não conectado.
- Método `disconnect(): Promise<void>` — desconecta e limpa estado.
- Método `sendMessage(receiverId: string, content: string): Promise<void>` — invoca `SendPrivateMessage` no hub.
- Padrão `withAutomaticReconnect()` e `LogLevel.Warning` (igual ao `PresenceHubService`).

### 2. `MessageApiService` — `src/app/core/services/message-api.service.ts`
```typescript
getHistory(otherUserId: string, page = 1, pageSize = 50): Observable<ChatMessage[]>
```
- `GET http://localhost:5000/api/messages/{otherUserId}?page=1&pageSize=50`.
- Usar `HttpClient` com o interceptor de token existente (ou incluir header Authorization manualmente se não houver interceptor).
- Verificar se já existe um `HttpInterceptor` no projeto; se não existir, incluir o header `Authorization: Bearer <token>` diretamente via `AuthService.getToken()`.

### 3. `OnlineUsersComponent` — tornar clicável
Adicionar:
```typescript
@Output() userSelected = new EventEmitter<OnlineUser>();
```
No template, no `.user-item`, adicionar `(click)="userSelected.emit(user)"` e estilo `cursor: pointer` + hover highlight neon (já provável no SCSS, confirmar).

Adicionar `tabindex="0"` e `(keydown.enter)="userSelected.emit(user)"` para acessibilidade.
Adicionar `aria-label="Abrir conversa com {{ user.email }}"` em cada item.

### 4. `DashboardComponent` — gerenciar conversa ativa

Adicionar ao componente:
```typescript
activeConversationUser = signal<OnlineUser | null>(null);
```

Método:
```typescript
openConversation(user: OnlineUser): void {
  this.activeConversationUser.set(user);
}

closeConversation(): void {
  this.activeConversationUser.set(null);
}
```

No `ngOnInit`: também inicializar `ChatHubService.connect()`.
No `logout()` e `ngOnDestroy()`: também chamar `chatHub.disconnect()`.

No template `dashboard.component.html`, dentro de `<!-- Chat View -->`:
- Quando `activeConversationUser()` for null: exibir o placeholder atual.
- Quando `activeConversationUser()` tiver valor: exibir `<app-private-chat>` com o usuário ativo.
- `OnlineUsersComponent` deve receber `(userSelected)="openConversation($event)"`.

### 5. `PrivateChatComponent` — `src/app/features/dashboard/components/private-chat/`

Arquivo: `private-chat.component.ts`, `private-chat.component.html`, `private-chat.component.scss`.

Inputs:
```typescript
@Input({ required: true }) otherUser!: OnlineUser;
```

Output:
```typescript
@Output() closed = new EventEmitter<void>();
```

Comportamento:
- No `ngOnInit`: chamar `MessageApiService.getHistory(otherUser.userId)` para carregar histórico.
  - Estado `loading = signal(true)` durante a chamada.
  - Estado `error = signal(false)` em caso de falha; exibir mensagem + botão "Tentar novamente".
- Assinar `ChatHubService.messages$` e filtrar apenas mensagens cujo `senderId === otherUser.userId` OU `senderId === myUserId` (conversa bilateral).
  - Adicionar mensagens novas ao array `messages`.
- Scroll automático para o final da lista a cada nova mensagem e ao carregar o histórico (`AfterViewInit` + `@ViewChild` no contêiner de scroll).

Envio de mensagem:
- Campo `<textarea>` ou `<input>` para o conteúdo.
- Botão "Enviar" e atalho Enter (Shift+Enter para nova linha se textarea).
- Durante envio: `sending = signal(true)`, botão desabilitado, spinner mini.
- Em caso de erro: exibir mensagem inline "Falha ao enviar. Tente novamente." com `sendError = signal(false)`.
- Após envio bem-sucedido: limpar campo, focar novamente no input, adicionar mensagem ao array local imediatamente (optimistic update).

Layout das mensagens:
- Mensagem enviada pelo próprio usuário: alinhada à direita, fundo `--bg-surface`, borda neon-cyan sutil.
- Mensagem do interlocutor: alinhada à esquerda, fundo `--bg-panel`, borda neon-pink sutil.
- Horário: `HH:mm` exibido abaixo do conteúdo em `--text-muted`.

Header do chat:
- Avatar circular com inicial do email do interlocutor (fundo neon-pink ou neon-cyan).
- Email do interlocutor.
- Botão "X" (fechar) que emite `closed` e retorna ao placeholder.

## Requisitos UX/UI

### Fluxo principal
1. Usuário clica em um online user → `DashboardComponent.openConversation(user)`.
2. `PrivateChatComponent` aparece na área central.
3. Histórico carrega com spinner.
4. Mensagens são exibidas; scroll vai para o final.
5. Usuário digita e envia → mensagem aparece imediatamente (lado direito).
6. Interlocutor responde via SignalR → mensagem aparece no lado esquerdo, scroll automático.
7. Botão fechar → volta ao placeholder.

### Estados obrigatórios
- Carregando histórico: spinner neon centralizado + texto "Carregando mensagens...".
- Histórico vazio: ícone de chat + texto "Nenhuma mensagem ainda. Diga olá!".
- Erro ao carregar: ícone de erro + "Não foi possível carregar as mensagens." + botão "Tentar novamente" neon-pink.
- Enviando: botão Enviar desabilitado + mini spinner inline.
- Erro ao enviar: texto inline abaixo do input "Falha ao enviar. Tente novamente." em `--danger`.

### Acessibilidade
- `<div role="log" aria-live="polite" aria-label="Mensagens">` no contêiner da lista.
- Input: `aria-label="Mensagem para {{ otherUser.email }}"`.
- Botão Enviar: `aria-label="Enviar mensagem"`, `type="submit"`.
- Botão Fechar: `aria-label="Fechar conversa"`.
- Cada item de usuário online clicável: `role="button"`, `tabindex="0"`.

### Tema cyberpunk
- Header com fundo `--bg-panel` e borda inferior `--border`.
- Área de mensagens com fundo `--bg-deep`, padding lateral.
- Mensagem própria: fundo `--bg-surface`, borda esquerda 3px `--neon-cyan`.
- Mensagem do outro: fundo `--bg-panel`, borda esquerda 3px `--neon-pink`.
- Input: fundo `--bg-surface`, borda `--border`, focus glow `--glow-cyan`.
- Botão Enviar: gradiente `--neon-cyan` → `--neon-pink`, glow no hover.

## Critérios de aceitação

- **Given** usuário B está online, **When** usuário A clica em B na lista, **Then** `PrivateChatComponent` abre com header mostrando email de B.
- **Given** histórico existe, **When** a conversa abre, **Then** mensagens são exibidas em ordem cronológica com scroll no final.
- **Given** usuário A envia mensagem para B, **When** B recebe via SignalR, **Then** mensagem aparece em tempo real no chat de B sem reload.
- **Given** usuário A está na conversa com B, **When** A envia mensagem, **Then** mensagem aparece imediatamente no lado direito do chat de A.
- **Given** API de histórico falha, **When** a conversa abre, **Then** estado de erro é exibido com botão "Tentar novamente".
- **Given** conversa aberta, **When** usuário clica em fechar, **Then** placeholder padrão retorna.
- **Given** campo de texto focado, **When** usuário pressiona Enter, **Then** mensagem é enviada.

## Definição de pronto (DoD)
- [ ] Build `ng build` sem erros.
- [ ] `ChatHubService` conecta ao `/hubs/chat` com token JWT.
- [ ] `MessageApiService.getHistory` retorna lista tipada.
- [ ] `OnlineUsersComponent` emite `userSelected` ao clicar; item acessível por teclado.
- [ ] `PrivateChatComponent` carrega histórico, exibe mensagens e envia via hub.
- [ ] Scroll automático ao carregar histórico e ao receber nova mensagem.
- [ ] Todos os estados (loading, vazio, erro, enviando, erro de envio) implementados.
- [ ] Layout diferenciado: mensagens próprias à direita, do outro à esquerda.
- [ ] Atributos ARIA conforme especificado.
