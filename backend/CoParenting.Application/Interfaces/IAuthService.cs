using CoParenting.Core.Entities;

namespace CoParenting.Application.Interfaces;

public sealed record ExternalLoginInfo(
    string Issuer,
    string Subject,
    string Email,
    string DisplayName,
    bool EmailVerified,
    IReadOnlyCollection<string>? Groups = null);

public sealed record AccountSessionResult(
    bool Success,
    string? RawToken,
    string? RawCsrfToken,
    AccountSession? Session,
    Account? Account,
    string? Error);

public sealed record AdminSessionResult(
    bool Success,
    string? RawToken,
    string? RawCsrfToken,
    PlatformAdminSession? Session,
    string? Error);

public interface IAuthService
{
    Task<AccountSessionResult> SignInExternalAsync(ExternalLoginInfo login);
    Task<AdminSessionResult> SignInPlatformAdminAsync(ExternalLoginInfo login);
    Task<AdminSessionResult> ValidateBreakGlassPasswordAsync(string password);
    Task<(bool Success, Account? Account, AccountSession? Session)> ValidateAccountSessionAsync(string rawToken);
    Task<(bool Success, PlatformAdminSession? Session)> ValidateAdminSessionAsync(string rawToken);
    Task<bool> ValidateAccountCsrfAsync(string rawToken, string rawCsrfToken);
    Task<bool> ValidateAdminCsrfAsync(string rawToken, string rawCsrfToken);
    Task RevokeAccountSessionAsync(string rawToken);
    Task RevokeAdminSessionAsync(string rawToken);
}
