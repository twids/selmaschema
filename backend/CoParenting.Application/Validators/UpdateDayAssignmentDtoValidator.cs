using CoParenting.Application.DTOs;
using FluentValidation;

namespace CoParenting.Application.Validators;

public class UpdateDayAssignmentDtoValidator : AbstractValidator<UpdateDayAssignmentDto>
{
    public UpdateDayAssignmentDtoValidator()
    {
        RuleFor(x => x.Parent)
            .Must(parent => parent == null || parent == "A" || parent == "B")
            .WithMessage("Parent must be 'A', 'B', or null");

        RuleFor(x => x.SpecialStatus)
            .MaximumLength(100)
            .WithMessage("SpecialStatus must not exceed 100 characters")
            .When(x => x.SpecialStatus != null);
    }
}
