using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Domain.Enums;
using Maham.Application.DTOs.Comment;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IActivityService _activityService;
    private readonly ISignalRService _signalRService;

    public CommentService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IActivityService activityService,
        ISignalRService signalRService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _activityService = activityService;
        _signalRService = signalRService;
    }

    private async Task ValidateCardAccessAsync(Guid cardId, Guid userId)
    {
        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        var isOwner = await _unitOfWork.Projects.Query()
            .AnyAsync(p => p.Id == card.Column.Board.ProjectId && p.OwnerId == userId);
        
        var isMember = await _unitOfWork.ProjectMembers.Query()
            .AnyAsync(m => m.ProjectId == card.Column.Board.ProjectId && m.UserId == userId);

        if (!isOwner && !isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this card.");
        }
    }

    public async Task<CommentResponseDto> GetCommentByIdAsync(Guid commentId, Guid userId)
    {
        var comment = await _unitOfWork.Comments.Query()
            .Include(c => c.Author)
            .Include(c => c.Card)
            .ThenInclude(card => card.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comment not found.");
        }

        await ValidateCardAccessAsync(comment.CardId, userId);

        return _mapper.Map<CommentResponseDto>(comment);
    }

    public async Task<IEnumerable<CommentResponseDto>> GetCardCommentsAsync(Guid cardId, Guid userId)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var comments = await _unitOfWork.Comments.Query()
            .Include(c => c.Author)
            .Where(c => c.CardId == cardId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CommentResponseDto>>(comments);
    }

    public async Task<CommentResponseDto> CreateCommentAsync(Guid cardId, Guid userId, CreateCommentDto dto)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        var comment = new Comment
        {
            CardId = cardId,
            AuthorId = userId,
            Content = dto.Content
        };

        await _unitOfWork.Comments.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            card!.Column.Board.ProjectId,
            userId,
            "Comment",
            comment.Id,
            "CommentAdded",
            $"Added a comment on card '{card.Title}'",
            cardId.ToString()
        );

        // Fetch complete comment with author details
        var completeComment = await _unitOfWork.Comments.Query()
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        var response = _mapper.Map<CommentResponseDto>(completeComment);
        await _signalRService.SendCommentAddedAsync(card.Column.BoardId, response);
        return response;
    }

    public async Task<CommentResponseDto> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentDto dto)
    {
        var comment = await _unitOfWork.Comments.Query()
            .Include(c => c.Card)
            .ThenInclude(card => card.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comment not found.");
        }

        if (comment.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You can only edit your own comments.");
        }

        comment.Content = dto.Content;
        comment.IsEdited = true;
        comment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Comments.Update(comment);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            comment.Card.Column.Board.ProjectId,
            userId,
            "Comment",
            commentId,
            "CommentUpdated",
            $"Updated a comment on card '{comment.Card.Title}'",
            comment.CardId.ToString()
        );

        // Fetch complete comment
        var completeComment = await _unitOfWork.Comments.Query()
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        var response = _mapper.Map<CommentResponseDto>(completeComment);
        await _signalRService.SendCommentUpdatedAsync(comment.Card.Column.BoardId, response);
        return response;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _unitOfWork.Comments.Query()
            .Include(c => c.Card)
            .ThenInclude(card => card.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comment not found.");
        }

        var isAuthor = comment.AuthorId == userId;
        var project = await _unitOfWork.Projects.GetByIdAsync(comment.Card.Column.Board.ProjectId);
        var isProjectAdmin = false;

        if (project != null)
        {
            if (project.OwnerId == userId)
            {
                isProjectAdmin = true;
            }
            else
            {
                var membership = await _unitOfWork.ProjectMembers.Query()
                    .FirstOrDefaultAsync(m => m.ProjectId == project.Id && m.UserId == userId);
                
                if (membership != null && (membership.Role == ProjectMemberRole.Admin || membership.Role == ProjectMemberRole.Owner))
                {
                    isProjectAdmin = true;
                }
            }
        }

        if (!isAuthor && !isProjectAdmin)
        {
            throw new UnauthorizedAccessException("Only the comment author or a project administrator can delete comments.");
        }

        var boardId = comment.Card.Column.BoardId;
        var projectId = comment.Card.Column.Board.ProjectId;
        var cardTitle = comment.Card.Title;
        var cardId = comment.CardId;

        _unitOfWork.Comments.Delete(comment);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Comment",
            commentId,
            "CommentDeleted",
            $"Deleted a comment on card '{cardTitle}'",
            cardId.ToString()
        );

        await _signalRService.SendCommentDeletedAsync(boardId, commentId);
    }
}
