using System;
using Maham.Application.DTOs.Auth;

namespace Maham.Application.DTOs.Comment;

public class CreateCommentDto
{
    public string Content { get; set; } = null!;
}

public class UpdateCommentDto
{
    public string Content { get; set; } = null!;
}

public class CommentResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid AuthorId { get; set; }
    public UserResponseDto Author { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
