using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Activity;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ActivityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private async Task ValidateProjectMemberAsync(Guid projectId, Guid userId)
    {
        var isMember = await _unitOfWork.ProjectMembers.Query()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
        
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        var isOwner = project?.OwnerId == userId;

        if (!isMember && !isOwner)
        {
            throw new UnauthorizedAccessException("User is not a member of this project.");
        }
    }

    public async Task<PagedResponseDto<ActivityLogResponseDto>> GetProjectActivityAsync(Guid projectId, Guid userId, int pageNumber, int pageSize)
    {
        await ValidateProjectMemberAsync(projectId, userId);

        var query = _unitOfWork.ActivityLogs.Query()
            .Include(a => a.User)
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResponseDto<ActivityLogResponseDto>
        {
            Items = _mapper.Map<IEnumerable<ActivityLogResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<IEnumerable<ActivityLogResponseDto>> GetBoardActivityAsync(Guid boardId, Guid userId)
    {
        var board = await _unitOfWork.Boards.GetByIdAsync(boardId);
        if (board == null)
        {
            throw new KeyNotFoundException("Board not found.");
        }

        await ValidateProjectMemberAsync(board.ProjectId, userId);

        // Fetch logs for this board's project, ordering by date
        var logs = await _unitOfWork.ActivityLogs.Query()
            .Include(a => a.User)
            .Where(a => a.ProjectId == board.ProjectId && (a.EntityId == boardId || a.EntityType == "Board" || a.EntityType == "Column" || a.EntityType == "Card"))
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ActivityLogResponseDto>>(logs);
    }

    public async Task<IEnumerable<ActivityLogResponseDto>> GetCardActivityAsync(Guid cardId, Guid userId)
    {
        var card = await _unitOfWork.Cards.Query()
            .Include(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(c => c.Id == cardId);

        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        await ValidateProjectMemberAsync(card.Column.Board.ProjectId, userId);

        var logs = await _unitOfWork.ActivityLogs.Query()
            .Include(a => a.User)
            .Where(a => a.EntityId == cardId || (a.EntityType == "Comment" && a.Metadata != null && a.Metadata.Contains(cardId.ToString())))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ActivityLogResponseDto>>(logs);
    }

    public async Task<IEnumerable<ActivityLogResponseDto>> GetUserActivityAsync(Guid userId)
    {
        var logs = await _unitOfWork.ActivityLogs.Query()
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ActivityLogResponseDto>>(logs);
    }

    public async Task LogActivityAsync(Guid projectId, Guid userId, string entityType, Guid entityId, string action, string description, string? metadata = null)
    {
        var log = new ActivityLog
        {
            EntityType = entityType,
            EntityId = entityId,
            ProjectId = projectId,
            UserId = userId,
            Action = action,
            Description = description,
            Metadata = metadata
        };

        await _unitOfWork.ActivityLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }
}
