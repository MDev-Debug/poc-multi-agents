namespace Chat.Api.Models;

public sealed class RefreshToken
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public AppUser User { get; set; } = null!;
	public string TokenHash { get; set; } = string.Empty;
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset ExpiresAt { get; set; }
	public DateTimeOffset? RevokedAt { get; set; }
}
