using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Attachment;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class AttachmentsController : BaseApiController
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _attachmentService.GetAttachmentByIdAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<AttachmentResponseDto>.SuccessResponse(result));
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var (content, fileName, contentType) = await _attachmentService.DownloadAttachmentAsync(id, CurrentUserId);
        return File(content, contentType, fileName);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _attachmentService.DeleteAttachmentAsync(id, CurrentUserId);
        return NoContent();
    }
}
