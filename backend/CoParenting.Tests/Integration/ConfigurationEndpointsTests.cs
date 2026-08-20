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

public class ConfigurationEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ConfigurationEndpointsTests()
    {
        _factory = new TestWebApplicationFactory(Guid.NewGuid().ToString());
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    #region GET /api/config/parent-names

    [Fact]
    public async Task GetParentNames_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/config/parent-names");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetParentNames_NoConfiguration_ShouldReturnDefaults()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        // Act
        var response = await _client.GetAsync("/api/config/parent-names");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var parentNames = await response.Content.ReadFromJsonAsync<ParentNamesDto>();
        parentNames.Should().NotBeNull();
        // Database is seeded with "Tomas" and "Carro" by default
        parentNames!.ParentAName.Should().Be("Tomas");
        parentNames.ParentBName.Should().Be("Carro");
    }

    [Fact]
    public async Task GetParentNames_WithCustomNames_ShouldReturnConfiguredNames()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        // Set custom names first (overwrite seed data)
        var setDto = new ParentNamesDto("Alice", "Bob");
        await _client.PutAsJsonAsync("/api/config/parent-names", setDto);

        // Act
        var response = await _client.GetAsync("/api/config/parent-names");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var parentNames = await response.Content.ReadFromJsonAsync<ParentNamesDto>();
        parentNames.Should().NotBeNull();
        parentNames!.ParentAName.Should().Be("Alice");
        parentNames.ParentBName.Should().Be("Bob");
    }

    [Fact]
    public async Task GetParentNames_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.SetSessionCookie(adminToken);

        // Act
        var response = await _client.GetAsync("/api/config/parent-names");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetParentNames_AsParentB_ShouldSucceed()
    {
        // Arrange
        var token = await LoginAsParentBAsync();
        _client.SetSessionCookie(token);

        // Act
        var response = await _client.GetAsync("/api/config/parent-names");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region PUT /api/config/parent-names

    [Fact]
    public async Task UpdateParentNames_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var dto = new ParentNamesDto("Test A", "Test B");

        // Act
        var response = await _client.PutAsJsonAsync("/api/config/parent-names", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateParentNames_WithAuth_ShouldUpdateConfiguration()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);
        var dto = new ParentNamesDto("Charlie", "Dana");

        // Act
        var response = await _client.PutAsJsonAsync("/api/config/parent-names", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ParentNamesDto>();
        result.Should().NotBeNull();
        result!.ParentAName.Should().Be("Charlie");
        result.ParentBName.Should().Be("Dana");
    }

    [Fact]
    public async Task UpdateParentNames_ShouldPersistAcrossRequests()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);
        var dto = new ParentNamesDto("Emma", "Frank");

        // Act - Update
        await _client.PutAsJsonAsync("/api/config/parent-names", dto);

        // Act - Retrieve
        var getResponse = await _client.GetAsync("/api/config/parent-names");

        // Assert
        var parentNames = await getResponse.Content.ReadFromJsonAsync<ParentNamesDto>();
        parentNames!.ParentAName.Should().Be("Emma");
        parentNames.ParentBName.Should().Be("Frank");
    }

    [Fact]
    public async Task UpdateParentNames_OverwriteExisting_ShouldUpdateBoth()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);

        // Set initial names
        await _client.PutAsJsonAsync("/api/config/parent-names", new ParentNamesDto("OldA", "OldB"));

        // Act - Update to new names
        var response = await _client.PutAsJsonAsync("/api/config/parent-names", new ParentNamesDto("NewA", "NewB"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ParentNamesDto>();
        result!.ParentAName.Should().Be("NewA");
        result.ParentBName.Should().Be("NewB");
    }

    [Fact]
    public async Task UpdateParentNames_WithLongNames_ShouldSucceed()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);
        var longNameA = "Alexander Christopher Montgomery III";
        var longNameB = "Victoria Elizabeth Thompson-Smith";
        var dto = new ParentNamesDto(longNameA, longNameB);

        // Act
        var response = await _client.PutAsJsonAsync("/api/config/parent-names", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ParentNamesDto>();
        result!.ParentAName.Should().Be(longNameA);
        result.ParentBName.Should().Be(longNameB);
    }

    [Fact]
    public async Task UpdateParentNames_WithSpecialCharacters_ShouldSucceed()
    {
        // Arrange
        var token = await LoginAsParentAAsync();
        _client.SetSessionCookie(token);
        var dto = new ParentNamesDto("André-François", "Björk Åström");

        // Act
        var response = await _client.PutAsJsonAsync("/api/config/parent-names", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ParentNamesDto>();
        result!.ParentAName.Should().Be("André-François");
        result.ParentBName.Should().Be("Björk Åström");
    }

    [Fact]
    public async Task UpdateParentNames_AsParentB_ShouldSucceed()
    {
        // Arrange
        var token = await LoginAsParentBAsync();
        _client.SetSessionCookie(token);
        var dto = new ParentNamesDto("Grace", "Henry");

        // Act
        var response = await _client.PutAsJsonAsync("/api/config/parent-names", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateParentNames_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.SetSessionCookie(adminToken);
        var dto = new ParentNamesDto("Ivan", "Julia");

        // Act
        var response = await _client.PutAsJsonAsync("/api/config/parent-names", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
