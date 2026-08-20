using CoParenting.Core.Entities;

namespace CoParenting.Application.DTOs;

public sealed record AccountDto(Guid Id, string Email, string DisplayName);

public sealed record MembershipSummaryDto(
    Guid FamilyId,
    string FamilyName,
    FamilyPermission Permission,
    ScheduleSide? Side,
    FamilyStatus Status);

public sealed record AuthMeDto(AccountDto Account, IReadOnlyList<MembershipSummaryDto> Memberships);

public sealed record AdminMeDto(
    string Email,
    string DisplayName,
    string AuthenticationMethod,
    DateTime ExpiresAt);

public sealed record BreakGlassLoginRequest(string Password);
public sealed record EndToEndLoginRequest(string Email, string DisplayName);

public sealed record FamilyDto(
    Guid Id,
    string Name,
    string TimeZoneId,
    string SideALabel,
    string SideBLabel,
    ExchangeDetailLevel ExchangeDetailLevel,
    FamilyStatus Status,
    Guid MyMemberId,
    FamilyPermission MyPermission,
    ScheduleSide? MySide,
    IReadOnlyList<CalendarSummaryDto> Calendars,
    int ActiveChildren,
    bool HasActiveSchedule);

public sealed record CalendarSummaryDto(Guid Id, string Name, bool IsActive, int ChildCount);

public sealed record CreateFamilyRequest(string Name);

public sealed record UpdateFamilyRequest(
    string Name,
    string TimeZoneId,
    string SideALabel,
    string SideBLabel,
    ExchangeDetailLevel ExchangeDetailLevel);

public sealed record MemberDto(
    Guid Id,
    Guid AccountId,
    string Email,
    string DisplayName,
    FamilyPermission Permission,
    ScheduleSide? Side,
    bool IsActive,
    DateTime JoinedAt);

public sealed record UpdateMemberRequest(FamilyPermission Permission, ScheduleSide? Side, string Reason);
public sealed record TransferOwnershipRequest(Guid NewOwnerMemberId, string Reason);

public sealed record ChildDto(Guid Id, string DisplayName, Guid? CalendarId, bool IsActive);
public sealed record CreateChildRequest(string DisplayName, Guid? CalendarId);
public sealed record UpdateChildRequest(string DisplayName, Guid? CalendarId, bool IsActive);

public sealed record ResidenceCalendarDto(Guid Id, string Name, bool IsActive, IReadOnlyList<ChildDto> Children);
public sealed record CreateCalendarRequest(string Name);
public sealed record UpdateCalendarRequest(string Name, bool IsActive);

public sealed record CreateInvitationRequest(
    FamilyPermission Permission,
    ScheduleSide? Side,
    string? EmailHint,
    int? ValidDays);

public sealed record InvitationDto(
    Guid Id,
    Guid FamilyId,
    string FamilyName,
    string? EmailHint,
    FamilyPermission Permission,
    ScheduleSide? Side,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? ConsumedAt,
    DateTime? RevokedAt,
    string Status);

public sealed record CreatedInvitationDto(InvitationDto Invitation, string Link, string Code);

public sealed record JoinPreviewDto(
    Guid InvitationId,
    Guid FamilyId,
    string FamilyName,
    string? EmailHint,
    FamilyPermission Permission,
    ScheduleSide? Side,
    DateTime ExpiresAt,
    bool AlreadyMember);

public sealed record JoinByCodeRequest(string Code);
public sealed record JoinResultDto(Guid FamilyId, string FamilyName, bool Joined);

public sealed record ScheduleParametersDto(
    DayOfWeek WeekendStartsOn = DayOfWeek.Friday,
    int WeekendLengthDays = 3,
    DayOfWeek? RecurringWeekday = null,
    bool RecurringWeekdayOvernight = false);

