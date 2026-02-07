using CoParenting.Application.DTOs;
using CoParenting.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoParenting.Tests.Unit.Validators;

public class UpdateCommentDtoValidatorTests
{
    private readonly UpdateCommentDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommentText_ShouldPass()
    {
        // Arrange
        var dto = new UpdateCommentDto("Updated comment text");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullCommentText_ShouldFail()
    {
        // Arrange
        var dto = new UpdateCommentDto(null!);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CommentText)
            .WithErrorMessage("CommentText is required");
    }

    [Fact]
    public void Validate_WithEmptyCommentText_ShouldFail()
    {
        // Arrange
        var dto = new UpdateCommentDto("");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CommentText)
            .WithErrorMessage("CommentText is required");
    }

    [Fact]
    public void Validate_WithTooLongCommentText_ShouldFail()
    {
        // Arrange
        var longComment = new string('X', 1001);
        var dto = new UpdateCommentDto(longComment);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CommentText)
            .WithErrorMessage("CommentText must not exceed 1000 characters");
    }

    [Fact]
    public void Validate_WithExactly1000CharCommentText_ShouldPass()
    {
        // Arrange
        var exactComment = new string('X', 1000);
        var dto = new UpdateCommentDto(exactComment);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
