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

public class AdminEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminEndpointsTests()
    {
        _factory = new TestWebApplicationFactory(Guid.NewGuid().ToString());
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task CreateMagicLink_AsAdmin_ShouldReturn200()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var request = new CreateMagicLinkRequest
        {
            Email = "newparent@test.com",
            Role = "ParentA",
            DisplayName = "New Parent"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/magic-links", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var magicLinkResponse = await response.Content.ReadFromJsonAsync<MagicLinkResponse>();
        magicLinkResponse.Should().NotBeNull();
        magicLinkResponse!.Token.Should().NotBeNullOrEmpty();
        magicLinkResponse.Token.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        magicLinkResponse.MagicLink.Should().Contain("/auth/magic?token=");
        magicLinkResponse.Role.Should().Be("ParentA");
    }

    [Fact]
    public async Task CreateMagicLink_AsParent_ShouldReturn403()
    {
        // Arrange
        var parentToken = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var request = new CreateMagicLinkRequest
        {
            Email = "newparent@test.com",
            Role = "ParentB",
            DisplayName = "New Parent B"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/magic-links", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateMagicLink_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var request = new CreateMagicLinkRequest
        {
            Email = "newparent@test.com",
            Role = "ParentA",
            DisplayName = "New Parent"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/magic-links", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllUsers_AsAdmin_ShouldReturn200()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
        users.Should().Contain(u => u.Role == "Admin");
    }

    [Fact]
    public async Task GetAllUsers_AsParent_ShouldReturn403()
    {
        // Arrange
        var parentToken = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsers_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPendingMagicLinks_AsAdmin_ShouldReturn200()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create a magic link first
        var createRequest = new CreateMagicLinkRequest
        {
            Email = "pending@test.com",
            Role = "ParentA",
            DisplayName = "Pending Parent"
        };
        await _client.PostAsJsonAsync("/api/admin/magic-links", createRequest);

        // Act
        var response = await _client.GetAsync("/api/admin/magic-links");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var magicLinks = await response.Content.ReadFromJsonAsync<List<MagicLinkResponse>>();
        magicLinks.Should().NotBeNull();
        magicLinks.Should().Contain(ml => ml.Email == "pending@test.com");
    }

    [Fact]
    public async Task GetPendingMagicLinks_AsParent_ShouldReturn403()
    {
        // Arrange
        var parentToken = await LoginAsParentAAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);

        // Act
        var response = await _client.GetAsync("/api/admin/magic-links");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPendingMagicLinks_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/magic-links");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

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
        // Create magic link for ParentA
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var (success, magicToken) = await authService.CreateMagicLinkAsync(
            "parenta@test.com",
            "ParentA",
            "Parent A");

        // Exchange magic token for session
        var request = new MagicTokenRequest { Token = magicToken!.Token };
        var response = await _client.PostAsJsonAsync("/api/auth/magic", request);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    #endregion
}
