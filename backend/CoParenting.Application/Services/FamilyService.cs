using System.Data;
using System.Text.Json;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoParenting.Application.Services;

public sealed class FamilyService(CoParentingDbContext db, ILogger<FamilyService> logger) : IFamilyService
{
    public async Task<IReadOnlyList<MembershipSummaryDto>> GetMembershipsAsync(Guid accountId) =>
        await db.FamilyMembers.AsNoTracking()
            .Where(x => x.AccountId == accountId && x.IsActive)
            .OrderBy(x => x.Family.Name)
            .Select(x => new MembershipSummaryDto(
                x.FamilyId,
                x.Family.Name,
                x.Permission,
                x.Side,
                x.Family.Status))
            .ToListAsync();

    public async Task<IReadOnlyList<FamilyDto>> GetFamiliesAsync(Guid accountId)
    {
        var ids = await db.FamilyMembers.AsNoTracking()
            .Where(x => x.AccountId == accountId && x.IsActive)
            .Select(x => x.FamilyId)
            .ToListAsync();
        var families = new List<FamilyDto>();
        foreach (var id in ids)
        {
            var family = await GetFamilyAsync(accountId, id);
            if (family != null)
            {
                families.Add(family);
            }
        }

        return families.OrderBy(x => x.Name).ToList();
    }

    public async Task<FamilyDto?> GetFamilyAsync(Guid accountId, Guid familyId)
    {
        var member = await GetActiveMemberAsync(accountId, familyId);
        if (member == null)
        {
            return null;
        }

        var family = await db.Families.AsNoTracking()
            .Include(x => x.Calendars)
            .ThenInclude(x => x.Children)
            .SingleAsync(x => x.Id == familyId);
        var childCount = await db.Children.CountAsync(x => x.FamilyId == familyId && x.IsActive);
        var hasSchedule = await db.ScheduleVersions.AnyAsync(x => x.FamilyId == familyId);
        return MapFamily(family, member, childCount, hasSchedule);
    }

    public async Task<FamilyDto> CreateFamilyAsync(Guid accountId, CreateFamilyRequest request)
    {
        var name = Required(request.Name, 120, "Family name is required");
        var now = DateTime.UtcNow;
        var family = new Family
        {
            Name = name,
            CreatedAt = now,
            UpdatedAt = now
        };
        var owner = new FamilyMember
        {
            Family = family,
            AccountId = accountId,
            Permission = FamilyPermission.Owner,
            JoinedAt = now
        };
        var calendar = new ResidenceCalendar
        {
            Family = family,
            Name = "Boendeschema",
            CreatedAt = now
        };
        db.Families.Add(family);
        db.FamilyMembers.Add(owner);
        db.ResidenceCalendars.Add(calendar);
        AddAudit(family.Id, accountId, "Family.Created", "Family", family.Id, "Initial family creation", new { family.Name });
        await db.SaveChangesAsync();
        return MapFamily(family, owner, 0, false, [calendar]);
    }

    public async Task<FamilyDto?> UpdateFamilyAsync(Guid accountId, Guid familyId, UpdateFamilyRequest request)
    {
        var owner = await RequireOwnerAsync(accountId, familyId);
        if (owner == null)
        {
            return null;
        }

        var family = await db.Families.Include(x => x.Calendars).ThenInclude(x => x.Children)
            .SingleAsync(x => x.Id == familyId);
        var before = new { family.Name, family.TimeZoneId, family.SideALabel, family.SideBLabel, family.ExchangeDetailLevel };
        family.Name = Required(request.Name, 120, "Family name is required");
        family.TimeZoneId = ValidateTimeZone(request.TimeZoneId);
        family.SideALabel = Required(request.SideALabel, 80, "Side A label is required");
        family.SideBLabel = Required(request.SideBLabel, 80, "Side B label is required");
        family.ExchangeDetailLevel = request.ExchangeDetailLevel;
        family.UpdatedAt = DateTime.UtcNow;
        AddAudit(familyId, accountId, "Family.SettingsUpdated", "Family", familyId, "Family owner update", new { before, after = request });
        await db.SaveChangesAsync();
        var childCount = await db.Children.CountAsync(x => x.FamilyId == familyId && x.IsActive);
        var hasSchedule = await db.ScheduleVersions.AnyAsync(x => x.FamilyId == familyId);
        return MapFamily(family, owner, childCount, hasSchedule);
    }

