using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Board;
using Maham.Application.DTOs.Column;
using Maham.Application.DTOs.Activity;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class BoardsController : BaseApiController
{
    private readonly IBoardService _boardService;
    private readonly IColumnService _columnService;
    private readonly IActivityService _activityService;

    public BoardsController(
        IBoardService boardService,
        IColumnService columnService,
        IActivityService activityService)
    {
        _boardService = boardService;
        _columnService = columnService;
        _activityService = activityService;
    }

    [HttpPost("/api/projects/{pid}/boards")]
    public async Task<IActionResult> Create(Guid pid, [FromBody] CreateBoardDto dto)
    {
        var result = await _boardService.CreateBoardAsync(pid, CurrentUserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponseDto<BoardResponseDto>.SuccessResponse(result, "Board created successfully."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _boardService.GetBoardDetailsAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<BoardDetailsDto>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoardDto dto)
    {
        var result = await _boardService.UpdateBoardAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<BoardResponseDto>.SuccessResponse(result, "Board updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _boardService.DeleteBoardAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpGet("{id}/columns")]
    public async Task<IActionResult> GetColumns(Guid id)
    {
        var details = await _boardService.GetBoardDetailsAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<ColumnResponseDto>>.SuccessResponse(details.Columns));
    }

    [HttpPost("{id}/columns")]
    public async Task<IActionResult> CreateColumn(Guid id, [FromBody] CreateColumnDto dto)
    {
        var result = await _columnService.CreateColumnAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<ColumnResponseDto>.SuccessResponse(result, "Column created successfully."));
    }

    [HttpPut("{id}/columns/reorder")]
    public async Task<IActionResult> ReorderColumns(Guid id, [FromBody] IEnumerable<ColumnReorderDto> dtos)
    {
        await _columnService.ReorderColumnsAsync(id, CurrentUserId, dtos);
        return Ok(ApiResponseDto<object>.SuccessResponse(null, "Columns reordered successfully."));
    }

    [HttpGet("{id}/activity")]
    public async Task<IActionResult> GetActivity(Guid id)
    {
        var result = await _activityService.GetBoardActivityAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<ActivityLogResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}/export")]
    public async Task<IActionResult> Export(Guid id)
    {
        var json = await _boardService.ExportBoardToJsonAsync(id, CurrentUserId);
        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"board_{id}_export.json");
    }
}
