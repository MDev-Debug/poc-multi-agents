using Chat.Domain.Entities;

namespace Chat.Application.Interfaces;

public interface IJwtTokenService
{
	string CreateToken(AppUser user);
}
