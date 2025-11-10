namespace CoParenting.API.DTOs;

public record DayAssignmentDto(
    int Id,
    DateTime Date,
    string? Parent,
    bool IsVAB,
    string? Comment
);

public record CreateDayAssignmentDto(
    DateTime Date,
    string? Parent,
    bool IsVAB,
    string? Comment
);

public record UpdateDayAssignmentDto(
    string? Parent,
    bool IsVAB,
    string? Comment
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
