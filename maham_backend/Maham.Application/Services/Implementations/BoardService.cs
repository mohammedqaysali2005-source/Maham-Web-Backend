using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Domain.Enums;
using Maham.Application.DTOs.Board;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class BoardService : IBoardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IActivityService _activityService;

    public BoardService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IActivityService activityService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _activityService = activityService;
    }

    private async Task ValidateProjectAccessAsync(Guid projectId, Guid userId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (currentUser?.Role == UserRole.Admin) return;

        var isOwner = await _unitOfWork.Projects.Query()
            .AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
        
        var isMember = await _unitOfWork.ProjectMembers.Query()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);

        if (!isOwner && !isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this project.");
        }
    }

    private async Task ValidateBoardAdminAsync(Guid boardId, Guid userId)
    {
        var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (currentUser?.Role == UserRole.Admin) return;

        var board = await _unitOfWork.Boards.GetByIdAsync(boardId);
        if (board == null)
        {
            throw new KeyNotFoundException("Board not found.");
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(board.ProjectId);
        if (project == null)
        {
            throw new KeyNotFoundException("Associated project not found.");
        }

        if (project.OwnerId == userId) return;

        var membership = await _unitOfWork.ProjectMembers.Query()
            .FirstOrDefaultAsync(m => m.ProjectId == board.ProjectId && m.UserId == userId);

        if (membership == null || (membership.Role != ProjectMemberRole.Admin && membership.Role != ProjectMemberRole.Owner))
        {
            throw new UnauthorizedAccessException("Only project owners or administrators can perform this action.");
        }
    }

    public async Task<IEnumerable<BoardResponseDto>> GetProjectBoardsAsync(Guid projectId, Guid userId)
    {
        await ValidateProjectAccessAsync(projectId, userId);

        var boards = await _unitOfWork.Boards.Query()
            .Where(b => b.ProjectId == projectId)
            .OrderBy(b => b.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BoardResponseDto>>(boards);
    }

    public async Task<BoardResponseDto> CreateBoardAsync(Guid projectId, Guid userId, CreateBoardDto dto)
    {
        await ValidateProjectAccessAsync(projectId, userId);

        var board = new Board
        {
            ProjectId = projectId,
            Name = dto.Name,
            Description = dto.Description,
            BackgroundColor = dto.BackgroundColor ?? "#f4f5f7"
        };

        await _unitOfWork.Boards.AddAsync(board);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Board",
            board.Id,
            "BoardCreated",
            $"Created the board '{board.Name}'"
        );

        return _mapper.Map<BoardResponseDto>(board);
    }

    public async Task<BoardDetailsDto> GetBoardDetailsAsync(Guid boardId, Guid userId)
    {
        var board = await _unitOfWork.Boards.Query()
            .Include(b => b.Columns.OrderBy(c => c.Order))
            .ThenInclude(c => c.Cards.OrderBy(card => card.Order))
            .ThenInclude(card => card.Assignee)
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board == null)
        {
            throw new KeyNotFoundException("Board not found.");
        }

        await ValidateProjectAccessAsync(board.ProjectId, userId);

        return _mapper.Map<BoardDetailsDto>(board);
    }

    public async Task<BoardResponseDto> UpdateBoardAsync(Guid boardId, Guid userId, UpdateBoardDto dto)
    {
        var board = await _unitOfWork.Boards.GetByIdAsync(boardId);
        if (board == null)
        {
            throw new KeyNotFoundException("Board not found.");
        }

        await ValidateProjectAccessAsync(board.ProjectId, userId);

        board.Name = dto.Name;
        board.Description = dto.Description;
        if (dto.BackgroundColor != null)
        {
            board.BackgroundColor = dto.BackgroundColor;
        }
        board.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Boards.Update(board);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            board.ProjectId,
            userId,
            "Board",
            boardId,
            "BoardUpdated",
            $"Updated board details for '{board.Name}'"
        );

        return _mapper.Map<BoardResponseDto>(board);
    }

    public async Task DeleteBoardAsync(Guid boardId, Guid userId)
    {
        await ValidateBoardAdminAsync(boardId, userId);

        var board = await _unitOfWork.Boards.GetByIdAsync(boardId);
        if (board == null)
        {
            throw new KeyNotFoundException("Board not found.");
        }

        _unitOfWork.Boards.Delete(board);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            board.ProjectId,
            userId,
            "Board",
            boardId,
            "BoardDeleted",
            $"Deleted the board '{board.Name}'"
        );
    }

    public async Task<string> ExportBoardToJsonAsync(Guid boardId, Guid userId)
    {
        var boardDetails = await GetBoardDetailsAsync(boardId, userId);
        
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(boardDetails, options);
    }
}
