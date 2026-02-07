using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.API.Services;

/// <summary>
/// Service for managing day assignments with business logic
/// </summary>
public class DayAssignmentService
{
    private readonly CoParentingDbContext _context;

    public DayAssignmentService(CoParentingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all day assignments for a specific month
    /// </summary>
    public async Task<List<DayAssignment>> GetMonthAssignmentsAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await _context.DayAssignments
            .Include(d => d.Comments)
            .Where(d => d.Date.Year == year && d.Date.Month == month)
            .OrderBy(d => d.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific day assignment
    /// </summary>
    public async Task<DayAssignment?> GetDayAssignmentAsync(DateTime date)
    {
        var utcDate = date.Kind == DateTimeKind.Unspecified ? new DateTime(date.Ticks, DateTimeKind.Utc) : date.ToUniversalTime();
        return await _context.DayAssignments
            .Include(d => d.Comments)
            .FirstOrDefaultAsync(d => d.Date.Date == utcDate.Date);
    }

    /// <summary>
    /// Creates or updates a day assignment
    /// </summary>
    public async Task<DayAssignment> UpsertDayAssignmentAsync(DateTime date, string? parent, bool isVAB, string? specialStatus)
    {
        var existing = await GetDayAssignmentAsync(date);

        if (existing != null)
        {
            existing.Parent = parent;
            existing.IsVAB = isVAB;
            existing.SpecialStatus = specialStatus;
            existing.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        var utcDate = date.Kind == DateTimeKind.Unspecified ? new DateTime(date.Ticks, DateTimeKind.Utc) : date.ToUniversalTime();
        var newAssignment = new DayAssignment
        {
            Date = new DateTime(utcDate.Year, utcDate.Month, utcDate.Day, 0, 0, 0, DateTimeKind.Utc),
            Parent = parent,
            IsVAB = isVAB,
            SpecialStatus = specialStatus,
            CreatedAt = DateTime.UtcNow
        };

        _context.DayAssignments.Add(newAssignment);
        await _context.SaveChangesAsync();
        return newAssignment;
    }

    /// <summary>
    /// Initializes a month with default assignments (odd weeks Parent A, even weeks Parent B)
    /// Week starts on Monday
    /// </summary>
    public async Task<List<DayAssignment>> InitializeMonthWithDefaultsAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        // Fetch all existing assignments for the month in a single query
        var existingAssignments = await _context.DayAssignments
            .Include(d => d.Comments)
            .Where(d => d.Date.Year == year && d.Date.Month == month)
            .ToDictionaryAsync(d => d.Date.Date, d => d);

        var assignments = new List<DayAssignment>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);

            // Skip if already assigned
            if (existingAssignments.TryGetValue(date, out var existing))
            {
                assignments.Add(existing);
                continue;
            }

            // Determine week number (starts on Monday)
            var weekNumber = GetWeekNumber(date);

            // Odd weeks = Parent A, Even weeks = Parent B
            var parent = weekNumber % 2 == 1 ? "A" : "B";

            var assignment = new DayAssignment
            {
                Date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc),
                Parent = parent,
                IsVAB = false,
                SpecialStatus = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.DayAssignments.Add(assignment);
            assignments.Add(assignment);
        }

        await _context.SaveChangesAsync();
        return assignments;
    }

    /// <summary>
    /// Gets week number where weeks start on Monday (exchange day)
    /// Week 1 = from first Monday of month onwards (or from day 1 if month starts on Mon-Sun before first Mon)
    /// </summary>
    private static int GetWeekNumber(DateTime date)
    {
        // Get the first day of the month
        var firstDay = new DateTime(date.Year, date.Month, 1);
        // Get the Monday of the week containing the first day (may be in previous month)
        // DayOfWeek: Sunday=0, Monday=1, ..., Saturday=6
        var firstMonday = firstDay.AddDays(1 - (int)firstDay.DayOfWeek);
        // Count weeks from first Monday to current date
        var weekNumber = (int)((date - firstMonday).TotalDays / 7) + 1;
        return weekNumber;
    }

    /// <summary>
    /// Gets statistics for a year
    /// </summary>
    public async Task<(int parentA, int parentB, int vab, int unassigned, int withComments)> GetYearStatisticsAsync(int year)
    {
        var assignments = await _context.DayAssignments
            .Include(d => d.Comments)
            .Where(d => d.Date.Year == year)
            .ToListAsync();

        var daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;
        var parentA = assignments.Count(a => a.Parent == "A");
        var parentB = assignments.Count(a => a.Parent == "B");
        var vab = assignments.Count(a => a.IsVAB);
        var withComments = assignments.Count(a => a.Comments.Any());
        var unassigned = daysInYear - parentA - parentB;

        return (parentA, parentB, vab, unassigned, withComments);
    }
}
