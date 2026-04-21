using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chat.Application.DTOs.Presence;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Hubs;

[Authorize]
public sealed class PresenceHub(IPresenceService presence) : Hub
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

	public Task<IReadOnlyList<OnlineUserDto>> GetOnlineUsers()
	{
		var users = presence.GetOnline()
			.Select(x => new OnlineUserDto
			{
				UserId = x.UserId,
				Email = x.Email,
				LastSeenAt = x.LastSeenAt,
			})
			.ToList();

		return Task.FromResult<IReadOnlyList<OnlineUserDto>>(users);
	}

	private Task BroadcastOnlineUsersAsync()
	{
		var users = presence.GetOnline()
			.Select(x => new OnlineUserDto
			{
				UserId = x.UserId,
				Email = x.Email,
				LastSeenAt = x.LastSeenAt,
			})
			.ToList();

		return Clients.All.SendAsync("OnlineUsers", users);
	}
}
