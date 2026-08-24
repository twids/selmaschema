using CoParenting.Application.Interfaces;
using CoParenting.Application.Services;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoParenting.Tests.Unit;

public sealed class AuthServiceV2Tests
{
    [Fact]
    public async Task Verified_oidc_identity_creates_account_without_invitation()
    {
        await using var db = Database();
        var service = Service(db);
        var result = await service.SignInExternalAsync(new ExternalLoginInfo(
            "https://id.widsell.nu/application/o/selma/", "subject-1", "new@example.test", "Ny person", true));
        result.Success.Should().BeTrue();
        result.Account!.Email.Should().Be("new@example.test");
        result.Session!.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
        (await db.Accounts.CountAsync()).Should().Be(1);
        (await db.ExternalIdentities.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Missing_or_unverified_email_is_rejected()
    {
        await using var db = Database();
        var service = Service(db);
        (await service.SignInExternalAsync(new ExternalLoginInfo("issuer", "subject", "", "", true))).Success.Should().BeFalse();
        (await service.SignInExternalAsync(new ExternalLoginInfo("issuer", "subject", "x@example.test", "X", false))).Success.Should().BeFalse();
        (await db.Accounts.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Admin_oidc_requires_group_and_creates_only_admin_session()
    {
        await using var db = Database();
        var service = Service(db);
        var rejected = await service.SignInPlatformAdminAsync(new ExternalLoginInfo("issuer", "subject", "admin@example.test", "Admin", true, []));
        rejected.Success.Should().BeFalse();
        var accepted = await service.SignInPlatformAdminAsync(new ExternalLoginInfo("issuer", "subject", "admin@example.test", "Admin", true, ["selma-platform-admins"]));
        accepted.Success.Should().BeTrue();
        (await db.Accounts.CountAsync()).Should().Be(0);
        (await db.PlatformAdminSessions.CountAsync()).Should().Be(1);
        (await db.AccountSessions.CountAsync()).Should().Be(0);
    }

    private static CoParentingDbContext Database() => new(new DbContextOptionsBuilder<CoParentingDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AuthService Service(CoParentingDbContext db) => new(
        db,
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AdminOidc:RequiredGroup"] = "selma-platform-admins"
        }).Build(),
        NullLogger<AuthService>.Instance);
}
