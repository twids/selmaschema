using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using CoParenting.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoParenting.Tests.Integration;

public class AuthEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests()
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
    public async Task AdminLogin_WithValidPassword_ShouldReturnSessionToken()
    {
        // Arrange
        var request = new AdminLoginRequest { Password = "admin123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Token.Should().MatchRegex("^[A-Za-z0-9_-]+$", "should be Base64URL encoded");
        authResponse.User.Should().NotBeNull();
        authResponse.User.Role.Should().Be("Admin");
        authResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(29));
    }

    [Fact]
    public async Task AdminLogin_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange
        var request = new AdminLoginRequest { Password = "wrong-password" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/admin/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ShouldReturnUserFromClaims()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Role.Should().Be("Admin");
        user.Email.Should().Be("admin@coparenting.local");
    }

    [Fact]
    public async Task GetMe_WithoutAuthHeader_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithInvalidToken_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithValidToken_ShouldInvalidateSession()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Logout
        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);

        // Assert - Logout succeeds
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Try to use same token for /me
        var meResponse = await _client.GetAsync("/api/auth/me");

        // Assert - Token no longer valid
        meResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExchangeMagicToken_WithValidToken_ShouldReturnSession()
    {
        // Arrange
        var magicToken = await CreateMagicLinkForParentAAsync();
        var request = new MagicTokenRequest { Token = magicToken };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/magic", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Token.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        authResponse.User.Role.Should().Be("ParentA");
        authResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(89));
    }

    [Fact]
    public async Task ExchangeMagicToken_WhenUsedTwice_ShouldReturn400()
    {
        // Arrange
        var magicToken = await CreateMagicLinkForParentAAsync();
        var request = new MagicTokenRequest { Token = magicToken };
        await _client.PostAsJsonAsync("/api/auth/magic", request); // Use it once

        // Act - Try to use it again
        var response = await _client.PostAsJsonAsync("/api/auth/magic", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExchangeMagicToken_WithExpiredToken_ShouldReturn400()
    {
        // Arrange - Create expired token directly in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();

        var user = new User
        {
            Email = "expired@test.com",
            Role = "ParentA",
            DisplayName = "Expired User",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var expiredToken = new MagicLinkToken
        {
            Token = "expired-token-123",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
            IsUsed = false
        };
        dbContext.MagicLinkTokens.Add(expiredToken);
        await dbContext.SaveChangesAsync();

        var request = new MagicTokenRequest { Token = "expired-token-123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/magic", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #region Helper Methods

    private async Task<string> LoginAsAdminAsync()
    {
        var request = new AdminLoginRequest { Password = "admin123" };
        var response = await _client.PostAsJsonAsync("/api/auth/admin/login", request);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    private async Task<string> CreateMagicLinkForParentAAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var (success, magicToken) = await authService.CreateMagicLinkAsync(
            "parenta@test.com",
            "ParentA",
            "Parent A Test");
        success.Should().BeTrue();
        return magicToken!.Token;
    }

    #endregion
}
