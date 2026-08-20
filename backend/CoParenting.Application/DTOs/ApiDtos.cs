namespace CoParenting.Application.DTOs;

public record CommentDto(
    int Id,
    int DayAssignmentId,
    string Parent,
    string CommentText,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);

public record DayAssignmentDto(
    int Id,
    DateOnly Date,
    string? Parent,
    bool IsVAB,
    string? SpecialStatus,
    List<CommentDto> ParentAComments,
    List<CommentDto> ParentBComments
);

public record CreateDayAssignmentDto(
    DateOnly Date,
    string? Parent,
    bool IsVAB,
    string? SpecialStatus
);

public record UpdateDayAssignmentDto(
    string? Parent,
    bool IsVAB,
    string? SpecialStatus
);

public record MonthDataDto(
    int Year,
    int Month,
    List<DayAssignmentDto> Days
);

public record ConfigurationDto(
    string Key,
    string Value
);

public record ParentNamesDto(
    string ParentAName,
    string ParentBName
);

public record StatisticsDto(
    int ParentADays,
    int ParentBDays,
    int VABDays,
    int UnassignedDays,
    int DaysWithComments
);

public record CreateCommentDto(
    string CommentText
);

public record UpdateCommentDto(
    string CommentText
);

// Authentication DTOs
public record UserDto
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public DateTime? LastLoginAt { get; init; }
}

public record AuthResponse
{
    public UserDto User { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }
}

public record AdminLoginRequest
{
    public string Password { get; init; } = string.Empty;
}

public record CreateInvitationRequest
{
    public string? EmailHint { get; init; }
    public string Role { get; init; } = string.Empty;
}

public record InvitationDto
{
    public int Id { get; init; }
    public string? EmailHint { get; init; }
    public string Role { get; init; } = string.Empty;
    public int CreatedByUserId { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime? ConsumedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record CreatedInvitationDto
{
    public InvitationDto Invitation { get; init; } = null!;
    public string InvitationUrl { get; init; } = string.Empty;
}

public record PendingInvitationDto
{
    public int InvitationId { get; init; }
    public string? EmailHint { get; init; }
    public string VerifiedEmail { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

// Change Request DTOs
public record ChangeRequestDto
{
    public int Id { get; init; }
    public int RequestedByUserId { get; init; }
    public string RequestedByName { get; init; } = string.Empty;
    public DateOnly RequestedForDate { get; init; }
    public string CurrentParent { get; init; } = string.Empty;
    public string RequestedParent { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? ReviewedByName { get; init; }
    public string? Comment { get; init; }
}

public record CreateChangeRequestDto
{
    public List<DateOnly> Dates { get; init; } = new();
    public string RequestedParent { get; init; } = string.Empty;
    public string? Comment { get; init; }
}

public record ReviewChangeRequestDto
{
    public bool Approved { get; init; }
    public string? Comment { get; init; }
}
