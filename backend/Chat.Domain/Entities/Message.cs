namespace Chat.Domain.Entities;

public sealed class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }

    public AppUser Sender { get; set; } = null!;
    public AppUser Receiver { get; set; } = null!;
}
