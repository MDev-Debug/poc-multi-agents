namespace Chat.Api.Auth;

public sealed class JwtOptions
{
	public string Issuer { get; init; } = "Chat";
	public string Audience { get; init; } = "Chat";
	public string SigningKey { get; init; } = "dev-signing-key-change-me-32-bytes-minimum";
	public int ExpirationMinutes { get; init; } = 120;
}
