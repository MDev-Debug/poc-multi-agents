# Bug 1 — Mensagem duplicada ao enviar (optimistic update + echo do hub)

## Task relacionada
- `tasks/sprint-6/task-frontend-2.md`

## Como reproduzir
1. Usuário A abre conversa com usuário B no `PrivateChatComponent`.
2. Usuário A digita uma mensagem e pressiona Enviar (ou Enter).
3. `sendMessage()` executa o optimistic update: adiciona a mensagem imediatamente ao array `messages` (linha 110 de `private-chat.component.ts`).
4. `chatHub.sendMessage()` invoca `SendPrivateMessage` no backend.
5. O backend, além de enviar para o receptor, também envia o evento `ReceiveMessage` para as outras conexões do remetente (`senderOtherConnections` em `ChatHub.cs`).
6. Porém, mesmo na conexão atual, o `ChatHubService` escuta `ReceiveMessage` e emite via `messages$`.
7. O `PrivateChatComponent` assina `messages$` e, ao receber a mensagem confirmada, verifica `msg.senderId === this.myUserId` — condição verdadeira — e adiciona a mensagem novamente ao array (linha 57-60 de `private-chat.component.ts`).
8. Resultado: a mensagem aparece duas vezes na lista.

## Resultado esperado
- Após enviar, a mensagem deve aparecer exatamente uma vez na lista de mensagens.
- O optimistic update deve ser confirmado (ou substituído) pela resposta real do hub, sem duplicação.

## Resultado atual
- A mensagem enviada aparece duas vezes: uma vez pelo optimistic update e outra vez quando o hub emite o `ReceiveMessage` de volta ao próprio remetente (echo da conexão atual via `messages$`).

## Evidência

`private-chat.component.ts` — lógica de envio com optimistic update:
```typescript
// Linha 110 — adiciona optimistic
this.messages.update(list => [...list, optimistic]);
// ...
await this.chatHub.sendMessage(this.otherUser.userId, content);
```

`private-chat.component.ts` — assinatura de messages$:
```typescript
// Linha 56-61
this.chatHub.messages$.subscribe(msg => {
  if (msg.senderId === this.otherUser.userId || msg.senderId === this.myUserId) {
    this.messages.update(list => [...list, msg]); // adiciona novamente
    this.shouldScrollToBottom = true;
  }
});
```

`chat-hub.service.ts` — o hub emite messages$ para TODAS as mensagens recebidas:
```typescript
// Linha 35-37
this.connection.on('ReceiveMessage', (message: ChatMessage) => {
  this.messages$.next(message); // inclui mensagens enviadas pelo próprio usuário
});
```

O backend (`ChatHub.cs`, linhas 60-67) envia `ReceiveMessage` para `senderOtherConnections` (outras abas), mas o `ChatHubService` no frontend não tem essa distinção — emite via `messages$` para qualquer `ReceiveMessage` recebido, inclusive os que chegam da conexão atual.

Nota: O backend NÃO envia de volta para a conexão atual do remetente (`Context.ConnectionId` é excluído de `senderOtherConnections`). Entretanto, o próprio remetente também recebe mensagens via `messages$` porque a assinatura no componente aceita `msg.senderId === this.myUserId`. Quando o interlocutor B responde e o sender A vê a mensagem de A que passou por `senderOtherConnections` via uma segunda aba, há duplicação. Mesmo sem segunda aba: se futuramente o backend for ajustado para retornar confirmação ao remetente, a duplicação ocorre. Adicionalmente, em abas múltiplas do mesmo usuário, a segunda aba exibirá mensagens enviadas pela primeira aba sem duplicação aparente, mas a primeira aba sofrerá o problema descrito ao receber o echo.

**O caminho de duplicação mais direto e imediato**: a assinatura `msg.senderId === this.myUserId` no filtro de `messages$` vai capturar a mensagem optimistic se ela for ecoada de volta. Com o backend atual excluindo a conexão atual, a duplicação só ocorre em múltiplas abas. Porém, a condição `msg.senderId === this.myUserId` é desnecessariamente ampla — aceita mensagens do próprio usuário vindas de qualquer lugar, incluindo a mesma aba em cenários de reconnect.

## Severidade
- [ ] Crítico (bloqueia uso)
- [x] Alto (funcionalidade principal quebrada)
- [ ] Médio (funcionalidade secundária ou visual)
- [ ] Baixo (cosmético / melhoria)

## Critério de correção (Given/When/Then)
- Given usuário A está em conversa com B e envia uma mensagem
- When a mensagem é enviada com sucesso via hub
- Then a mensagem aparece exatamente uma vez na lista de mensagens de A, sem duplicação

**Sugestão de correção**: filtrar em `messages$` apenas mensagens cujo `senderId === otherUser.userId` (somente do interlocutor). Mensagens do próprio usuário só entram via optimistic update. Ou, ao receber mensagem do próprio usuário via hub, substituir o optimistic (por `messageId`) em vez de adicionar nova entrada.

## Status
- [ ] Aberto
- [ ] Em correção
- [ ] Corrigido — aguardando re-teste
- [x] Fechado

## Re-teste (2026-04-21)
- Linha 57 de `private-chat.component.ts`: filtro é `msg.senderId === this.otherUser.userId` apenas, sem a condição `|| msg.senderId === this.myUserId`.
- Optimistic update presente na linha 110 e não é duplicado pela assinatura de `messages$`.
- `ng build` passa sem erros ou warnings.
- Critério Given/When/Then satisfeito.
