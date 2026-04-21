namespace Chat.Application.DTOs.Auth;

public sealed class AuthResponse
{
	public Guid UserId { get; init; }
	public string Email { get; init; } = string.Empty;
	public string Token { get; init; } = string.Empty;
	public string RefreshToken { get; init; } = string.Empty;
}
