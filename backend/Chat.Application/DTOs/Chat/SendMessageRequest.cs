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
