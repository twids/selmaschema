using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoParenting.Tests.Unit.Services;

public class CommentServiceTests : IDisposable
{
    private readonly CoParentingDbContext _context;
    private readonly CommentService _service;

    public CommentServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<CoParentingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CoParentingDbContext(options);

        // Create service
        _service = new CommentService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region AddCommentAsync Tests

    [Fact]
    public async Task AddCommentAsync_ShouldCreateCommentWithCorrectParent()
    {
        // Arrange
        var dayAssignmentId = 1;
        var parent = "A";
        var commentText = "Test comment";

        // Act
        var result = await _service.AddCommentAsync(dayAssignmentId, parent, commentText);

        // Assert
        result.Should().NotBeNull();
        result.DayAssignmentId.Should().Be(dayAssignmentId);
        result.Parent.Should().Be(parent);
        result.CommentText.Should().Be(commentText);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.ModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task AddCommentAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var dayAssignmentId = 1;
        var parent = "B";
        var commentText = "Persisted comment";

        // Act
        await _service.AddCommentAsync(dayAssignmentId, parent, commentText);

        // Assert
        var comments = await _context.Comments
            .Where(c => c.DayAssignmentId == dayAssignmentId)
            .ToListAsync();
        comments.Should().HaveCount(1);
        comments[0].CommentText.Should().Be(commentText);
    }

    [Fact]
    public async Task AddCommentAsync_ParentA_ShouldSetParentCorrectly()
    {
        // Arrange
        var dayAssignmentId = 5;
        var parent = "A";
        var commentText = "Parent A comment";

        // Act
        var result = await _service.AddCommentAsync(dayAssignmentId, parent, commentText);

        // Assert
        result.Parent.Should().Be("A");
    }

    [Fact]
    public async Task AddCommentAsync_ParentB_ShouldSetParentCorrectly()
    {
        // Arrange
        var dayAssignmentId = 5;
        var parent = "B";
        var commentText = "Parent B comment";

        // Act
        var result = await _service.AddCommentAsync(dayAssignmentId, parent, commentText);

        // Assert
        result.Parent.Should().Be("B");
    }

    #endregion

    #region GetCommentsForDayAsync Tests

    [Fact]
    public async Task GetCommentsForDayAsync_NoComments_ShouldReturnEmptyList()
    {
        // Arrange
        var dayAssignmentId = 99;

        // Act
        var result = await _service.GetCommentsForDayAsync(dayAssignmentId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCommentsForDayAsync_WithComments_ShouldReturnAllCommentsForDay()
    {
        // Arrange
        var dayAssignmentId = 10;
        await _service.AddCommentAsync(dayAssignmentId, "A", "First comment");
        await _service.AddCommentAsync(dayAssignmentId, "B", "Second comment");
        await _service.AddCommentAsync(999, "A", "Different day comment");

        // Act
        var result = await _service.GetCommentsForDayAsync(dayAssignmentId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.CommentText == "First comment");
        result.Should().Contain(c => c.CommentText == "Second comment");
        result.Should().NotContain(c => c.CommentText == "Different day comment");
    }

    [Fact]
    public async Task GetCommentsForDayAsync_ShouldReturnOrderedByCreatedAt()
    {
        // Arrange
        var dayAssignmentId = 15;
        
        // Add comments with slight delays to ensure different timestamps
        var comment1 = await _service.AddCommentAsync(dayAssignmentId, "A", "Third comment");
        await Task.Delay(10);
        var comment2 = await _service.AddCommentAsync(dayAssignmentId, "B", "First comment");
        await Task.Delay(10);
        var comment3 = await _service.AddCommentAsync(dayAssignmentId, "A", "Second comment");

        // Act
        var result = await _service.GetCommentsForDayAsync(dayAssignmentId);

        // Assert
        result.Should().HaveCount(3);
        result[0].CommentText.Should().Be("Third comment"); // oldest
        result[1].CommentText.Should().Be("First comment");
        result[2].CommentText.Should().Be("Second comment"); // newest
    }

    [Fact]
    public async Task GetCommentsForDayAsync_MultipleParents_ShouldReturnBothParentsComments()
    {
        // Arrange
        var dayAssignmentId = 20;
        await _service.AddCommentAsync(dayAssignmentId, "A", "Parent A comment");
        await _service.AddCommentAsync(dayAssignmentId, "B", "Parent B comment");

        // Act
        var result = await _service.GetCommentsForDayAsync(dayAssignmentId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Parent == "A" && c.CommentText == "Parent A comment");
        result.Should().Contain(c => c.Parent == "B" && c.CommentText == "Parent B comment");
    }

    #endregion

    #region UpdateCommentAsync Tests

    [Fact]
    public async Task UpdateCommentAsync_ExistingComment_ShouldUpdateTextAndModifiedAt()
    {
        // Arrange
        var comment = await _service.AddCommentAsync(1, "A", "Original text");
        var newText = "Updated text";

        // Act
        var result = await _service.UpdateCommentAsync(comment.Id, newText);

        // Assert
        result.Should().NotBeNull();
        result!.CommentText.Should().Be(newText);
        result.ModifiedAt.Should().NotBeNull();
        result.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateCommentAsync_ShouldPersistChanges()
    {
        // Arrange
        var comment = await _service.AddCommentAsync(1, "B", "Original text");
        var newText = "Persisted update";

        // Act
        await _service.UpdateCommentAsync(comment.Id, newText);

        // Assert
        var updated = await _context.Comments.FindAsync(comment.Id);
        updated!.CommentText.Should().Be(newText);
        updated.ModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateCommentAsync_NonExistentComment_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.UpdateCommentAsync(nonExistentId, "New text");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCommentAsync_ShouldNotChangeCreatedAtOrParent()
    {
        // Arrange
        var comment = await _service.AddCommentAsync(1, "A", "Original text");
        var originalCreatedAt = comment.CreatedAt;
        var originalParent = comment.Parent;

        // Act
        var result = await _service.UpdateCommentAsync(comment.Id, "Updated text");

        // Assert
        result!.CreatedAt.Should().Be(originalCreatedAt);
        result.Parent.Should().Be(originalParent);
    }

    #endregion

    #region DeleteCommentAsync Tests

    [Fact]
    public async Task DeleteCommentAsync_ExistingComment_ShouldReturnTrueAndRemove()
    {
        // Arrange
        var comment = await _service.AddCommentAsync(1, "A", "To be deleted");

        // Act
        var result = await _service.DeleteCommentAsync(comment.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Comments.FindAsync(comment.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCommentAsync_NonExistentComment_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.DeleteCommentAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldNotAffectOtherComments()
    {
        // Arrange
        var comment1 = await _service.AddCommentAsync(1, "A", "Keep this");
        var comment2 = await _service.AddCommentAsync(1, "B", "Delete this");
        var comment3 = await _service.AddCommentAsync(1, "A", "Keep this too");

        // Act
        await _service.DeleteCommentAsync(comment2.Id);

        // Assert
        var remaining = await _service.GetCommentsForDayAsync(1);
        remaining.Should().HaveCount(2);
        remaining.Should().Contain(c => c.Id == comment1.Id);
        remaining.Should().Contain(c => c.Id == comment3.Id);
        remaining.Should().NotContain(c => c.Id == comment2.Id);
    }

    #endregion
}
