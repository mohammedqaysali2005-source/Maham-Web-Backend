using System;

namespace Maham.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CardId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = null!;
    public bool IsEdited { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Card Card { get; set; } = null!;
    public User Author { get; set; } = null!;
}
