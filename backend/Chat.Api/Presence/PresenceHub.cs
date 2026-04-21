using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chat.Api.Contracts.Presence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Presence;

[Authorize]
public sealed class PresenceHub(PresenceService presence) : Hub
{
	public override async Task OnConnectedAsync()
	{
		var userIdStr =
			Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
			Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

		var email =
			Context.User?.FindFirstValue(JwtRegisteredClaimNames.Email) ??
			Context.User?.FindFirstValue(ClaimTypes.Email);

		if (Guid.TryParse(userIdStr, out var userId) && !string.IsNullOrWhiteSpace(email))
		{
			presence.Connected(userId, email, DateTimeOffset.UtcNow);
			await BroadcastOnlineUsersAsync();
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
			presence.Disconnected(userId);
			await BroadcastOnlineUsersAsync();
		}

		await base.OnDisconnectedAsync(exception);
	}

	public Task<IReadOnlyList<OnlineUserResponse>> GetOnlineUsers()
	{
		var users = presence.GetOnline()
			.Select(x => new OnlineUserResponse
			{
				UserId = x.UserId,
				Email = x.Email,
				LastSeenAt = x.LastSeenAt,
			})
			.ToList();

		return Task.FromResult<IReadOnlyList<OnlineUserResponse>>(users);
	}

	private Task BroadcastOnlineUsersAsync()
	{
		var users = presence.GetOnline()
			.Select(x => new OnlineUserResponse
			{
				UserId = x.UserId,
				Email = x.Email,
				LastSeenAt = x.LastSeenAt,
			})
			.ToList();

		return Clients.All.SendAsync("OnlineUsers", users);
	}
}
