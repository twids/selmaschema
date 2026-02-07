using CoParenting.Application.DTOs;
using CoParenting.Application.Validators;
using FluentAssertions;

namespace CoParenting.Tests.Unit.Validators;

public class ReviewChangeRequestDtoValidatorTests
{
    private readonly ReviewChangeRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsValid()
    {
        // Arrange
        var dto = new ReviewChangeRequestDto
        {
            Approved = true,
            Comment = "Looks good!"
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
        var dto = new ReviewChangeRequestDto
        {
            Approved = false,
            Comment = null
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenApprovedIsFalse()
    {
        // Arrange
        var dto = new ReviewChangeRequestDto
        {
            Approved = false,
            Comment = "Sorry, can't do it"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenCommentTooLong()
    {
        // Arrange
        var dto = new ReviewChangeRequestDto
        {
            Approved = true,
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
        var dto = new ReviewChangeRequestDto
        {
            Approved = true,
            Comment = new string('x', 1000)
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenCommentIsEmpty()
    {
        // Arrange
        var dto = new ReviewChangeRequestDto
        {
            Approved = true,
            Comment = ""
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
