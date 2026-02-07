using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoParenting.Tests.Unit.Services;

public class AuthServiceTests : IDisposable
{
    private readonly CoParentingDbContext _context;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<CoParentingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CoParentingDbContext(options);

        // Setup mocks
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Create service
        _authService = new AuthService(_context, _configurationMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateMagicLinkAsync_ShouldGenerateBase64UrlToken_WithoutPlusSlashEquals()
    {
        // Arrange
        var email = "parent@test.com";
        var role = "ParentA";
        var displayName = "Test Parent";

        // Act
        var (success, token) = await _authService.CreateMagicLinkAsync(email, role, displayName);

        // Assert
        success.Should().BeTrue();
        token.Should().NotBeNull();
        token!.Token.Should().NotContain("+", "Base64URL should replace + with -");
        token.Token.Should().NotContain("/", "Base64URL should replace / with _");
        token.Token.Should().NotContain("=", "Base64URL should remove padding");
        token.Token.Should().MatchRegex("^[A-Za-z0-9_-]+$", "Base64URL should only contain URL-safe characters");
    }

    [Fact]
    public async Task CreateMagicLinkAsync_ShouldCreateTokenWithCorrectExpiry()
    {
        // Arrange
        var email = "parent@test.com";
        var role = "ParentB";
        var displayName = "Test Parent B";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var (success, token) = await _authService.CreateMagicLinkAsync(email, role, displayName);

        // Assert
        success.Should().BeTrue();
        token.Should().NotBeNull();
        token!.ExpiresAt.Should().BeCloseTo(beforeCreation.AddHours(24), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ValidateAdminPasswordAsync_AdminSession_ShouldUseBase64UrlToken()
    {
        // Arrange
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        _configurationMock.Setup(c => c["Auth:AdminPasswordHash"]).Returns(adminPasswordHash);

        // Act
        var (success, session, user) = await _authService.ValidateAdminPasswordAsync("admin123");

        // Assert
        success.Should().BeTrue();
        session.Should().NotBeNull();
        session!.Token.Should().NotContain("+");
        session.Token.Should().NotContain("/");
        session.Token.Should().NotContain("=");
        session.Token.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public async Task ValidateAdminPasswordAsync_ShouldCreateSessionWith30DayExpiry()
    {
        // Arrange
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        _configurationMock.Setup(c => c["Auth:AdminPasswordHash"]).Returns(adminPasswordHash);
        var beforeCreation = DateTime.UtcNow;

        // Act
        var (success, session, user) = await _authService.ValidateAdminPasswordAsync("admin123");

        // Assert
        success.Should().BeTrue();
        session.Should().NotBeNull();
        session!.ExpiresAt.Should().BeCloseTo(beforeCreation.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExchangeMagicTokenAsync_ShouldCreateSessionWith90DayExpiry()
    {
        // Arrange
        // First create a magic link
        var (createSuccess, magicToken) = await _authService.CreateMagicLinkAsync("parent@test.com", "ParentA", "Test Parent");
        createSuccess.Should().BeTrue();
        magicToken.Should().NotBeNull();
        var beforeExchange = DateTime.UtcNow;

        // Act
        var (success, session, user, error) = await _authService.ExchangeMagicTokenAsync(magicToken!.Token);

        // Assert
        success.Should().BeTrue();
        session.Should().NotBeNull();
        session!.ExpiresAt.Should().BeCloseTo(beforeExchange.AddDays(90), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExchangeMagicTokenAsync_Session_ShouldUseBase64UrlToken()
    {
        // Arrange
        var (createSuccess, magicToken) = await _authService.CreateMagicLinkAsync("parent@test.com", "ParentA", "Test Parent");
        createSuccess.Should().BeTrue();

        // Act
        var (success, session, user, error) = await _authService.ExchangeMagicTokenAsync(magicToken!.Token);

        // Assert
        success.Should().BeTrue();
        session.Should().NotBeNull();
        session!.Token.Should().NotContain("+");
        session!.Token.Should().NotContain("/");
        session!.Token.Should().NotContain("=");
        session.Token.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public async Task ExchangeMagicTokenAsync_ShouldReturnError_WhenTokenAlreadyUsed()
    {
        // Arrange
        var (createSuccess, magicToken) = await _authService.CreateMagicLinkAsync("parent@test.com", "ParentA", "Test Parent");
        await _authService.ExchangeMagicTokenAsync(magicToken!.Token); // Use it once

        // Act
        var (success, session, user, error) = await _authService.ExchangeMagicTokenAsync(magicToken.Token);

        // Assert
        success.Should().BeFalse();
        error.Should().Be("Token already used");
    }

    [Fact]
    public async Task ExchangeMagicTokenAsync_ShouldReturnError_WhenTokenExpired()
    {
        // Arrange
        var user = new User
        {
            Email = "parent@test.com",
            Role = "ParentA",
            DisplayName = "Test Parent",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var expiredToken = new MagicLinkToken
        {
            Token = "expired-token",
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow.AddHours(-25),
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired 1 hour ago
            IsUsed = false
        };
        _context.MagicLinkTokens.Add(expiredToken);
        await _context.SaveChangesAsync();

        // Act
        var (success, session, _, error) = await _authService.ExchangeMagicTokenAsync(expiredToken.Token);

        // Assert
        success.Should().BeFalse();
        error.Should().Be("Token expired");
    }

    [Fact]
    public async Task ValidateAdminPasswordAsync_ShouldReturnFalse_WhenPasswordIncorrect()
    {
        // Arrange
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        _configurationMock.Setup(c => c["Auth:AdminPasswordHash"]).Returns(adminPasswordHash);

        // Act
        var (success, session, user) = await _authService.ValidateAdminPasswordAsync("wrong-password");

        // Assert
        success.Should().BeFalse();
        session.Should().BeNull();
        user.Should().BeNull();
    }

    [Fact]
    public async Task CreateMagicLinkAsync_ShouldReturnFalse_WhenRoleInvalid()
    {
        // Arrange
        var email = "parent@test.com";
        var invalidRole = "InvalidRole";
        var displayName = "Test Parent";

        // Act
        var (success, token) = await _authService.CreateMagicLinkAsync(email, invalidRole, displayName);

        // Assert
        success.Should().BeFalse();
        token.Should().BeNull();
    }

    [Fact]
    public async Task ValidateSessionAsync_ShouldReturnTrue_WhenSessionValid()
    {
        // Arrange
        var user = new User
        {
            Email = "parent@test.com",
            Role = "ParentA",
            DisplayName = "Test Parent",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var session = new Session
        {
            Token = "valid-session-token",
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var (success, returnedUser) = await _authService.ValidateSessionAsync("valid-session-token");

        // Assert
        success.Should().BeTrue();
        returnedUser.Should().NotBeNull();
        returnedUser!.Email.Should().Be("parent@test.com");
        returnedUser.Role.Should().Be("ParentA");
    }

    [Fact]
    public async Task ValidateSessionAsync_ShouldReturnFalse_WhenSessionExpired()
    {
        // Arrange
        var user = new User
        {
            Email = "parent@test.com",
            Role = "ParentA",
            DisplayName = "Test Parent",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var expiredSession = new Session
        {
            Token = "expired-session-token",
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow.AddDays(-31),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };
        _context.Sessions.Add(expiredSession);
        await _context.SaveChangesAsync();

        // Act
        var (success, returnedUser) = await _authService.ValidateSessionAsync("expired-session-token");

        // Assert
        success.Should().BeFalse();
        returnedUser.Should().BeNull();

        // Verify session was marked as inactive
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Token == "expired-session-token");
        session!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task InvalidateSessionAsync_ShouldMarkSessionInactive()
    {
        // Arrange
        var user = new User
        {
            Email = "parent@test.com",
            Role = "ParentA",
            DisplayName = "Test Parent",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var session = new Session
        {
            Token = "active-session-token",
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var success = await _authService.InvalidateSessionAsync("active-session-token");

        // Assert
        success.Should().BeTrue();
        var updatedSession = await _context.Sessions.FirstOrDefaultAsync(s => s.Token == "active-session-token");
        updatedSession!.IsActive.Should().BeFalse();
    }
}
