using CoParenting.Application.DTOs;

namespace CoParenting.Application.Interfaces;

public interface IScheduleService
{
    Task<SchedulePreviewDto?> PreviewAsync(Guid accountId, Guid familyId, Guid calendarId, ScheduleDraftRequest request);
    Task<ScheduleVersionDto?> CreateVersionAsync(Guid accountId, Guid familyId, Guid calendarId, ScheduleDraftRequest request);
    Task<IReadOnlyList<ScheduleVersionDto>?> GetVersionsAsync(Guid accountId, Guid familyId, Guid calendarId);
    Task<CalendarMonthDto?> GetMonthAsync(Guid accountId, Guid familyId, Guid calendarId, int year, int month);
    Task<CalendarDayDto?> SetOverrideAsync(Guid accountId, Guid familyId, Guid calendarId, DateOnly date, DayOverrideRequest request);
    Task<bool> ClearOverrideAsync(Guid accountId, Guid familyId, Guid calendarId, DateOnly date);
    Task<CommentDto?> AddCommentAsync(Guid accountId, Guid familyId, Guid calendarId, DateOnly date, CreateCommentRequest request);
    Task<CommentDto?> UpdateCommentAsync(Guid accountId, Guid familyId, Guid commentId, UpdateCommentRequest request);
    Task<bool> DeleteCommentAsync(Guid accountId, Guid familyId, Guid commentId);
    Task<IReadOnlyList<ChangeRequestDto>?> GetChangeRequestsAsync(Guid accountId, Guid familyId);
    Task<ChangeRequestDto?> CreateChangeRequestAsync(Guid accountId, Guid familyId, Guid calendarId, CreateChangeRequestRequest request);
    Task<ChangeRequestDto?> ReviewChangeRequestAsync(Guid accountId, Guid familyId, Guid requestId, ReviewChangeRequestRequest request);
}
