using System;
using FluentValidation;
using Maham.Application.DTOs.Auth;
using Maham.Application.DTOs.Project;
using Maham.Application.DTOs.Board;
using Maham.Application.DTOs.Column;
using Maham.Application.DTOs.Card;
using Maham.Application.DTOs.Comment;

namespace Maham.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().Length(2, 150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(3, 200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(3, 200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class CreateBoardDtoValidator : AbstractValidator<CreateBoardDto>
{
    public CreateBoardDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(3, 200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.BackgroundColor)
            .Matches(@"^#[0-9a-fA-F]{6}$")
            .WithMessage("BackgroundColor must be in hex format (e.g. #ffffff).")
            .When(x => !string.IsNullOrEmpty(x.BackgroundColor));
    }
}

public class UpdateBoardDtoValidator : AbstractValidator<UpdateBoardDto>
{
    public UpdateBoardDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(3, 200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.BackgroundColor)
            .Matches(@"^#[0-9a-fA-F]{6}$")
            .WithMessage("BackgroundColor must be in hex format (e.g. #ffffff).")
            .When(x => !string.IsNullOrEmpty(x.BackgroundColor));
    }
}

public class CreateColumnDtoValidator : AbstractValidator<CreateColumnDto>
{
    public CreateColumnDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
        RuleFor(x => x.WipLimit).GreaterThan(0).When(x => x.WipLimit.HasValue);
    }
}

public class UpdateColumnDtoValidator : AbstractValidator<UpdateColumnDto>
{
    public UpdateColumnDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
        RuleFor(x => x.WipLimit).GreaterThan(0).When(x => x.WipLimit.HasValue);
    }
}

public class CreateCardDtoValidator : AbstractValidator<CreateCardDto>
{
    public CreateCardDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(1, 300);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.StoryPoints).InclusiveBetween(0, 100).When(x => x.StoryPoints.HasValue);
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("DueDate must be today or in the future.")
            .When(x => x.DueDate.HasValue);
    }
}

public class UpdateCardDtoValidator : AbstractValidator<UpdateCardDto>
{
    public UpdateCardDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(1, 300);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.StoryPoints).InclusiveBetween(0, 100).When(x => x.StoryPoints.HasValue);
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("DueDate must be today or in the future.")
            .When(x => x.DueDate.HasValue);
    }
}

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(1, 2000);
    }
}

public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
{
    public UpdateCommentDtoValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(1, 2000);
    }
}
