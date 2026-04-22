# patch-backend-1 — Criptografia de Mensagens (AES-256-GCM)

## Vulnerabilidade
**CRÍTICA** — Mensagens de chat armazenadas em texto plano na coluna `Content` (nvarchar(4000)).
Qualquer acesso ao banco de dados (DBA, backup, SQL injection, dump) expõe todo o histórico de conversas privadas.

## Solução
Implementar `IEncryptionService` usando AES-256-GCM com chave configurada via variável de ambiente.
Cada mensagem recebe IV e tag únicos (autenticação de integridade). O dado cifrado é armazenado como Base64.

## Arquivos a criar/modificar

### 1. `backend/Chat.Application/Interfaces/IEncryptionService.cs` (NOVO)

```csharp
namespace Chat.Application.Interfaces;

/// <summary>
/// Serviço de criptografia simétrica para dados sensíveis em repouso.
/// Implementação usa AES-256-GCM (AEAD): confidencialidade + integridade.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Cifra o texto plano e retorna string Base64 no formato: IV(12b)+CipherText+Tag(16b).
    /// </summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decifra e verifica a integridade. Lança <see cref="CryptographicException"/> se corrompido/adulterado.
    /// </summary>
    string Decrypt(string ciphertext);
}
```

---

### 2. `backend/Chat.Infrastructure/Services/EncryptionService.cs` (NOVO)

```csharp
using System.Security.Cryptography;
using System.Text;
using Chat.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Chat.Infrastructure.Services;

/// <summary>
/// Implementação de AES-256-GCM.
/// Formato do payload armazenado (Base64 de bytes concatenados):
///   [12 bytes IV] + [N bytes ciphertext] + [16 bytes GCM tag]
///
/// Configuração obrigatória (uma das duas):
///   - Variável de ambiente: CHAT_ENCRYPTION_KEY (Base64, 32 bytes = 256 bits)
///   - appsettings: Encryption:Key (mesmo formato)
/// </summary>
public sealed class EncryptionService : IEncryptionService
{
    private const int IvSize  = 12;  // GCM recomenda 96 bits
    private const int TagSize = 16;  // 128-bit authentication tag

    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        // Prioridade: variável de ambiente > appsettings
        var keyBase64 =
            Environment.GetEnvironmentVariable("CHAT_ENCRYPTION_KEY")
            ?? configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "A chave de criptografia não está configurada. " +
                "Defina a variável de ambiente CHAT_ENCRYPTION_KEY " +
                "ou a configuração Encryption:Key com 32 bytes em Base64.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
            throw new InvalidOperationException(
                "CHAT_ENCRYPTION_KEY deve ter exatamente 32 bytes (256 bits) em Base64.");
    }

    public string Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var iv             = new byte[IvSize];
        RandomNumberGenerator.Fill(iv);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag        = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(iv, plaintextBytes, ciphertext, tag);

        // Layout: IV | ciphertext | tag
        var combined = new byte[IvSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(iv,         0, combined, 0,                       IvSize);
        Buffer.BlockCopy(ciphertext, 0, combined, IvSize,                  ciphertext.Length);
        Buffer.BlockCopy(tag,        0, combined, IvSize + ciphertext.Length, TagSize);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);

        var combined = Convert.FromBase64String(ciphertext);

        if (combined.Length < IvSize + TagSize)
            throw new CryptographicException("Payload cifrado inválido: tamanho insuficiente.");

        var iv             = combined[..IvSize];
        var tag            = combined[^TagSize..];
        var encryptedBytes = combined[IvSize..^TagSize];
        var plaintextBytes = new byte[encryptedBytes.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(iv, encryptedBytes, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
```

---

### 3. `backend/Chat.Application/Interfaces/IMessageService.cs` — sem alteração necessária

A interface permanece igual; a mudança é transparente na implementação.

---

### 4. `backend/Chat.Infrastructure/Services/MessageService.cs` — MODIFICAR

Adicionar `IEncryptionService` no construtor e integrar nos métodos `SaveAsync` e `GetHistoryAsync`.

```csharp
using Chat.Application.DTOs.Chat;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Services;

public sealed class MessageService(ChatDbContext db, IEncryptionService encryption) : IMessageService
{
    public async Task<MessageDto> SaveAsync(Guid senderId, Guid receiverId, string content, CancellationToken ct)
    {
        var sender = await db.Users.AsNoTracking().FirstAsync(u => u.Id == senderId, ct);

        // Cifra o conteúdo antes de persistir
        var encryptedContent = encryption.Encrypt(content);

        var message = new Message
        {
            Id         = Guid.NewGuid(),
            SenderId   = senderId,
            ReceiverId = receiverId,
            Content    = encryptedContent,     // ← cifrado
            SentAt     = DateTimeOffset.UtcNow,
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);

        // Retorna o conteúdo em texto plano para o cliente
        return new MessageDto
        {
            MessageId   = message.Id,
            SenderId    = message.SenderId,
            SenderEmail = sender.Email,
            Content     = content,              // ← texto plano
            SentAt      = message.SentAt,
        };
    }

    public async Task<IReadOnlyList<MessageDto>> GetHistoryAsync(
        Guid userA, Guid userB, int page, int pageSize, CancellationToken ct)
    {
        var rows = await db.Messages
            .AsNoTracking()
            .Where(m =>
                (m.SenderId == userA && m.ReceiverId == userB) ||
                (m.SenderId == userB && m.ReceiverId == userA))
            .OrderBy(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(db.Users.AsNoTracking(),
                m => m.SenderId,
                u => u.Id,
                (m, u) => new
                {
                    m.Id,
                    m.SenderId,
                    u.Email,
                    m.Content,        // ← ainda cifrado neste ponto
                    m.SentAt,
                })
            .ToListAsync(ct);

        // Decifra no lado da aplicação, após o fetch
        return rows.Select(r => new MessageDto
        {
            MessageId   = r.Id,
            SenderId    = r.SenderId,
            SenderEmail = r.Email,
            Content     = encryption.Decrypt(r.Content),  // ← decifrado
            SentAt      = r.SentAt,
        }).ToList();
    }
}
```

