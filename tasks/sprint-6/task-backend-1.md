# Backend — Entidade Message + Migration + ChatHub + Endpoint de histórico

## Contexto
O projeto já possui autenticação JWT, SignalR para presença (`PresenceHub`) e Clean Architecture com as camadas `Chat.Domain`, `Chat.Application`, `Chat.Infrastructure` e `Chat.Api`. Esta task adiciona a infraestrutura necessária para mensagens privadas em tempo real.

## Escopo

### Inclui:
- Entidade `Message` no domínio (`Chat.Domain`).
- Migration EF Core para criar a tabela `Messages` no banco CHAT.
- Interface `IMessageService` e implementação `MessageService` na camada `Chat.Application`.
- `ChatHub` (SignalR) em `Chat.Api/Hubs/ChatHub.cs` com método `SendPrivateMessage`.
- Endpoint REST `GET /api/messages/{otherUserId}?page=1&pageSize=50` em `Chat.Api/Controllers/MessagesController.cs`.
- Registro do hub e do controller no `Program.cs`.

### Não inclui:
- Read receipts / marcação de lido no banco.
- Notificações push externas.
- Paginação com cursor (somente offset simples).
- Nenhuma mudança nos hubs ou serviços existentes (`PresenceHub`, `PresenceService`).

## Dependências
- SQL Server local: `Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;`
- JWT configurado (o hub extrai `sub` e `email` do `ClaimsPrincipal`).

## Requisitos funcionais

### 1. Entidade `Chat.Domain/Entities/Message.cs`
```
Id          : Guid (PK)
SenderId    : Guid (FK → AppUser.Id)
ReceiverId  : Guid (FK → AppUser.Id)
Content     : string (max 4000 chars)
SentAt      : DateTimeOffset
```

### 2. `ChatDbContext` — adicionar `DbSet<Message> Messages`
Configurar no `OnModelCreating`:
- PK em `Id`.
- FK `SenderId` → `AppUser` (sem cascade delete — `DeleteBehavior.Restrict`).
- FK `ReceiverId` → `AppUser` (sem cascade delete — `DeleteBehavior.Restrict`).
- Índice composto `(SenderId, ReceiverId, SentAt)` para otimizar consultas de histórico.
- `Content` com `HasMaxLength(4000)`.

### 3. Migration
Executar `dotnet ef migrations add AddMessages --project Chat.Infrastructure --startup-project Chat.Api` e aplicar com `dotnet ef database update --project Chat.Infrastructure --startup-project Chat.Api`.

### 4. DTOs em `Chat.Application/DTOs/Chat/`

`MessageDto.cs`:
```
MessageId  : Guid
SenderId   : Guid
SenderEmail: string
Content    : string
SentAt     : DateTimeOffset
```

`SendMessageRequest.cs` (usado internamente pelo hub):
```
ReceiverId : Guid
Content    : string (required, 1–4000 chars)
```

### 5. Interface `Chat.Application/Interfaces/IMessageService.cs`
```csharp
Task<MessageDto> SaveAsync(Guid senderId, Guid receiverId, string content, CancellationToken ct);
Task<IReadOnlyList<MessageDto>> GetHistoryAsync(Guid userA, Guid userB, int page, int pageSize, CancellationToken ct);
```

### 6. Implementação `Chat.Application/Services/MessageService.cs`
- `SaveAsync`: cria `Message`, salva no banco, retorna `MessageDto`.
- `GetHistoryAsync`: busca mensagens onde `(SenderId == userA AND ReceiverId == userB) OR (SenderId == userB AND ReceiverId == userA)`, ordena por `SentAt ASC`, aplica paginação `Skip((page-1)*pageSize).Take(pageSize)`.

### 7. `Chat.Api/Hubs/ChatHub.cs`
- Decorar com `[Authorize]`.
- Extrair `userId` (Guid) e `email` do `ClaimsPrincipal` (claims `sub` / `NameIdentifier`).
- Método: `Task SendPrivateMessage(Guid receiverId, string content)`.
  - Validar `content` não vazio e `content.Length <= 4000`; caso contrário, retornar sem enviar.
  - Chamar `IMessageService.SaveAsync`.
  - Chamar `IUserConnectionTracker.GetConnectionIds(receiverId)` para obter as conexões do receptor.
  - Enviar evento `ReceiveMessage` (com `MessageDto`) para as conexões do receptor via `Clients.Clients(connectionIds)`.
  - Enviar o mesmo evento `ReceiveMessage` de volta para as conexões do próprio remetente (exceto a conexão atual) via `Clients.Clients(senderOtherConnectionIds)` — para suporte a múltiplas abas.

### 8. `IUserConnectionTracker` e `UserConnectionTracker`
Criar `Chat.Application/Interfaces/IUserConnectionTracker.cs`:
```csharp
void AddConnection(Guid userId, string connectionId);
void RemoveConnection(Guid userId, string connectionId);
IReadOnlyList<string> GetConnectionIds(Guid userId);
```

Implementação `Chat.Infrastructure/Services/UserConnectionTracker.cs` usando `ConcurrentDictionary<Guid, HashSet<string>>` com lock.

Registrar como `Singleton` no `Program.cs`.

No `ChatHub.OnConnectedAsync` e `OnDisconnectedAsync`: chamar `AddConnection` / `RemoveConnection`.

### 9. `Chat.Api/Controllers/MessagesController.cs`
- `[Authorize]` + `[ApiController]` + `[Route("api/messages")]`.
- `GET /{otherUserId}?page=1&pageSize=50`:
  - Extrair `userId` do JWT.
  - Chamar `IMessageService.GetHistoryAsync(userId, otherUserId, page, pageSize)`.
  - Retornar `Ok(messages)`.
  - Validar que `pageSize` máximo é 100.

### 10. `Program.cs` — registros a adicionar
```csharp
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddSingleton<IUserConnectionTracker, UserConnectionTracker>();
// ...
app.MapHub<ChatHub>("/hubs/chat");
```
CORS já configurado deve incluir `/hubs/chat` (verificar se a política existente usa `AllowAnyOrigin` ou URL específica — manter consistência com o hub de presença).

## Critérios de aceitação

- **Given** dois usuários autenticados conectados ao ChatHub, **When** o usuário A invoca `SendPrivateMessage(receiverId: B, content: "Oi")`, **Then** o usuário B recebe o evento `ReceiveMessage` com o DTO correto via SignalR.
- **Given** usuário autenticado, **When** faz `GET /api/messages/{otherUserId}`, **Then** retorna lista de mensagens ordenadas por `SentAt ASC` com status 200.
- **Given** `content` vazio ou > 4000 chars, **When** `SendPrivateMessage` é invocado, **Then** nenhuma mensagem é salva nem enviada.
- **Given** usuário não autenticado, **When** tenta conectar ao `/hubs/chat`, **Then** recebe 401.
- **Given** migration aplicada, **When** banco inicia, **Then** tabela `Messages` existe com as colunas e FKs corretas.

## Definição de pronto (DoD)
- [ ] Build `dotnet build` sem erros ou warnings novos.
- [ ] Migration aplicada com sucesso no banco CHAT local.
- [ ] `ChatHub` acessível em `ws://localhost:5000/hubs/chat` com token válido.
- [ ] `GET /api/messages/{id}` retorna 200 com array (pode ser vazio).
- [ ] Erros de validação (content vazio/longo, usuário não autenticado) tratados com respostas adequadas.
- [ ] Nenhuma alteração nos arquivos existentes de presença (`PresenceHub.cs`, `PresenceService.cs`, `IPresenceService.cs`).
