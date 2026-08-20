namespace CoParenting.Core.Entities;

public enum FamilyPermission
{
    Owner,
    Editor,
    Viewer
}

public enum ScheduleSide
{
    A,
    B
}

public enum ExchangeDetailLevel
{
    Day,
    DayAndTime,
    DayTimeAndPlace
}

public enum FamilyStatus
{
    Active,
    Suspended
}

public enum ScheduleTemplate
{
    AlternatingWeeks,
    TwoTwoThree,
    TwoTwoFiveFive,
    ThreeFourFourThree,
    PrimaryAlternateWeekends
}

public enum ChangeRequestStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsDisabled { get; set; }
    public ICollection<ExternalIdentity> ExternalIdentities { get; set; } = [];
    public ICollection<FamilyMember> Memberships { get; set; } = [];
}

public class ExternalIdentity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public string Issuer { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string NormalizedIssuer { get; set; } = string.Empty;
    public string NormalizedSubject { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
}

public class AccountSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public string CsrfTokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class PlatformAdminSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AuthenticationMethod { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public string CsrfTokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class Family
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "Europe/Stockholm";
    public string SideALabel { get; set; } = "Hem A";
    public string SideBLabel { get; set; } = "Hem B";
    public ExchangeDetailLevel ExchangeDetailLevel { get; set; } = ExchangeDetailLevel.Day;
    public FamilyStatus Status { get; set; } = FamilyStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<FamilyMember> Members { get; set; } = [];
    public ICollection<ResidenceCalendar> Calendars { get; set; } = [];
}

public class FamilyMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public FamilyPermission Permission { get; set; }
    public ScheduleSide? Side { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
}

public class Child
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public Guid? ResidenceCalendarId { get; set; }
    public ResidenceCalendar? ResidenceCalendar { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class ResidenceCalendar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public ICollection<Child> Children { get; set; } = [];
    public ICollection<ScheduleVersion> ScheduleVersions { get; set; } = [];
}

public class ScheduleVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public Guid CalendarId { get; set; }
    public ResidenceCalendar Calendar { get; set; } = null!;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly AnchorDate { get; set; }
    public ScheduleSide AnchorSide { get; set; }
    public ScheduleTemplate Template { get; set; }
    public string ParametersJson { get; set; } = "{}";
    public TimeOnly? ChangeoverTime { get; set; }
    public string? ChangeoverPlace { get; set; }
    public Guid CreatedByMemberId { get; set; }
    public FamilyMember CreatedByMember { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class DayOverride
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public Guid CalendarId { get; set; }
    public ResidenceCalendar Calendar { get; set; } = null!;
    public DateOnly Date { get; set; }
    public ScheduleSide? Side { get; set; }
    public bool IsVab { get; set; }
    public string? SpecialStatus { get; set; }
    public TimeOnly? ChangeoverTime { get; set; }
    public string? ChangeoverPlace { get; set; }
    public Guid UpdatedByMemberId { get; set; }
    public FamilyMember UpdatedByMember { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }
}

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public Guid CalendarId { get; set; }
    public ResidenceCalendar Calendar { get; set; } = null!;
    public DateOnly Date { get; set; }
    public Guid AuthorMemberId { get; set; }
    public FamilyMember AuthorMember { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ChangeRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public Guid CalendarId { get; set; }
    public ResidenceCalendar Calendar { get; set; } = null!;
    public Guid RequestedByMemberId { get; set; }
    public FamilyMember RequestedByMember { get; set; } = null!;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public ScheduleSide RequestedSide { get; set; }
    public string? Message { get; set; }
    public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;
    public Guid? ReviewedByMemberId { get; set; }
    public FamilyMember? ReviewedByMember { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class FamilyInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public string LinkTokenHash { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public string? EmailHint { get; set; }
    public FamilyPermission Permission { get; set; }
    public ScheduleSide? Side { get; set; }
    public Guid CreatedByMemberId { get; set; }
    public FamilyMember CreatedByMember { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? RedeemedByAccountId { get; set; }
    public Account? RedeemedByAccount { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
}

public class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? FamilyId { get; set; }
    public Family? Family { get; set; }
    public Guid? ActorAccountId { get; set; }
    public Account? ActorAccount { get; set; }
    public string ActorType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
