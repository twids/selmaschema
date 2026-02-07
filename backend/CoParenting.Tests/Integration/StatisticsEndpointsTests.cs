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

public class StatisticsEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public StatisticsEndpointsTests()
    {
        _factory = new TestWebApplicationFactory(Guid.NewGuid().ToString());
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region GET /api/statistics/{year}

    [Fact]
    public async Task GetYearStatistics_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetYearStatistics_EmptyYear_ShouldReturnZeros()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<StatisticsDto>();
        stats.Should().NotBeNull();
        stats!.ParentADays.Should().Be(0);
        stats.ParentBDays.Should().Be(0);
        stats.VABDays.Should().Be(0);
        // Empty year means all 365 days are unassigned
        stats.UnassignedDays.Should().Be(365);
        stats.DaysWithComments.Should().Be(0);
    }

    [Fact]
    public async Task GetYearStatistics_WithAssignments_ShouldCountCorrectly()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create various assignments
        await _client.PutAsJsonAsync("/api/days/2026/2/1", new UpdateDayAssignmentDto("A", false, null));
        await _client.PutAsJsonAsync("/api/days/2026/2/2", new UpdateDayAssignmentDto("A", false, null));
        await _client.PutAsJsonAsync("/api/days/2026/2/3", new UpdateDayAssignmentDto("B", false, null));
        await _client.PutAsJsonAsync("/api/days/2026/2/4", new UpdateDayAssignmentDto(null, false, null));

        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<StatisticsDto>();
        stats.Should().NotBeNull();
        stats!.ParentADays.Should().Be(2);
        stats.ParentBDays.Should().Be(1);
        // Unassigned = all days in year minus assigned days (365 - 3 = 362)
        stats.UnassignedDays.Should().Be(362);
    }

    [Fact]
    public async Task GetYearStatistics_WithVABDays_ShouldCountVABCorrectly()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create assignments with VAB
        await _client.PutAsJsonAsync("/api/days/2026/3/1", new UpdateDayAssignmentDto("A", true, null));
        await _client.PutAsJsonAsync("/api/days/2026/3/2", new UpdateDayAssignmentDto("B", true, null));
        await _client.PutAsJsonAsync("/api/days/2026/3/3", new UpdateDayAssignmentDto("A", false, null));

        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<StatisticsDto>();
        stats.Should().NotBeNull();
        stats!.VABDays.Should().Be(2);
        stats.ParentADays.Should().Be(2); // Both VAB and non-VAB count
        stats.ParentBDays.Should().Be(1);
    }

    [Fact]
    public async Task GetYearStatistics_WithComments_ShouldCountDaysWithComments()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create assignments
        var day1Response = await _client.PutAsJsonAsync("/api/days/2026/4/1", new UpdateDayAssignmentDto("A", false, null));
        var day1 = await day1Response.Content.ReadFromJsonAsync<DayAssignmentDto>();

        var day2Response = await _client.PutAsJsonAsync("/api/days/2026/4/2", new UpdateDayAssignmentDto("B", false, null));
        var day2 = await day2Response.Content.ReadFromJsonAsync<DayAssignmentDto>();

        await _client.PutAsJsonAsync("/api/days/2026/4/3", new UpdateDayAssignmentDto("A", false, null));

        // Add comments to first two days
        await _client.PostAsJsonAsync($"/api/comments/{day1!.Id}", new CreateCommentDto("Comment on day 1"));
        await _client.PostAsJsonAsync($"/api/comments/{day2!.Id}", new CreateCommentDto("Comment on day 2"));

        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<StatisticsDto>();
        stats.Should().NotBeNull();
        stats!.DaysWithComments.Should().Be(2);
    }

    [Fact]
    public async Task GetYearStatistics_MultipleMonths_ShouldAggregateAcrossYear()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create assignments in different months
        await _client.PutAsJsonAsync("/api/days/2026/1/15", new UpdateDayAssignmentDto("A", false, null));
        await _client.PutAsJsonAsync("/api/days/2026/6/20", new UpdateDayAssignmentDto("A", false, null));
        await _client.PutAsJsonAsync("/api/days/2026/12/25", new UpdateDayAssignmentDto("B", false, null));

        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<StatisticsDto>();
        stats.Should().NotBeNull();
        stats!.ParentADays.Should().Be(2);
        stats.ParentBDays.Should().Be(1);
    }

    [Fact]
    public async Task GetYearStatistics_DifferentYears_ShouldNotMixData()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create assignments in 2026
        await _client.PutAsJsonAsync("/api/days/2026/5/1", new UpdateDayAssignmentDto("A", false, null));
        await _client.PutAsJsonAsync("/api/days/2026/5/2", new UpdateDayAssignmentDto("A", false, null));

        // Create assignments in 2027
        await _client.PutAsJsonAsync("/api/days/2027/5/1", new UpdateDayAssignmentDto("B", false, null));

        // Act - Get 2026 stats
        var response2026 = await _client.GetAsync("/api/statistics/2026");
        var stats2026 = await response2026.Content.ReadFromJsonAsync<StatisticsDto>();

        // Act - Get 2027 stats
        var response2027 = await _client.GetAsync("/api/statistics/2027");
        var stats2027 = await response2027.Content.ReadFromJsonAsync<StatisticsDto>();

        // Assert
        stats2026!.ParentADays.Should().Be(2);
        stats2026.ParentBDays.Should().Be(0);

        stats2027!.ParentADays.Should().Be(0);
        stats2027.ParentBDays.Should().Be(1);
    }

    [Fact]
    public async Task GetYearStatistics_MultipleCommentsOnOneDay_ShouldCountDayOnce()
    {
        // Arrange
        var tokenA = await LoginAsParentAAsync();
        var tokenB = await LoginAsParentBAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        // Create assignment
        var dayResponse = await _client.PutAsJsonAsync("/api/days/2026/6/15", new UpdateDayAssignmentDto("A", false, null));
        var day = await dayResponse.Content.ReadFromJsonAsync<DayAssignmentDto>();

        // Add multiple comments from different parents
        await _client.PostAsJsonAsync($"/api/comments/{day!.Id}", new CreateCommentDto("Comment 1 from A"));
        await _client.PostAsJsonAsync($"/api/comments/{day.Id}", new CreateCommentDto("Comment 2 from A"));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        await _client.PostAsJsonAsync($"/api/comments/{day.Id}", new CreateCommentDto("Comment from B"));

        // Act
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        var stats = await response.Content.ReadFromJsonAsync<StatisticsDto>();
        stats!.DaysWithComments.Should().Be(1, "multiple comments on same day should count as one");
    }

    [Fact]
    public async Task GetYearStatistics_AsParentB_ShouldSucceed()
    {
        // Arrange
        var token = await LoginAsParentBAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetYearStatistics_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetYearStatistics_ComplexScenario_ShouldCalculateAllMetricsCorrectly()
    {
        // Arrange
        var tokenA = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        // Create a complex scenario:
        // - 3 days for Parent A (1 with VAB, 1 with comment)
        // - 2 days for Parent B (1 with VAB)
        // - 1 unassigned day
        var day1 = await CreateDayAsync("2026/7/1", "A", false);
        var day2 = await CreateDayAsync("2026/7/2", "A", true); // VAB
        var day3 = await CreateDayAsync("2026/7/3", "A", false);
        var day4 = await CreateDayAsync("2026/7/4", "B", false);
        var day5 = await CreateDayAsync("2026/7/5", "B", true); // VAB
        await CreateDayAsync("2026/7/6", null, false); // Unassigned

        // Add comment to day3
        await _client.PostAsJsonAsync($"/api/comments/{day3.Id}", new CreateCommentDto("Comment"));

        // Act
        var response = await _client.GetAsync("/api/statistics/2026");

        // Assert
        var stats = await response.Content.ReadFromJsonAsync<StatisticsDto>();
        stats.Should().NotBeNull();
        stats!.ParentADays.Should().Be(3);
        stats.ParentBDays.Should().Be(2);
        stats.VABDays.Should().Be(2);
        // Unassigned = days in year minus assigned to A or B (365 - 5 = 360)
        // Note: days with Parent=null are still counted in total days but not in A or B counts
        stats.UnassignedDays.Should().Be(360);
        stats.DaysWithComments.Should().Be(1);
    }

    #endregion

    #region Helper Methods

    private async Task<string> LoginAsAdminAsync()
    {
        var request = new AdminLoginRequest { Password = "admin123" };
        var response = await _client.PostAsJsonAsync("/api/auth/admin/login", request);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    private async Task<string> LoginAsParentAAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var (success, magicToken) = await authService.CreateMagicLinkAsync(
            "parenta@test.com",
            "ParentA",
            "Parent A Test");

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
            "Parent B Test");

        var request = new MagicTokenRequest { Token = magicToken!.Token };
        var response = await _client.PostAsJsonAsync("/api/auth/magic", request);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    private async Task<DayAssignmentDto> CreateDayAsync(string dateStr, string? parent, bool isVAB)
    {
        var response = await _client.PutAsJsonAsync($"/api/days/{dateStr}", 
            new UpdateDayAssignmentDto(parent, isVAB, null));
        var day = await response.Content.ReadFromJsonAsync<DayAssignmentDto>();
        return day!;
    }

    #endregion
}
