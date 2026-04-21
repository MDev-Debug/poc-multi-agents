namespace Chat.Application.Interfaces;

public interface IUserConnectionTracker
{
    void AddConnection(Guid userId, string connectionId);
    void RemoveConnection(Guid userId, string connectionId);
    IReadOnlyList<string> GetConnectionIds(Guid userId);
}
