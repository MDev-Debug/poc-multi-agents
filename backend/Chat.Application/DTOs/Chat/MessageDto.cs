namespace Chat.Application.DTOs.Chat;

public sealed class MessageDto
{
    public Guid MessageId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderEmail { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset SentAt { get; init; }
}
