using CoParenting.Application.DTOs;
using FluentValidation;

namespace CoParenting.Application.Validators;

public class CreateChangeRequestDtoValidator : AbstractValidator<CreateChangeRequestDto>
{
    public CreateChangeRequestDtoValidator()
    {
        RuleFor(x => x.Dates)
            .NotEmpty()
            .WithMessage("At least one date is required");

        RuleFor(x => x.RequestedParent)
            .NotEmpty()
            .WithMessage("RequestedParent is required")
            .Must(parent => parent == "A" || parent == "B")
            .WithMessage("RequestedParent must be 'A' or 'B'");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("Comment must not exceed 1000 characters")
            .When(x => x.Comment != null);
    }
}
