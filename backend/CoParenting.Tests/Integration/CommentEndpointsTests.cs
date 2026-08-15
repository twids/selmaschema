using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoParenting.Tests.Integration;

public class CommentEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CommentEndpointsTests()
    {
        _factory = new TestWebApplicationFactory(Guid.NewGuid().ToString());
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region POST /api/comments/{dayAssignmentId}

    [Fact]
    public async Task AddComment_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var commentDto = new CreateCommentDto("Test comment");

        // Act
        var response = await _client.PostAsJsonAsync("/api/comments/1", commentDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddComment_AsParentA_ShouldDeriveParentFromClaims()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        // Create a day assignment first
        var dayDto = new UpdateDayAssignmentDto("A", false, null);
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/2/15", dayDto);
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        var commentDto = new CreateCommentDto("Comment from Parent A");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", commentDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comment = await response.Content.ReadFromJsonAsync<CommentDto>();
        comment.Should().NotBeNull();
        comment!.Parent.Should().Be("A", "should derive parent from authenticated user's role");
        comment.CommentText.Should().Be("Comment from Parent A");
        comment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task AddComment_AsParentB_ShouldDeriveParentFromClaims()
    {
        // Arrange
        var token = await LoginAsParentBAsync();
        _client.SetSessionCookie(token);

        // Create a day assignment first
        var dayDto = new UpdateDayAssignmentDto("B", false, null);
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/2/20", dayDto);
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        var commentDto = new CreateCommentDto("Comment from Parent B");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", commentDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comment = await response.Content.ReadFromJsonAsync<CommentDto>();
        comment.Should().NotBeNull();
        comment!.Parent.Should().Be("B", "should derive parent from authenticated user's role");
        comment.CommentText.Should().Be("Comment from Parent B");
    }

    [Fact]
    public async Task AddComment_AsAdmin_ShouldReturn400()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.SetSessionCookie(adminToken);

        // Create a day assignment first as ParentA
        var parentToken = await LoginAsParentAAsync();
        _client.SetSessionCookie(parentToken);
        var dayDto = new UpdateDayAssignmentDto("A", false, null);
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/2/25", dayDto);
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        // Switch back to admin
        _client.SetSessionCookie(adminToken);
        var commentDto = new CreateCommentDto("Admin comment");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", commentDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddComment_WithLongText_ShouldSucceed()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        var dayDto = new UpdateDayAssignmentDto("A", false, null);
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/3/1", dayDto);
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        var longComment = new string('A', 500);
        var commentDto = new CreateCommentDto(longComment);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", commentDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comment = await response.Content.ReadFromJsonAsync<CommentDto>();
        comment!.CommentText.Should().Be(longComment);
    }

    #endregion

    #region GET /api/comments/{dayAssignmentId}

    [Fact]
    public async Task GetComments_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/comments/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetComments_NoDayAssignment_ShouldReturnEmptyList()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        // Act
        var response = await _client.GetAsync("/api/comments/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>();
        comments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetComments_WithComments_ShouldReturnAllComments()
    {
        // Arrange
        var tokenA = await LoginAsParentAAsync();
        var tokenB = await LoginAsParentBAsync();

        // Create a day as ParentA
        _client.SetSessionCookie(tokenA);
        var dayDto = new UpdateDayAssignmentDto("A", false, null);
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/3/10", dayDto);
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        // Add comment from ParentA
        await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", new CreateCommentDto("Parent A comment"));

        // Add comment from ParentB
        _client.SetSessionCookie(tokenB);
        await _client.PostAsJsonAsync($"/api/comments/{day.Id}", new CreateCommentDto("Parent B comment"));

        // Act - Get comments as any user
        _client.SetSessionCookie(tokenA);
        var response = await _client.GetAsync($"/api/comments/{day.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>();
        comments.Should().HaveCount(2);
        comments.Should().Contain(c => c.Parent == "A" && c.CommentText == "Parent A comment");
        comments.Should().Contain(c => c.Parent == "B" && c.CommentText == "Parent B comment");
    }

    [Fact]
    public async Task GetComments_ShouldReturnOrderedByCreatedAt()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        var dayDto = new UpdateDayAssignmentDto("A", false, null);
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/3/15", dayDto);
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        // Add comments with delays
        await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", new CreateCommentDto("First"));
        await Task.Delay(10);
        await _client.PostAsJsonAsync($"/api/comments/{day.Id}", new CreateCommentDto("Second"));
        await Task.Delay(10);
        await _client.PostAsJsonAsync($"/api/comments/{day.Id}", new CreateCommentDto("Third"));

        // Act
        var response = await _client.GetAsync($"/api/comments/{day.Id}");

        // Assert
        var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>();
        comments!.Should().HaveCount(3);
        comments[0].CommentText.Should().Be("First");
        comments[1].CommentText.Should().Be("Second");
        comments[2].CommentText.Should().Be("Third");
    }

    #endregion

    #region PUT /api/comments/{commentId}

    [Fact]
    public async Task UpdateComment_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var updateDto = new UpdateCommentDto("Updated text");

        // Act
        var response = await _client.PutAsJsonAsync("/api/comments/1", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateComment_ExistingComment_ShouldUpdateText()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        // Create day and comment
        var dayDto = new UpdateDayAssignmentDto("A", false, null);
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/3/20", dayDto);
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        var createDto = new CreateCommentDto("Original text");
        var createResponse = await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", createDto);
        var comment = await createResponse.Content.ReadFromJsonAsync<CommentDto>();

        // Act
        var updateDto = new UpdateCommentDto("Updated text");
        var response = await _client.PutAsJsonAsync($"/api/comments/{comment!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CommentDto>();
        updated!.CommentText.Should().Be("Updated text");
        updated.ModifiedAt.Should().NotBeNull();
        updated.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task UpdateComment_NonExistentComment_ShouldReturn404()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);
        var updateDto = new UpdateCommentDto("Updated text");

        // Act
        var response = await _client.PutAsJsonAsync("/api/comments/9999", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/comments/{commentId}

    [Fact]
    public async Task DeleteComment_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.DeleteAsync("/api/comments/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteComment_ExistingComment_ShouldRemoveAndReturn204()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        // Create day and comment
        var dayDto = new UpdateDayAssignmentDto("A", false, null);
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/3/25", dayDto);
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        var createDto = new CreateCommentDto("To be deleted");
        var createResponse = await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", createDto);
        var comment = await createResponse.Content.ReadFromJsonAsync<CommentDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/comments/{comment!.Id}");

        // Assert - Delete succeeds
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify comment is gone
        var getResponse = await _client.GetAsync($"/api/comments/{day.Id}");
        var comments = await getResponse.Content.ReadFromJsonAsync<List<CommentDto>>();
        comments.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteComment_NonExistentComment_ShouldReturn404()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        // Act
        var response = await _client.DeleteAsync("/api/comments/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    private async Task<string> LoginAsAdminAsync()
    {
        return await _factory.AuthenticateClientAsync(_client, "Admin", "admin@test.com");
    }

    private async Task<string> LoginAsParentAAsync()
    {
        return await _factory.AuthenticateClientAsync(_client, "ParentA", "parenta@test.com");
    }

    private async Task<string> LoginAsParentBAsync()
    {
        return await _factory.AuthenticateClientAsync(_client, "ParentB", "parentb@test.com");
    }

    #endregion
}
