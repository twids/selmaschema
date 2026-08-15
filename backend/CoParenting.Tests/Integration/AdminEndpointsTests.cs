using System.Net;
using System.Net.Http.Json;
using CoParenting.Application.DTOs;
using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using CoParenting.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CoParenting.Tests.Integration;

public class AdminEndpointsTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory = new($"InvitationTests_{Guid.NewGuid()}");

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Admin_CanCreateAdminInvitation_WithoutPersistingRawToken()
    {
        using var client = await CreateClientForRoleAsync("Admin", "admin@test.se");

        var response = await client.PostAsJsonAsync(
            "/api/invitations",
            new CreateInvitationRequest { Role = "Admin", EmailHint = "hint@test.se" });
        var created = await response.Content.ReadFromJsonAsync<CreatedInvitationDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        created!.Invitation.Role.Should().Be("Admin");
        created.Invitation.EmailHint.Should().Be("hint@test.se");
        created.InvitationUrl.Should().StartWith("https://localhost/api/auth/invitations/");

        var rawToken = created.InvitationUrl.Split('/').Last();
        using var scope = _factory.Services.CreateScope();
        var stored = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>().Invitations.Single();
        stored.TokenHash.Should().Be(TokenService.HashToken(rawToken));
        stored.TokenHash.Should().NotContain(rawToken);
    }

    [Fact]
    public async Task Parent_CanCreateParentInvitation_ButNotAdminInvitation()
    {
        using var client = await CreateClientForRoleAsync("ParentA", "parent@test.se");

        var parentResponse = await client.PostAsJsonAsync(
            "/api/invitations",
            new CreateInvitationRequest { Role = "ParentB" });
        var adminResponse = await client.PostAsJsonAsync(
            "/api/invitations",
            new CreateInvitationRequest { Role = "Admin" });

        parentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        adminResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UsersSeeOwnInvitations_WhileAdminSeesAll()
    {
        using var first = await CreateClientForRoleAsync("ParentA", "first@test.se");
        using var second = await CreateClientForRoleAsync("ParentB", "second@test.se");
        using var admin = await CreateClientForRoleAsync("Admin", "admin@test.se");
        await first.PostAsJsonAsync("/api/invitations", new CreateInvitationRequest { Role = "ParentA" });
        await second.PostAsJsonAsync("/api/invitations", new CreateInvitationRequest { Role = "ParentB" });

        var own = await first.GetFromJsonAsync<List<InvitationDto>>("/api/invitations");
        var all = await admin.GetFromJsonAsync<List<InvitationDto>>("/api/invitations");

        own.Should().ContainSingle();
        own![0].CreatedByName.Should().Be("first@test.se");
        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task AdminUsersEndpoint_RemainsAdminOnly()
    {
        using var parent = await CreateClientForRoleAsync("ParentA", "parent@test.se");
        using var admin = await CreateClientForRoleAsync("Admin", "admin@test.se");

        (await parent.GetAsync("/api/admin/users")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await admin.GetAsync("/api/admin/users")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<HttpClient> CreateClientForRoleAsync(string role, string email)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
        var user = new User
        {
            Email = email,
            Role = role,
            DisplayName = email,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var raw = TokenService.GenerateToken();
        context.Sessions.Add(new Session
        {
            UserId = user.Id,
            TokenHash = TokenService.HashToken(raw),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        });
        await context.SaveChangesAsync();
        return _factory.CreateAuthenticatedClient(raw);
    }
}
