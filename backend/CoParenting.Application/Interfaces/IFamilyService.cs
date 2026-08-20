using CoParenting.Application.DTOs;
using CoParenting.Core.Entities;

namespace CoParenting.Application.Interfaces;

public interface IFamilyService
{
    Task<IReadOnlyList<MembershipSummaryDto>> GetMembershipsAsync(Guid accountId);
    Task<IReadOnlyList<FamilyDto>> GetFamiliesAsync(Guid accountId);
    Task<FamilyDto?> GetFamilyAsync(Guid accountId, Guid familyId);
    Task<FamilyDto> CreateFamilyAsync(Guid accountId, CreateFamilyRequest request);
    Task<FamilyDto?> UpdateFamilyAsync(Guid accountId, Guid familyId, UpdateFamilyRequest request);
    Task<FamilyMember?> GetActiveMemberAsync(Guid accountId, Guid familyId);
    Task<IReadOnlyList<MemberDto>?> GetMembersAsync(Guid accountId, Guid familyId);
    Task<MemberDto?> UpdateMemberAsync(Guid accountId, Guid familyId, Guid memberId, UpdateMemberRequest request);
    Task<bool> TransferOwnershipAsync(Guid accountId, Guid familyId, TransferOwnershipRequest request);
    Task<IReadOnlyList<ChildDto>?> GetChildrenAsync(Guid accountId, Guid familyId);
    Task<ChildDto?> CreateChildAsync(Guid accountId, Guid familyId, CreateChildRequest request);
    Task<ChildDto?> UpdateChildAsync(Guid accountId, Guid familyId, Guid childId, UpdateChildRequest request);
    Task<IReadOnlyList<ResidenceCalendarDto>?> GetCalendarsAsync(Guid accountId, Guid familyId);
    Task<ResidenceCalendarDto?> CreateCalendarAsync(Guid accountId, Guid familyId, CreateCalendarRequest request);
    Task<ResidenceCalendarDto?> UpdateCalendarAsync(Guid accountId, Guid familyId, Guid calendarId, UpdateCalendarRequest request);
    Task<CreatedInvitationDto?> CreateInvitationAsync(Guid accountId, Guid familyId, CreateInvitationRequest request, string publicBaseUrl);
    Task<IReadOnlyList<InvitationDto>?> GetInvitationsAsync(Guid accountId, Guid familyId);
    Task<bool> RevokeInvitationAsync(Guid accountId, Guid familyId, Guid invitationId);
    Task<bool> IsInvitationTokenValidAsync(string rawToken);
    Task<JoinPreviewDto?> GetJoinPreviewByTokenAsync(Guid accountId, string rawToken);
    Task<JoinPreviewDto?> GetJoinPreviewByCodeAsync(Guid accountId, string code);
    Task<JoinResultDto?> RedeemTokenAsync(Guid accountId, string rawToken);
    Task<JoinResultDto?> RedeemCodeAsync(Guid accountId, string code);
}
