using System;

namespace Maham.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
