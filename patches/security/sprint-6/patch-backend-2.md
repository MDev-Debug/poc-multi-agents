# patch-backend-2 — Segredos em Configuração + Rate Limiting no SignalR

## Vulnerabilidades

### VUL-B2a — ALTA: Chave JWT hardcoded em appsettings.json
- **Arquivo**: `backend/Chat.Api/appsettings.json` linha 8
- **Problema**: `"SigningKey": "dev-signing-key-change-me-32-bytes-minimum"` está no repositório git. Qualquer pessoa com acesso ao código pode forjar tokens JWT válidos.
- **CVSS estimado**: 9.1 (CRITICAL)

### VUL-B2b — ALTA: Connection string com senha em appsettings.json
- **Arquivo**: `backend/Chat.Api/appsettings.json` linha 3
- **Problema**: `Password=YourStrong!Passw0rd` exposta em texto plano no repositório.

### VUL-B2c — MÉDIA: Ausência de Rate Limiting nos endpoints de autenticação e no SignalR Hub
- **Endpoints**: `POST /api/auth/login`, `POST /api/auth/register`, `POST /api/auth/refresh`
- **Hub**: `ChatHub.SendPrivateMessage`
- **Problema**: Não há proteção contra brute-force em login, nem contra flood de mensagens no hub.

### VUL-B2d — MÉDIA: JWT com expiração de 120 minutos sem mecanismo de revogação
- **Arquivo**: `backend/Chat.Api/appsettings.json` linha 9, `JwtOptions.cs`
- **Problema**: Token comprometido fica válido por 2 horas. Não há blacklist de access tokens.

---

## Solução

### 1. Remover segredos do appsettings.json

**`backend/Chat.Api/appsettings.json`** — remover valores sensíveis, usar placeholders:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Issuer": "Chat",
    "Audience": "Chat",
    "SigningKey": "",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 14
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Alteração importante**: `ExpirationMinutes` reduzido de **120 para 15 minutos**.
Com refresh token rotation já implementado, 15 minutos é suficiente e limita a janela de uso de tokens comprometidos.

---

### 2. Definir segredos via variáveis de ambiente (desenvolvimento local)

```powershell
# PowerShell — setar variáveis para desenvolvimento local
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
$env:Jwt__SigningKey = "<gerar-chave-de-32-chars-minimo>"
```

**Alternativa para dev**: `dotnet user-secrets` (não comita em git):
```bash
cd backend/Chat.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CHAT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:SigningKey" "<chave-segura>"
```

---

### 3. Adicionar Rate Limiting — `backend/Chat.Api/Program.cs`

Adicionar o seguinte **antes** de `builder.Build()`:

```csharp
// ── Rate Limiting ──────────────────────────────────────────────────────────
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    // Política para endpoints de autenticação (brute-force protection)
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit         = 10;
        opt.Window              = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit          = 0;
    });

    // Política global para o resto da API
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit         = 200;
        opt.Window              = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit          = 0;
    });

    // Resposta padrão quando o limite é excedido
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

Adicionar **depois** de `app.Build()` e antes de `app.UseAuthentication()`:

```csharp
app.UseRateLimiter();
```

---

### 4. Aplicar política nos controllers — `AuthController.cs`

```csharp
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]   // ← adicionar este atributo
public sealed class AuthController(...) : ControllerBase
{
    // ... sem outras alterações
}
```

---

### 5. Rate Limiting no ChatHub — via middleware de mensagens

No `ChatHub.SendPrivateMessage`, adicionar verificação de tamanho mínimo e throttle por conexão usando um dicionário em memória. Solução simples e sem dependências externas:

```csharp
// Adicionar campo ao ChatHub:
private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (int Count, DateTime Window)>
    _rateLimitMap = new();

private bool IsRateLimited(string connectionId)
{
    var now = DateTime.UtcNow;
    var entry = _rateLimitMap.GetOrAdd(connectionId, _ => (0, now));

    // Reset janela de 1 minuto
    if ((now - entry.Window).TotalSeconds >= 60)
    {
        _rateLimitMap[connectionId] = (1, now);
        return false;
    }

    if (entry.Count >= 60) // máximo 60 mensagens/minuto por conexão
        return true;

    _rateLimitMap[connectionId] = (entry.Count + 1, entry.Window);
    return false;
}

// No início de SendPrivateMessage:
public async Task SendPrivateMessage(Guid receiverId, string content)
{
    if (IsRateLimited(Context.ConnectionId))
    {
        await Clients.Caller.SendAsync("Error", "Rate limit excedido. Aguarde antes de enviar novas mensagens.");
        return;
    }

    if (string.IsNullOrWhiteSpace(content) || content.Length > 4000)
        return;

    // ... resto do método sem alteração
}
```

---

### 6. Validar que JwtOptions.SigningKey não está vazio em Program.cs

Adicionar validação após `Get<JwtOptions>()`:

```csharp
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

// Garantir que a chave está configurada e é suficientemente forte
if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey não está configurada ou tem menos de 32 caracteres. " +
        "Use variável de ambiente Jwt__SigningKey ou dotnet user-secrets.");
}
```

---

### 7. Adicionar .gitignore para user-secrets e arquivos de ambiente

Verificar se `.gitignore` contém (adicionar se não tiver):

```gitignore
# Segredos locais
**/secrets.json
**/.env
**/.env.local
**/appsettings.Local.json
```

---

## Checklist de aplicação

- [ ] Remover `SigningKey` e `Password` do `appsettings.json` (deixar strings vazias)
- [ ] Reduzir `ExpirationMinutes` de 120 para 15
- [ ] Configurar segredos via `dotnet user-secrets` ou variáveis de ambiente
- [ ] Adicionar `AddRateLimiter` e `UseRateLimiter` em `Program.cs`
- [ ] Decorar `AuthController` com `[EnableRateLimiting("auth")]`
- [ ] Adicionar rate limiting por conexão no `ChatHub`
- [ ] Adicionar validação de `SigningKey` em `Program.cs`
- [ ] Verificar `.gitignore`
- [ ] `dotnet build` — 0 erros
