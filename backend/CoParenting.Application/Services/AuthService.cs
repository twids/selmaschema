using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoParenting.Application.Services;

public sealed class AuthService(
    CoParentingDbContext db,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AccountSessionResult> SignInExternalAsync(ExternalLoginInfo login)
    {
        var error = ValidateExternalLogin(login);
        if (error != null)
        {
            return FailedAccount(error);
        }

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync()
            : null;

        try
        {
            var now = DateTime.UtcNow;
            var normalizedIssuer = NormalizeIssuer(login.Issuer);
            var normalizedSubject = login.Subject.Trim();
            var normalizedEmail = NormalizeEmail(login.Email);
            var identity = await db.ExternalIdentities
                .Include(x => x.Account)
                .SingleOrDefaultAsync(x => x.NormalizedIssuer == normalizedIssuer &&
                                           x.NormalizedSubject == normalizedSubject);

            Account account;
            if (identity != null)
            {
                account = identity.Account;
                identity.LastLoginAt = now;
            }
            else
            {
                account = await db.Accounts.SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail)
                    ?? new Account
                    {
                        Email = login.Email.Trim(),
                        NormalizedEmail = normalizedEmail,
                        DisplayName = DisplayName(login),
                        CreatedAt = now
                    };

                if (db.Entry(account).State == EntityState.Detached)
                {
                    db.Accounts.Add(account);
                }

                db.ExternalIdentities.Add(new ExternalIdentity
                {
                    Account = account,
                    Issuer = login.Issuer.Trim(),
                    Subject = login.Subject.Trim(),
                    NormalizedIssuer = normalizedIssuer,
                    NormalizedSubject = normalizedSubject,
                    CreatedAt = now,
                    LastLoginAt = now
                });
            }

            if (account.IsDisabled)
            {
                return FailedAccount("Account is disabled");
            }

            account.Email = login.Email.Trim();
            account.NormalizedEmail = normalizedEmail;
            account.DisplayName = DisplayName(login);
            account.LastLoginAt = now;
            var result = CreateAccountSession(account, now);
            await db.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return result;
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "OIDC account linking conflicted with another request");
            return FailedAccount("Identity could not be linked");
        }
    }

    public async Task<AdminSessionResult> SignInPlatformAdminAsync(ExternalLoginInfo login)
    {
        var error = ValidateExternalLogin(login);
        if (error != null)
        {
            return FailedAdmin(error);
        }

        var requiredGroup = configuration["AdminOidc:RequiredGroup"] ?? "selma-platform-admins";
        if (login.Groups == null || !login.Groups.Contains(requiredGroup, StringComparer.Ordinal))
        {
            logger.LogWarning("Platform admin login rejected because required group is missing for {Email}", login.Email);
            return FailedAdmin("Platform administrator group is required");
        }

        var normalizedEmail = NormalizeEmail(login.Email);
        var normalizedIssuer = NormalizeIssuer(login.Issuer);
        var normalizedSubject = login.Subject.Trim();
        var accountId = await db.ExternalIdentities
            .Where(x => x.NormalizedIssuer == normalizedIssuer && x.NormalizedSubject == normalizedSubject)
            .Select(x => (Guid?)x.AccountId)
            .SingleOrDefaultAsync();
        accountId ??= await db.Accounts
            .Where(x => x.NormalizedEmail == normalizedEmail)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync();

        var now = DateTime.UtcNow;
        var result = CreateAdminSession(
            accountId,
            login.Subject.Trim(),
            login.Email.Trim(),
            DisplayName(login),
            "oidc",
            now);
        db.PlatformAdminSessions.Add(result.Session!);
        await db.SaveChangesAsync();
        return result;
    }

    public async Task<AdminSessionResult> ValidateBreakGlassPasswordAsync(string password)
    {
        if (!configuration.GetValue<bool>("Auth:BreakGlass:Enabled"))
        {
            return FailedAdmin("Break-glass login is disabled");
        }

        var passwordHash = configuration["Auth:BreakGlass:PasswordHash"];
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            logger.LogError("Break-glass password hash is missing");
            return FailedAdmin("Break-glass login is not configured");
        }

        bool valid;
        try
        {
            valid = BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Break-glass password hash is invalid");
            return FailedAdmin("Break-glass login is not configured");
        }

        if (!valid)
        {
            logger.LogWarning("Invalid break-glass login attempt");
            return FailedAdmin("Invalid credentials");
        }

        var now = DateTime.UtcNow;
        var result = CreateAdminSession(
            null,
            "break-glass",
            "break-glass@selma.invalid",
            "Lokal reservadmin",
            "break-glass",
            now);
        db.PlatformAdminSessions.Add(result.Session!);
        db.AuditEvents.Add(new AuditEvent
        {
            ActorType = "BreakGlass",
            Action = "PlatformAdmin.BreakGlassLogin",
            TargetType = "Platform",
            TargetId = "selma",
            Reason = "Emergency access",
            CreatedAt = now
        });
        await db.SaveChangesAsync();
        return result;
    }

    public async Task<(bool Success, Account? Account, AccountSession? Session)> ValidateAccountSessionAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return (false, null, null);
        }

        var session = await db.AccountSessions.Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.TokenHash == TokenService.HashToken(rawToken));
        if (session == null || session.RevokedAt != null || session.ExpiresAt <= DateTime.UtcNow || session.Account.IsDisabled)
        {
            return (false, null, null);
        }

        return (true, session.Account, session);
    }

    public async Task<(bool Success, PlatformAdminSession? Session)> ValidateAdminSessionAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return (false, null);
        }

        var session = await db.PlatformAdminSessions
            .SingleOrDefaultAsync(x => x.TokenHash == TokenService.HashToken(rawToken));
        return session == null || session.RevokedAt != null || session.ExpiresAt <= DateTime.UtcNow
            ? (false, null)
            : (true, session);
    }

    public async Task<bool> ValidateAccountCsrfAsync(string rawToken, string rawCsrfToken)
    {
        var tokenHash = TokenService.HashToken(rawToken);
        var csrfHash = TokenService.HashToken(rawCsrfToken);
        return await db.AccountSessions.AnyAsync(x =>
            x.TokenHash == tokenHash && x.CsrfTokenHash == csrfHash &&
            x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<bool> ValidateAdminCsrfAsync(string rawToken, string rawCsrfToken)
    {
        var tokenHash = TokenService.HashToken(rawToken);
        var csrfHash = TokenService.HashToken(rawCsrfToken);
        return await db.PlatformAdminSessions.AnyAsync(x =>
            x.TokenHash == tokenHash && x.CsrfTokenHash == csrfHash &&
            x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task RevokeAccountSessionAsync(string rawToken)
    {
        var tokenHash = TokenService.HashToken(rawToken);
        var session = await db.AccountSessions.SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.RevokedAt == null);
        if (session != null)
        {
            session.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task RevokeAdminSessionAsync(string rawToken)
    {
        var tokenHash = TokenService.HashToken(rawToken);
        var session = await db.PlatformAdminSessions.SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.RevokedAt == null);
        if (session != null)
        {
            session.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    private AccountSessionResult CreateAccountSession(Account account, DateTime now)
    {
        var rawToken = TokenService.GenerateToken();
        var rawCsrf = TokenService.GenerateToken();
        var session = new AccountSession
        {
            Account = account,
            TokenHash = TokenService.HashToken(rawToken),
            CsrfTokenHash = TokenService.HashToken(rawCsrf),
            CreatedAt = now,
            ExpiresAt = now.AddDays(30)
        };
        db.AccountSessions.Add(session);
        return new AccountSessionResult(true, rawToken, rawCsrf, session, account, null);
    }

    private static AdminSessionResult CreateAdminSession(
        Guid? accountId,
        string subject,
        string email,
        string displayName,
        string method,
        DateTime now)
    {
        var rawToken = TokenService.GenerateToken();
        var rawCsrf = TokenService.GenerateToken();
        var session = new PlatformAdminSession
        {
            AccountId = accountId,
            Subject = subject,
            Email = email,
            DisplayName = displayName,
            AuthenticationMethod = method,
            TokenHash = TokenService.HashToken(rawToken),
            CsrfTokenHash = TokenService.HashToken(rawCsrf),
            CreatedAt = now,
            ExpiresAt = now.AddHours(1)
        };
        return new AdminSessionResult(true, rawToken, rawCsrf, session, null);
    }

    private static string? ValidateExternalLogin(ExternalLoginInfo login)
    {
        if (string.IsNullOrWhiteSpace(login.Issuer) || string.IsNullOrWhiteSpace(login.Subject))
        {
            return "OIDC identity is incomplete";
        }

        return !login.EmailVerified || string.IsNullOrWhiteSpace(login.Email)
            ? "A verified email address is required"
            : null;
    }

    private static string DisplayName(ExternalLoginInfo login) =>
        string.IsNullOrWhiteSpace(login.DisplayName) ? login.Email.Trim() : login.DisplayName.Trim();

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static string NormalizeIssuer(string issuer)
    {
        var trimmed = issuer.Trim().TrimEnd('/');
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return trimmed;
        }

        return new UriBuilder(uri)
        {
            Scheme = uri.Scheme.ToLowerInvariant(),
            Host = uri.Host.ToLowerInvariant()
        }.Uri.AbsoluteUri.TrimEnd('/');
    }

    private static AccountSessionResult FailedAccount(string error) => new(false, null, null, null, null, error);
    private static AdminSessionResult FailedAdmin(string error) => new(false, null, null, null, error);
}
