using System;
using System.Collections.Generic;
using Maham.Application.DTOs.Column;

namespace Maham.Application.DTOs.Board;

public class CreateBoardDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? BackgroundColor { get; set; }
}

public class UpdateBoardDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? BackgroundColor { get; set; }
}

public class BoardResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? BackgroundColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BoardDetailsDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? BackgroundColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<ColumnResponseDto> Columns { get; set; } = new List<ColumnResponseDto>();
}