public sealed record ScheduleDraftRequest(
    ScheduleTemplate Template,
    DateOnly AnchorDate,
    ScheduleSide AnchorSide,
    DateOnly EffectiveFrom,
    ScheduleParametersDto? Parameters,
    TimeOnly? ChangeoverTime,
    string? ChangeoverPlace);

public sealed record ScheduleVersionDto(
    Guid Id,
    ScheduleTemplate Template,
    DateOnly AnchorDate,
    ScheduleSide AnchorSide,
    DateOnly EffectiveFrom,
    ScheduleParametersDto Parameters,
    TimeOnly? ChangeoverTime,
    string? ChangeoverPlace,
    DateTime CreatedAt);

public sealed record SchedulePreviewDto(IReadOnlyList<CalendarDayDto> Days, DateTime ValidUntil);

public sealed record CalendarDayDto(
    DateOnly Date,
    ScheduleSide? Side,
    bool IsOverride,
    bool IsVab,
    string? SpecialStatus,
    TimeOnly? ChangeoverTime,
    string? ChangeoverPlace,
    bool HasRecurringVisit,
    IReadOnlyList<CommentDto> Comments);

public sealed record CalendarMonthDto(int Year, int Month, IReadOnlyList<CalendarDayDto> Days);

public sealed record DayOverrideRequest(
    ScheduleSide? Side,
    bool IsVab,
    string? SpecialStatus,
    TimeOnly? ChangeoverTime,
    string? ChangeoverPlace);

public sealed record CommentDto(Guid Id, Guid AuthorMemberId, string AuthorName, string Text, DateTime CreatedAt, DateTime? UpdatedAt);
public sealed record CreateCommentRequest(string Text);
public sealed record UpdateCommentRequest(string Text);

public sealed record ChangeRequestDto(
    Guid Id,
    Guid CalendarId,
    string CalendarName,
    Guid RequestedByMemberId,
    string RequestedByName,
    DateOnly FromDate,
    DateOnly ToDate,
    ScheduleSide RequestedSide,
    string? Message,
    ChangeRequestStatus Status,
    DateTime CreatedAt,
    DateTime? ReviewedAt);

public sealed record CreateChangeRequestRequest(
    DateOnly FromDate,
    DateOnly ToDate,
    ScheduleSide RequestedSide,
    string? Message);

public sealed record ReviewChangeRequestRequest(bool Approved, string? Message);

public sealed record AdminFamilyListItemDto(
    Guid Id,
    string Name,
    string TimeZoneId,
    FamilyStatus Status,
    int MemberCount,
    int CalendarCount,
    int ScheduleVersionCount,
    DateTime UpdatedAt);

public sealed record AdminFamilyDetailDto(
    AdminFamilyListItemDto Family,
    IReadOnlyList<MemberDto> Members,
    IReadOnlyList<CalendarSummaryDto> Calendars,
    IReadOnlyList<AdminScheduleVersionDto> ScheduleVersions,
    IReadOnlyList<AdminInvitationDto> Invitations,
    IReadOnlyList<AuditEventDto> Activity);

public sealed record AdminScheduleVersionDto(
    Guid Id,
    string CalendarName,
    ScheduleTemplate Template,
    DateOnly EffectiveFrom,
    DateTime CreatedAt);

public sealed record AdminInvitationDto(
    Guid Id,
    string? EmailHint,
    FamilyPermission Permission,
    ScheduleSide? Side,
    DateTime ExpiresAt,
    string Status);

public sealed record AdminReasonRequest(string Reason);
public sealed record AdminUpdateFamilyRequest(string? Name, string? TimeZoneId, FamilyStatus? Status, string Reason);
public sealed record AdminUpdateMemberRequest(FamilyPermission Permission, ScheduleSide? Side, string Reason);
public sealed record AdminTransferOwnershipRequest(Guid NewOwnerMemberId, string Reason);

public sealed record AuditEventDto(
    Guid Id,
    Guid? FamilyId,
    string ActorType,
    string Action,
    string TargetType,
    string TargetId,
    string Reason,
    DateTime CreatedAt);
