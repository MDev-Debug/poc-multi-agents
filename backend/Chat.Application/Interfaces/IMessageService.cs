using Chat.Application.DTOs.Chat;

namespace Chat.Application.Interfaces;

public interface IMessageService
{
    Task<MessageDto> SaveAsync(Guid senderId, Guid receiverId, string content, CancellationToken ct);
    Task<IReadOnlyList<MessageDto>> GetHistoryAsync(Guid userA, Guid userB, int page, int pageSize, CancellationToken ct);
}
