using CoParenting.Application.Interfaces;
using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoParenting.Tests.Unit.Services;

public class AuthServiceTests : IDisposable
{
    private readonly CoParentingDbContext _context;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _context = CreateContext(Guid.NewGuid().ToString());
        _service = CreateService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Tokens_AreUrlSafe_AndHashIsOneWay()
    {
        var raw = TokenService.GenerateToken();
        var hash = TokenService.HashToken(raw);

        raw.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        hash.Should().HaveLength(64);
        hash.Should().NotBe(raw);
        TokenService.HashToken(raw).Should().Be(hash);
    }

    [Fact]
    public async Task LocalAdminLogin_StoresOnlyHashedSession_ForThirtyDays()
    {
        var result = await _service.ValidateAdminPasswordAsync("admin123");

        result.Success.Should().BeTrue();
        result.RawToken.Should().NotBeNullOrWhiteSpace();
        result.User!.IsLocalAdmin.Should().BeTrue();
        result.Session!.TokenHash.Should().Be(TokenService.HashToken(result.RawToken!));
        result.Session.TokenHash.Should().NotBe(result.RawToken);
        result.Session.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("ParentA")]
    [InlineData("ParentB")]
    public async Task Parent_CanCreateParentInvitations(string invitedRole)
    {
        var creator = await AddUserAsync("creator@test.se", "ParentA");

        var result = await _service.CreateInvitationAsync(creator.Id, creator.Role, invitedRole, null);

        result.Success.Should().BeTrue();
        result.RawToken.Should().NotBeNullOrWhiteSpace();
        result.Invitation!.TokenHash.Should().Be(TokenService.HashToken(result.RawToken!));
        result.Invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Parent_CannotCreateAdminInvitation()
    {
        var creator = await AddUserAsync("creator@test.se", "ParentA");

        var result = await _service.CreateInvitationAsync(creator.Id, creator.Role, "Admin", null);

        result.Success.Should().BeFalse();
        _context.Invitations.Should().BeEmpty();
    }

    [Fact]
    public async Task DifferentVerifiedEmailThanHint_IsAllowedAfterConfirmation()
    {
        var creator = await AddUserAsync("admin@test.se", "Admin");
        var created = await _service.CreateInvitationAsync(creator.Id, creator.Role, "ParentA", "hint@test.se");

        var result = await _service.CompleteInvitationAsync(
            created.Invitation!.Id,
            Login("actual@test.se", "subject-1"));

        result.Success.Should().BeTrue();
        result.User!.Email.Should().Be("actual@test.se");
        result.User.Role.Should().Be("ParentA");
        created.Invitation.EmailHint.Should().Be("hint@test.se");
    }

    [Fact]
    public async Task ExactVerifiedEmail_LinksExistingAccount_AndPreservesItsRole()
    {
        var creator = await AddUserAsync("admin@test.se", "Admin");
        var existing = await AddUserAsync("existing@test.se", "ParentB");
        var created = await _service.CreateInvitationAsync(creator.Id, creator.Role, "ParentA", null);

        var result = await _service.CompleteInvitationAsync(
            created.Invitation!.Id,
            Login(existing.Email, "subject-existing"));

        result.Success.Should().BeTrue();
        result.User!.Id.Should().Be(existing.Id);
        result.User.Role.Should().Be("ParentB");
        (await _context.ExternalIdentities.SingleAsync()).UserId.Should().Be(existing.Id);
    }

    [Fact]
    public async Task MissingOrUnverifiedEmail_DoesNotConsumeInvitation()
    {
        var creator = await AddUserAsync("admin@test.se", "Admin");
        var created = await _service.CreateInvitationAsync(creator.Id, creator.Role, "ParentA", null);

        var result = await _service.CompleteInvitationAsync(
            created.Invitation!.Id,
            new ExternalLoginInfo("https://id.test", "sub", "user@test.se", "User", false));

        result.Success.Should().BeFalse();
        (await _context.Invitations.FindAsync(created.Invitation.Id))!.ConsumedAt.Should().BeNull();
    }

    [Fact]
    public async Task LinkedIdentity_CannotUseInvitationToChangeRole()
    {
        var creator = await AddUserAsync("admin@test.se", "Admin");
        var user = await AddUserAsync("parent@test.se", "ParentB");
        _context.ExternalIdentities.Add(new ExternalIdentity
        {
            UserId = user.Id,
            Issuer = "https://id.test",
            Subject = "linked-subject",
            NormalizedIssuer = "https://id.test",
            NormalizedSubject = "linked-subject",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        var created = await _service.CreateInvitationAsync(creator.Id, creator.Role, "ParentA", null);

        var result = await _service.CompleteInvitationAsync(
            created.Invitation!.Id,
            Login(user.Email, "linked-subject"));

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("change role");
        user.Role.Should().Be("ParentB");
        (await _context.Invitations.FindAsync(created.Invitation.Id))!.ConsumedAt.Should().BeNull();
    }

    [Fact]
    public async Task NormalOidcLogin_RequiresLinkedIdentityOrExactExistingEmail()
    {
        var denied = await _service.SignInExternalAsync(Login("unknown@test.se", "unknown"));
        var existing = await AddUserAsync("known@test.se", "ParentA");
        var linked = await _service.SignInExternalAsync(Login(existing.Email, "known"));

        denied.Success.Should().BeFalse();
        denied.Error.Should().Be("Invitation required");
        linked.Success.Should().BeTrue();
        linked.User!.Id.Should().Be(existing.Id);
    }

    [Fact]
    public async Task Invitation_IsSingleUse()
    {
        var creator = await AddUserAsync("admin@test.se", "Admin");
        var created = await _service.CreateInvitationAsync(creator.Id, creator.Role, "ParentA", null);

        var first = await _service.CompleteInvitationAsync(created.Invitation!.Id, Login("one@test.se", "one"));
        var second = await _service.CompleteInvitationAsync(created.Invitation.Id, Login("two@test.se", "two"));

        first.Success.Should().BeTrue();
        second.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ExpiredInvitation_IsRejected()
    {
        var creator = await AddUserAsync("admin@test.se", "Admin");
        var invitation = new Invitation
        {
            TokenHash = TokenService.HashToken("expired"),
            Role = "ParentA",
            CreatedByUserId = creator.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            ConcurrencyToken = Guid.NewGuid()
        };
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        var result = await _service.CompleteInvitationAsync(invitation.Id, Login("user@test.se", "expired"));

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrentRedemption_AllowsExactlyOneCompletion()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var setup = CreateContext(databaseName);
        var creator = new User
        {
            Email = "admin@test.se",
            Role = "Admin",
            DisplayName = "Admin",
            CreatedAt = DateTime.UtcNow
        };
        setup.Users.Add(creator);
        await setup.SaveChangesAsync();
        var setupService = CreateService(setup);
        var created = await setupService.CreateInvitationAsync(creator.Id, creator.Role, "ParentA", null);

        await using var firstContext = CreateContext(databaseName);
        await using var secondContext = CreateContext(databaseName);
        var tasks = new[]
        {
            CreateService(firstContext).CompleteInvitationAsync(created.Invitation!.Id, Login("one@test.se", "one")),
            CreateService(secondContext).CompleteInvitationAsync(created.Invitation.Id, Login("two@test.se", "two"))
        };

        var results = await Task.WhenAll(tasks);
        results.Count(r => r.Success).Should().Be(1);
    }

    private async Task<User> AddUserAsync(string email, string role)
    {
        var user = new User
        {
            Email = email,
            Role = role,
            DisplayName = email,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private static ExternalLoginInfo Login(string email, string subject) =>
        new("https://id.test", subject, email, email, true);

    private static CoParentingDbContext CreateContext(string databaseName)
    {
        return new CoParentingDbContext(new DbContextOptionsBuilder<CoParentingDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options);
    }

    private static AuthService CreateService(CoParentingDbContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:LocalAdmin:Enabled"] = "true",
                ["Auth:LocalAdmin:PasswordHash"] = BCrypt.Net.BCrypt.HashPassword("admin123")
            })
            .Build();
        return new AuthService(context, configuration, NullLogger<AuthService>.Instance);
    }
}
