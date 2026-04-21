namespace Chat.Application.DTOs.Chat;

public sealed class SendMessageRequest
{
    public Guid ReceiverId { get; init; }
    public string Content { get; init; } = string.Empty;
}