    public Task<FamilyMember?> GetActiveMemberAsync(Guid accountId, Guid familyId) =>
        db.FamilyMembers.SingleOrDefaultAsync(x =>
            x.AccountId == accountId && x.FamilyId == familyId && x.IsActive && x.Family.Status == FamilyStatus.Active);

    public async Task<IReadOnlyList<MemberDto>?> GetMembersAsync(Guid accountId, Guid familyId)
    {
        if (await GetActiveMemberAsync(accountId, familyId) == null)
        {
            return null;
        }

        return await db.FamilyMembers.AsNoTracking().Where(x => x.FamilyId == familyId)
            .OrderByDescending(x => x.Permission == FamilyPermission.Owner)
            .ThenBy(x => x.Account.DisplayName)
            .Select(x => new MemberDto(
                x.Id, x.AccountId, x.Account.Email, x.Account.DisplayName,
                x.Permission, x.Side, x.IsActive, x.JoinedAt))
            .ToListAsync();
    }

    public async Task<MemberDto?> UpdateMemberAsync(Guid accountId, Guid familyId, Guid memberId, UpdateMemberRequest request)
    {
        var owner = await RequireOwnerAsync(accountId, familyId);
        if (owner == null)
        {
            return null;
        }

        var member = await db.FamilyMembers.Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.Id == memberId && x.FamilyId == familyId && x.IsActive);
        if (member == null ||
            (request.Permission == FamilyPermission.Owner && member.Id != owner.Id) ||
            (member.Permission == FamilyPermission.Owner && request.Permission != FamilyPermission.Owner))
        {
            return null;
        }

