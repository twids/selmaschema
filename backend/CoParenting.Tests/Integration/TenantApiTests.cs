using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoParenting.Application.DTOs;
using CoParenting.Tests.Fixtures;
using FluentAssertions;

namespace CoParenting.Tests.Integration;

public sealed class TenantApiTests(V2WebApplicationFactory factory) : IClassFixture<V2WebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Create_family_me_and_cross_tenant_access_are_isolated()
    {
        var first = await factory.AddAccountAsync("first-api@example.test");
        using var firstClient = factory.CreateSecureClient();
        V2WebApplicationFactory.Authenticate(firstClient, first.SessionToken, first.CsrfToken);
        var createdResponse = await firstClient.PostAsJsonAsync("/api/families", new CreateFamilyRequest("Första familjen"));
        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createdResponse.Content.ReadFromJsonAsync<FamilyDto>(JsonOptions);
        var me = await firstClient.GetFromJsonAsync<AuthMeDto>("/api/auth/me", JsonOptions);
        me!.Memberships.Should().ContainSingle(x => x.FamilyId == created!.Id && x.Permission == Core.Entities.FamilyPermission.Owner);

        var second = await factory.AddAccountAsync("second-api@example.test");
        using var secondClient = factory.CreateSecureClient();
        V2WebApplicationFactory.Authenticate(secondClient, second.SessionToken, second.CsrfToken);
        (await secondClient.GetAsync($"/api/families/{created!.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await secondClient.GetAsync($"/api/families/{created.Id}/members")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Authenticated_mutation_requires_session_bound_csrf_token()
    {
        var account = await factory.AddAccountAsync("csrf@example.test");
        using var client = factory.CreateSecureClient();
        client.DefaultRequestHeaders.Add("Cookie", $"__Host-selma-session={account.SessionToken}; __Host-selma-csrf={account.CsrfToken}");
        var rejected = await client.PostAsJsonAsync("/api/families", new CreateFamilyRequest("CSRF"));
        rejected.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        V2WebApplicationFactory.Authenticate(client, account.SessionToken, account.CsrfToken);
    }

    [Fact]
    public async Task Break_glass_sets_only_short_admin_cookie_with_security_attributes()
    {
        using var client = factory.CreateSecureClient();
        var response = await client.PostAsJsonAsync("/api/admin/auth/break-glass", new BreakGlassLoginRequest("correct horse battery staple"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(x => x.StartsWith("__Host-selma-admin-session=") && x.Contains("secure", StringComparison.OrdinalIgnoreCase) && x.Contains("httponly", StringComparison.OrdinalIgnoreCase) && x.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase) && x.Contains("path=/", StringComparison.OrdinalIgnoreCase));
        cookies.Should().Contain(x => x.StartsWith("__Host-selma-admin-csrf=") && !x.Contains("httponly", StringComparison.OrdinalIgnoreCase));
        cookies.Should().NotContain(x => x.StartsWith("__Host-selma-session="));
    }

    [Fact]
    public async Task Test_oidc_login_sets_thirty_day_account_cookie_and_logout_revokes_it()
    {
        using var client = factory.CreateSecureClient();
        client.DefaultRequestHeaders.Add("X-Selma-E2E-Secret", "integration-login-secret");
        var login = await client.PostAsJsonAsync("/api/auth/test-login",
            new EndToEndLoginRequest("cookie@example.test", "Cookie Test"));
        login.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var cookies = login.Headers.GetValues("Set-Cookie").ToList();
        var sessionCookie = cookies.Single(x => x.StartsWith("__Host-selma-session="));
        var csrfCookie = cookies.Single(x => x.StartsWith("__Host-selma-csrf="));
        sessionCookie.Should().ContainEquivalentOf("secure").And.ContainEquivalentOf("httponly")
            .And.ContainEquivalentOf("samesite=lax").And.ContainEquivalentOf("path=/");
        csrfCookie.Should().ContainEquivalentOf("samesite=strict").And.NotContainEquivalentOf("httponly");
        var expiresText = sessionCookie.Split(';').Single(x => x.TrimStart().StartsWith("expires=", StringComparison.OrdinalIgnoreCase)).Split('=', 2)[1];
        DateTimeOffset.Parse(expiresText).Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));

        var session = CookiePair(sessionCookie);
        var csrf = CookiePair(csrfCookie);
        client.DefaultRequestHeaders.Remove("X-Selma-E2E-Secret");
        client.DefaultRequestHeaders.Add("Cookie", $"{session}; {csrf}");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.Split('=', 2)[1]);
        (await client.PostAsync("/api/auth/logout", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync("/api/auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_cookie_authenticates_only_admin_api()
    {
        using var client = factory.CreateSecureClient();
        var login = await client.PostAsJsonAsync("/api/admin/auth/break-glass",
            new BreakGlassLoginRequest("correct horse battery staple"));
        var cookies = login.Headers.GetValues("Set-Cookie").ToList();
        client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies.Select(CookiePair)));
        (await client.GetAsync("/api/admin/families")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/families")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static string CookiePair(string setCookie) => setCookie.Split(';', 2)[0];
}
