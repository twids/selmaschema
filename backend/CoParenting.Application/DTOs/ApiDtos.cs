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
    DateTime Date,
    string? Parent,
    bool IsVAB,
    string? SpecialStatus,
    List<CommentDto> ParentAComments,
    List<CommentDto> ParentBComments
);

public record CreateDayAssignmentDto(
    DateTime Date,
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
    public string Token { get; init; } = string.Empty;
    public UserDto User { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }
}

public record AdminLoginRequest
{
    public string Password { get; init; } = string.Empty;
}

public record MagicTokenRequest
{
    public string Token { get; init; } = string.Empty;
}

public record CreateMagicLinkRequest
{
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

public record MagicLinkResponse
{
    public string Token { get; init; } = string.Empty;
    public string MagicLink { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public DateTime? CreatedAt { get; init; }
}

// Change Request DTOs
public record ChangeRequestDto
{
    public int Id { get; init; }
    public int RequestedByUserId { get; init; }
    public string RequestedByName { get; init; } = string.Empty;
    public DateTime RequestedForDate { get; init; }
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
    public List<DateTime> Dates { get; init; } = new();
    public string RequestedParent { get; init; } = string.Empty;
    public string? Comment { get; init; }
}

public record ReviewChangeRequestDto
{
    public bool Approved { get; init; }
    public string? Comment { get; init; }
}
