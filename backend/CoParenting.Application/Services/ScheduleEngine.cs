using CoParenting.Application.DTOs;
using CoParenting.Core.Entities;

namespace CoParenting.Application.Services;

public sealed record ScheduleOccurrence(ScheduleSide Side, bool HasRecurringVisit);

public static class ScheduleEngine
{
    private static readonly ScheduleSide[] AlternatingWeeks = Pattern(7, 7);
    private static readonly ScheduleSide[] TwoTwoThree =
    [
        ScheduleSide.A, ScheduleSide.A,
        ScheduleSide.B, ScheduleSide.B,
        ScheduleSide.A, ScheduleSide.A, ScheduleSide.A,
        ScheduleSide.B, ScheduleSide.B,
        ScheduleSide.A, ScheduleSide.A,
        ScheduleSide.B, ScheduleSide.B, ScheduleSide.B
    ];
    private static readonly ScheduleSide[] TwoTwoFiveFive =
    [
        ScheduleSide.A, ScheduleSide.A,
        ScheduleSide.B, ScheduleSide.B,
        ScheduleSide.A, ScheduleSide.A, ScheduleSide.A, ScheduleSide.A, ScheduleSide.A,
        ScheduleSide.B, ScheduleSide.B, ScheduleSide.B, ScheduleSide.B, ScheduleSide.B
    ];
    private static readonly ScheduleSide[] ThreeFourFourThree =
    [
        ScheduleSide.A, ScheduleSide.A, ScheduleSide.A,
        ScheduleSide.B, ScheduleSide.B, ScheduleSide.B, ScheduleSide.B,
        ScheduleSide.A, ScheduleSide.A, ScheduleSide.A, ScheduleSide.A,
        ScheduleSide.B, ScheduleSide.B, ScheduleSide.B
    ];

    public static ScheduleOccurrence GetOccurrence(
        ScheduleTemplate template,
        DateOnly anchorDate,
        ScheduleSide anchorSide,
        DateOnly date,
        ScheduleParametersDto? parameters = null)
    {
        parameters ??= new ScheduleParametersDto();
        if (template == ScheduleTemplate.PrimaryAlternateWeekends)
        {
            return PrimaryResidence(anchorDate, anchorSide, date, parameters);
        }

        var pattern = template switch
        {
            ScheduleTemplate.AlternatingWeeks => AlternatingWeeks,
            ScheduleTemplate.TwoTwoThree => TwoTwoThree,
            ScheduleTemplate.TwoTwoFiveFive => TwoTwoFiveFive,
            ScheduleTemplate.ThreeFourFourThree => ThreeFourFourThree,
            _ => throw new ArgumentOutOfRangeException(nameof(template))
        };
        var index = Mod(date.DayNumber - anchorDate.DayNumber, pattern.Length);
        var side = pattern[index];
        if (anchorSide == ScheduleSide.B)
        {
            side = Opposite(side);
        }

        return new ScheduleOccurrence(side, false);
    }

    private static ScheduleOccurrence PrimaryResidence(
        DateOnly anchorDate,
        ScheduleSide primary,
        DateOnly date,
        ScheduleParametersDto parameters)
    {
        if (parameters.WeekendLengthDays is < 1 or > 7)
        {
            throw new ArgumentException("Weekend length must be between one and seven days");
        }

        var secondary = Opposite(primary);
        var offset = date.DayNumber - anchorDate.DayNumber;
        var cycle = FloorDiv(offset, 14);
        var isAlternateWeekend = IsInsideAlternateWeekend(anchorDate.AddDays(cycle * 14), date, parameters) ||
                                 IsInsideAlternateWeekend(anchorDate.AddDays((cycle - 1) * 14), date, parameters);
        var recurringVisit = parameters.RecurringWeekday != null && date.DayOfWeek == parameters.RecurringWeekday;
        var side = isAlternateWeekend || (recurringVisit && parameters.RecurringWeekdayOvernight)
            ? secondary
            : primary;
        return new ScheduleOccurrence(side, recurringVisit);
    }

    private static bool IsInsideAlternateWeekend(
        DateOnly cycleStart,
        DateOnly date,
        ScheduleParametersDto parameters)
    {
        var secondWeek = cycleStart.AddDays(7);
        var delta = Mod((int)parameters.WeekendStartsOn - (int)secondWeek.DayOfWeek, 7);
        var start = secondWeek.AddDays(delta);
        var endExclusive = start.AddDays(parameters.WeekendLengthDays);
        return date >= start && date < endExclusive;
    }

    private static ScheduleSide[] Pattern(int aDays, int bDays) =>
        Enumerable.Repeat(ScheduleSide.A, aDays)
            .Concat(Enumerable.Repeat(ScheduleSide.B, bDays))
            .ToArray();

    private static ScheduleSide Opposite(ScheduleSide side) =>
        side == ScheduleSide.A ? ScheduleSide.B : ScheduleSide.A;

    private static int Mod(int value, int divisor) => ((value % divisor) + divisor) % divisor;

    private static int FloorDiv(int value, int divisor) =>
        value >= 0 ? value / divisor : -((-value + divisor - 1) / divisor);
}
