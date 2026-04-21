namespace Chat.Application.Models;

public sealed record PresenceEntry(Guid UserId, string Email, DateTimeOffset LastSeenAt);
