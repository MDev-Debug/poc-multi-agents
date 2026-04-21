using System.ComponentModel.DataAnnotations;

namespace Chat.Application.DTOs.Auth;

public sealed class RegisterRequest
{
	[Required]
	[EmailAddress]
	public string Email { get; init; } = string.Empty;

	[Required]
	[MinLength(6)]
	public string Password { get; init; } = string.Empty;
}
