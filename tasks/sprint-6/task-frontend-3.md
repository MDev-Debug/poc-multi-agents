# Frontend — Notificações de mensagem não lida na lista de presença

## Contexto
Com o chat privado funcionando (task-frontend-2), esta task adiciona o comportamento de notificação: quando uma mensagem chega de um usuário cuja conversa NÃO está aberta, um badge visual aparece ao lado do nome desse usuário na lista de presença. Quando o usuário abre a conversa, o badge é zerado.

## Escopo

### Inclui:
- Serviço de contagem de mensagens não lidas: `UnreadMessagesService`.
- Badge visual no `OnlineUsersComponent` por usuário com mensagens não lidas.
- Integração com `ChatHubService.messages$` para incrementar contagem ao receber mensagem.
- Integração com `DashboardComponent.activeConversationUser` para zerar contagem ao abrir conversa.

### Não inclui:
- Persistência de contagem no banco ou localStorage (in-memory apenas, zerado ao recarregar).
- Notificações push (browser Notification API).
- Badge global no título da aba (favicon counter).
- Notificações para mensagens enviadas pelo próprio usuário.

## Dependências
- task-frontend-1 concluída (`AuthService.getUserId()` disponível).
- task-frontend-2 concluída (`ChatHubService.messages$` disponível, `DashboardComponent.activeConversationUser` disponível).

## Requisitos funcionais

### 1. `UnreadMessagesService` — `src/app/core/services/unread-messages.service.ts`

```typescript
@Injectable({ providedIn: 'root' })
export class UnreadMessagesService {
  // Map<senderId, count>
  private readonly counts = new BehaviorSubject<Map<string, number>>(new Map());
  readonly counts$ = this.counts.asObservable();

  increment(senderId: string): void { ... }
  clear(userId: string): void { ... }
  getCount(userId: string): number { ... }
}
```

- `increment(senderId)`: incrementa o contador para aquele remetente e emite o novo Map.
- `clear(userId)`: zera o contador daquele userId e emite o novo Map.
- `getCount(userId)`: retorna o valor atual (0 se não existir).

### 2. Integração no `DashboardComponent`

No `ngOnInit`, após conectar os hubs, assinar `ChatHubService.messages$`:
```typescript
this.subs.add(
  this.chatHub.messages$.subscribe(msg => {
    const activeUser = this.activeConversationUser();
    // Só notifica se a conversa com o remetente NÃO está aberta
    if (!activeUser || activeUser.userId !== msg.senderId) {
      // Não notificar mensagens enviadas pelo próprio usuário
      if (msg.senderId !== this.auth.getUserId()) {
        this.unreadMessages.increment(msg.senderId);
      }
    }
  })
);
```

No método `openConversation(user)`: após setar `activeConversationUser`, chamar `this.unreadMessages.clear(user.userId)`.

Injetar `UnreadMessagesService` no `DashboardComponent` e passar `unreadCounts$` para o `OnlineUsersComponent`.

### 3. `OnlineUsersComponent` — exibir badge de não lidas

Adicionar Input:
```typescript
@Input() unreadCounts: Map<string, number> = new Map();
```

No template, dentro do `.user-item`, adicionar condicionalmente o badge:
```html
@if ((unreadCounts.get(user.userId) ?? 0) > 0) {
  <span
    class="unread-badge"
    [attr.aria-label]="(unreadCounts.get(user.userId)) + ' mensagem(ns) não lida(s) de ' + user.email"
    role="status">
    {{ unreadCounts.get(user.userId) }}
  </span>
}
```

No `DashboardComponent` template, passar o Map para `OnlineUsersComponent`:
```html
<app-online-users
  [users]="(presenceHub.onlineUsers$ | async) ?? []"
  [unreadCounts]="(unreadMessages.counts$ | async) ?? emptyMap"
  (userSelected)="openConversation($event)">
</app-online-users>
```
Onde `emptyMap = new Map<string, number>()` definido no componente.

### 4. SCSS do badge — `online-users.component.scss`

```scss
.unread-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 20px;
  height: 20px;
  padding: 0 5px;
  border-radius: 10px;
  background: var(--neon-pink);
  color: #fff;
  font-size: 11px;
  font-weight: 700;
  margin-left: auto;
  box-shadow: var(--glow-pink);
  animation: pulse-badge 1.5s ease-in-out infinite;
}

@keyframes pulse-badge {
  0%, 100% { opacity: 1; transform: scale(1); }
  50%       { opacity: 0.75; transform: scale(1.15); }
}
```

O `.user-item` deve ter `display: flex; align-items: center` para que o badge fique alinhado à direita.

## Requisitos UX/UI

### Fluxo de notificação
1. Usuário A está no dashboard com conversa fechada (placeholder visível).
2. Usuário B envia mensagem para A via chat privado.
3. A recebe o evento `ReceiveMessage` no `ChatHubService`.
4. `DashboardComponent` detecta que a conversa com B não está aberta → incrementa contador.
5. Badge neon-pink aparece ao lado do email de B na lista de presença com o número de mensagens.
6. A clica em B → conversa abre → `clear(B.userId)` → badge desaparece.

### Estados do badge
- Badge visível: fundo `--neon-pink`, texto branco, glow pink, animação pulse.
- Badge ausente: elemento não renderizado (sem espaço reservado).
- Máximo exibido: se count > 99, exibir "99+".

### Acessibilidade
- `aria-label` descritivo no badge (conforme modelo acima).
- `role="status"` para leitores de tela.
- A animação de pulse deve respeitar `prefers-reduced-motion`: envolver a animação em `@media (prefers-reduced-motion: no-preference)`.

## Critérios de aceitação

- **Given** usuário A está no dashboard sem nenhuma conversa aberta, **When** usuário B envia mensagem para A, **Then** badge com "1" aparece ao lado do email de B na lista de presença de A.
- **Given** badge com "2" exibido para B, **When** A clica em B para abrir a conversa, **Then** badge desaparece imediatamente.
- **Given** A já está na conversa com B (conversa aberta), **When** B envia nova mensagem, **Then** nenhum badge é incrementado para B.
- **Given** usuário A envia mensagem para B, **When** o evento retorna para A (echo do hub), **Then** A NÃO vê badge para si mesmo.
- **Given** count for maior que 99, **When** badge é exibido, **Then** mostra "99+".
- **Given** `prefers-reduced-motion` ativo, **When** badge é exibido, **Then** animação de pulse não ocorre.

## Definição de pronto (DoD)
- [ ] Build `ng build` sem erros.
- [ ] `UnreadMessagesService` implementado com `increment`, `clear` e `counts$`.
- [ ] `DashboardComponent` assina `messages$`, chama `increment` somente quando conversa não está aberta e o remetente não é o próprio usuário.
- [ ] `DashboardComponent.openConversation` chama `unreadMessages.clear`.
- [ ] `OnlineUsersComponent` recebe `unreadCounts` e renderiza badge condicionalmente.
- [ ] Badge animado com pulse neon-pink.
- [ ] Contagem "99+" para valores acima de 99.
- [ ] `prefers-reduced-motion` respeitado.
- [ ] Atributos ARIA `aria-label` e `role="status"` presentes no badge.
