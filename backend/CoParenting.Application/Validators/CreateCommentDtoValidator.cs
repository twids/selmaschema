using CoParenting.Application.DTOs;
using FluentValidation;

namespace CoParenting.Application.Validators;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.CommentText)
            .NotEmpty()
            .WithMessage("CommentText is required")
            .MaximumLength(1000)
            .WithMessage("CommentText must not exceed 1000 characters");
    }
}
