# patch-backend-3 — Validação de Input, HTTPS e Cabeçalhos de Segurança

## Vulnerabilidades

### VUL-B3a — MÉDIA: DTOs sem validação completa (SendMessageRequest)
- **Arquivo**: `backend/Chat.Application/DTOs/Chat/SendMessageRequest.cs`
- **Problema**: `SendMessageRequest` não possui nenhuma anotação de validação. O `Content` pode chegar vazio ou com 0 caracteres antes de chegar no hub (caso seja usado diretamente em um endpoint REST). `ReceiverId` não tem `[Required]`.
- **Observação**: A validação no `ChatHub.SendPrivateMessage` (`string.IsNullOrWhiteSpace + Length > 4000`) é correta mas defensiva — o DTO não valida.

### VUL-B3b — BAIXA: CORS excessivamente permissivo em AllowedHosts
- **Arquivo**: `backend/Chat.Api/appsettings.json`
- **Problema**: `"AllowedHosts": "*"` permite qualquer host. Em produção, deve ser restrito ao domínio real.

### VUL-B3c — MÉDIA: Ausência de cabeçalhos de segurança HTTP
- **Arquivo**: `backend/Chat.Api/Program.cs`
- **Problema**: Sem `Content-Security-Policy`, `X-Frame-Options`, `X-Content-Type-Options`, `Strict-Transport-Security`.

### VUL-B3d — BAIXA: HTTPS não enforçado em desenvolvimento
- **Arquivo**: `backend/Chat.Api/Program.cs`
- **Problema**: `app.UseHttpsRedirection()` não está presente. Tokens JWT trafegam em HTTP simples.

### VUL-B3e — MÉDIA: Ausência de validação de `[ApiController]` para `[Required]`
- **Arquivos**: `RegisterRequest.cs`, `LoginRequest.cs`
- **Problema**: `[Required]` está presente mas sem `[StringLength]` — email pode ter comprimento arbitrário, password sem limite máximo (risco de DoS por hash de senha extremamente longa).

---

## Solução

### 1. Corrigir `SendMessageRequest.cs` com validações

```csharp
using System.ComponentModel.DataAnnotations;

namespace Chat.Application.DTOs.Chat;

public sealed class SendMessageRequest
{
    [Required]
    public Guid ReceiverId { get; init; }

    [Required]
    [MinLength(1)]
    [MaxLength(4000)]
    public string Content { get; init; } = string.Empty;
}
```

---

### 2. Corrigir `RegisterRequest.cs` — adicionar MaxLength em Password

```csharp
using System.ComponentModel.DataAnnotations;

namespace Chat.Application.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]           // aumentar de 6 para 8 chars mínimo
    [MaxLength(128)]         // limitar para evitar DoS no hash bcrypt/PBKDF2
    public string Password { get; init; } = string.Empty;
}
```

---

### 3. Corrigir `LoginRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Chat.Application.DTOs.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(128)]    // mesmo limite do register para evitar DoS
    public string Password { get; init; } = string.Empty;
}
```

---

### 4. Adicionar cabeçalhos de segurança e HTTPS — `Program.cs`

Adicionar logo após `var app = builder.Build();`:

```csharp
// ── Cabeçalhos de segurança HTTP ───────────────────────────────────────────
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options",  "nosniff");
    context.Response.Headers.Append("X-Frame-Options",          "DENY");
    context.Response.Headers.Append("Referrer-Policy",          "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy",       "geolocation=(), microphone=(), camera=()");

    // HSTS apenas em produção (não em dev, onde não há cert TLS local)
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append(
            "Strict-Transport-Security",
            "max-age=31536000; includeSubDomains");
    }

    await next();
});

// Redirecionar HTTP → HTTPS fora do desenvolvimento
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```

---

### 5. Restringir `AllowedHosts` em produção

Em `appsettings.json` (development pode manter `*`):

```json
{
  "AllowedHosts": "*"
}
```

Em `appsettings.Production.json` (criar se não existir):

```json
{
  "AllowedHosts": "chat.suaempresa.com;api.suaempresa.com",
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

---

### 6. Revisar CORS para produção

Em `Program.cs`, ajustar a política CORS para suportar múltiplos ambientes:

```csharp
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
        ?? ["http://localhost:4200"];

    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

Em `appsettings.Development.json`:
```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  }
}
```

---

## Checklist de aplicação

- [ ] Atualizar `SendMessageRequest.cs` com `[Required]`, `[MinLength(1)]`, `[MaxLength(4000)]`
- [ ] Atualizar `RegisterRequest.cs` — `MinLength(8)`, `MaxLength(128)`, `MaxLength(320)` no email
- [ ] Atualizar `LoginRequest.cs` — `MaxLength(128)`, `MaxLength(320)`
- [ ] Adicionar middleware de cabeçalhos de segurança em `Program.cs`
- [ ] Adicionar `UseHttpsRedirection` para produção em `Program.cs`
- [ ] Configurar CORS via `appsettings` em vez de hardcode
- [ ] Criar `appsettings.Production.json` com `AllowedHosts` restrito
- [ ] `dotnet build` — 0 erros
