using System;
using System.Threading.Tasks;

namespace Maham.Application.Services.Interfaces;

public interface ISignalRService
{
    Task SendCardMovedAsync(Guid boardId, Guid cardId, Guid fromColumnId, Guid toColumnId, int newOrder);
    Task SendCardCreatedAsync(Guid boardId, object card);
    Task SendCardUpdatedAsync(Guid boardId, object card);
    Task SendCardDeletedAsync(Guid boardId, Guid cardId);
    Task SendCommentAddedAsync(Guid boardId, object comment);
    Task SendCommentUpdatedAsync(Guid boardId, object comment);
    Task SendCommentDeletedAsync(Guid boardId, Guid commentId);
    Task SendAttachmentAddedAsync(Guid boardId, object attachment);
    Task SendAttachmentDeletedAsync(Guid boardId, Guid attachmentId);
    Task SendColumnReorderedAsync(Guid boardId, object columns);
    Task SendColumnCreatedAsync(Guid boardId, object column);
    Task SendColumnDeletedAsync(Guid boardId, Guid columnId);
    Task SendMemberJoinedAsync(Guid boardId, Guid userId, string fullName);
    Task SendCardAssignedAsync(Guid boardId, Guid cardId, object? assignee);
}
