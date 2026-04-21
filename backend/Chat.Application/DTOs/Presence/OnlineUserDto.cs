namespace Chat.Application.DTOs.Presence;

public sealed class OnlineUserDto
{
	public Guid UserId { get; init; }
	public string Email { get; init; } = string.Empty;
	public DateTimeOffset LastSeenAt { get; init; }
}
