using CoParenting.Core.Entities;

namespace CoParenting.Application.Interfaces;

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
