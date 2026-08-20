using CoParenting.Application.DTOs;
using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Tests.Unit;

public sealed class AdminServiceTests
{
    [Fact]
    public async Task Support_action_requires_reason_and_audits_without_private_content()
    {
        await using var db = new CoParentingDbContext(new DbContextOptionsBuilder<CoParentingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var family = new Family { Name = "Family", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Families.Add(family);
        db.Comments.Add(new Comment
        {
            Family = family,
            Calendar = new ResidenceCalendar { Family = family, Name = "Calendar", CreatedAt = DateTime.UtcNow },
            AuthorMember = new FamilyMember
            {
                Family = family,
                Account = new Account { Email = "x@test", NormalizedEmail = "X@TEST", DisplayName = "X", CreatedAt = DateTime.UtcNow },
                Permission = FamilyPermission.Owner,
                JoinedAt = DateTime.UtcNow
            },
            Date = new DateOnly(2026, 1, 1), Text = "privat text", CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var service = new AdminService(db);
        await FluentActions.Invoking(() => service.UpdateFamilyAsync(null, family.Id,
            new AdminUpdateFamilyRequest(null, null, FamilyStatus.Suspended, ""))).Should().ThrowAsync<ArgumentException>();
        (await service.UpdateFamilyAsync(null, family.Id,
            new AdminUpdateFamilyRequest(null, null, FamilyStatus.Suspended, "Supportorsak"))).Should().BeTrue();
        var audit = await service.GetAuditAsync(family.Id);
        audit.Should().ContainSingle(x => x.Action == "PlatformAdmin.FamilyUpdated" && x.Reason == "Supportorsak");
        var detailJson = System.Text.Json.JsonSerializer.Serialize(await service.GetFamilyAsync(family.Id));
        detailJson.Should().NotContain("privat text");
    }
}
