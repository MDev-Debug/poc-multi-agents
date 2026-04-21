using Chat.Application.DTOs.Chat;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Services;

public sealed class MessageService(ChatDbContext db) : IMessageService
{
    public async Task<MessageDto> SaveAsync(Guid senderId, Guid receiverId, string content, CancellationToken ct)
    {
        var sender = await db.Users.AsNoTracking().FirstAsync(u => u.Id == senderId, ct);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTimeOffset.UtcNow,
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);

        return new MessageDto
        {
            MessageId = message.Id,
            SenderId = message.SenderId,
            SenderEmail = sender.Email,
            Content = message.Content,
            SentAt = message.SentAt,
        };
    }

    public async Task<IReadOnlyList<MessageDto>> GetHistoryAsync(Guid userA, Guid userB, int page, int pageSize, CancellationToken ct)
    {
        var messages = await db.Messages
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
                (m, u) => new MessageDto
                {
                    MessageId = m.Id,
                    SenderId = m.SenderId,
                    SenderEmail = u.Email,
                    Content = m.Content,
                    SentAt = m.SentAt,
                })
            .ToListAsync(ct);

        return messages;
    }
}
