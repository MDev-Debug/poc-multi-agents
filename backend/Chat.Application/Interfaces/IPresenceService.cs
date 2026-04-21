using Chat.Application.Models;

namespace Chat.Application.Interfaces;

public interface IPresenceService
{
	void Connected(Guid userId, string email, DateTimeOffset now);
	void Disconnected(Guid userId);
	IReadOnlyList<PresenceEntry> GetOnline();
}
