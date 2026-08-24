using CoParenting.Application.DTOs;
using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using FluentAssertions;

namespace CoParenting.Tests.Unit;

public sealed class ScheduleEngineTests
{
    public static IEnumerable<object[]> ExactPatterns()
    {
        yield return [ScheduleTemplate.AlternatingWeeks, "AAAAAAABBBBBBB"];
        yield return [ScheduleTemplate.TwoTwoThree, "AABBAAABBAABBB"];
        yield return [ScheduleTemplate.TwoTwoFiveFive, "AABBAAAAABBBBB"];
        yield return [ScheduleTemplate.ThreeFourFourThree, "AAABBBBAAAABBB"];
    }

    [Theory]
    [MemberData(nameof(ExactPatterns))]
    public void Template_has_the_exact_fourteen_day_cycle(ScheduleTemplate template, string expected)
    {
        var anchor = new DateOnly(2026, 9, 1);
        var actual = string.Concat(Enumerable.Range(0, 14)
            .Select(offset => ScheduleEngine.GetOccurrence(template, anchor, ScheduleSide.A, anchor.AddDays(offset)).Side));
        actual.Should().Be(expected);
    }

    [Fact]
    public void Anchor_side_B_inverts_the_pattern_and_works_before_anchor()
    {
        var anchor = new DateOnly(2026, 9, 8);
        ScheduleEngine.GetOccurrence(ScheduleTemplate.AlternatingWeeks, anchor, ScheduleSide.B, anchor).Side.Should().Be(ScheduleSide.B);
        ScheduleEngine.GetOccurrence(ScheduleTemplate.AlternatingWeeks, anchor, ScheduleSide.B, anchor.AddDays(-1)).Side.Should().Be(ScheduleSide.A);
    }

    [Fact]
    public void Primary_residence_supports_alternate_weekend_and_recurring_overnight()
    {
        var anchor = new DateOnly(2026, 8, 31); // Monday
        var parameters = new ScheduleParametersDto(DayOfWeek.Friday, 3, DayOfWeek.Wednesday, true);
        ScheduleEngine.GetOccurrence(ScheduleTemplate.PrimaryAlternateWeekends, anchor, ScheduleSide.A, new DateOnly(2026, 9, 11), parameters).Side.Should().Be(ScheduleSide.B);
        ScheduleEngine.GetOccurrence(ScheduleTemplate.PrimaryAlternateWeekends, anchor, ScheduleSide.A, new DateOnly(2026, 9, 2), parameters)
            .Should().Be(new ScheduleOccurrence(ScheduleSide.B, true));
        ScheduleEngine.GetOccurrence(ScheduleTemplate.PrimaryAlternateWeekends, anchor, ScheduleSide.A, new DateOnly(2026, 9, 3), parameters).Side.Should().Be(ScheduleSide.A);
    }
}
