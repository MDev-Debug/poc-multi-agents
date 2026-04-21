using Chat.Application.DTOs.Auth;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ChatDbContext db, IJwtTokenService tokenService, IRefreshTokenService refreshTokenService) : ControllerBase
{
	[HttpPost("register")]
	public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
	{
		var normalizedEmail = request.Email.Trim().ToLowerInvariant();
		var exists = await db.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
		if (exists)
		{
			return Conflict(new { message = "E-mail já cadastrado." });
		}

		var passwordHasher = new PasswordHasher<AppUser>();
		var user = new AppUser
		{
			Id = Guid.NewGuid(),
			Email = normalizedEmail,
			CreatedAt = DateTimeOffset.UtcNow,
		};
		user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

		db.Users.Add(user);
		await db.SaveChangesAsync(cancellationToken);

		var token = tokenService.CreateToken(user);
		var refreshToken = await refreshTokenService.CreateForUserAsync(user, cancellationToken);
		return Ok(new AuthResponse { UserId = user.Id, Email = user.Email, Token = token, RefreshToken = refreshToken });
	}

	[HttpPost("login")]
	public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
	{
		var normalizedEmail = request.Email.Trim().ToLowerInvariant();
		var user = await db.Users.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
		if (user is null)
		{
			return Unauthorized(new { message = "Credenciais inválidas." });
		}

		var passwordHasher = new PasswordHasher<AppUser>();
		var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
		if (result is PasswordVerificationResult.Failed)
		{
			return Unauthorized(new { message = "Credenciais inválidas." });
		}

		var token = tokenService.CreateToken(user);
		var refreshToken = await refreshTokenService.CreateForUserAsync(user, cancellationToken);
		return Ok(new AuthResponse { UserId = user.Id, Email = user.Email, Token = token, RefreshToken = refreshToken });
	}

	[HttpPost("refresh")]
	public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
	{
		var rotated = await refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken);
		if (rotated is null)
		{
			return Unauthorized(new { message = "Refresh token inválido." });
		}

		var (user, newRefreshToken) = rotated.Value;
		var token = tokenService.CreateToken(user);
		return Ok(new AuthResponse { UserId = user.Id, Email = user.Email, Token = token, RefreshToken = newRefreshToken });
	}
}
