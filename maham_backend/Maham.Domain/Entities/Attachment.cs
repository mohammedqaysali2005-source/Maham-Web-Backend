using System;

namespace Maham.Domain.Entities;

public class Attachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CardId { get; set; }
    public Guid UploadedById { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = null!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Card Card { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
