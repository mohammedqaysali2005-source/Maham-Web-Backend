using System;
using System.Collections.Generic;
using Maham.Application.DTOs.Card;

namespace Maham.Application.DTOs.Column;

public class CreateColumnDto
{
    public string Name { get; set; } = null!;
    public int? WipLimit { get; set; }
}

public class UpdateColumnDto
{
    public string Name { get; set; } = null!;
    public int? WipLimit { get; set; }
}

public class ColumnResponseDto
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = null!;
    public int Order { get; set; }
    public int? WipLimit { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<CardResponseDto> Cards { get; set; } = new List<CardResponseDto>();
}

public class ColumnReorderDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
}