---

### 5. `backend/Chat.Api/Program.cs` — MODIFICAR (registro no DI)

Adicionar após os outros `AddScoped`/`AddSingleton`:

```csharp
// Dentro do bloco de registro de serviços, antes de builder.Build():
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
```

Importação necessária no topo:
```csharp
using Chat.Infrastructure.Services;
```

---

### 6. Migration para alterar coluna `Content`

O tipo da coluna deve mudar de `nvarchar(4000)` para `nvarchar(max)` porque o payload Base64 de AES-256-GCM é maior que o texto plano original:
- Texto de 4000 chars UTF-8 → ~4000 bytes → + 12 (IV) + 16 (tag) → Base64 ≈ 5372 chars

Criar arquivo: `backend/Chat.Infrastructure/Data/Migrations/YYYYMMDDHHMMSS_EncryptMessageContent.cs`

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Infrastructure.Data.Migrations
{
    public partial class EncryptMessageContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alterar coluna para nvarchar(max) para comportar o payload cifrado
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
```

Atualizar também `ChatDbContext.OnModelCreating` para remover o `HasMaxLength(4000)`:

```csharp
// Em ChatDbContext.cs, entidade Message — remover a linha:
entity.Property(x => x.Content).HasMaxLength(4000);
// Substituir por:
entity.Property(x => x.Content);  // nvarchar(max) — payload cifrado Base64
```

---

### 7. Configuração da chave via environment variable

**Desenvolvimento** — gerar chave de 32 bytes aleatórios:
```powershell
# PowerShell: gerar chave e imprimir como Base64
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
# Ou via .NET:
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

**Definir no ambiente local:**
```powershell
$env:CHAT_ENCRYPTION_KEY = "<base64-de-32-bytes>"
```

**Produção** — via variável de ambiente do container/host (NUNCA commitar em appsettings.json):
```
CHAT_ENCRYPTION_KEY=<base64-de-32-bytes>
```

**Desenvolvimento via appsettings.Development.json** (não commitar):
```json
{
  "Encryption": {
    "Key": "<base64-de-32-bytes>"
  }
}
```

---

### 8. Script de migração de dados existentes

Se o banco já possuir mensagens em texto plano, executar este script C# de migração one-shot **antes** de atualizar o serviço em produção:

```csharp
// MigrateExistingMessages.cs — script de migration de dados
// Executar UMA VEZ, após aplicar a migration de schema mas antes de deployar o novo código

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
var enc = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

var messages = await db.Messages.ToListAsync();
foreach (var msg in messages)
{
    // Detectar se já está cifrado (Base64 válido com tamanho > 28 bytes)
    // Heurística: texto plano não começa com um Base64 de exatamente os tamanhos esperados
    if (!IsLikelyEncrypted(msg.Content))
    {
        msg.Content = enc.Encrypt(msg.Content);
    }
}
await db.SaveChangesAsync();

static bool IsLikelyEncrypted(string content)
{
    // Payload mínimo: 12 (IV) + 0 (msg vazia) + 16 (tag) = 28 bytes → Base64 = 40 chars
    if (content.Length < 40) return false;
    try { Convert.FromBase64String(content); return true; }
    catch { return false; }
}
```

---

## Checklist de aplicação

- [ ] Criar `IEncryptionService.cs` em `Chat.Application/Interfaces/`
- [ ] Criar `EncryptionService.cs` em `Chat.Infrastructure/Services/`
- [ ] Modificar `MessageService.cs` com injeção de `IEncryptionService`
- [ ] Registrar `AddSingleton<IEncryptionService, EncryptionService>()` em `Program.cs`
- [ ] Remover `HasMaxLength(4000)` do `ChatDbContext`
- [ ] Gerar e aplicar migration `EncryptMessageContent`
- [ ] Definir `CHAT_ENCRYPTION_KEY` no ambiente (nunca em código/git)
- [ ] Executar script de migração de dados existentes se necessário
- [ ] Rodar `dotnet build` — 0 erros
- [ ] Testar envio e recebimento de mensagem end-to-end
- [ ] Verificar no banco que `Content` está em Base64 (não texto plano)
