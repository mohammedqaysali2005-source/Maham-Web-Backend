using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Maham.Infrastructure.Hubs;

public class UserPresenceInfo
{
    public string ConnectionId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string BoardId { get; set; } = null!;
}

[Authorize]
public class BoardHub : Hub
{
    // Thread-safe dictionary tracking presence per connection
    private static readonly ConcurrentDictionary<string, UserPresenceInfo> PresenceMap = new();

    public async Task JoinBoard(string boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"board-{boardId}");

        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                     ?? Context.User?.FindFirst("sub")?.Value 
                     ?? Context.ConnectionId;

        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value 
                       ?? Context.User?.FindFirst("name")?.Value 
                       ?? "عضو متصل";

        var info = new UserPresenceInfo
        {
            ConnectionId = Context.ConnectionId,
            UserId = userId,
            UserName = userName,
            BoardId = boardId
        };

        PresenceMap[Context.ConnectionId] = info;

        await BroadcastBoardPresence(boardId);
    }

    public async Task LeaveBoard(string boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board-{boardId}");
        PresenceMap.TryRemove(Context.ConnectionId, out _);

        await BroadcastBoardPresence(boardId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (PresenceMap.TryRemove(Context.ConnectionId, out var info))
        {
            await BroadcastBoardPresence(info.BoardId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastBoardPresence(string boardId)
    {
        var onlineUsers = PresenceMap.Values
            .Where(x => x.BoardId == boardId)
            .Select(x => new { x.UserId, x.UserName })
            .GroupBy(x => x.UserId)
            .Select(g => g.First())
            .ToList();

        await Clients.Group($"board-{boardId}").SendAsync("PresenceUpdate", onlineUsers);
    }
}
