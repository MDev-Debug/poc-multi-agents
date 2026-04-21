using System.Collections.Concurrent;
using Chat.Application.Interfaces;
using Chat.Application.Models;

namespace Chat.Application.Services;

public sealed class PresenceService : IPresenceService
{
	private readonly ConcurrentDictionary<Guid, PresenceEntry> _online = new();
	private readonly ConcurrentDictionary<Guid, int> _connectionsPerUser = new();

	public void Connected(Guid userId, string email, DateTimeOffset now)
	{
		_online.AddOrUpdate(
			userId,
			_ => new PresenceEntry(userId, email, now),
			(_, existing) => existing with { LastSeenAt = now, Email = email });

		_connectionsPerUser.AddOrUpdate(userId, _ => 1, (_, current) => current + 1);
	}

	public void Disconnected(Guid userId)
	{
		if (!_connectionsPerUser.TryGetValue(userId, out var current))
		{
			return;
		}

		var next = Math.Max(0, current - 1);
		if (next == 0)
		{
			_connectionsPerUser.TryRemove(userId, out _);
			_online.TryRemove(userId, out _);
			return;
		}

		_connectionsPerUser[userId] = next;
	}

	public IReadOnlyList<PresenceEntry> GetOnline()
	{
		return _online.Values
			.OrderBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
			.ToList();
	}
}
