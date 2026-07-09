using System;
using System.Collections.Generic;
using Maham.Domain.Enums;

namespace Maham.Domain.Entities;

public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public CardStatus Status { get; set; } = CardStatus.Todo;
    public Guid? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? CoverImageUrl { get; set; }
    public int Order { get; set; }
    public int? StoryPoints { get; set; }
    public string? Labels { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Column Column { get; set; } = null!;
    public User? Assignee { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
