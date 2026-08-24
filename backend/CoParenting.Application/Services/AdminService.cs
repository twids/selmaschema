using System.Text.Json;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Application.Services;

public sealed class AdminService(CoParentingDbContext db) : IAdminService
{
    public async Task<IReadOnlyList<AdminFamilyListItemDto>> GetFamiliesAsync() =>
        await db.Families.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new AdminFamilyListItemDto(
                x.Id,
                x.Name,
                x.TimeZoneId,
                x.Status,
                x.Members.Count(m => m.IsActive),
                x.Calendars.Count(c => c.IsActive),
                db.ScheduleVersions.Count(v => v.FamilyId == x.Id),
                x.UpdatedAt))
            .ToListAsync();

    public async Task<AdminFamilyDetailDto?> GetFamilyAsync(Guid familyId)
    {
        var family = await db.Families.AsNoTracking().SingleOrDefaultAsync(x => x.Id == familyId);
        if (family == null)
        {
            return null;
        }

        var members = await db.FamilyMembers.AsNoTracking().Where(x => x.FamilyId == familyId)
            .OrderByDescending(x => x.Permission == FamilyPermission.Owner)
            .ThenBy(x => x.Account.DisplayName)
            .Select(x => new MemberDto(x.Id, x.AccountId, x.Account.Email, x.Account.DisplayName,
                x.Permission, x.Side, x.IsActive, x.JoinedAt))
            .ToListAsync();
        var calendars = await db.ResidenceCalendars.AsNoTracking().Where(x => x.FamilyId == familyId)
            .OrderBy(x => x.Name)
            .Select(x => new CalendarSummaryDto(x.Id, x.Name, x.IsActive, x.Children.Count(c => c.IsActive)))
            .ToListAsync();
        var scheduleCount = await db.ScheduleVersions.CountAsync(x => x.FamilyId == familyId);
        var scheduleVersions = await db.ScheduleVersions.AsNoTracking()
            .Where(x => x.FamilyId == familyId)
            .OrderByDescending(x => x.EffectiveFrom)
            .Select(x => new AdminScheduleVersionDto(x.Id, x.Calendar.Name, x.Template, x.EffectiveFrom, x.CreatedAt))
            .ToListAsync();
        var now = DateTime.UtcNow;
        var invitationRows = await db.FamilyInvitations.AsNoTracking()
            .Where(x => x.FamilyId == familyId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.EmailHint, x.Permission, x.Side, x.ExpiresAt, x.ConsumedAt, x.RevokedAt })
            .ToListAsync();
        var invitations = invitationRows.Select(x => new AdminInvitationDto(
            x.Id, x.EmailHint, x.Permission, x.Side, x.ExpiresAt,
            x.RevokedAt != null ? "Revoked" : x.ConsumedAt != null ? "Consumed" : x.ExpiresAt <= now ? "Expired" : "Active"))
            .ToList();
        var activity = await GetAuditAsync(familyId, 100);
        var summary = new AdminFamilyListItemDto(
            family.Id, family.Name, family.TimeZoneId, family.Status,
            members.Count(x => x.IsActive), calendars.Count(x => x.IsActive), scheduleCount, family.UpdatedAt);
        return new AdminFamilyDetailDto(summary, members, calendars, scheduleVersions, invitations, activity);
    }

    public async Task<bool> UpdateFamilyAsync(Guid? actorAccountId, Guid familyId, AdminUpdateFamilyRequest request)
    {
        var family = await db.Families.SingleOrDefaultAsync(x => x.Id == familyId);
        if (family == null)
        {
            return false;
        }

        var reason = RequiredReason(request.Reason);
        var before = new { family.Name, family.TimeZoneId, family.Status };
        if (request.Name != null)
        {
            family.Name = Required(request.Name, 120, "Family name is required");
        }

        if (request.TimeZoneId != null)
        {
            var zone = Required(request.TimeZoneId, 100, "Time zone is required");
            _ = TimeZoneInfo.FindSystemTimeZoneById(zone);
            family.TimeZoneId = zone;
        }

        if (request.Status != null)
        {
            family.Status = request.Status.Value;
        }

        family.UpdatedAt = DateTime.UtcNow;
        AddAudit(familyId, actorAccountId, "PlatformAdmin.FamilyUpdated", "Family", familyId, reason, new { before, after = request });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMemberAsync(
        Guid? actorAccountId,
        Guid familyId,
        Guid memberId,
        AdminUpdateMemberRequest request)
    {
        var member = await db.FamilyMembers.SingleOrDefaultAsync(x => x.Id == memberId && x.FamilyId == familyId && x.IsActive);
        if (member == null ||
            (request.Permission == FamilyPermission.Owner && member.Permission != FamilyPermission.Owner) ||
            (member.Permission == FamilyPermission.Owner && request.Permission != FamilyPermission.Owner))
        {
            return false;
        }

        var reason = RequiredReason(request.Reason);
        var before = new { member.Permission, member.Side };
        member.Permission = request.Permission;
        member.Side = request.Side;
        member.ConcurrencyToken = Guid.NewGuid();
        AddAudit(familyId, actorAccountId, "PlatformAdmin.MemberUpdated", "FamilyMember", memberId, reason, new { before, after = request });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TransferOwnershipAsync(
        Guid? actorAccountId,
        Guid familyId,
        AdminTransferOwnershipRequest request)
    {
        var reason = RequiredReason(request.Reason);
        var currentOwner = await db.FamilyMembers.SingleOrDefaultAsync(x =>
            x.FamilyId == familyId && x.IsActive && x.Permission == FamilyPermission.Owner);
        var newOwner = await db.FamilyMembers.SingleOrDefaultAsync(x =>
            x.Id == request.NewOwnerMemberId && x.FamilyId == familyId && x.IsActive);
        if (currentOwner == null || newOwner == null || currentOwner.Id == newOwner.Id)
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
        AddAudit(familyId, actorAccountId, "PlatformAdmin.OwnershipTransferred", "FamilyMember", newOwner.Id, reason, new
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

    public async Task<bool> RevokeInvitationAsync(
        Guid? actorAccountId,
        Guid familyId,
        Guid invitationId,
        string reason)
    {
        var value = await db.FamilyInvitations.SingleOrDefaultAsync(x =>
            x.Id == invitationId && x.FamilyId == familyId && x.ConsumedAt == null && x.RevokedAt == null);
        if (value == null)
        {
            return false;
        }

        value.RevokedAt = DateTime.UtcNow;
        value.ConcurrencyToken = Guid.NewGuid();
        AddAudit(familyId, actorAccountId, "PlatformAdmin.InvitationRevoked", "FamilyInvitation", invitationId,
            RequiredReason(reason), null);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> RevokeAccountSessionsAsync(Guid? actorAccountId, Guid accountId, string reason)
    {
        var normalizedReason = RequiredReason(reason);
        var now = DateTime.UtcNow;
        var sessions = await db.AccountSessions.Where(x => x.AccountId == accountId && x.RevokedAt == null && x.ExpiresAt > now)
            .ToListAsync();
        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        AddAudit(null, actorAccountId, "PlatformAdmin.AccountSessionsRevoked", "Account", accountId,
            normalizedReason, new { count = sessions.Count });
        await db.SaveChangesAsync();
        return sessions.Count;
    }

    public async Task<IReadOnlyList<AuditEventDto>> GetAuditAsync(Guid? familyId, int take = 100) =>
        await db.AuditEvents.AsNoTracking()
            .Where(x => familyId == null || x.FamilyId == familyId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .Select(x => new AuditEventDto(x.Id, x.FamilyId, x.ActorType, x.Action,
                x.TargetType, x.TargetId, x.Reason, x.CreatedAt))
            .ToListAsync();

    private void AddAudit(
        Guid? familyId,
        Guid? actorAccountId,
        string action,
        string targetType,
        Guid targetId,
        string reason,
        object? metadata)
    {
        db.AuditEvents.Add(new AuditEvent
        {
            FamilyId = familyId,
            ActorAccountId = actorAccountId,
            ActorType = actorAccountId == null ? "BreakGlass" : "PlatformAdmin",
            Action = action,
            TargetType = targetType,
            TargetId = targetId.ToString(),
            Reason = reason,
            MetadataJson = metadata == null ? "{}" : JsonSerializer.Serialize(metadata),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static string RequiredReason(string value) => Required(value, 500, "Reason is required");

    private static string Required(string value, int maxLength, string message)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0 || normalized.Length > maxLength)
        {
            throw new ArgumentException(message);
        }

        return normalized;
    }
}
