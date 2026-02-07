using CoParenting.Application.DTOs;
using FluentValidation;

namespace CoParenting.Application.Validators;

public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
{
    public UpdateCommentDtoValidator()
    {
        RuleFor(x => x.CommentText)
            .NotEmpty()
            .WithMessage("CommentText is required")
            .MaximumLength(1000)
            .WithMessage("CommentText must not exceed 1000 characters");
    }
}
