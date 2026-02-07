using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CoParenting.API.Services;

public interface IAuthService
{
    Task<(bool Success, Session? Session, User? User)> ValidateAdminPasswordAsync(string password);
    Task<(bool Success, MagicLinkToken? Token)> CreateMagicLinkAsync(string email, string role, string displayName);
    Task<(bool Success, Session? Session, User? User, string? Error)> ExchangeMagicTokenAsync(string token);
    Task<(bool Success, User? User)> ValidateSessionAsync(string sessionToken);
    Task<bool> InvalidateSessionAsync(string sessionToken);
    Task<List<MagicLinkToken>> GetPendingMagicLinksAsync();
    Task<List<User>> GetAllUsersAsync();
}

public class AuthService : IAuthService
{
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

    public async Task<(bool Success, Session? Session, User? User)> ValidateAdminPasswordAsync(string password)
    {
        var adminPasswordHash = _configuration["Auth:AdminPasswordHash"];
        
        if (string.IsNullOrEmpty(adminPasswordHash))
        {
            _logger.LogError("Admin password hash not configured");
            return (false, null, null);
        }

        bool isValid;
        try
        {
            // Try BCrypt verification
            isValid = BCrypt.Net.BCrypt.Verify(password, adminPasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BCrypt verification failed, hash: {Hash}", adminPasswordHash);
            
            // Fallback for development: check if password matches plain text (insecure, dev only!)
            if (adminPasswordHash == password)
            {
                _logger.LogWarning("Using plain text password comparison (DEVELOPMENT ONLY)");
                isValid = true;
            }
            else
            {
                return (false, null, null);
            }
        }
        
        if (!isValid)
        {
            _logger.LogWarning("Failed admin login attempt");
            return (false, null, null);
        }

        // Find or create admin user
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Role == "Admin");

        if (adminUser == null)
        {
            adminUser = new User
            {
                Email = "admin@coparenting.local",
                Role = "Admin",
                DisplayName = "Administrator",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }

        // Update last login
        adminUser.LastLoginAt = DateTime.UtcNow;

        // Create admin session (30 days)
        var session = new Session
        {
            Token = GenerateSecureToken(),
            UserId = adminUser.Id,
            User = adminUser,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin logged in successfully");
        return (true, session, adminUser);
    }

    public async Task<(bool Success, MagicLinkToken? Token)> CreateMagicLinkAsync(
        string email, 
        string role, 
        string displayName)
    {
        if (role != "ParentA" && role != "ParentB")
        {
            _logger.LogWarning("Invalid role attempted: {Role}", role);
            return (false, null);
        }

        // Find or create user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null)
        {
            user = new User
            {
                Email = email,
                Role = role,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else if (user.Role != role)
        {
            // Update role if it changed
            user.Role = role;
            user.DisplayName = displayName;
            await _context.SaveChangesAsync();
        }

        // Create magic link token (24 hours expiry)
        var magicToken = new MagicLinkToken
        {
            Token = GenerateSecureToken(),
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        _context.MagicLinkTokens.Add(magicToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Magic link created for user {Email} with role {Role}", email, role);
        return (true, magicToken);
    }

    public async Task<(bool Success, Session? Session, User? User, string? Error)> ExchangeMagicTokenAsync(string token)
    {
        var magicToken = await _context.MagicLinkTokens
            .Include(mt => mt.User)
            .FirstOrDefaultAsync(mt => mt.Token == token);

        if (magicToken == null)
        {
            return (false, null, null, "Invalid token");
        }

        if (magicToken.IsUsed)
        {
            return (false, null, null, "Token already used");
        }

        if (magicToken.ExpiresAt < DateTime.UtcNow)
        {
            return (false, null, null, "Token expired");
        }

        // Mark token as used
        magicToken.IsUsed = true;
        magicToken.UsedAt = DateTime.UtcNow;

        // Update user last login
        magicToken.User.LastLoginAt = DateTime.UtcNow;

        // Create long-lived session (90 days)
        var session = new Session
        {
            Token = GenerateSecureToken(),
            UserId = magicToken.UserId,
            User = magicToken.User,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(90),
            IsActive = true
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Magic token exchanged for session for user {Email}", magicToken.User.Email);
        return (true, session, magicToken.User, null);
    }

    public async Task<(bool Success, User? User)> ValidateSessionAsync(string sessionToken)
    {
        var session = await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Token == sessionToken && s.IsActive);

        if (session == null)
        {
            return (false, null);
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync();
            return (false, null);
        }

        return (true, session.User);
    }

    public async Task<bool> InvalidateSessionAsync(string sessionToken)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Token == sessionToken);

        if (session == null)
        {
            return false;
        }

        session.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Session invalidated for user {UserId}", session.UserId);
        return true;
    }

    public async Task<List<MagicLinkToken>> GetPendingMagicLinksAsync()
    {
        return await _context.MagicLinkTokens
            .Include(mt => mt.User)
            .Where(mt => !mt.IsUsed && mt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(mt => mt.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Email)
            .ToListAsync();
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[64]; // 512 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
