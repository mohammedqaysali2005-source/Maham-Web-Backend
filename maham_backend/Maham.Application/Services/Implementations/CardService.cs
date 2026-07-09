using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Domain.Enums;
using Maham.Application.DTOs.Card;
using Maham.Application.DTOs.Auth;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class CardService : ICardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IActivityService _activityService;
    private readonly ISignalRService _signalRService;
    private readonly IFileService _fileService;

    public CardService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IActivityService activityService,
        ISignalRService signalRService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _activityService = activityService;
        _signalRService = signalRService;
        _fileService = fileService;
    }

    private async Task ValidateCardAccessAsync(Guid cardId, Guid userId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (currentUser?.Role == UserRole.Admin) return;

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

    private async Task ValidateColumnAccessAsync(Guid columnId, Guid userId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (currentUser?.Role == UserRole.Admin) return;

        var column = await _unitOfWork.Columns.Query()
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId);

        if (column == null)
        {
            throw new KeyNotFoundException("Column not found.");
        }

        var isOwner = await _unitOfWork.Projects.Query()
            .AnyAsync(p => p.Id == column.Board.ProjectId && p.OwnerId == userId);
        
        var isMember = await _unitOfWork.ProjectMembers.Query()
            .AnyAsync(m => m.ProjectId == column.Board.ProjectId && m.UserId == userId);

        if (!isOwner && !isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this column.");
        }
    }

    public async Task<CardResponseDto> GetCardByIdAsync(Guid cardId, Guid userId)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Assignee)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        return _mapper.Map<CardResponseDto>(card);
    }

    public async Task<CardResponseDto> CreateCardAsync(Guid columnId, Guid userId, CreateCardDto dto)
    {
        await ValidateColumnAccessAsync(columnId, userId);

        var column = await _unitOfWork.Columns.Query()
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId);

        if (column == null)
        {
            throw new KeyNotFoundException("Column not found.");
        }

        if (column.WipLimit.HasValue)
        {
            var currentCardsCount = await _unitOfWork.Cards.Query()
                .CountAsync(c => c.ColumnId == columnId);

            if (currentCardsCount >= column.WipLimit.Value)
            {
                throw new InvalidOperationException($"WIP Limit reached. You cannot add more than {column.WipLimit.Value} cards to this column.");
            }
        }

        var existingCardsCount = await _unitOfWork.Cards.Query()
            .CountAsync(c => c.ColumnId == columnId);

        // Map status based on column name if matching Todo/InProgress/Done
        var status = CardStatus.Todo;
        var columnNameLower = column.Name.ToLower();
        if (columnNameLower.Contains("progress") || columnNameLower.Contains("doing"))
        {
            status = CardStatus.InProgress;
        }
        else if (columnNameLower.Contains("done") || columnNameLower.Contains("complete"))
        {
            status = CardStatus.Done;
        }

        var card = new Card
        {
            ColumnId = columnId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = status,
            DueDate = dto.DueDate,
            StoryPoints = dto.StoryPoints,
            Labels = dto.Labels,
            Order = existingCardsCount
        };

        await _unitOfWork.Cards.AddAsync(card);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            column.Board.ProjectId,
            userId,
            "Card",
            card.Id,
            "CardCreated",
            $"Created the card '{card.Title}' in column '{column.Name}'"
        );

        var response = _mapper.Map<CardResponseDto>(card);
        await _signalRService.SendCardCreatedAsync(column.BoardId, response);
        return response;
    }

    public async Task<CardResponseDto> UpdateCardAsync(Guid cardId, Guid userId, UpdateCardDto dto)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        card.Title = dto.Title;
        card.Description = dto.Description;
        card.Priority = dto.Priority;
        card.DueDate = dto.DueDate;
        card.StoryPoints = dto.StoryPoints;
        card.Labels = dto.Labels;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            card.Column.Board.ProjectId,
            userId,
            "Card",
            cardId,
            "CardUpdated",
            $"Updated card details for '{card.Title}'"
        );

        var response = _mapper.Map<CardResponseDto>(card);
        await _signalRService.SendCardUpdatedAsync(card.Column.BoardId, response);
        return response;
    }

    public async Task DeleteCardAsync(Guid cardId, Guid userId)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        var boardId = card.Column.BoardId;
        var columnId = card.ColumnId;
        var projectId = card.Column.Board.ProjectId;

        if (!string.IsNullOrEmpty(card.CoverImageUrl))
        {
            _fileService.DeleteFile(card.CoverImageUrl);
        }

        _unitOfWork.Cards.Delete(card);
        await _unitOfWork.SaveChangesAsync();

        // Adjust order of remaining cards
        var cards = await _unitOfWork.Cards.Query()
            .Where(c => c.ColumnId == columnId)
            .OrderBy(c => c.Order)
            .ToListAsync();

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].Order = i;
            _unitOfWork.Cards.Update(cards[i]);
        }
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Card",
            cardId,
            "CardDeleted",
            $"Deleted the card '{card.Title}'"
        );

        await _signalRService.SendCardDeletedAsync(boardId, cardId);
    }

    public async Task<CardResponseDto> MoveCardAsync(Guid cardId, Guid userId, MoveCardDto dto)
    {
        await ValidateCardAccessAsync(cardId, userId);
        await ValidateColumnAccessAsync(dto.ToColumnId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        var destColumn = await _unitOfWork.Columns.Query()
            .Include(col => col.Board)
            .FirstOrDefaultAsync(col => col.Id == dto.ToColumnId);

        if (card == null || destColumn == null)
        {
            throw new KeyNotFoundException("Card or column not found.");
        }

        var sourceColumnId = card.ColumnId;
        var boardId = card.Column.BoardId;
        var projectId = card.Column.Board.ProjectId;

        // Check WIP limit if moving columns
        if (sourceColumnId != dto.ToColumnId && destColumn.WipLimit.HasValue)
        {
            var destCardsCount = await _unitOfWork.Cards.Query()
                .CountAsync(c => c.ColumnId == dto.ToColumnId);

            if (destCardsCount >= destColumn.WipLimit.Value)
            {
                throw new InvalidOperationException($"Destination Column WIP Limit reached. You cannot move cards here.");
            }
        }

        // Adjust cards in source column
        var sourceCards = await _unitOfWork.Cards.Query()
            .Where(c => c.ColumnId == sourceColumnId && c.Id != cardId && c.Order > card.Order)
            .ToListAsync();

        foreach (var sc in sourceCards)
        {
            sc.Order--;
            _unitOfWork.Cards.Update(sc);
        }

        // Adjust cards in destination column
        var destCards = await _unitOfWork.Cards.Query()
            .Where(c => c.ColumnId == dto.ToColumnId && c.Order >= dto.NewOrder)
            .ToListAsync();

        foreach (var dc in destCards)
        {
            dc.Order++;
            _unitOfWork.Cards.Update(dc);
        }

        // Update card's Column and Order
        card.ColumnId = dto.ToColumnId;
        card.Order = dto.NewOrder;
        card.UpdatedAt = DateTime.UtcNow;

        // Map status based on destination column name
        var columnNameLower = destColumn.Name.ToLower();
        if (columnNameLower.Contains("progress") || columnNameLower.Contains("doing"))
        {
            card.Status = CardStatus.InProgress;
        }
        else if (columnNameLower.Contains("done") || columnNameLower.Contains("complete"))
        {
            card.Status = CardStatus.Done;
        }
        else
        {
            card.Status = CardStatus.Todo;
        }

        _unitOfWork.Cards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Card",
            cardId,
            "CardMoved",
            $"Moved card '{card.Title}' to column '{destColumn.Name}' at position {dto.NewOrder}"
        );

        await _signalRService.SendCardMovedAsync(boardId, cardId, sourceColumnId, dto.ToColumnId, dto.NewOrder);

        return _mapper.Map<CardResponseDto>(card);
    }

    public async Task<CardResponseDto> AssignCardAsync(Guid cardId, Guid userId, AssignCardDto dto)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        User? assignee = null;
        if (dto.AssigneeId.HasValue)
        {
            assignee = await _unitOfWork.Users.GetByIdAsync(dto.AssigneeId.Value);
            if (assignee == null)
            {
                throw new KeyNotFoundException("Assignee user not found.");
            }

            var isMember = await _unitOfWork.ProjectMembers.Query()
                .AnyAsync(m => m.ProjectId == card.Column.Board.ProjectId && m.UserId == dto.AssigneeId.Value);
            
            var project = await _unitOfWork.Projects.GetByIdAsync(card.Column.Board.ProjectId);
            var isOwner = project?.OwnerId == dto.AssigneeId.Value;

            if (!isMember && !isOwner)
            {
                throw new ArgumentException("The assigned user must be a member of the project.");
            }
        }

        card.AssigneeId = dto.AssigneeId;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        var desc = assignee != null 
            ? $"Assigned card '{card.Title}' to '{assignee.FullName}'" 
            : $"Unassigned card '{card.Title}'";

        await _activityService.LogActivityAsync(
            card.Column.Board.ProjectId,
            userId,
            "Card",
            cardId,
            "CardAssigned",
            desc
        );

        var mappedAssignee = assignee != null ? _mapper.Map<UserResponseDto>(assignee) : null;
        await _signalRService.SendCardAssignedAsync(card.Column.BoardId, cardId, mappedAssignee);

        var response = _mapper.Map<CardResponseDto>(card);
        response.Assignee = mappedAssignee;
        return response;
    }

    public async Task<CardResponseDto> UpdatePriorityAsync(Guid cardId, Guid userId, UpdatePriorityDto dto)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        var oldPriority = card.Priority;
        card.Priority = dto.Priority;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            card.Column.Board.ProjectId,
            userId,
            "Card",
            cardId,
            "CardPriorityUpdated",
            $"Updated card '{card.Title}' priority from '{oldPriority}' to '{dto.Priority}'"
        );

        var response = _mapper.Map<CardResponseDto>(card);
        await _signalRService.SendCardUpdatedAsync(card.Column.BoardId, response);
        return response;
    }

    public async Task<CardResponseDto> UpdateDueDateAsync(Guid cardId, Guid userId, UpdateDueDateDto dto)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        card.DueDate = dto.DueDate;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        var desc = dto.DueDate.HasValue 
            ? $"Set card '{card.Title}' due date to {dto.DueDate.Value:yyyy-MM-dd}" 
            : $"Removed card '{card.Title}' due date";

        await _activityService.LogActivityAsync(
            card.Column.Board.ProjectId,
            userId,
            "Card",
            cardId,
            "CardDueDateUpdated",
            desc
        );

        var response = _mapper.Map<CardResponseDto>(card);
        await _signalRService.SendCardUpdatedAsync(card.Column.BoardId, response);
        return response;
    }

    public async Task<CardResponseDto> UpdateStatusAsync(Guid cardId, Guid userId, UpdateStatusDto dto)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        var oldStatus = card.Status;
        card.Status = dto.Status;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            card.Column.Board.ProjectId,
            userId,
            "Card",
            cardId,
            "CardStatusUpdated",
            $"Updated card '{card.Title}' status from '{oldStatus}' to '{dto.Status}'"
        );

        var response = _mapper.Map<CardResponseDto>(card);
        await _signalRService.SendCardUpdatedAsync(card.Column.BoardId, response);
        return response;
    }

    public async Task<CardResponseDto> UpdateLabelsAsync(Guid cardId, Guid userId, UpdateLabelsDto dto)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        card.Labels = dto.Labels;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            card.Column.Board.ProjectId,
            userId,
            "Card",
            cardId,
            "CardLabelsUpdated",
            $"Updated labels for card '{card.Title}'"
        );

        var response = _mapper.Map<CardResponseDto>(card);
        await _signalRService.SendCardUpdatedAsync(card.Column.BoardId, response);
        return response;
    }

    public async Task<CardResponseDto> UploadCoverAsync(Guid cardId, Guid userId, IFormFile file)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        if (!string.IsNullOrEmpty(card.CoverImageUrl))
        {
            _fileService.DeleteFile(card.CoverImageUrl);
        }

        var coverUrl = await _fileService.SaveFileAsync(file, "uploads/covers");
        card.CoverImageUrl = coverUrl;
        card.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Cards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            card.Column.Board.ProjectId,
            userId,
            "Card",
            cardId,
            "CardCoverUploaded",
            $"Uploaded cover image for card '{card.Title}'"
        );

        var response = _mapper.Map<CardResponseDto>(card);
        await _signalRService.SendCardUpdatedAsync(card.Column.BoardId, response);
        return response;
    }

    public async Task DeleteCoverAsync(Guid cardId, Guid userId)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        if (!string.IsNullOrEmpty(card.CoverImageUrl))
        {
            _fileService.DeleteFile(card.CoverImageUrl);
            card.CoverImageUrl = null;
            card.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Cards.Update(card);
            await _unitOfWork.SaveChangesAsync();

            await _activityService.LogActivityAsync(
                card.Column.Board.ProjectId,
                userId,
                "Card",
                cardId,
                "CardCoverDeleted",
                $"Removed cover image for card '{card.Title}'"
            );

            var response = _mapper.Map<CardResponseDto>(card);
            await _signalRService.SendCardUpdatedAsync(card.Column.BoardId, response);
        }
    }

    public async Task<IEnumerable<CardResponseDto>> GetUserAssignedCardsAsync(Guid userId)
    {
        var cards = await _unitOfWork.Cards.Query()
            .Include(c => c.Assignee)
            .Where(c => c.AssigneeId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CardResponseDto>>(cards);
    }

    public async Task<IEnumerable<CardResponseDto>> GetUserCardsDueTodayAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var cards = await _unitOfWork.Cards.Query()
            .Include(c => c.Assignee)
            .Where(c => c.AssigneeId == userId && c.DueDate.HasValue && c.DueDate.Value.Date == today)
            .OrderBy(c => c.DueDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CardResponseDto>>(cards);
    }

    public async Task<IEnumerable<CardResponseDto>> GetProjectCardsAsync(Guid projectId, Guid userId)
    {
        await ValidateColumnAccessAsync(projectId, userId); // Validates project membership

        var cards = await _unitOfWork.Cards.Query()
            .Include(c => c.Assignee)
            .Where(c => c.Column.Board.ProjectId == projectId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CardResponseDto>>(cards);
    }

    public async Task<IEnumerable<CardResponseDto>> GetProjectOverdueCardsAsync(Guid projectId, Guid userId)
    {
        await ValidateColumnAccessAsync(projectId, userId); // Validates project membership

        var now = DateTime.UtcNow;
        var cards = await _unitOfWork.Cards.Query()
            .Include(c => c.Assignee)
            .Where(c => c.Column.Board.ProjectId == projectId && c.DueDate.HasValue && c.DueDate.Value < now && c.Status != CardStatus.Done)
            .OrderBy(c => c.DueDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CardResponseDto>>(cards);
    }

    public async Task ReorderCardsAsync(Guid columnId, Guid userId, IEnumerable<CardReorderDto> dtos)
    {
        await ValidateColumnAccessAsync(columnId, userId);

        var column = await _unitOfWork.Columns.Query()
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId);

        if (column == null)
        {
            throw new KeyNotFoundException("Column not found.");
        }

        var cards = await _unitOfWork.Cards.Query()
            .Where(c => c.ColumnId == columnId)
            .ToListAsync();

        foreach (var dto in dtos)
        {
            var card = cards.FirstOrDefault(c => c.Id == dto.Id);
            if (card != null)
            {
                card.Order = dto.Order;
                _unitOfWork.Cards.Update(card);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            column.Board.ProjectId,
            userId,
            "Column",
            columnId,
            "CardReordered",
            $"Reordered cards inside column '{column.Name}'"
        );
    }
}
