using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CoParenting.Application.DTOs;
using CoParenting.Tests.Fixtures;
using CoParenting.Application.Interfaces;
using CoParenting.Infrastructure.Data;
using CoParenting.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace CoParenting.Tests.Integration;

public class AuthEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory = new($"AuthTests_{Guid.NewGuid()}");
    private readonly HttpClient _client;

    public AuthEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task AdminLogin_SetsHardenedCookie_AndReturnsNoToken()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/admin/login",
            new AdminLoginRequest { Password = "admin123" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookie = response.Headers.GetValues("Set-Cookie").Single(v => v.StartsWith("__Host-selma-session="));
        var normalizedCookie = cookie.ToLowerInvariant();
        normalizedCookie.Should().Contain("secure");
        normalizedCookie.Should().Contain("httponly");
        normalizedCookie.Should().Contain("samesite=lax");
        normalizedCookie.Should().Contain("path=/");
        normalizedCookie.Should().Contain("expires=");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.TryGetProperty("token", out _).Should().BeFalse();
        document.RootElement.GetProperty("user").GetProperty("role").GetString().Should().Be("Admin");
    }

    [Fact]
    public async Task AdminLogin_WithInvalidPassword_IsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/admin/login",
            new AdminLoginRequest { Password = "wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.Contains("Set-Cookie").Should().BeFalse();
    }

    [Fact]
    public async Task Me_UsesSessionCookie()
    {
        await LoginAsAdminAsync();

        var response = await _client.GetAsync("/api/auth/me");
        var user = await response.Content.ReadFromJsonAsync<UserDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        user!.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task BearerToken_IsNotAccepted()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", "any-token");

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OidcLogin_UsesAuthorizationCodeWithPkce()
    {
        var response = await _client.GetAsync("/api/auth/login?returnUrl=%2Fchange-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = response.Headers.Location!.ToString();
        location.Should().StartWith("https://id.test/authorize?");
        location.Should().Contain("response_type=code");
        location.Should().Contain("code_challenge=");
        location.Should().Contain("code_challenge_method=S256");
        location.Should().Contain("scope=openid profile email");
    }

    [Fact]
    public async Task OidcCallback_WithInvalidState_ReturnsSwedishLoginErrorRoute()
    {
        var response = await _client.GetAsync("/signin-oidc?code=test&state=invalid");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/login?error=oidc_failed");
    }

    [Fact]
    public async Task OidcCallbackTicket_ShowsPendingIdentity_AndCompletesInvitation()
    {
        int invitationId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var creator = new User
            {
                Email = "creator@test.se",
                Role = "Admin",
                DisplayName = "Creator",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(creator);
            await context.SaveChangesAsync();
            var service = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var created = await service.CreateInvitationAsync(
                creator.Id,
                creator.Role,
                "ParentA",
                "hint@test.se");
            invitationId = created.Invitation!.Id;
        }

        using var callbackClient = _factory.CreateOidcCallbackClient(
            invitationId,
            "actual@test.se",
            "callback-subject");
        var pendingResponse = await callbackClient.GetAsync("/api/auth/invitations/pending");
        var pending = await pendingResponse.Content.ReadFromJsonAsync<PendingInvitationDto>();
        var completeResponse = await callbackClient.PostAsync("/api/auth/invitations/complete", null);

        pendingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        pending!.EmailHint.Should().Be("hint@test.se");
        pending.VerifiedEmail.Should().Be("actual@test.se");
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        completeResponse.Headers.GetValues("Set-Cookie").Should().Contain(v =>
            v.StartsWith("__Host-selma-session="));

        using var verifyScope = _factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
        db.Invitations.Single(i => i.Id == invitationId).ConsumedAt.Should().NotBeNull();
        db.Users.Single(u => u.Email == "actual@test.se").Role.Should().Be("ParentA");
    }

    [Fact]
    public async Task Logout_RevokesSessionAndClearsCookie()
    {
        await LoginAsAdminAsync();

        var logout = await _client.PostAsync("/api/auth/logout", null);
        var me = await _client.GetAsync("/api/auth/me");

        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);
        logout.Headers.GetValues("Set-Cookie").Should().Contain(v =>
            v.StartsWith("__Host-selma-session=") && v.Contains("expires=", StringComparison.OrdinalIgnoreCase));
        me.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData("/change-requests", "/change-requests")]
    [InlineData("https://evil.example", "/")]
    [InlineData("//evil.example", "/")]
    [InlineData("/\\evil.example", "/")]
    public void ReturnUrl_IsRestrictedToLocalRelativePaths(string? candidate, string expected)
    {
        CoParenting.API.Endpoints.AuthEndpoints.SafeReturnUrl(candidate).Should().Be(expected);
    }

    private async Task LoginAsAdminAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/admin/login",
            new AdminLoginRequest { Password = "admin123" });
        response.EnsureSuccessStatusCode();
    }
}
