using CoParenting.Application.DTOs;
using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CoParenting.Tests.Unit;

public sealed class ScheduleServiceTests
{
    [Fact]
    public async Task Exact_preview_is_required_before_future_version_is_created()
    {
        await using var db = Database();
        var setup = await SetupAsync(db);
        var service = new ScheduleService(db, new MemoryCache(new MemoryCacheOptions()));
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var draft = Draft(tomorrow, ScheduleTemplate.TwoTwoThree);
        await FluentActions.Invoking(() => service.CreateVersionAsync(setup.Account.Id, setup.Family.Id, setup.Calendar.Id, draft))
            .Should().ThrowAsync<ArgumentException>().WithMessage("*Preview*");
        var preview = await service.PreviewAsync(setup.Account.Id, setup.Family.Id, setup.Calendar.Id, draft);
        preview!.Days.Should().HaveCount(42);
        var version = await service.CreateVersionAsync(setup.Account.Id, setup.Family.Id, setup.Calendar.Id, draft);
        version!.Template.Should().Be(ScheduleTemplate.TwoTwoThree);
    }

    [Fact]
    public async Task Version_boundary_and_manual_override_are_projected_without_destroying_history()
    {
        await using var db = Database();
        var setup = await SetupAsync(db);
        var first = new DateOnly(2026, 10, 1);
        db.ScheduleVersions.AddRange(
            Version(setup, first, ScheduleTemplate.AlternatingWeeks, ScheduleSide.A),
            Version(setup, first.AddDays(10), ScheduleTemplate.AlternatingWeeks, ScheduleSide.B));
        await db.SaveChangesAsync();
        var service = new ScheduleService(db, new MemoryCache(new MemoryCacheOptions()));
        var month = await service.GetMonthAsync(setup.Account.Id, setup.Family.Id, setup.Calendar.Id, 2026, 10);
        month!.Days.Single(x => x.Date == first).Side.Should().Be(ScheduleSide.A);
        month.Days.Single(x => x.Date == first.AddDays(10)).Side.Should().Be(ScheduleSide.B);
        await service.SetOverrideAsync(setup.Account.Id, setup.Family.Id, setup.Calendar.Id, first.AddDays(10),
            new DayOverrideRequest(ScheduleSide.A, false, null, null, null));
        (await service.GetMonthAsync(setup.Account.Id, setup.Family.Id, setup.Calendar.Id, 2026, 10))!
            .Days.Single(x => x.Date == first.AddDays(10)).Should().Match<CalendarDayDto>(x => x.Side == ScheduleSide.A && x.IsOverride);
        (await service.ClearOverrideAsync(setup.Account.Id, setup.Family.Id, setup.Calendar.Id, first.AddDays(10))).Should().BeTrue();
        (await service.GetMonthAsync(setup.Account.Id, setup.Family.Id, setup.Calendar.Id, 2026, 10))!
            .Days.Single(x => x.Date == first.AddDays(10)).Should().Match<CalendarDayDto>(x => x.Side == ScheduleSide.B && !x.IsOverride);
        (await db.ScheduleVersions.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Viewer_cannot_preview_or_mutate_calendar()
    {
        await using var db = Database();
        var setup = await SetupAsync(db);
        var viewer = new Account { Email = "viewer@test", NormalizedEmail = "VIEWER@TEST", DisplayName = "Viewer", CreatedAt = DateTime.UtcNow };
        db.Accounts.Add(viewer);
        db.FamilyMembers.Add(new FamilyMember { FamilyId = setup.Family.Id, Account = viewer, Permission = FamilyPermission.Viewer, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var service = new ScheduleService(db, new MemoryCache(new MemoryCacheOptions()));
        (await service.PreviewAsync(viewer.Id, setup.Family.Id, setup.Calendar.Id, Draft(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), ScheduleTemplate.AlternatingWeeks))).Should().BeNull();
        (await service.SetOverrideAsync(viewer.Id, setup.Family.Id, setup.Calendar.Id, new DateOnly(2026, 10, 2), new DayOverrideRequest(ScheduleSide.B, false, null, null, null))).Should().BeNull();
    }

    [Theory]
    [InlineData(ExchangeDetailLevel.Day, "18:00", null)]
    [InlineData(ExchangeDetailLevel.DayAndTime, null, null)]
    [InlineData(ExchangeDetailLevel.DayTimeAndPlace, "18:00", null)]
    public async Task Exchange_detail_level_is_enforced(
        ExchangeDetailLevel detailLevel,
        string? time,
        string? place)
    {
        await using var db = Database();
        var setup = await SetupAsync(db);
        setup.Family.ExchangeDetailLevel = detailLevel;
        await db.SaveChangesAsync();
        var service = new ScheduleService(db, new MemoryCache(new MemoryCacheOptions()));
        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var request = new ScheduleDraftRequest(
            ScheduleTemplate.AlternatingWeeks, start, ScheduleSide.A, start,
            new ScheduleParametersDto(), time == null ? null : TimeOnly.Parse(time), place);
        await FluentActions.Invoking(() => service.PreviewAsync(
            setup.Account.Id, setup.Family.Id, setup.Calendar.Id, request)).Should().ThrowAsync<ArgumentException>();
    }

    private static ScheduleDraftRequest Draft(DateOnly start, ScheduleTemplate template) =>
        new(template, start, ScheduleSide.A, start, new ScheduleParametersDto(), null, null);

    private static ScheduleVersion Version(
        (Account Account, Family Family, FamilyMember Owner, ResidenceCalendar Calendar) setup,
        DateOnly effective,
        ScheduleTemplate template,
        ScheduleSide side) => new()
    {
        FamilyId = setup.Family.Id, CalendarId = setup.Calendar.Id, EffectiveFrom = effective,
        AnchorDate = effective, AnchorSide = side, Template = template, ParametersJson = "{}",
        CreatedByMemberId = setup.Owner.Id, CreatedAt = DateTime.UtcNow
    };

    private static async Task<(Account Account, Family Family, FamilyMember Owner, ResidenceCalendar Calendar)> SetupAsync(CoParentingDbContext db)
    {
        var account = new Account { Email = "owner@test", NormalizedEmail = "OWNER@TEST", DisplayName = "Owner", CreatedAt = DateTime.UtcNow };
        var family = new Family { Name = "Family", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var owner = new FamilyMember { Account = account, Family = family, Permission = FamilyPermission.Owner, JoinedAt = DateTime.UtcNow };
        var calendar = new ResidenceCalendar { Family = family, Name = "Calendar", CreatedAt = DateTime.UtcNow };
        db.AddRange(account, family, owner, calendar);
        await db.SaveChangesAsync();
        return (account, family, owner, calendar);
    }

    private static CoParentingDbContext Database() => new(new DbContextOptionsBuilder<CoParentingDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
