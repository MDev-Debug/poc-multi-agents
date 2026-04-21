using System.ComponentModel.DataAnnotations;

namespace Chat.Application.DTOs.Auth;

public sealed class LoginRequest
{
	[Required]
	[EmailAddress]
	public string Email { get; init; } = string.Empty;

	[Required]
	public string Password { get; init; } = string.Empty;
}
