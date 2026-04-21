using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chat.Application.DTOs.Chat;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/messages")]
public sealed class MessagesController(IMessageService messageService) : ControllerBase
{
    [HttpGet("{otherUserId:guid}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetHistory(
        Guid otherUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100)
            pageSize = 100;

        var userIdStr =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var messages = await messageService.GetHistoryAsync(userId, otherUserId, page, pageSize, cancellationToken);
        return Ok(messages);
    }
}
