# Frontend — Fix filtro de presença + persistir userId no AuthService

## Contexto
Atualmente o usuário logado aparece na própria lista de usuários online. O problema ocorre porque o frontend não filtra o usuário atual ao renderizar a lista recebida do SignalR. Além disso, o `AuthService` salva o email mas não salva o `userId` em sessionStorage, o que será necessário para as features de chat da sprint.

## Escopo

### Inclui:
- Persistir `userId` no `sessionStorage` via `AuthService` (método `saveUserId` + `getUserId`).
- Atualizar o fluxo de login e registro no `AuthComponent` para chamar `saveUserId`.
- Filtrar o próprio usuário da lista `onlineUsers$` no `PresenceHubService` ou no `OnlineUsersComponent`.
- Atualizar o contador "ONLINE" para refletir apenas outros usuários.

### Não inclui:
- Nenhuma alteração no backend.
- Nenhuma alteração no `PresenceHub.cs`.
- Chat ou notificações (cobertos pelas tasks-frontend-2 e 3).

## Dependências
- `AuthService` em `src/app/core/services/auth.service.ts`.
- `PresenceHubService` em `src/app/core/services/presence-hub.service.ts`.
- `OnlineUsersComponent` em `src/app/features/dashboard/components/online-users/`.
- `AuthComponent` (login/cadastro) em `src/app/features/auth/`.

## Requisitos funcionais

### 1. `AuthService` — persistir e recuperar userId
Adicionar em `auth.service.ts`:
```typescript
private readonly userIdKey = 'chat_user_id';

saveUserId(userId: string): void {
  sessionStorage.setItem(this.userIdKey, userId);
}

getUserId(): string | null {
  return sessionStorage.getItem(this.userIdKey);
}
```
Atualizar `clearTokens()` para também remover `chat_user_id` do sessionStorage.

### 2. `AuthComponent` — chamar `saveUserId` após login/registro
No callback de sucesso de login e de registro (onde já se chama `auth.saveTokens` e `auth.saveEmail`), adicionar:
```typescript
this.auth.saveUserId(response.userId);
```
O campo `userId` já existe em `AuthResponse` (tipo retornado pela API).

### 3. Filtro na lista de usuários online
Opção preferencial: filtrar no `PresenceHubService` ao receber o evento `OnlineUsers`, excluindo o userId do próprio usuário:

```typescript
this.connection.on('OnlineUsers', (users: OnlineUser[]) => {
  const myId = this.auth.getUserId();
  const filtered = (users ?? []).filter(u => u.userId !== myId);
  this.onlineUsers$.next(filtered);
});
```

Isso garante que o filtro é centralizado e qualquer consumidor do observable já recebe a lista limpa.

### 4. `OnlineUsersComponent` — nenhuma mudança necessária para o filtro
O componente continuará recebendo `users` via `@Input()`. O contador `users.length` já refletirá o valor correto após o filtro no serviço.

## Requisitos UX/UI
- O contador "ONLINE" no painel de presença deve mostrar somente o número de outros usuários (não inclui o próprio usuário).
- Se nenhum outro usuário estiver online, exibir o estado vazio "Nenhum usuário online" normalmente.
- Nenhuma alteração visual além da remoção do próprio usuário da lista.

## Critérios de aceitação

- **Given** usuário A está logado, **When** abre o dashboard, **Then** seu próprio email NÃO aparece na lista de presença.
- **Given** usuário A e B estão logados, **When** A visualiza a lista, **Then** apenas B aparece; contador mostra "1".
- **Given** o usuário faz logout e loga novamente, **When** o dashboard carrega, **Then** o filtro continua funcionando (userId foi salvo corretamente).
- **Given** usuário A está logado sem mais ninguém online, **When** visualiza o painel, **Then** exibe "Nenhum usuário online" e contador "0".

## Definição de pronto (DoD)
- [ ] Build `ng build` sem erros.
- [ ] `AuthService` possui `saveUserId` e `getUserId` e `clearTokens` limpa o userId.
- [ ] `AuthComponent` chama `saveUserId` no sucesso de login e registro.
- [ ] Usuário logado não aparece na própria lista de presença.
- [ ] Contador "ONLINE" exclui o próprio usuário.
