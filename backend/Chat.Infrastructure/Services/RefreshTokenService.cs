using System.Security.Cryptography;
using System.Text;
using Chat.Application.Interfaces;
using Chat.Application.Options;
using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Infrastructure.Services;

public sealed class RefreshTokenService(ChatDbContext db, IOptions<JwtOptions> jwtOptions) : IRefreshTokenService
{
	private readonly JwtOptions _options = jwtOptions.Value;

	public async Task<(AppUser user, string newRefreshToken)?> RotateAsync(string refreshToken, CancellationToken cancellationToken)
	{
		var now = DateTimeOffset.UtcNow;
		var tokenHash = ComputeSha256Hex(refreshToken);

		var existing = await db.RefreshTokens
			.Include(x => x.User)
			.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

		if (existing is null)
		{
			return null;
		}

		if (existing.RevokedAt is not null || existing.ExpiresAt <= now)
		{
			return null;
		}

		await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

		existing.RevokedAt = now;

		var (newToken, newEntity) = CreateRefreshToken(existing.UserId, now);
		db.RefreshTokens.Add(newEntity);
		await db.SaveChangesAsync(cancellationToken);

		await tx.CommitAsync(cancellationToken);
		return (existing.User, newToken);
	}

	public async Task<string> CreateForUserAsync(AppUser user, CancellationToken cancellationToken)
	{
		var now = DateTimeOffset.UtcNow;
		var (token, entity) = CreateRefreshToken(user.Id, now);
		db.RefreshTokens.Add(entity);
		await db.SaveChangesAsync(cancellationToken);
		return token;
	}

	private (string token, RefreshToken entity) CreateRefreshToken(Guid userId, DateTimeOffset now)
	{
		var bytes = RandomNumberGenerator.GetBytes(64);
		var token = Base64UrlEncoder.Encode(bytes);

		var entity = new RefreshToken
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			TokenHash = ComputeSha256Hex(token),
			CreatedAt = now,
			ExpiresAt = now.AddDays(_options.RefreshTokenExpirationDays),
		};

		return (token, entity);
	}

	private static string ComputeSha256Hex(string value)
	{
		var bytes = Encoding.UTF8.GetBytes(value);
		var hash = SHA256.HashData(bytes);
		return Convert.ToHexString(hash).ToLowerInvariant();
	}
}
