namespace Chat.Domain.Entities;

public sealed class AppUser
{
	public Guid Id { get; set; }
	public string Email { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public DateTimeOffset CreatedAt { get; set; }
}
