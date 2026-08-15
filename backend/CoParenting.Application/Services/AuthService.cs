using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoParenting.Application.Services;

public class AuthService : IAuthService
{
    private static readonly HashSet<string> ParentRoles = new(StringComparer.Ordinal)
    {
        "ParentA",
        "ParentB"
    };

    private readonly CoParentingDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        CoParentingDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SessionResult> ValidateAdminPasswordAsync(string password)
    {
        if (!_configuration.GetValue<bool>("Auth:LocalAdmin:Enabled"))
        {
            return FailedSession("Local admin login is disabled");
        }

        var adminPasswordHash = _configuration["Auth:LocalAdmin:PasswordHash"];
        if (string.IsNullOrWhiteSpace(adminPasswordHash))
        {
            _logger.LogError("Local admin password hash is not configured");
            return FailedSession("Local admin login is not configured");
        }

        bool isValid;
        try
        {
            isValid = BCrypt.Net.BCrypt.Verify(password, adminPasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Local admin password hash is invalid");
            return FailedSession("Local admin login is not configured");
        }

        if (!isValid)
        {
            _logger.LogWarning("Failed local admin login attempt");
            return FailedSession("Invalid credentials");
        }

        var now = DateTime.UtcNow;
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.IsLocalAdmin);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Email = "local-admin@selma.invalid",
                Role = "Admin",
                DisplayName = "Lokal reservadmin",
                CreatedAt = now,
                IsLocalAdmin = true
            };
            _context.Users.Add(adminUser);
        }

        adminUser.LastLoginAt = now;
        var result = CreateSession(adminUser, now);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<SessionResult> SignInExternalAsync(ExternalLoginInfo login)
    {
        var validationError = ValidateExternalLogin(login);
        if (validationError != null)
        {
            return FailedSession(validationError);
        }

        await using var transaction = await BeginTransactionIfSupportedAsync();
        try
        {
            var now = DateTime.UtcNow;
            var normalizedIssuer = NormalizeIssuer(login.Issuer);
            var normalizedSubject = NormalizeSubject(login.Subject);
            var identity = await _context.ExternalIdentities
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.NormalizedIssuer == normalizedIssuer &&
                                          e.NormalizedSubject == normalizedSubject);

            User? user;
            if (identity != null)
            {
                user = identity.User;
                identity.LastLoginAt = now;
            }
            else
            {
                user = await _context.Users.FirstOrDefaultAsync(u =>
                    !u.IsLocalAdmin && u.Email == login.Email);
                if (user == null)
                {
                    return FailedSession("Invitation required");
                }

                _context.ExternalIdentities.Add(CreateExternalIdentity(user, login, now));
            }

            user.LastLoginAt = now;
            var result = CreateSession(user, now);
            await _context.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return result;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Concurrent external identity link was rejected");
            return FailedSession("Identity could not be linked");
        }
    }

    public async Task<SessionResult> CompleteInvitationAsync(int invitationId, ExternalLoginInfo login)
    {
        var validationError = ValidateExternalLogin(login);
        if (validationError != null)
        {
            return FailedSession(validationError);
        }

        await using var transaction = await BeginTransactionIfSupportedAsync();
        try
        {
            var now = DateTime.UtcNow;
            var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.Id == invitationId);
            if (invitation == null || invitation.ConsumedAt != null || invitation.ExpiresAt <= now)
            {
                return FailedSession("Invitation is invalid, expired, or already used");
            }

            var normalizedIssuer = NormalizeIssuer(login.Issuer);
            var normalizedSubject = NormalizeSubject(login.Subject);
            var identity = await _context.ExternalIdentities
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.NormalizedIssuer == normalizedIssuer &&
                                          e.NormalizedSubject == normalizedSubject);

            User user;
            if (identity != null)
            {
                user = identity.User;
                if (!string.Equals(user.Role, invitation.Role, StringComparison.Ordinal))
                {
                    return FailedSession("A linked identity cannot use an invitation to change role");
                }

                identity.LastLoginAt = now;
            }
            else
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
                    !u.IsLocalAdmin && u.Email == login.Email);
                if (existingUser != null)
                {
                    user = existingUser;
                }
                else
                {
                    user = new User
                    {
                        Email = login.Email,
                        Role = invitation.Role,
                        DisplayName = string.IsNullOrWhiteSpace(login.DisplayName) ? login.Email : login.DisplayName,
                        CreatedAt = now
                    };
                    _context.Users.Add(user);
                }

                _context.ExternalIdentities.Add(CreateExternalIdentity(user, login, now));
            }

            user.LastLoginAt = now;
            invitation.ConsumedAt = now;
            invitation.RedeemedByUser = user;
            invitation.ConcurrencyToken = Guid.NewGuid();
            var result = CreateSession(user, now);

            await _context.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrent invitation redemption was rejected");
            return FailedSession("Invitation was already used");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Invitation redemption conflicted with an existing identity");
            return FailedSession("Identity could not be linked");
        }
    }

    public async Task<(bool Success, User? User)> ValidateSessionAsync(string rawSessionToken)
    {
        if (string.IsNullOrWhiteSpace(rawSessionToken))
        {
            return (false, null);
        }

        var hash = TokenService.HashToken(rawSessionToken);
        var session = await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.TokenHash == hash && s.IsActive);

        if (session == null)
        {
            return (false, null);
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync();
            return (false, null);
        }

        return (true, session.User);
    }

    public async Task<bool> InvalidateSessionAsync(string rawSessionToken)
    {
        if (string.IsNullOrWhiteSpace(rawSessionToken))
        {
            return false;
        }

        var hash = TokenService.HashToken(rawSessionToken);
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.TokenHash == hash && s.IsActive);
        if (session == null)
        {
            return false;
        }

        session.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<InvitationCreationResult> CreateInvitationAsync(
        int creatorUserId,
        string creatorRole,
        string role,
        string? emailHint)
    {
        if (!ParentRoles.Contains(role) && !(role == "Admin" && creatorRole == "Admin"))
        {
            return new InvitationCreationResult(false, null, null, "Role is not allowed");
        }

        emailHint = string.IsNullOrWhiteSpace(emailHint) ? null : emailHint.Trim();
        var rawToken = TokenService.GenerateToken();
        var now = DateTime.UtcNow;
        var invitation = new Invitation
        {
            TokenHash = TokenService.HashToken(rawToken),
            EmailHint = emailHint,
            Role = role,
            CreatedByUserId = creatorUserId,
            CreatedAt = now,
            ExpiresAt = now.AddHours(24),
            ConcurrencyToken = Guid.NewGuid()
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();
        return new InvitationCreationResult(true, rawToken, invitation, null);
    }

    public async Task<Invitation?> GetValidInvitationByTokenAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return null;
        }

        var hash = TokenService.HashToken(rawToken);
        return await _context.Invitations.AsNoTracking().FirstOrDefaultAsync(i =>
            i.TokenHash == hash && i.ConsumedAt == null && i.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<Invitation?> GetValidInvitationByIdAsync(int invitationId)
    {
        return await _context.Invitations.AsNoTracking().FirstOrDefaultAsync(i =>
            i.Id == invitationId && i.ConsumedAt == null && i.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<List<Invitation>> GetInvitationsAsync(int requesterUserId, bool isAdmin)
    {
        return await _context.Invitations
            .AsNoTracking()
            .Include(i => i.CreatedByUser)
            .Where(i => isAdmin || i.CreatedByUserId == requesterUserId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Email)
            .ToListAsync();
    }

    private SessionResult CreateSession(User user, DateTime now)
    {
        var rawToken = TokenService.GenerateToken();
        var session = new Session
        {
            TokenHash = TokenService.HashToken(rawToken),
            User = user,
            CreatedAt = now,
            ExpiresAt = now.AddDays(30),
            IsActive = true
        };
        _context.Sessions.Add(session);
        return new SessionResult(true, rawToken, session, user, null);
    }

    private static ExternalIdentity CreateExternalIdentity(User user, ExternalLoginInfo login, DateTime now)
    {
        return new ExternalIdentity
        {
            User = user,
            Issuer = login.Issuer,
            Subject = login.Subject,
            NormalizedIssuer = NormalizeIssuer(login.Issuer),
            NormalizedSubject = NormalizeSubject(login.Subject),
            CreatedAt = now,
            LastLoginAt = now
        };
    }

    private static string? ValidateExternalLogin(ExternalLoginInfo login)
    {
        if (string.IsNullOrWhiteSpace(login.Issuer) || string.IsNullOrWhiteSpace(login.Subject))
        {
            return "OIDC identity is incomplete";
        }

        if (!login.EmailVerified || string.IsNullOrWhiteSpace(login.Email))
        {
            return "A verified email address is required";
        }

        return null;
    }

    private static string NormalizeIssuer(string issuer)
    {
        var trimmed = issuer.Trim().TrimEnd('/');
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return trimmed;
        }

        var builder = new UriBuilder(uri)
        {
            Scheme = uri.Scheme.ToLowerInvariant(),
            Host = uri.Host.ToLowerInvariant()
        };
        return builder.Uri.AbsoluteUri.TrimEnd('/');
    }

    private static string NormalizeSubject(string subject) => subject.Trim();

    private static SessionResult FailedSession(string error) => new(false, null, null, null, error);

    private async Task<IDbContextTransaction?> BeginTransactionIfSupportedAsync()
    {
        return _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync()
            : null;
    }
}
