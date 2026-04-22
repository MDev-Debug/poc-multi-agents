using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Hubs;

[Authorize]
public sealed class ChatHub(IMessageService messageService, IUserConnectionTracker connectionTracker) : Hub
{
    private static readonly ConcurrentDictionary<string, (int Count, DateTime Window)>
        _rateLimitMap = new();

    private bool IsRateLimited(string connectionId)
    {
        var now   = DateTime.UtcNow;
        var entry = _rateLimitMap.GetOrAdd(connectionId, _ => (0, now));

        // Reset janela de 1 minuto
        if ((now - entry.Window).TotalSeconds >= 60)
        {
            _rateLimitMap[connectionId] = (1, now);
            return false;
        }

        if (entry.Count >= 60) // máximo 60 mensagens/minuto por conexão
            return true;

        _rateLimitMap[connectionId] = (entry.Count + 1, entry.Window);
        return false;
    }

    public override async Task OnConnectedAsync()
    {
        var userIdStr =
            Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdStr, out var userId))
        {
            connectionTracker.AddConnection(userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdStr =
            Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdStr, out var userId))
        {
            connectionTracker.RemoveConnection(userId, Context.ConnectionId);
        }

        _rateLimitMap.TryRemove(Context.ConnectionId, out _);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendPrivateMessage(Guid receiverId, string content)
    {
        if (IsRateLimited(Context.ConnectionId))
        {
            await Clients.Caller.SendAsync("Error", "Rate limit excedido. Aguarde antes de enviar novas mensagens.");
            return;
        }

        if (string.IsNullOrWhiteSpace(content) || content.Length > 4000)
            return;

        var userIdStr =
            Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdStr, out var senderId))
            return;

        var messageDto = await messageService.SaveAsync(senderId, receiverId, content, Context.ConnectionAborted);

        var receiverConnections = connectionTracker.GetConnectionIds(receiverId);
        if (receiverConnections.Count > 0)
        {
            await Clients.Clients(receiverConnections).SendAsync("ReceiveMessage", messageDto);
        }

        var senderOtherConnections = connectionTracker.GetConnectionIds(senderId)
            .Where(c => c != Context.ConnectionId)
            .ToList();

        if (senderOtherConnections.Count > 0)
        {
            await Clients.Clients(senderOtherConnections).SendAsync("ReceiveMessage", messageDto);
        }
    }
}
