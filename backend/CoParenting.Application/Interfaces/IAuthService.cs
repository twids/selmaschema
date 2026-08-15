using CoParenting.Core.Entities;

namespace CoParenting.Application.Interfaces;

public sealed record ExternalLoginInfo(
    string Issuer,
    string Subject,
    string Email,
    string DisplayName,
    bool EmailVerified);

public sealed record SessionResult(
    bool Success,
    string? RawToken,
    Session? Session,
    User? User,
    string? Error);

public sealed record InvitationCreationResult(
    bool Success,
    string? RawToken,
    Invitation? Invitation,
    string? Error);

public interface IAuthService
{
    Task<SessionResult> ValidateAdminPasswordAsync(string password);
    Task<SessionResult> SignInExternalAsync(ExternalLoginInfo login);
    Task<SessionResult> CompleteInvitationAsync(int invitationId, ExternalLoginInfo login);
    Task<(bool Success, User? User)> ValidateSessionAsync(string rawSessionToken);
    Task<bool> InvalidateSessionAsync(string rawSessionToken);
    Task<InvitationCreationResult> CreateInvitationAsync(int creatorUserId, string creatorRole, string role, string? emailHint);
    Task<Invitation?> GetValidInvitationByTokenAsync(string rawToken);
    Task<Invitation?> GetValidInvitationByIdAsync(int invitationId);
    Task<List<Invitation>> GetInvitationsAsync(int requesterUserId, bool isAdmin);
    Task<List<User>> GetAllUsersAsync();
}
