namespace Chat.Api.Presence;

public sealed record PresenceEntry(Guid UserId, string Email, DateTimeOffset LastSeenAt);
