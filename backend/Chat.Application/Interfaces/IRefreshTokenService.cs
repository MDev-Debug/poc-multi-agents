using Chat.Domain.Entities;

namespace Chat.Application.Interfaces;

public interface IRefreshTokenService
{
	Task<(AppUser user, string newRefreshToken)?> RotateAsync(string refreshToken, CancellationToken cancellationToken);
	Task<string> CreateForUserAsync(AppUser user, CancellationToken cancellationToken);
}
