using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using CoParenting.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoParenting.Tests.Integration;

public class ChangeRequestEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ChangeRequestEndpointsTests()
    {
        _factory = new TestWebApplicationFactory(Guid.NewGuid().ToString());
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<string> LoginAsParentAAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var (success, magicToken) = await authService.CreateMagicLinkAsync(
            "parenta@test.com",
            "ParentA",
            "Parent A");

        var request = new MagicTokenRequest { Token = magicToken!.Token };
        var response = await _client.PostAsJsonAsync("/api/auth/magic", request);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    private async Task<string> LoginAsParentBAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var (success, magicToken) = await authService.CreateMagicLinkAsync(
            "parentb@test.com",
            "ParentB",
            "Parent B");

        var request = new MagicTokenRequest { Token = magicToken!.Token };
        var response = await _client.PostAsJsonAsync("/api/auth/magic", request);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    #region POST /api/change-requests Tests

    [Fact]
    public async Task CreateChangeRequests_ShouldReturn200_WithValidData()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new DateOnly(2025, 3, 15), new DateOnly(2025, 3, 16) },
            RequestedParent = "A",
            Comment = "Need to switch for work"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/change-requests", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<ChangeRequestDto>>();
        result.Should().HaveCount(2);
        result.All(cr => cr.RequestedParent == "A").Should().BeTrue();
        result.All(cr => cr.Status == "Pending").Should().BeTrue();
        result.All(cr => cr.Comment == "Need to switch for work").Should().BeTrue();
    }

    [Fact]
    public async Task CreateChangeRequests_ShouldReturn401_WhenNotAuthenticated()
    {
        // Arrange
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new DateOnly(2025, 3, 15) },
            RequestedParent = "A"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/change-requests", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateChangeRequests_ShouldReturn400_WhenDatesEmpty()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly>(),
            RequestedParent = "A"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/change-requests", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateChangeRequests_ShouldReturn400_WhenCommentTooLong()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var dto = new CreateChangeRequestDto
        {
            Dates = new List<DateOnly> { new DateOnly(2025, 3, 15) },
            RequestedParent = "A",
            Comment = new string('x', 1001) // Too long
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/change-requests", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/change-requests Tests

    [Fact]
    public async Task GetMyChangeRequests_ShouldReturnCreatedRequests()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
        var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = parentAUser.Id,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        context.ChangeRequests.Add(changeRequest);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/change-requests");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<ChangeRequestDto>>();
        result.Should().ContainSingle();
        result!.First().RequestedByName.Should().Be("Parent A");
    }

    [Fact]
    public async Task GetMyChangeRequests_ShouldReturnRequestsAffectingUsersDays()
    {
        // Arrange - Create Parent B first
        var tokenB = await LoginAsParentBAsync();
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
        var parentBUser = await context.Users.FirstAsync(u => u.Role == "ParentB");
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = parentBUser.Id, // Created by Parent B
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "A", // Affects Parent A's day
            RequestedParent = "B",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        context.ChangeRequests.Add(changeRequest);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/change-requests");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<ChangeRequestDto>>();
        result.Should().ContainSingle();
        result!.First().RequestedByName.Should().Be("Parent B");
    }

    [Fact]
    public async Task GetMyChangeRequests_ShouldReturn401_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/change-requests");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/change-requests/pending Tests

    [Fact]
    public async Task GetPendingChangeRequests_ShouldReturnOnlyPending()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
        var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
        context.ChangeRequests.AddRange(
            new ChangeRequest
            {
                RequestedByUserId = parentAUser.Id,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new ChangeRequest
            {
                RequestedByUserId = parentAUser.Id,
                RequestedForDate = new DateOnly(2025, 3, 16),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Approved",
                ReviewedByUserId = parentAUser.Id,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/change-requests/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<ChangeRequestDto>>();
        result.Should().ContainSingle();
        result!.First().Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GetPendingChangeRequests_ShouldReturn401_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/change-requests/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/change-requests/{id} Tests

    [Fact]
    public async Task GetChangeRequest_ShouldReturn200_WhenExists()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
        var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = parentAUser.Id,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        context.ChangeRequests.Add(changeRequest);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/change-requests/{changeRequest.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ChangeRequestDto>();
        result!.Id.Should().Be(changeRequest.Id);
        result.RequestedByName.Should().Be("Parent A");
    }

    [Fact]
    public async Task GetChangeRequest_ShouldReturn404_WhenNotExists()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/change-requests/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetChangeRequest_ShouldReturn401_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/change-requests/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/change-requests/{id}/review Tests

    [Fact]
    public async Task ReviewChangeRequest_ShouldApprove_AndUpdateDayAssignment()
    {
        // Arrange - Create both users first
        var tokenA = await LoginAsParentAAsync();
        var tokenB = await LoginAsParentBAsync();
        
        int changeRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
            var changeRequest = new ChangeRequest
            {
                RequestedByUserId = parentAUser.Id,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            context.ChangeRequests.Add(changeRequest);

            var dayAssignment = new DayAssignment
            {
                Date = new DateOnly(2025, 3, 15),
                Parent = "B",
                IsVAB = false,
                CreatedAt = DateTime.UtcNow
            };
            context.DayAssignments.Add(dayAssignment);
            await context.SaveChangesAsync();
            changeRequestId = changeRequest.Id;
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB); // Parent B reviews
        var reviewDto = new ReviewChangeRequestDto
        {
            Approved = true,
            Comment = "Approved!"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/change-requests/{changeRequestId}/review", reviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ChangeRequestDto>();
        result!.Status.Should().Be("Approved");
        result.Comment.Should().Be("Approved!");
        result.ReviewedByName.Should().Be("Parent B");

        // Verify day assignment updated
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var updatedDay = await context.DayAssignments.FirstAsync(d => d.Date == new DateOnly(2025, 3, 15));
            updatedDay.Parent.Should().Be("A");
        }
    }

    [Fact]
    public async Task ReviewChangeRequest_ShouldReject_WithoutUpdatingDayAssignment()
    {
        // Arrange - Create both users first
        var tokenA = await LoginAsParentAAsync();
        var tokenB = await LoginAsParentBAsync();
        
        int changeRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
            var changeRequest = new ChangeRequest
            {
                RequestedByUserId = parentAUser.Id,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            context.ChangeRequests.Add(changeRequest);

            var dayAssignment = new DayAssignment
            {
                Date = new DateOnly(2025, 3, 15),
                Parent = "B",
                IsVAB = false,
                CreatedAt = DateTime.UtcNow
            };
            context.DayAssignments.Add(dayAssignment);
            await context.SaveChangesAsync();
            changeRequestId = changeRequest.Id;
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var reviewDto = new ReviewChangeRequestDto
        {
            Approved = false,
            Comment = "Sorry, can't do it"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/change-requests/{changeRequestId}/review", reviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ChangeRequestDto>();
        result!.Status.Should().Be("Rejected");

        // Verify day assignment NOT updated
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var unchangedDay = await context.DayAssignments.FirstAsync(d => d.Date == new DateOnly(2025, 3, 15));
            unchangedDay.Parent.Should().Be("B");
        }
    }

    [Fact]
    public async Task ReviewChangeRequest_ShouldReturn404_WhenRequestNotFound()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var reviewDto = new ReviewChangeRequestDto { Approved = true };

        // Act
        var response = await _client.PostAsJsonAsync("/api/change-requests/999/review", reviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReviewChangeRequest_ShouldReturn400_WhenAlreadyReviewed()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        int changeRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
            var changeRequest = new ChangeRequest
            {
                RequestedByUserId = parentAUser.Id,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Approved",
                ReviewedByUserId = parentAUser.Id,
                ReviewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            context.ChangeRequests.Add(changeRequest);
            await context.SaveChangesAsync();
            changeRequestId = changeRequest.Id;
        }

        var reviewDto = new ReviewChangeRequestDto { Approved = true };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/change-requests/{changeRequestId}/review", reviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReviewChangeRequest_ShouldReturn401_WhenNotAuthenticated()
    {
        // Arrange
        var reviewDto = new ReviewChangeRequestDto { Approved = true };

        // Act
        var response = await _client.PostAsJsonAsync("/api/change-requests/1/review", reviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region DELETE /api/change-requests/{id} Tests

    [Fact]
    public async Task CancelChangeRequest_ShouldReturn204_WhenSuccessful()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        int changeRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
            var changeRequest = new ChangeRequest
            {
                RequestedByUserId = parentAUser.Id,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            context.ChangeRequests.Add(changeRequest);
            await context.SaveChangesAsync();
            changeRequestId = changeRequest.Id;
        }

        // Act
        var response = await _client.DeleteAsync($"/api/change-requests/{changeRequestId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify status updated to Cancelled
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var cancelled = await context.ChangeRequests.FindAsync(changeRequestId);
            cancelled!.Status.Should().Be("Cancelled");
        }
    }

    [Fact]
    public async Task CancelChangeRequest_ShouldReturn404_WhenRequestNotFound()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync("/api/change-requests/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelChangeRequest_ShouldReturn400_WhenNotCreator()
    {
        // Arrange
        var tokenA = await LoginAsParentAAsync();
        int changeRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
            var changeRequest = new ChangeRequest
            {
                RequestedByUserId = parentAUser.Id,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            context.ChangeRequests.Add(changeRequest);
            await context.SaveChangesAsync();
            changeRequestId = changeRequest.Id;
        }

        var tokenB = await LoginAsParentBAsync(); // Different user
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        // Act
        var response = await _client.DeleteAsync($"/api/change-requests/{changeRequestId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelChangeRequest_ShouldReturn400_WhenNotPending()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        int changeRequestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var parentAUser = await context.Users.FirstAsync(u => u.Role == "ParentA");
            var changeRequest = new ChangeRequest
            {
                RequestedByUserId = parentAUser.Id,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Approved",
                ReviewedByUserId = parentAUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.ChangeRequests.Add(changeRequest);
            await context.SaveChangesAsync();
            changeRequestId = changeRequest.Id;
        }

        // Act
        var response = await _client.DeleteAsync($"/api/change-requests/{changeRequestId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelChangeRequest_ShouldReturn401_WhenNotAuthenticated()
    {
        // Act
        var response = await _client.DeleteAsync("/api/change-requests/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
