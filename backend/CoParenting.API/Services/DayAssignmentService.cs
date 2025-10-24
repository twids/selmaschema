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
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await _context.DayAssignments
            .Where(d => d.Date >= startDate && d.Date <= endDate)
            .OrderBy(d => d.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific day assignment
    /// </summary>
    public async Task<DayAssignment?> GetDayAssignmentAsync(DateTime date)
    {
        return await _context.DayAssignments
            .FirstOrDefaultAsync(d => d.Date.Date == date.Date);
    }

    /// <summary>
    /// Creates or updates a day assignment
    /// </summary>
    public async Task<DayAssignment> UpsertDayAssignmentAsync(DateTime date, string? parent, bool isVAB, string? comment)
    {
        var existing = await GetDayAssignmentAsync(date);

        if (existing != null)
        {
            existing.Parent = parent;
            existing.IsVAB = isVAB;
            existing.Comment = comment;
            existing.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        var newAssignment = new DayAssignment
        {
            Date = date.Date,
            Parent = parent,
            IsVAB = isVAB,
            Comment = comment,
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
        var startDate = new DateTime(year, month, 1);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var assignments = new List<DayAssignment>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            
            // Skip if already assigned
            var existing = await GetDayAssignmentAsync(date);
            if (existing != null)
            {
                assignments.Add(existing);
                continue;
            }

            // Determine week number (ISO 8601 week numbering)
            var weekNumber = GetIso8601WeekNumber(date);
            
            // Odd weeks = Parent A, Even weeks = Parent B
            var parent = weekNumber % 2 == 1 ? "A" : "B";

            var assignment = new DayAssignment
            {
                Date = date,
                Parent = parent,
                IsVAB = false,
                Comment = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.DayAssignments.Add(assignment);
            assignments.Add(assignment);
        }

        await _context.SaveChangesAsync();
        return assignments;
    }

    /// <summary>
    /// Gets ISO 8601 week number (week starts on Monday)
    /// </summary>
    private static int GetIso8601WeekNumber(DateTime date)
    {
        var day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            date = date.AddDays(3);
        }

        return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            date,
            System.Globalization.CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);
    }

    /// <summary>
    /// Gets statistics for a year
    /// </summary>
    public async Task<(int parentA, int parentB, int vab, int unassigned, int withComments)> GetYearStatisticsAsync(int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);
        var daysInYear = endDate.DayOfYear;

        var assignments = await _context.DayAssignments
            .Where(d => d.Date >= startDate && d.Date <= endDate)
            .ToListAsync();

        var parentA = assignments.Count(a => a.Parent == "A");
        var parentB = assignments.Count(a => a.Parent == "B");
        var vab = assignments.Count(a => a.IsVAB);
        var withComments = assignments.Count(a => !string.IsNullOrEmpty(a.Comment));
        var unassigned = daysInYear - parentA - parentB;

        return (parentA, parentB, vab, unassigned, withComments);
    }
}