        var before = new { member.Permission, member.Side };
        member.Permission = request.Permission;
        member.Side = request.Side;
        member.ConcurrencyToken = Guid.NewGuid();
        AddAudit(familyId, accountId, "FamilyMember.Updated", "FamilyMember", member.Id, RequiredReason(request.Reason), new { before, after = request });
        await db.SaveChangesAsync();
        return MapMember(member);
    }

    public async Task<bool> TransferOwnershipAsync(Guid accountId, Guid familyId, TransferOwnershipRequest request)
    {
        var currentOwner = await RequireOwnerAsync(accountId, familyId);
        if (currentOwner == null)
        {
            return false;
        }

        var newOwner = await db.FamilyMembers.SingleOrDefaultAsync(x =>
            x.Id == request.NewOwnerMemberId && x.FamilyId == familyId && x.IsActive);
        if (newOwner == null || newOwner.Id == currentOwner.Id)
        {
            return false;
        }

        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync() : null;
        if (db.Database.IsRelational())
        {
            await db.FamilyMembers.Where(x => x.Id == currentOwner.Id)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.Permission, FamilyPermission.Editor)
                    .SetProperty(x => x.ConcurrencyToken, Guid.NewGuid()));
            await db.FamilyMembers.Where(x => x.Id == newOwner.Id && x.IsActive)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.Permission, FamilyPermission.Owner)
                    .SetProperty(x => x.ConcurrencyToken, Guid.NewGuid()));
            db.Entry(currentOwner).State = EntityState.Unchanged;
            db.Entry(newOwner).State = EntityState.Unchanged;
        }
        else
        {
            currentOwner.Permission = FamilyPermission.Editor;
            currentOwner.ConcurrencyToken = Guid.NewGuid();
            newOwner.Permission = FamilyPermission.Owner;
            newOwner.ConcurrencyToken = Guid.NewGuid();
        }
        AddAudit(familyId, accountId, "Family.OwnershipTransferred", "FamilyMember", newOwner.Id, RequiredReason(request.Reason), new
        {
            previousOwnerMemberId = currentOwner.Id,
            newOwnerMemberId = newOwner.Id
        });
        await db.SaveChangesAsync();
        if (transaction != null)
        {
            await transaction.CommitAsync();
        }

        return true;
    }

    public async Task<IReadOnlyList<ChildDto>?> GetChildrenAsync(Guid accountId, Guid familyId)
    {
        if (await GetActiveMemberAsync(accountId, familyId) == null)
        {
            return null;
        }

        return await db.Children.AsNoTracking().Where(x => x.FamilyId == familyId)
            .OrderBy(x => x.DisplayName)
            .Select(x => new ChildDto(x.Id, x.DisplayName, x.ResidenceCalendarId, x.IsActive))
            .ToListAsync();
    }

    public async Task<ChildDto?> CreateChildAsync(Guid accountId, Guid familyId, CreateChildRequest request)
    {
        if (await RequireOwnerAsync(accountId, familyId) == null ||
            !await CalendarBelongsToFamilyAsync(familyId, request.CalendarId))
        {
            return null;
        }

        var child = new Child
        {
            FamilyId = familyId,
            DisplayName = Required(request.DisplayName, 100, "Child display name is required"),
            ResidenceCalendarId = request.CalendarId,
            CreatedAt = DateTime.UtcNow
        };
        db.Children.Add(child);
        AddAudit(familyId, accountId, "Child.Created", "Child", child.Id, "Family owner update", new { child.DisplayName, child.ResidenceCalendarId });
        await db.SaveChangesAsync();
        return MapChild(child);
    }

    public async Task<ChildDto?> UpdateChildAsync(Guid accountId, Guid familyId, Guid childId, UpdateChildRequest request)
    {
        if (await RequireOwnerAsync(accountId, familyId) == null ||
            !await CalendarBelongsToFamilyAsync(familyId, request.CalendarId))
        {
            return null;
        }

        var child = await db.Children.SingleOrDefaultAsync(x => x.Id == childId && x.FamilyId == familyId);
        if (child == null)
        {
            return null;
        }

        var before = new { child.DisplayName, child.ResidenceCalendarId, child.IsActive };
        child.DisplayName = Required(request.DisplayName, 100, "Child display name is required");
        child.ResidenceCalendarId = request.CalendarId;
        child.IsActive = request.IsActive;
        AddAudit(familyId, accountId, "Child.Updated", "Child", child.Id, "Family owner update", new { before, after = request });
        await db.SaveChangesAsync();
        return MapChild(child);
    }

    public async Task<IReadOnlyList<ResidenceCalendarDto>?> GetCalendarsAsync(Guid accountId, Guid familyId)
    {
        if (await GetActiveMemberAsync(accountId, familyId) == null)
        {
            return null;
        }

        return await db.ResidenceCalendars.AsNoTracking().Where(x => x.FamilyId == familyId)
            .Include(x => x.Children)
            .OrderBy(x => x.Name)
            .Select(x => new ResidenceCalendarDto(
                x.Id, x.Name, x.IsActive,
                x.Children.OrderBy(c => c.DisplayName)
                    .Select(c => new ChildDto(c.Id, c.DisplayName, c.ResidenceCalendarId, c.IsActive)).ToList()))
            .ToListAsync();
    }

    public async Task<ResidenceCalendarDto?> CreateCalendarAsync(Guid accountId, Guid familyId, CreateCalendarRequest request)
    {
        if (await RequireOwnerAsync(accountId, familyId) == null)
        {
            return null;
        }

        var calendar = new ResidenceCalendar
        {
            FamilyId = familyId,
            Name = Required(request.Name, 120, "Calendar name is required"),
            CreatedAt = DateTime.UtcNow
        };
        db.ResidenceCalendars.Add(calendar);
        AddAudit(familyId, accountId, "Calendar.Created", "ResidenceCalendar", calendar.Id, "Family owner update", new { calendar.Name });
        await db.SaveChangesAsync();
        return MapCalendar(calendar);
    }

    public async Task<ResidenceCalendarDto?> UpdateCalendarAsync(Guid accountId, Guid familyId, Guid calendarId, UpdateCalendarRequest request)
    {
        if (await RequireOwnerAsync(accountId, familyId) == null)
        {
            return null;
        }

        var calendar = await db.ResidenceCalendars.Include(x => x.Children)
            .SingleOrDefaultAsync(x => x.Id == calendarId && x.FamilyId == familyId);
        if (calendar == null)
        {
            return null;
        }

        var before = new { calendar.Name, calendar.IsActive };
        calendar.Name = Required(request.Name, 120, "Calendar name is required");
        calendar.IsActive = request.IsActive;
        AddAudit(familyId, accountId, "Calendar.Updated", "ResidenceCalendar", calendar.Id, "Family owner update", new { before, after = request });
        await db.SaveChangesAsync();
        return MapCalendar(calendar);
    }

    public async Task<CreatedInvitationDto?> CreateInvitationAsync(
        Guid accountId,
        Guid familyId,
        CreateInvitationRequest request,
        string publicBaseUrl)
    {
        var owner = await RequireOwnerAsync(accountId, familyId);
        if (owner == null || request.Permission == FamilyPermission.Owner)
        {
            return null;
        }

        var days = Math.Clamp(request.ValidDays ?? 7, 1, 30);
        var token = TokenService.GenerateToken();
        var code = TokenService.GenerateInvitationCode();
        var now = DateTime.UtcNow;
        var invitation = new FamilyInvitation
        {
            FamilyId = familyId,
            LinkTokenHash = TokenService.HashToken(token),
            CodeHash = TokenService.HashToken(TokenService.NormalizeInvitationCode(code)),
            EmailHint = string.IsNullOrWhiteSpace(request.EmailHint) ? null : request.EmailHint.Trim(),
            Permission = request.Permission,
            Side = request.Side,
            CreatedByMemberId = owner.Id,
            CreatedAt = now,
            ExpiresAt = now.AddDays(days)
        };
        db.FamilyInvitations.Add(invitation);
        AddAudit(familyId, accountId, "Invitation.Created", "FamilyInvitation", invitation.Id, "Family owner invitation", new
        {
            invitation.Permission,
            invitation.Side,
            invitation.ExpiresAt
        });
        await db.SaveChangesAsync();
        var dto = MapInvitation(invitation, owner.Family.Name);
        return new CreatedInvitationDto(dto, $"{publicBaseUrl.TrimEnd('/')}/join/{Uri.EscapeDataString(token)}", code);
    }

    public async Task<IReadOnlyList<InvitationDto>?> GetInvitationsAsync(Guid accountId, Guid familyId)
    {
        if (await RequireOwnerAsync(accountId, familyId) == null)
        {
            return null;
        }

        return await db.FamilyInvitations.AsNoTracking().Where(x => x.FamilyId == familyId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InvitationDto(
                x.Id, x.FamilyId, x.Family.Name, x.EmailHint, x.Permission, x.Side,
                x.CreatedAt, x.ExpiresAt, x.ConsumedAt, x.RevokedAt,
                x.RevokedAt != null ? "Revoked" : x.ConsumedAt != null ? "Consumed" : x.ExpiresAt <= DateTime.UtcNow ? "Expired" : "Active"))
            .ToListAsync();
    }

    public async Task<bool> RevokeInvitationAsync(Guid accountId, Guid familyId, Guid invitationId)
    {
        if (await RequireOwnerAsync(accountId, familyId) == null)
        {
            return false;
        }

        var invitation = await db.FamilyInvitations.SingleOrDefaultAsync(x =>
            x.Id == invitationId && x.FamilyId == familyId && x.ConsumedAt == null && x.RevokedAt == null);
        if (invitation == null)
        {
            return false;
        }

        invitation.RevokedAt = DateTime.UtcNow;
        invitation.ConcurrencyToken = Guid.NewGuid();
        AddAudit(familyId, accountId, "Invitation.Revoked", "FamilyInvitation", invitationId, "Family owner revoked invitation", null);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<JoinPreviewDto?> GetJoinPreviewByTokenAsync(Guid accountId, string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return null;
        }

        var hash = TokenService.HashToken(rawToken);
        return await GetJoinPreviewAsync(accountId, x => x.LinkTokenHash == hash);
    }

    public Task<JoinPreviewDto?> GetJoinPreviewByCodeAsync(Guid accountId, string code)
    {
        var normalized = TokenService.NormalizeInvitationCode(code);
        return normalized.Length != 8
            ? Task.FromResult<JoinPreviewDto?>(null)
            : GetJoinPreviewAsync(accountId, x => x.CodeHash == TokenService.HashToken(normalized));
    }

    private async Task<JoinPreviewDto?> GetJoinPreviewAsync(
        Guid accountId,
        System.Linq.Expressions.Expression<Func<FamilyInvitation, bool>> predicate)
    {
        var invitation = await db.FamilyInvitations.AsNoTracking().Include(x => x.Family)
            .Where(predicate)
            .SingleOrDefaultAsync(x => x.ConsumedAt == null && x.RevokedAt == null &&
                                       x.ExpiresAt > DateTime.UtcNow);
        if (invitation == null)
        {
            return null;
        }

        var member = await db.FamilyMembers.AnyAsync(x =>
            x.FamilyId == invitation.FamilyId && x.AccountId == accountId && x.IsActive);
        return new JoinPreviewDto(
            invitation.Id, invitation.FamilyId, invitation.Family.Name, invitation.EmailHint,
            invitation.Permission, invitation.Side, invitation.ExpiresAt, member);
    }

    public async Task<bool> IsInvitationTokenValidAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return false;
        }

        var hash = TokenService.HashToken(rawToken);
        return await db.FamilyInvitations.AsNoTracking().AnyAsync(x =>
            x.LinkTokenHash == hash && x.ConsumedAt == null && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow);
    }

    public Task<JoinResultDto?> RedeemTokenAsync(Guid accountId, string rawToken) =>
        RedeemAsync(accountId, x => x.LinkTokenHash == TokenService.HashToken(rawToken));

    public Task<JoinResultDto?> RedeemCodeAsync(Guid accountId, string code)
    {
        var normalized = TokenService.NormalizeInvitationCode(code);
        return normalized.Length != 8
            ? Task.FromResult<JoinResultDto?>(null)
            : RedeemAsync(accountId, x => x.CodeHash == TokenService.HashToken(normalized));
    }

    private async Task<JoinResultDto?> RedeemAsync(
        Guid accountId,
        System.Linq.Expressions.Expression<Func<FamilyInvitation, bool>> predicate)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted)
            : null;
        try
        {
            var invitation = await db.FamilyInvitations.Include(x => x.Family)
                .SingleOrDefaultAsync(predicate);
            if (invitation == null || invitation.ConsumedAt != null || invitation.RevokedAt != null ||
                invitation.ExpiresAt <= DateTime.UtcNow)
            {
                return null;
            }

            if (await db.FamilyMembers.AnyAsync(x => x.FamilyId == invitation.FamilyId && x.AccountId == accountId && x.IsActive))
            {
                return new JoinResultDto(invitation.FamilyId, invitation.Family.Name, false);
            }

            var now = DateTime.UtcNow;
            if (db.Database.IsRelational())
            {
                var claimed = await db.FamilyInvitations
                    .Where(x => x.Id == invitation.Id && x.ConcurrencyToken == invitation.ConcurrencyToken &&
                                x.ConsumedAt == null && x.RevokedAt == null && x.ExpiresAt > now)
                    .ExecuteUpdateAsync(updates => updates
                        .SetProperty(x => x.ConsumedAt, now)
                        .SetProperty(x => x.RedeemedByAccountId, accountId)
                        .SetProperty(x => x.ConcurrencyToken, Guid.NewGuid()));
                if (claimed != 1)
                {
                    return null;
                }
                db.Entry(invitation).State = EntityState.Unchanged;
            }
            else
            {
                invitation.ConsumedAt = now;
                invitation.RedeemedByAccountId = accountId;
                invitation.ConcurrencyToken = Guid.NewGuid();
            }

            db.FamilyMembers.Add(new FamilyMember
            {
                FamilyId = invitation.FamilyId,
                AccountId = accountId,
                Permission = invitation.Permission,
                Side = invitation.Side,
                JoinedAt = now
            });
            AddAudit(invitation.FamilyId, accountId, "Invitation.Redeemed", "FamilyInvitation", invitation.Id, "Invitation redemption", new
            {
                invitation.Permission,
                invitation.Side
            });
            await db.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return new JoinResultDto(invitation.FamilyId, invitation.Family.Name, true);
        }
        catch (Exception exception) when (exception is DbUpdateException or InvalidOperationException)
        {
            logger.LogWarning(exception, "Concurrent family invitation redemption was rejected");
            return null;
        }
    }

    private Task<FamilyMember?> RequireOwnerAsync(Guid accountId, Guid familyId) =>
        db.FamilyMembers.Include(x => x.Family).SingleOrDefaultAsync(x =>
            x.AccountId == accountId && x.FamilyId == familyId && x.IsActive &&
            x.Permission == FamilyPermission.Owner && x.Family.Status == FamilyStatus.Active);

    private Task<bool> CalendarBelongsToFamilyAsync(Guid familyId, Guid? calendarId) =>
        calendarId == null
            ? Task.FromResult(true)
            : db.ResidenceCalendars.AnyAsync(x => x.Id == calendarId && x.FamilyId == familyId && x.IsActive);

    private void AddAudit(Guid? familyId, Guid? accountId, string action, string targetType, Guid targetId, string reason, object? metadata)
    {
        db.AuditEvents.Add(new AuditEvent
        {
            FamilyId = familyId,
            ActorAccountId = accountId,
            ActorType = "Account",
            Action = action,
            TargetType = targetType,
            TargetId = targetId.ToString(),
            Reason = reason,
            MetadataJson = metadata == null ? "{}" : JsonSerializer.Serialize(metadata),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static FamilyDto MapFamily(
        Family family,
        FamilyMember member,
        int childCount,
        bool hasSchedule,
        IEnumerable<ResidenceCalendar>? calendars = null)
    {
        calendars ??= family.Calendars;
        return new FamilyDto(
            family.Id, family.Name, family.TimeZoneId, family.SideALabel, family.SideBLabel,
            family.ExchangeDetailLevel, family.Status, member.Id, member.Permission, member.Side,
            calendars.OrderBy(x => x.Name).Select(x => new CalendarSummaryDto(x.Id, x.Name, x.IsActive, x.Children.Count(c => c.IsActive))).ToList(),
            childCount, hasSchedule);
    }

    private static MemberDto MapMember(FamilyMember member) =>
        new(member.Id, member.AccountId, member.Account.Email, member.Account.DisplayName,
            member.Permission, member.Side, member.IsActive, member.JoinedAt);

    private static ChildDto MapChild(Child child) =>
        new(child.Id, child.DisplayName, child.ResidenceCalendarId, child.IsActive);

    private static ResidenceCalendarDto MapCalendar(ResidenceCalendar calendar) =>
        new(calendar.Id, calendar.Name, calendar.IsActive,
            calendar.Children.Select(MapChild).OrderBy(x => x.DisplayName).ToList());

    private static InvitationDto MapInvitation(FamilyInvitation invitation, string familyName) =>
        new(invitation.Id, invitation.FamilyId, familyName, invitation.EmailHint,
            invitation.Permission, invitation.Side, invitation.CreatedAt, invitation.ExpiresAt,
            invitation.ConsumedAt, invitation.RevokedAt, "Active");

    private static string Required(string value, int maxLength, string message)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || trimmed.Length > maxLength)
        {
            throw new ArgumentException(message);
        }

        return trimmed;
    }

    private static string RequiredReason(string value) => Required(value, 500, "Reason is required");

    private static string ValidateTimeZone(string value)
    {
        var id = Required(value, 100, "Time zone is required");
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(id);
            return id;
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException("Unknown time zone");
        }
    }
}
