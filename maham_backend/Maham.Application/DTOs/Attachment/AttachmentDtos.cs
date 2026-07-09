using System;
using Maham.Application.DTOs.Auth;

namespace Maham.Application.DTOs.Attachment;

public class AttachmentResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid UploadedById { get; set; }
    public UserResponseDto UploadedBy { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
}
