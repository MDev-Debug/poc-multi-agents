using System.Collections.Concurrent;
using Chat.Application.Interfaces;

namespace Chat.Infrastructure.Services;

public sealed class UserConnectionTracker : IUserConnectionTracker
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    public void AddConnection(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            var connections = _connections.GetOrAdd(userId, _ => new HashSet<string>());
            connections.Add(connectionId);
        }
    }

    public void RemoveConnection(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _connections.TryRemove(userId, out _);
                }
            }
        }
    }

    public IReadOnlyList<string> GetConnectionIds(Guid userId)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(userId, out var connections))
            {
                return connections.ToList();
            }
            return Array.Empty<string>();
        }
    }
}
