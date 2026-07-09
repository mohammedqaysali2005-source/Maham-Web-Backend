using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Maham.Application.Services.Interfaces;
using Maham.Infrastructure.Hubs;

namespace Maham.Infrastructure.Services;

public class SignalRService : ISignalRService
{
    private readonly IHubContext<BoardHub> _hubContext;

    public SignalRService(IHubContext<BoardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendCardMovedAsync(Guid boardId, Guid cardId, Guid fromColumnId, Guid toColumnId, int newOrder)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("CardMoved", new { cardId, fromColumnId, toColumnId, newOrder });
    }

    public async Task SendCardCreatedAsync(Guid boardId, object card)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("CardCreated", card);
    }

    public async Task SendCardUpdatedAsync(Guid boardId, object card)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("CardUpdated", card);
    }

    public async Task SendCardDeletedAsync(Guid boardId, Guid cardId)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("CardDeleted", cardId);
    }

    public async Task SendCommentAddedAsync(Guid boardId, object comment)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("CommentAdded", comment);
    }

    public async Task SendCommentUpdatedAsync(Guid boardId, object comment)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("CommentUpdated", comment);
    }

    public async Task SendCommentDeletedAsync(Guid boardId, Guid commentId)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("CommentDeleted", commentId);
    }

    public async Task SendAttachmentAddedAsync(Guid boardId, object attachment)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("AttachmentAdded", attachment);
    }

    public async Task SendAttachmentDeletedAsync(Guid boardId, Guid attachmentId)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("AttachmentDeleted", attachmentId);
    }

    public async Task SendColumnReorderedAsync(Guid boardId, object columns)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("ColumnReordered", columns);
    }

    public async Task SendColumnCreatedAsync(Guid boardId, object column)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("ColumnCreated", column);
    }

    public async Task SendColumnDeletedAsync(Guid boardId, Guid columnId)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("ColumnDeleted", columnId);
    }

    public async Task SendMemberJoinedAsync(Guid boardId, Guid userId, string fullName)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("MemberJoined", new { userId, fullName });
    }

    public async Task SendCardAssignedAsync(Guid boardId, Guid cardId, object? assignee)
    {
        await _hubContext.Clients.Group($"board-{boardId}").SendAsync("CardAssigned", new { cardId, assignee });
    }
}
