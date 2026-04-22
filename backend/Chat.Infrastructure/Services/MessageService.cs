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
