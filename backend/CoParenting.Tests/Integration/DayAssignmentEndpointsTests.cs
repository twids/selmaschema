using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Infrastructure.Data;
using CoParenting.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoParenting.Tests.Integration;

public class DayAssignmentEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DayAssignmentEndpointsTests()
    {
        _factory = new TestWebApplicationFactory(Guid.NewGuid().ToString());
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region GET /api/days/{year}/{month}

    [Fact]
    public async Task GetMonthAssignments_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/days/2026/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMonthAssignments_WithAuth_EmptyMonth_ShouldReturnEmptyList()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/days/2026/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var monthData = await response.Content.ReadFromJsonAsync<MonthDataDto>();
        monthData.Should().NotBeNull();
        monthData!.Year.Should().Be(2026);
        monthData.Month.Should().Be(2);
        monthData.Days.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMonthAssignments_WithInitializedMonth_ShouldReturnAssignments()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Initialize month
        await _client.PostAsync("/api/days/2026/2/initialize", null);

        // Act
        var response = await _client.GetAsync("/api/days/2026/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var monthData = await response.Content.ReadFromJsonAsync<MonthDataDto>();
        monthData.Should().NotBeNull();
        monthData!.Year.Should().Be(2026);
        monthData.Month.Should().Be(2);
        monthData.Days.Should().HaveCount(28); // February 2026 has 28 days
        monthData.Days.Should().AllSatisfy(d => d.Date.Year.Should().Be(2026));
        monthData.Days.Should().AllSatisfy(d => d.Date.Month.Should().Be(2));
    }

    #endregion

    #region GET /api/days/{year}/{month}/{day}

    [Fact]
    public async Task GetDayAssignment_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/days/2026/2/15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDayAssignment_NonExistentDay_ShouldReturn404()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/days/2026/2/15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDayAssignment_ExistingDay_ShouldReturnAssignment()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a day assignment
        var updateDto = new UpdateDayAssignmentDto("A", false, null);
        await _client.PutAsJsonAsync("/api/days/2026/2/15", updateDto);

        // Act
        var response = await _client.GetAsync("/api/days/2026/2/15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignment = await response.Content.ReadFromJsonAsync<DayAssignmentDto>();
        assignment.Should().NotBeNull();
        assignment!.Date.Should().Be(new DateOnly(2026, 2, 15));
        assignment.Parent.Should().Be("A");
        assignment.IsVAB.Should().BeFalse();
    }

    #endregion

    #region PUT /api/days/{year}/{month}/{day}

    [Fact]
    public async Task UpdateDayAssignment_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var updateDto = new UpdateDayAssignmentDto("A", false, null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/days/2026/2/15", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateDayAssignment_CreateNew_ShouldReturnCreatedAssignment()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateDto = new UpdateDayAssignmentDto("B", false, "Holiday");

        // Act
        var response = await _client.PutAsJsonAsync("/api/days/2026/2/20", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignment = await response.Content.ReadFromJsonAsync<DayAssignmentDto>();
        assignment.Should().NotBeNull();
        assignment!.Date.Should().Be(new DateOnly(2026, 2, 20));
        assignment.Parent.Should().Be("B");
        assignment.IsVAB.Should().BeFalse();
        assignment.SpecialStatus.Should().Be("Holiday");
    }

    [Fact]
    public async Task UpdateDayAssignment_UpdateExisting_ShouldModifyAssignment()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create initial
        var initialDto = new UpdateDayAssignmentDto("A", false, null);
        await _client.PutAsJsonAsync("/api/days/2026/2/25", initialDto);

        // Act - Update to different values
        var updateDto = new UpdateDayAssignmentDto("B", true, "Sick Day");
        var response = await _client.PutAsJsonAsync("/api/days/2026/2/25", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignment = await response.Content.ReadFromJsonAsync<DayAssignmentDto>();
        assignment.Should().NotBeNull();
        assignment!.Parent.Should().Be("B");
        assignment.IsVAB.Should().BeTrue();
        assignment.SpecialStatus.Should().Be("Sick Day");
    }

    [Fact]
    public async Task UpdateDayAssignment_SetVAB_ShouldSetFlag()
    {
        // Arrange
        var token = await LoginAsParentBAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateDto = new UpdateDayAssignmentDto("A", true, null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/days/2026/3/10", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignment = await response.Content.ReadFromJsonAsync<DayAssignmentDto>();
        assignment!.IsVAB.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateDayAssignment_SetUnassigned_ShouldSetParentNull()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateDto = new UpdateDayAssignmentDto(null, false, null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/days/2026/3/15", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignment = await response.Content.ReadFromJsonAsync<DayAssignmentDto>();
        assignment!.Parent.Should().BeNull();
    }

    #endregion

    #region POST /api/days/{year}/{month}/initialize

    [Fact]
    public async Task InitializeMonth_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.PostAsync("/api/days/2026/2/initialize", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InitializeMonth_ShouldCreateAllDaysForMonth()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/days/2026/2/initialize", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var monthData = await response.Content.ReadFromJsonAsync<MonthDataDto>();
        monthData.Should().NotBeNull();
        monthData!.Days.Should().HaveCount(28); // February 2026 has 28 days
    }

    [Fact]
    public async Task InitializeMonth_ShouldFollowOddEvenWeekPattern()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/days/2026/6/initialize", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var monthData = await response.Content.ReadFromJsonAsync<MonthDataDto>();
        monthData.Should().NotBeNull();

        // June 2026 starts on Monday (week 1, odd -> Parent A)
        var firstDay = monthData!.Days.First(d => d.Date.Day == 1);
        firstDay.Parent.Should().Be("A", "June 1, 2026 is in an odd week");

        // Verify pattern consistency: consecutive days in same week have same parent
        var secondDay = monthData.Days.First(d => d.Date.Day == 2);
        secondDay.Parent.Should().Be("A", "June 2 is still in week 1");
    }

    [Fact]
    public async Task InitializeMonth_DifferentMonths_ShouldHaveCorrectDayCounts()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Initialize different months
        var feb2026 = await _client.PostAsync("/api/days/2026/2/initialize", null);
        var june2026 = await _client.PostAsync("/api/days/2026/6/initialize", null);
        var dec2026 = await _client.PostAsync("/api/days/2026/12/initialize", null);

        // Assert
        var febData = await feb2026.Content.ReadFromJsonAsync<MonthDataDto>();
        febData!.Days.Should().HaveCount(28, "February 2026 has 28 days");

        var juneData = await june2026.Content.ReadFromJsonAsync<MonthDataDto>();
        juneData!.Days.Should().HaveCount(30, "June has 30 days");

        var decData = await dec2026.Content.ReadFromJsonAsync<MonthDataDto>();
        decData!.Days.Should().HaveCount(31, "December has 31 days");
    }

    #endregion

    #region Helper Methods

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

    #endregion
}
