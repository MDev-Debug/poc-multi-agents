namespace Chat.Api.Contracts.Presence;

public sealed class OnlineUserResponse
{
	public Guid UserId { get; init; }
	public string Email { get; init; } = string.Empty;
	public DateTimeOffset LastSeenAt { get; init; }
}
