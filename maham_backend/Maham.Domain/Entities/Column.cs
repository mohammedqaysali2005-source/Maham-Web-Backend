using System;
using System.Collections.Generic;

namespace Maham.Domain.Entities;

public class Column
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BoardId { get; set; }
    public string Name { get; set; } = null!;
    public int Order { get; set; }
    public int? WipLimit { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Board Board { get; set; } = null!;
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
