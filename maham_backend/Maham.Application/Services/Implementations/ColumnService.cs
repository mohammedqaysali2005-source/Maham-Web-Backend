using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Application.DTOs.Column;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class ColumnService : IColumnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IActivityService _activityService;
    private readonly ISignalRService _signalRService;

    public ColumnService(
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

    private async Task ValidateBoardAccessAsync(Guid boardId, Guid userId)
    {
        var board = await _unitOfWork.Boards.GetByIdAsync(boardId);
        if (board == null)
        {
            throw new KeyNotFoundException("Board not found.");
        }

        var isOwner = await _unitOfWork.Projects.Query()
            .AnyAsync(p => p.Id == board.ProjectId && p.OwnerId == userId);
        
        var isMember = await _unitOfWork.ProjectMembers.Query()
            .AnyAsync(m => m.ProjectId == board.ProjectId && m.UserId == userId);

        if (!isOwner && !isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this board.");
        }
    }

    private async Task ValidateColumnAccessAsync(Guid columnId, Guid userId)
    {
        var column = await _unitOfWork.Columns.Query()
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId);

        if (column == null)
        {
            throw new KeyNotFoundException("Column not found.");
        }

        await ValidateBoardAccessAsync(column.BoardId, userId);
    }

    public async Task<ColumnResponseDto> GetColumnByIdAsync(Guid columnId, Guid userId)
    {
        await ValidateColumnAccessAsync(columnId, userId);

        var column = await _unitOfWork.Columns.Query()
            .Include(c => c.Cards.OrderBy(card => card.Order))
            .ThenInclude(card => card.Assignee)
            .FirstOrDefaultAsync(c => c.Id == columnId);

        return _mapper.Map<ColumnResponseDto>(column);
    }

    public async Task<ColumnResponseDto> CreateColumnAsync(Guid boardId, Guid userId, CreateColumnDto dto)
    {
        await ValidateBoardAccessAsync(boardId, userId);

        var board = await _unitOfWork.Boards.GetByIdAsync(boardId);
        var existingColumnsCount = await _unitOfWork.Columns.Query()
            .CountAsync(c => c.BoardId == boardId);

        var column = new Column
        {
            BoardId = boardId,
            Name = dto.Name,
            WipLimit = dto.WipLimit,
            Order = existingColumnsCount
        };

        await _unitOfWork.Columns.AddAsync(column);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            board!.ProjectId,
            userId,
            "Column",
            column.Id,
            "ColumnCreated",
            $"Created the column '{column.Name}' in board '{board.Name}'"
        );

        var response = _mapper.Map<ColumnResponseDto>(column);
        await _signalRService.SendColumnCreatedAsync(boardId, response);
        return response;
    }

    public async Task<ColumnResponseDto> UpdateColumnAsync(Guid columnId, Guid userId, UpdateColumnDto dto)
    {
        await ValidateColumnAccessAsync(columnId, userId);

        var column = await _unitOfWork.Columns.Query()
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId);

        if (column == null)
        {
            throw new KeyNotFoundException("Column not found.");
        }

        column.Name = dto.Name;
        column.WipLimit = dto.WipLimit;

        _unitOfWork.Columns.Update(column);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            column.Board.ProjectId,
            userId,
            "Column",
            columnId,
            "ColumnUpdated",
            $"Updated the column details for '{column.Name}'"
        );

        return _mapper.Map<ColumnResponseDto>(column);
    }

    public async Task DeleteColumnAsync(Guid columnId, Guid userId)
    {
        await ValidateColumnAccessAsync(columnId, userId);

        var column = await _unitOfWork.Columns.Query()
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId);

        if (column == null)
        {
            throw new KeyNotFoundException("Column not found.");
        }

        var boardId = column.BoardId;
        var projectId = column.Board.ProjectId;

        _unitOfWork.Columns.Delete(column);
        await _unitOfWork.SaveChangesAsync();

        // Fix orders of remaining columns
        var columns = await _unitOfWork.Columns.Query()
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Order)
            .ToListAsync();

        for (int i = 0; i < columns.Count; i++)
        {
            columns[i].Order = i;
            _unitOfWork.Columns.Update(columns[i]);
        }
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Column",
            columnId,
            "ColumnDeleted",
            $"Deleted the column '{column.Name}'"
        );

        await _signalRService.SendColumnDeletedAsync(boardId, columnId);
    }

    public async Task ReorderColumnsAsync(Guid boardId, Guid userId, IEnumerable<ColumnReorderDto> dtos)
    {
        await ValidateBoardAccessAsync(boardId, userId);

        var board = await _unitOfWork.Boards.GetByIdAsync(boardId);
        if (board == null)
        {
            throw new KeyNotFoundException("Board not found.");
        }

        var columns = await _unitOfWork.Columns.Query()
            .Where(c => c.BoardId == boardId)
            .ToListAsync();

        foreach (var dto in dtos)
        {
            var col = columns.FirstOrDefault(c => c.Id == dto.Id);
            if (col != null)
            {
                col.Order = dto.Order;
                _unitOfWork.Columns.Update(col);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            board.ProjectId,
            userId,
            "Board",
            boardId,
            "ColumnReordered",
            $"Reordered the columns in board '{board.Name}'"
        );

        // Fetch reordered list to send
        var reordered = columns.Select(c => new { id = c.Id, order = c.Order }).ToList();
        await _signalRService.SendColumnReorderedAsync(boardId, reordered);
    }
}
