using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Hubs;

[Authorize]
public sealed class ChatHub(IMessageService messageService, IUserConnectionTracker connectionTracker) : Hub
{
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

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendPrivateMessage(Guid receiverId, string content)
    {
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
