using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maham.Application.DTOs.Common;
using Maham.Application.DTOs.Column;
using Maham.Application.DTOs.Card;
using Maham.Application.Services.Interfaces;

namespace Maham.API.Controllers;

[Authorize]
public class ColumnsController : BaseApiController
{
    private readonly IColumnService _columnService;
    private readonly ICardService _cardService;

    public ColumnsController(IColumnService columnService, ICardService cardService)
    {
        _columnService = columnService;
        _cardService = cardService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _columnService.GetColumnByIdAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<ColumnResponseDto>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateColumnDto dto)
    {
        var result = await _columnService.UpdateColumnAsync(id, CurrentUserId, dto);
        return Ok(ApiResponseDto<ColumnResponseDto>.SuccessResponse(result, "Column updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _columnService.DeleteColumnAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpGet("{id}/cards")]
    public async Task<IActionResult> GetCards(Guid id)
    {
        var col = await _columnService.GetColumnByIdAsync(id, CurrentUserId);
        return Ok(ApiResponseDto<IEnumerable<CardResponseDto>>.SuccessResponse(col.Cards));
    }

    [HttpPost("{id}/cards")]
    public async Task<IActionResult> CreateCard(Guid id, [FromBody] CreateCardDto dto)
    {
        var result = await _cardService.CreateCardAsync(id, CurrentUserId, dto);
        return CreatedAtAction("GetById", "Cards", new { id = result.Id }, ApiResponseDto<CardResponseDto>.SuccessResponse(result, "Card created successfully."));
    }

    [HttpPut("{id}/cards/reorder")]
    public async Task<IActionResult> ReorderCards(Guid id, [FromBody] IEnumerable<CardReorderDto> dtos)
    {
        await _cardService.ReorderCardsAsync(id, CurrentUserId, dtos);
        return Ok(ApiResponseDto<object>.SuccessResponse(null, "Cards reordered successfully."));
    }
}
