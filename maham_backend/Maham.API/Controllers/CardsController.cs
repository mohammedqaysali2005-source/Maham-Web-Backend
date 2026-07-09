using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Card;
using Maham.Application.DTOs.Comment;
using Maham.Application.DTOs.Attachment;
using Maham.Application.DTOs.Activity;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class CardsController : BaseApiController
{
    private readonly ICardService _cardService;
    private readonly ICommentService _commentService;
    private readonly IAttachmentService _attachmentService;
    private readonly IActivityService _activityService;

    public CardsController(
        ICardService cardService,
        ICommentService commentService,
        IAttachmentService attachmentService,
        IActivityService activityService)
    {
        _cardService = cardService;
        _commentService = commentService;
        _attachmentService = attachmentService;
        _activityService = activityService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _cardService.GetCardByIdAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCardDto dto)
    {
        var result = await _cardService.UpdateCardAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Card updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _cardService.DeleteCardAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpPut("{id}/move")]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveCardDto dto)
    {
        var result = await _cardService.MoveCardAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Card moved successfully."));
    }

    [HttpPut("{id}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignCardDto dto)
    {
        var result = await _cardService.AssignCardAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Card assignee updated."));
    }

    [HttpPut("{id}/priority")]
    public async Task<IActionResult> UpdatePriority(Guid id, [FromBody] UpdatePriorityDto dto)
    {
        var result = await _cardService.UpdatePriorityAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Card priority updated."));
    }

    [HttpPut("{id}/duedate")]
    public async Task<IActionResult> UpdateDueDate(Guid id, [FromBody] UpdateDueDateDto dto)
    {
        var result = await _cardService.UpdateDueDateAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Card due date updated."));
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        var result = await _cardService.UpdateStatusAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Card status updated."));
    }

    [HttpPost("{id}/cover")]
    public async Task<IActionResult> UploadCover(Guid id, IFormFile file)
    {
        var result = await _cardService.UploadCoverAsync(id, CurrentUserId, file);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Cover image uploaded successfully."));
    }

    [HttpDelete("{id}/cover")]
    public async Task<IActionResult> DeleteCover(Guid id)
    {
        await _cardService.DeleteCoverAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<object>.SuccessResponse(null, "Cover image removed."));
    }

    [HttpPut("{id}/labels")]
    public async Task<IActionResult> UpdateLabels(Guid id, [FromBody] UpdateLabelsDto dto)
    {
        var result = await _cardService.UpdateLabelsAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Card labels updated."));
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetComments(Guid id)
    {
        var result = await _commentService.GetCardCommentsAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<CommentResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateCommentDto dto)
    {
        var result = await _commentService.CreateCommentAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CommentResponseDto>.SuccessResponse(result, "Comment added."));
    }

    [HttpGet("{id}/attachments")]
    public async Task<IActionResult> GetAttachments(Guid id)
    {
        var result = await _attachmentService.GetCardAttachmentsAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<AttachmentResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("{id}/attachments")]
    public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file)
    {
        var result = await _attachmentService.UploadAttachmentAsync(id, CurrentUserId, file);
        return Ok(ApiResponseDto<AttachmentResponseDto>.SuccessResponse(result, "Attachment uploaded successfully."));
    }

    [HttpGet("{id}/activity")]
    public async Task<IActionResult> GetActivity(Guid id)
    {
        var result = await _activityService.GetCardActivityAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<ActivityLogResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("assigned-to-me")]
    public async Task<IActionResult> GetAssignedToMe()
    {
        var result = await _cardService.GetUserAssignedCardsAsync(CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<CardResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("due-today")]
    public async Task<IActionResult> GetDueToday()
    {
        var result = await _cardService.GetUserCardsDueTodayAsync(CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<CardResponseDto>>.SuccessResponse(result));
    }
}
