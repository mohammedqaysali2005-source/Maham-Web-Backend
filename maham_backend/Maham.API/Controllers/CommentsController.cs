using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Comment;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class CommentsController : BaseApiController
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _commentService.GetCommentByIdAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<CommentResponseDto>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentDto dto)
    {
        var result = await _commentService.UpdateCommentAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<CommentResponseDto>.SuccessResponse(result, "Comment updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _commentService.DeleteCommentAsync(id, CurrentUserId);
        return NoContent();
    }
}
