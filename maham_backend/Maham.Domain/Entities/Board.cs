using System;
using System.Collections.Generic;

namespace Maham.Domain.Entities;

public class Board
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? BackgroundColor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ICollection<Column> Columns { get; set; } = new List<Column>();
}
