using System.Text.Json;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CoParenting.Application.Services;

public sealed class ScheduleService(CoParentingDbContext db, IMemoryCache previewCache) : IScheduleService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SchedulePreviewDto?> PreviewAsync(
        Guid accountId,
        Guid familyId,
        Guid calendarId,
        ScheduleDraftRequest request)
    {
        var access = await GetAccessAsync(accountId, familyId, calendarId);
        if (access == null || access.Value.Member.Permission == FamilyPermission.Viewer)
        {
            return null;
        }

        ValidateDraft(access.Value.Family, request, false);
        var days = Enumerable.Range(0, 42)
            .Select(offset => ProjectDraftDay(request, request.EffectiveFrom.AddDays(offset)))
            .ToList();
        var validUntil = DateTime.UtcNow.AddMinutes(15);
        previewCache.Set(PreviewKey(accountId, familyId, calendarId, request), true, validUntil);
        return new SchedulePreviewDto(days, validUntil);
    }

    public async Task<ScheduleVersionDto?> CreateVersionAsync(
        Guid accountId,
        Guid familyId,
        Guid calendarId,
        ScheduleDraftRequest request)
    {
        var access = await GetAccessAsync(accountId, familyId, calendarId);
        if (access == null || access.Value.Member.Permission != FamilyPermission.Owner)
        {
            return null;
        }

        ValidateDraft(access.Value.Family, request, true);
        var previewKey = PreviewKey(accountId, familyId, calendarId, request);
        if (!previewCache.TryGetValue(previewKey, out _))
        {
            throw new ArgumentException("Preview this exact schedule before activating it");
        }
        if (await db.ScheduleVersions.AnyAsync(x => x.CalendarId == calendarId && x.EffectiveFrom == request.EffectiveFrom))
        {
            throw new ArgumentException("A schedule version already starts on that date");
        }

        var version = new ScheduleVersion
        {
            FamilyId = familyId,
            CalendarId = calendarId,
            EffectiveFrom = request.EffectiveFrom,
            AnchorDate = request.AnchorDate,
            AnchorSide = request.AnchorSide,
            Template = request.Template,
            ParametersJson = JsonSerializer.Serialize(request.Parameters ?? new ScheduleParametersDto(), JsonOptions),
            ChangeoverTime = request.ChangeoverTime,
            ChangeoverPlace = NormalizeOptional(request.ChangeoverPlace, 200),
            CreatedByMemberId = access.Value.Member.Id,
            CreatedAt = DateTime.UtcNow
        };
        db.ScheduleVersions.Add(version);
        AddAudit(familyId, accountId, "Schedule.VersionCreated", "ScheduleVersion", version.Id, new
        {
            version.CalendarId,
            version.Template,
            version.EffectiveFrom,
            version.AnchorDate,
            version.AnchorSide
        });
        await db.SaveChangesAsync();
        previewCache.Remove(previewKey);
        return MapVersion(version);
    }

    public async Task<IReadOnlyList<ScheduleVersionDto>?> GetVersionsAsync(Guid accountId, Guid familyId, Guid calendarId)
    {
        if (await GetAccessAsync(accountId, familyId, calendarId) == null)
        {
            return null;
        }

        var versions = await db.ScheduleVersions.AsNoTracking()
            .Where(x => x.FamilyId == familyId && x.CalendarId == calendarId)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync();
        return versions.Select(MapVersion).ToList();
    }

    public async Task<CalendarMonthDto?> GetMonthAsync(Guid accountId, Guid familyId, Guid calendarId, int year, int month)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2200 ||
            await GetAccessAsync(accountId, familyId, calendarId) == null)
        {
            return null;
        }

        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var versions = await db.ScheduleVersions.AsNoTracking()
            .Where(x => x.CalendarId == calendarId && x.EffectiveFrom <= last)
            .OrderBy(x => x.EffectiveFrom)
            .ToListAsync();
        var overrides = await db.DayOverrides.AsNoTracking()
            .Where(x => x.CalendarId == calendarId && x.Date >= first && x.Date <= last)
            .ToDictionaryAsync(x => x.Date);
        var comments = await db.Comments.AsNoTracking()
            .Include(x => x.AuthorMember).ThenInclude(x => x.Account)
            .Where(x => x.CalendarId == calendarId && x.Date >= first && x.Date <= last)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        var commentsByDate = comments.GroupBy(x => x.Date).ToDictionary(x => x.Key, x => x.Select(MapComment).ToList());

        var days = new List<CalendarDayDto>(last.Day);
        for (var date = first; date <= last; date = date.AddDays(1))
        {
            days.Add(ProjectStoredDay(date, versions, overrides.GetValueOrDefault(date), commentsByDate.GetValueOrDefault(date) ?? []));
        }

        return new CalendarMonthDto(year, month, days);
    }

    public async Task<CalendarDayDto?> SetOverrideAsync(
        Guid accountId,
        Guid familyId,
        Guid calendarId,
        DateOnly date,
        DayOverrideRequest request)
    {
        var access = await GetAccessAsync(accountId, familyId, calendarId);
        if (access == null || access.Value.Member.Permission == FamilyPermission.Viewer)
        {
            return null;
        }

        ValidateDetail(access.Value.Family, request.ChangeoverTime, request.ChangeoverPlace);
        var value = await db.DayOverrides.SingleOrDefaultAsync(x => x.CalendarId == calendarId && x.Date == date);
        if (value == null)
        {
            value = new DayOverride { FamilyId = familyId, CalendarId = calendarId, Date = date };
            db.DayOverrides.Add(value);
        }

        value.Side = request.Side;
        value.IsVab = request.IsVab;
        value.SpecialStatus = NormalizeOptional(request.SpecialStatus, 80);
        value.ChangeoverTime = request.ChangeoverTime;
        value.ChangeoverPlace = NormalizeOptional(request.ChangeoverPlace, 200);
        value.UpdatedByMemberId = access.Value.Member.Id;
        value.UpdatedAt = DateTime.UtcNow;
        AddAudit(familyId, accountId, "Calendar.DayOverrideSet", "DayOverride", value.Id, new { calendarId, date });
        await db.SaveChangesAsync();
        return (await GetMonthAsync(accountId, familyId, calendarId, date.Year, date.Month))?.Days.Single(x => x.Date == date);
    }

    public async Task<bool> ClearOverrideAsync(Guid accountId, Guid familyId, Guid calendarId, DateOnly date)
    {
        var access = await GetAccessAsync(accountId, familyId, calendarId);
        if (access == null || access.Value.Member.Permission == FamilyPermission.Viewer)
        {
            return false;
        }

        var value = await db.DayOverrides.SingleOrDefaultAsync(x =>
            x.FamilyId == familyId && x.CalendarId == calendarId && x.Date == date);
        if (value == null)
        {
            return false;
        }

        db.DayOverrides.Remove(value);
        AddAudit(familyId, accountId, "Calendar.DayOverrideCleared", "DayOverride", value.Id, new { calendarId, date });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<CommentDto?> AddCommentAsync(
        Guid accountId,
        Guid familyId,
        Guid calendarId,
        DateOnly date,
        CreateCommentRequest request)
    {
        var access = await GetAccessAsync(accountId, familyId, calendarId);
        if (access == null || access.Value.Member.Permission == FamilyPermission.Viewer)
        {
            return null;
        }

        var comment = new Comment
        {
            FamilyId = familyId,
            CalendarId = calendarId,
            Date = date,
            AuthorMemberId = access.Value.Member.Id,
            Text = Required(request.Text, 2000, "Comment is required"),
            CreatedAt = DateTime.UtcNow
        };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        comment.AuthorMember = access.Value.Member;
        return MapComment(comment);
    }

    public async Task<CommentDto?> UpdateCommentAsync(
        Guid accountId,
        Guid familyId,
        Guid commentId,
        UpdateCommentRequest request)
    {
        var member = await db.FamilyMembers.Include(x => x.Account).SingleOrDefaultAsync(x =>
            x.AccountId == accountId && x.FamilyId == familyId && x.IsActive && x.Family.Status == FamilyStatus.Active);
        var comment = await db.Comments.Include(x => x.AuthorMember).ThenInclude(x => x.Account)
            .SingleOrDefaultAsync(x => x.Id == commentId && x.FamilyId == familyId);
        if (member == null || comment == null ||
            (comment.AuthorMemberId != member.Id && member.Permission != FamilyPermission.Owner))
        {
            return null;
        }

        comment.Text = Required(request.Text, 2000, "Comment is required");
        comment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapComment(comment);
    }

    public async Task<bool> DeleteCommentAsync(Guid accountId, Guid familyId, Guid commentId)
    {
        var member = await db.FamilyMembers.SingleOrDefaultAsync(x =>
            x.AccountId == accountId && x.FamilyId == familyId && x.IsActive && x.Family.Status == FamilyStatus.Active);
        var comment = await db.Comments.SingleOrDefaultAsync(x => x.Id == commentId && x.FamilyId == familyId);
        if (member == null || comment == null ||
            (comment.AuthorMemberId != member.Id && member.Permission != FamilyPermission.Owner))
        {
            return false;
        }

        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<ChangeRequestDto>?> GetChangeRequestsAsync(Guid accountId, Guid familyId)
    {
        if (await GetFamilyMemberAsync(accountId, familyId) == null)
        {
            return null;
        }

        var values = await db.ChangeRequests.AsNoTracking()
            .Include(x => x.Calendar)
            .Include(x => x.RequestedByMember).ThenInclude(x => x.Account)
            .Where(x => x.FamilyId == familyId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return values.Select(MapChangeRequest).ToList();
    }

    public async Task<ChangeRequestDto?> CreateChangeRequestAsync(
        Guid accountId,
        Guid familyId,
        Guid calendarId,
        CreateChangeRequestRequest request)
    {
        var access = await GetAccessAsync(accountId, familyId, calendarId);
        if (access == null || access.Value.Member.Permission == FamilyPermission.Viewer ||
            request.ToDate < request.FromDate || request.ToDate.DayNumber - request.FromDate.DayNumber > 31)
        {
            return null;
        }

        var value = new ChangeRequest
        {
            FamilyId = familyId,
            CalendarId = calendarId,
            RequestedByMemberId = access.Value.Member.Id,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            RequestedSide = request.RequestedSide,
            Message = NormalizeOptional(request.Message, 2000),
            CreatedAt = DateTime.UtcNow
        };
        db.ChangeRequests.Add(value);
        await db.SaveChangesAsync();
        value.Calendar = access.Value.Calendar;
        value.RequestedByMember = access.Value.Member;
        return MapChangeRequest(value);
    }

    public async Task<ChangeRequestDto?> ReviewChangeRequestAsync(
        Guid accountId,
        Guid familyId,
        Guid requestId,
        ReviewChangeRequestRequest request)
    {
        var member = await GetFamilyMemberAsync(accountId, familyId);
        if (member == null || member.Permission == FamilyPermission.Viewer)
        {
            return null;
        }

        var value = await db.ChangeRequests.Include(x => x.Calendar)
            .Include(x => x.RequestedByMember).ThenInclude(x => x.Account)
            .SingleOrDefaultAsync(x => x.Id == requestId && x.FamilyId == familyId && x.Status == ChangeRequestStatus.Pending);
        if (value == null || value.RequestedByMemberId == member.Id)
        {
            return null;
        }

        value.Status = request.Approved ? ChangeRequestStatus.Approved : ChangeRequestStatus.Rejected;
        value.ReviewedAt = DateTime.UtcNow;
        value.ReviewedByMemberId = member.Id;
        if (request.Approved)
        {
            for (var date = value.FromDate; date <= value.ToDate; date = date.AddDays(1))
            {
                var dayOverride = await db.DayOverrides.SingleOrDefaultAsync(x => x.CalendarId == value.CalendarId && x.Date == date);
                if (dayOverride == null)
                {
                    dayOverride = new DayOverride { FamilyId = familyId, CalendarId = value.CalendarId, Date = date };
                    db.DayOverrides.Add(dayOverride);
                }

                dayOverride.Side = value.RequestedSide;
                dayOverride.UpdatedByMemberId = member.Id;
                dayOverride.UpdatedAt = DateTime.UtcNow;
            }
        }

        AddAudit(familyId, accountId, request.Approved ? "ChangeRequest.Approved" : "ChangeRequest.Rejected",
            "ChangeRequest", value.Id, new { value.FromDate, value.ToDate, value.RequestedSide });
        await db.SaveChangesAsync();
        return MapChangeRequest(value);
    }

    private async Task<(FamilyMember Member, Family Family, ResidenceCalendar Calendar)?> GetAccessAsync(
        Guid accountId,
        Guid familyId,
        Guid calendarId)
    {
        var member = await db.FamilyMembers.Include(x => x.Account).Include(x => x.Family)
            .SingleOrDefaultAsync(x => x.AccountId == accountId && x.FamilyId == familyId && x.IsActive &&
                                       x.Family.Status == FamilyStatus.Active);
        if (member == null)
        {
            return null;
        }

        var calendar = await db.ResidenceCalendars.SingleOrDefaultAsync(x =>
            x.Id == calendarId && x.FamilyId == familyId && x.IsActive);
        return calendar == null ? null : (member, member.Family, calendar);
    }

    private Task<FamilyMember?> GetFamilyMemberAsync(Guid accountId, Guid familyId) =>
        db.FamilyMembers.Include(x => x.Account).SingleOrDefaultAsync(x =>
            x.AccountId == accountId && x.FamilyId == familyId && x.IsActive && x.Family.Status == FamilyStatus.Active);

    private static CalendarDayDto ProjectDraftDay(ScheduleDraftRequest request, DateOnly date)
    {
        var occurrence = ScheduleEngine.GetOccurrence(request.Template, request.AnchorDate, request.AnchorSide, date, request.Parameters);
        return new CalendarDayDto(date, occurrence.Side, false, false, null,
            request.ChangeoverTime, request.ChangeoverPlace, occurrence.HasRecurringVisit, []);
    }

    private static CalendarDayDto ProjectStoredDay(
        DateOnly date,
        IReadOnlyList<ScheduleVersion> versions,
        DayOverride? dayOverride,
        IReadOnlyList<CommentDto> comments)
    {
        var version = versions.LastOrDefault(x => x.EffectiveFrom <= date);
        ScheduleSide? side = null;
        var visit = false;
        if (version != null)
        {
            var parameters = DeserializeParameters(version.ParametersJson);
            var occurrence = ScheduleEngine.GetOccurrence(version.Template, version.AnchorDate, version.AnchorSide, date, parameters);
            side = occurrence.Side;
            visit = occurrence.HasRecurringVisit;
        }

        return new CalendarDayDto(
            date,
            dayOverride != null ? dayOverride.Side : side,
            dayOverride != null,
            dayOverride?.IsVab ?? false,
            dayOverride?.SpecialStatus,
            dayOverride?.ChangeoverTime ?? version?.ChangeoverTime,
            dayOverride?.ChangeoverPlace ?? version?.ChangeoverPlace,
            visit,
            comments);
    }

    private static void ValidateDraft(Family family, ScheduleDraftRequest request, bool requireFuture)
    {
        if (requireFuture && request.EffectiveFrom <= LocalToday(family.TimeZoneId))
        {
            throw new ArgumentException("A new schedule must start on a future date");
        }

        _ = ScheduleEngine.GetOccurrence(request.Template, request.AnchorDate, request.AnchorSide, request.EffectiveFrom, request.Parameters);
        ValidateDetail(family, request.ChangeoverTime, request.ChangeoverPlace);
    }

    private static void ValidateDetail(Family family, TimeOnly? time, string? place)
    {
        switch (family.ExchangeDetailLevel)
        {
            case ExchangeDetailLevel.Day when time != null || !string.IsNullOrWhiteSpace(place):
                throw new ArgumentException("This family uses day-only exchanges");
            case ExchangeDetailLevel.DayAndTime when time == null || !string.IsNullOrWhiteSpace(place):
                throw new ArgumentException("An exchange time is required and place is not enabled");
            case ExchangeDetailLevel.DayTimeAndPlace when time == null || string.IsNullOrWhiteSpace(place):
                throw new ArgumentException("Exchange time and place are required");
        }
    }

    private void AddAudit(Guid familyId, Guid accountId, string action, string targetType, Guid targetId, object metadata)
    {
        db.AuditEvents.Add(new AuditEvent
        {
            FamilyId = familyId,
            ActorAccountId = accountId,
            ActorType = "Account",
            Action = action,
            TargetType = targetType,
            TargetId = targetId.ToString(),
            Reason = "Family member action",
            MetadataJson = JsonSerializer.Serialize(metadata, JsonOptions),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static ScheduleVersionDto MapVersion(ScheduleVersion value) =>
        new(value.Id, value.Template, value.AnchorDate, value.AnchorSide, value.EffectiveFrom,
            DeserializeParameters(value.ParametersJson), value.ChangeoverTime, value.ChangeoverPlace, value.CreatedAt);

    private static ScheduleParametersDto DeserializeParameters(string json) =>
        JsonSerializer.Deserialize<ScheduleParametersDto>(json, JsonOptions) ?? new ScheduleParametersDto();

    private static CommentDto MapComment(Comment value) =>
        new(value.Id, value.AuthorMemberId, value.AuthorMember.Account.DisplayName, value.Text, value.CreatedAt, value.UpdatedAt);

    private static ChangeRequestDto MapChangeRequest(ChangeRequest value) =>
        new(value.Id, value.CalendarId, value.Calendar.Name, value.RequestedByMemberId,
            value.RequestedByMember.Account.DisplayName, value.FromDate, value.ToDate,
            value.RequestedSide, value.Message, value.Status, value.CreatedAt, value.ReviewedAt);

    private static DateOnly LocalToday(string timeZoneId)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));
    }

    private static string PreviewKey(Guid accountId, Guid familyId, Guid calendarId, ScheduleDraftRequest request) =>
        $"schedule-preview:{accountId}:{familyId}:{calendarId}:{TokenService.HashToken(JsonSerializer.Serialize(request, JsonOptions))}";

    private static string Required(string value, int maxLength, string message)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0 || normalized.Length > maxLength)
        {
            throw new ArgumentException(message);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized?.Length > maxLength)
        {
            throw new ArgumentException($"Value may be at most {maxLength} characters");
        }

        return normalized;
    }
}
