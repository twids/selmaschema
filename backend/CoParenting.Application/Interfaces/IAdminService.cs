using CoParenting.Application.DTOs;

namespace CoParenting.Application.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<AdminFamilyListItemDto>> GetFamiliesAsync();
    Task<AdminFamilyDetailDto?> GetFamilyAsync(Guid familyId);
    Task<bool> UpdateFamilyAsync(Guid? actorAccountId, Guid familyId, AdminUpdateFamilyRequest request);
    Task<bool> UpdateMemberAsync(Guid? actorAccountId, Guid familyId, Guid memberId, AdminUpdateMemberRequest request);
    Task<bool> TransferOwnershipAsync(Guid? actorAccountId, Guid familyId, AdminTransferOwnershipRequest request);
    Task<bool> RevokeInvitationAsync(Guid? actorAccountId, Guid familyId, Guid invitationId, string reason);
    Task<int> RevokeAccountSessionsAsync(Guid? actorAccountId, Guid accountId, string reason);
    Task<IReadOnlyList<AuditEventDto>> GetAuditAsync(Guid? familyId, int take = 100);
}
