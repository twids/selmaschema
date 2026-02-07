using CoParenting.Application.DTOs;
using FluentValidation;

namespace CoParenting.Application.Validators;

public class ReviewChangeRequestDtoValidator : AbstractValidator<ReviewChangeRequestDto>
{
    public ReviewChangeRequestDtoValidator()
    {
        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("Comment must not exceed 1000 characters")
            .When(x => x.Comment != null);
    }
}
