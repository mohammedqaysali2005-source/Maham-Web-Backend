using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Domain.Enums;
using Maham.Application.DTOs.Attachment;
using Maham.Application.Services.Interfaces;

namespace Maham.Application.Services.Implementations;

public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IActivityService _activityService;
    private readonly ISignalRService _signalRService;
    private readonly IFileService _fileService;
    private readonly IWebHostEnvironment _env;

    public AttachmentService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IActivityService activityService,
        ISignalRService signalRService,
        IFileService fileService,
        IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _activityService = activityService;
        _signalRService = signalRService;
        _fileService = fileService;
        _env = env;
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

    public async Task<AttachmentResponseDto> GetAttachmentByIdAsync(Guid attachmentId, Guid userId)
    {
        var attachment = await _unitOfWork.Attachments.Query()
            .Include(a => a.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
        {
            throw new KeyNotFoundException("Attachment not found.");
        }

        await ValidateCardAccessAsync(attachment.CardId, userId);

        return _mapper.Map<AttachmentResponseDto>(attachment);
    }

    public async Task<IEnumerable<AttachmentResponseDto>> GetCardAttachmentsAsync(Guid cardId, Guid userId)
    {
        await ValidateCardAccessAsync(cardId, userId);

        var attachments = await _unitOfWork.Attachments.Query()
            .Include(a => a.UploadedBy)
            .Where(a => a.CardId == cardId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AttachmentResponseDto>>(attachments);
    }

    public async Task<AttachmentResponseDto> UploadAttachmentAsync(Guid cardId, Guid userId, IFormFile file)
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

        var folder = "uploads/attachments";
        var relativePath = await _fileService.SaveFileAsync(file, folder);

        var attachment = new Attachment
        {
            CardId = cardId,
            UploadedById = userId,
            FileName = file.FileName,
            FilePath = relativePath,
            FileSize = file.Length,
            MimeType = file.ContentType
        };

        await _unitOfWork.Attachments.AddAsync(attachment);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            card.Column.Board.ProjectId,
            userId,
            "Card",
            cardId,
            "AttachmentAdded",
            $"Uploaded attachment '{file.FileName}' to card '{card.Title}'"
        );

        // Fetch complete attachment details
        var completeAttachment = await _unitOfWork.Attachments.Query()
            .Include(a => a.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == attachment.Id);

        var response = _mapper.Map<AttachmentResponseDto>(completeAttachment);
        await _signalRService.SendAttachmentAddedAsync(card.Column.BoardId, response);
        return response;
    }

    public async Task<(byte[] content, string fileName, string contentType)> DownloadAttachmentAsync(Guid attachmentId, Guid userId)
    {
        var attachment = await _unitOfWork.Attachments.GetByIdAsync(attachmentId);
        if (attachment == null)
        {
            throw new KeyNotFoundException("Attachment not found.");
        }

        await ValidateCardAccessAsync(attachment.CardId, userId);

        var baseDir = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativePath = attachment.FilePath.TrimStart('/');
        var absolutePath = Path.Combine(baseDir, relativePath);

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Physical file not found on server.");
        }

        var content = await File.ReadAllBytesAsync(absolutePath);
        return (content, attachment.FileName, attachment.MimeType);
    }

    public async Task DeleteAttachmentAsync(Guid attachmentId, Guid userId)
    {
        var attachment = await _unitOfWork.Attachments.Query()
            .Include(a => a.Card)
            .ThenInclude(c => c.Column)
            .ThenInclude(col => col.Board)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
        {
            throw new KeyNotFoundException("Attachment not found.");
        }

        await ValidateCardAccessAsync(attachment.CardId, userId);

        var boardId = attachment.Card.Column.BoardId;
        var projectId = attachment.Card.Column.Board.ProjectId;
        var cardTitle = attachment.Card.Title;
        var fileName = attachment.FileName;

        // Delete physical file
        _fileService.DeleteFile(attachment.FilePath);

        _unitOfWork.Attachments.Delete(attachment);
        await _unitOfWork.SaveChangesAsync();

        await _activityService.LogActivityAsync(
            projectId,
            userId,
            "Card",
            attachment.CardId,
            "AttachmentDeleted",
            $"Deleted attachment '{fileName}' from card '{cardTitle}'"
        );

        await _signalRService.SendAttachmentDeletedAsync(boardId, attachmentId);
    }
}
