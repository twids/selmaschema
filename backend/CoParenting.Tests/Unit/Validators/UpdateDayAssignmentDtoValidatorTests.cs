using CoParenting.Application.DTOs;
using CoParenting.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoParenting.Tests.Unit.Validators;

public class UpdateDayAssignmentDtoValidatorTests
{
    private readonly UpdateDayAssignmentDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidParentA_ShouldPass()
    {
        // Arrange
        var dto = new UpdateDayAssignmentDto("A", false, null);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidParentB_ShouldPass()
    {
        // Arrange
        var dto = new UpdateDayAssignmentDto("B", false, null);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullParent_ShouldPass()
    {
        // Arrange
        var dto = new UpdateDayAssignmentDto(null, false, null);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidParent_ShouldFail()
    {
        // Arrange
        var dto = new UpdateDayAssignmentDto("C", false, null);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Parent)
            .WithErrorMessage("Parent must be 'A', 'B', or null");
    }

    [Fact]
    public void Validate_WithEmptyParent_ShouldFail()
    {
        // Arrange
        var dto = new UpdateDayAssignmentDto("", false, null);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Parent);
    }

    [Fact]
    public void Validate_WithValidSpecialStatus_ShouldPass()
    {
        // Arrange
        var dto = new UpdateDayAssignmentDto("A", false, "Holiday");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithTooLongSpecialStatus_ShouldFail()
    {
        // Arrange
        var longStatus = new string('X', 101);
        var dto = new UpdateDayAssignmentDto("A", false, longStatus);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SpecialStatus)
            .WithErrorMessage("SpecialStatus must not exceed 100 characters");
    }

    [Fact]
    public void Validate_WithExactly100CharSpecialStatus_ShouldPass()
    {
        // Arrange
        var exactStatus = new string('X', 100);
        var dto = new UpdateDayAssignmentDto("A", false, exactStatus);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
