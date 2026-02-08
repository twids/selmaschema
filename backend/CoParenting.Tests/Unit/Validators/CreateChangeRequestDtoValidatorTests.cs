using CoParenting.Application.DTOs;
using CoParenting.Application.Validators;
using FluentAssertions;

namespace CoParenting.Tests.Unit.Validators;

public class CreateChangeRequestDtoValidatorTests
{
    private readonly CreateChangeRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsValid()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new(2025, 3, 15) },
            RequestedParent = "A",
            Comment = "Need to switch for work"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenCommentIsNull()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new DateOnly(2025, 3, 15) },
            RequestedParent = "A",
            Comment = null
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WithMultipleDates()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly>
            {
                new DateOnly(2025, 3, 15),
                new DateOnly(2025, 3, 16),
                new DateOnly(2025, 3, 17)
            },
            RequestedParent = "B"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDatesIsEmpty()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly>(),
            RequestedParent = "A"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Dates");
        result.Errors.First().ErrorMessage.Should().Be("At least one date is required");
    }

    [Fact]
    public void Validate_ShouldFail_WhenRequestedParentIsEmpty()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new DateOnly(2025, 3, 15) },
            RequestedParent = ""
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RequestedParent");
    }

    [Fact]
    public void Validate_ShouldFail_WhenRequestedParentIsInvalid()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new DateOnly(2025, 3, 15) },
            RequestedParent = "X"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "RequestedParent" && e.ErrorMessage == "RequestedParent must be 'A' or 'B'");
    }

    [Fact]
    public void Validate_ShouldFail_WhenCommentTooLong()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new DateOnly(2025, 3, 15) },
            RequestedParent = "A",
            Comment = new string('x', 1001)
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Comment");
        result.Errors.First().ErrorMessage.Should().Be("Comment must not exceed 1000 characters");
    }

    [Fact]
    public void Validate_ShouldPass_WhenCommentIsMaxLength()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new DateOnly(2025, 3, 15) },
            RequestedParent = "A",
            Comment = new string('x', 1000)
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
