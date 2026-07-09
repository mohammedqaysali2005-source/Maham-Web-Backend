using System;
using System.Collections.Generic;
using Maham.Application.DTOs.Auth;
using Maham.Domain.Enums;

namespace Maham.Application.DTOs.Card;

public class CreateCardDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public int? StoryPoints { get; set; }
    public string? Labels { get; set; }
}

public class UpdateCardDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Priority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public int? StoryPoints { get; set; }
    public string? Labels { get; set; }
}

public class CardResponseDto
{
    public Guid Id { get; set; }
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Priority Priority { get; set; }
    public CardStatus Status { get; set; }
    public Guid? AssigneeId { get; set; }
    public UserResponseDto? Assignee { get; set; }
    public DateTime? DueDate { get; set; }
    public string? CoverImageUrl { get; set; }
    public int Order { get; set; }
    public int? StoryPoints { get; set; }
    public string? Labels { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MoveCardDto
{
    public Guid ToColumnId { get; set; }
    public int NewOrder { get; set; }
}

public class AssignCardDto
{
    public Guid? AssigneeId { get; set; }
}

public class UpdatePriorityDto
{
    public Priority Priority { get; set; }
}

public class UpdateDueDateDto
{
    public DateTime? DueDate { get; set; }
}

public class UpdateStatusDto
{
    public CardStatus Status { get; set; }
}

public class UpdateLabelsDto
{
    public string? Labels { get; set; }
}

public class CardReorderDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
}
