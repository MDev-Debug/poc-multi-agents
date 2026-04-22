namespace Chat.Application.Options;

public sealed class JwtOptions
{
	public string Issuer { get; init; } = "Chat";
	public string Audience { get; init; } = "Chat";
	public string SigningKey { get; init; } = string.Empty;
	public int ExpirationMinutes { get; init; } = 15;
	public int RefreshTokenExpirationDays { get; init; } = 14;
}
