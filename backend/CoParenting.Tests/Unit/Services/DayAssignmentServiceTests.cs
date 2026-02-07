using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoParenting.Tests.Unit.Services;

public class DayAssignmentServiceTests : IDisposable
{
    private readonly CoParentingDbContext _context;
    private readonly DayAssignmentService _service;

    public DayAssignmentServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<CoParentingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CoParentingDbContext(options);

        // Create service
        _service = new DayAssignmentService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetWeekNumber Tests

    /// <summary>
    /// QUIRK DOCUMENTATION: GetWeekNumber uses a custom week calculation where:
    /// - Week 1 starts from the Monday of the week containing the 1st of the month (may be in previous month)
    /// - Weeks increment every Monday throughout the month
    /// - This differs from ISO 8601 week numbers but provides consistent parent assignment
    /// </summary>
    [Fact]
    public void GetWeekNumber_MonthStartsOnMonday_ShouldReturnWeek1()
    {
        // Arrange: June 1, 2026 is a Monday
        var date = new DateTime(2026, 6, 1);

        // Act: Use reflection to call private method
        var weekNumber = CallGetWeekNumber(date);

        // Assert
        weekNumber.Should().Be(1, "June 1, 2026 is a Monday, so it's the start of week 1");
    }

    [Fact]
    public void GetWeekNumber_MonthStartsOnSunday_FirstDayIsWeek1()
    {
        // Arrange: Feb 1, 2026 is a Sunday (EDGE CASE mentioned in plan)
        var date = new DateTime(2026, 2, 1);

        // Act
        var weekNumber = CallGetWeekNumber(date);

        // Assert
        // QUIRK: Sunday Feb 1 is in the same week as Monday Feb 2, so it's week 1
        weekNumber.Should().Be(1, "Feb 1, 2026 (Sunday) is before the first Monday but counted as week 1");
    }

    [Fact]
    public void GetWeekNumber_MonthStartsOnTuesday_FirstDayIsWeek1()
    {
        // Arrange: July 1, 2025 is a Tuesday
        var date = new DateTime(2025, 7, 1);

        // Act
        var weekNumber = CallGetWeekNumber(date);

        // Assert
        // The first Monday is June 30 (previous month), so July 1 is part of week 1
        weekNumber.Should().Be(1, "July 1, 2025 (Tuesday) is in the same week as the Monday before it");
    }

    [Fact]
    public void GetWeekNumber_SecondMondayOfMonth_ShouldReturnWeek2()
    {
        // Arrange: Feb 9, 2026 is the second Monday (Feb 2 was first Monday)
        var date = new DateTime(2026, 2, 9);

        // Act
        var weekNumber = CallGetWeekNumber(date);

        // Assert
        weekNumber.Should().Be(2, "Feb 9, 2026 is the second Monday, so it's week 2");
    }

    [Fact]
    public void GetWeekNumber_LastDayOfMonth_CorrectWeekNumber()
    {
        // Arrange: Feb 28, 2026 is the last day of February (Saturday)
        var date = new DateTime(2026, 2, 28);

        // Act
        var weekNumber = CallGetWeekNumber(date);

        // Assert
        // Feb 1 is Sunday, so first Monday is Feb 2
        // Feb 2-8 = week 1, Feb 9-15 = week 2, Feb 16-22 = week 3, Feb 23-28 = week 4
        weekNumber.Should().Be(4, "Feb 28, 2026 is in the 4th week");
    }

    [Fact]
    public void GetWeekNumber_MiddleOfMonth_CorrectWeekNumber()
    {
        // Arrange: Feb 15, 2026 (Sunday)
        var date = new DateTime(2026, 2, 15);

        // Act
        var weekNumber = CallGetWeekNumber(date);

        // Assert
        weekNumber.Should().Be(2, "Feb 15, 2026 is in the second full week");
    }

    #endregion

    #region InitializeMonthWithDefaultsAsync Tests

    [Fact]
    public async Task InitializeMonth_ShouldCreateAllDaysInMonth()
    {
        // Arrange
        var year = 2026;
        var month = 2; // February 2026 has 28 days

        // Act
        var assignments = await _service.InitializeMonthWithDefaultsAsync(year, month);

        // Assert
        assignments.Should().HaveCount(28, "February 2026 has 28 days");
        assignments.Should().AllSatisfy(a =>
        {
            a.Date.Year.Should().Be(year);
            a.Date.Month.Should().Be(month);
            a.IsVAB.Should().BeFalse();
            a.SpecialStatus.Should().BeNull();
        });
    }

    [Fact]
    public async Task InitializeMonth_ShouldFollowOddEvenWeekPattern()
    {
        // Arrange
        var year = 2026;
        var month = 2; // Feb 2026 starts on Sunday

        // Act
        var assignments = await _service.InitializeMonthWithDefaultsAsync(year, month);

        // Assert
        // Feb 1-8: Week 1 (odd) = Parent A
        // Feb 9-15: Week 2 (even) = Parent B
        // Feb 16-22: Week 3 (odd) = Parent A
        // Feb 23-28: Week 4 (even) = Parent B
        
        // Check first week (1-8)
        assignments.Where(a => a.Date.Day >= 1 && a.Date.Day <= 8)
            .Should().AllSatisfy(a => a.Parent.Should().Be("A", $"Day {a.Date.Day} is in week 1 (odd)"));
        
        // Check second week (9-15)
        assignments.Where(a => a.Date.Day >= 9 && a.Date.Day <= 15)
            .Should().AllSatisfy(a => a.Parent.Should().Be("B", $"Day {a.Date.Day} is in week 2 (even)"));
        
        // Check third week (16-22)
        assignments.Where(a => a.Date.Day >= 16 && a.Date.Day <= 22)
            .Should().AllSatisfy(a => a.Parent.Should().Be("A", $"Day {a.Date.Day} is in week 3 (odd)"));
        
        // Check fourth week (23-28)
        assignments.Where(a => a.Date.Day >= 23 && a.Date.Day <= 28)
            .Should().AllSatisfy(a => a.Parent.Should().Be("B", $"Day {a.Date.Day} is in week 4 (even)"));
    }

    [Fact]
    public async Task InitializeMonth_ShouldNotOverwriteExistingAssignments()
    {
        // Arrange
        var year = 2026;
        var month = 2;
        
        // Create an existing assignment for Feb 5 with custom values
        var existingDate = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc);
        var existing = new DayAssignment
        {
            Date = existingDate,
            Parent = "B", // Manually assigned to B even though week 1 should be A
            IsVAB = true,
            SpecialStatus = "Holiday",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.DayAssignments.Add(existing);
        await _context.SaveChangesAsync();

        // Act
        var assignments = await _service.InitializeMonthWithDefaultsAsync(year, month);

        // Assert
        var feb5 = assignments.First(a => a.Date.Day == 5);
        feb5.Parent.Should().Be("B", "Existing assignment should not be overwritten");
        feb5.IsVAB.Should().BeTrue("Existing VAB flag should be preserved");
        feb5.SpecialStatus.Should().Be("Holiday", "Existing special status should be preserved");
    }

    [Fact]
    public async Task InitializeMonth_LeapYear_ShouldCreate29Days()
    {
        // Arrange
        var year = 2024; // Leap year
        var month = 2;

        // Act
        var assignments = await _service.InitializeMonthWithDefaultsAsync(year, month);

        // Assert
        assignments.Should().HaveCount(29, "February 2024 is a leap year with 29 days");
    }

    #endregion

    #region UpsertDayAssignmentAsync Tests

    [Fact]
    public async Task UpsertDay_NewDay_ShouldCreateAssignment()
    {
        // Arrange
        var date = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);
        var parent = "A";
        var isVAB = false;
        var specialStatus = "Normal";

        // Act
        var result = await _service.UpsertDayAssignmentAsync(date, parent, isVAB, specialStatus);

        // Assert
        result.Should().NotBeNull();
        result.Date.Date.Should().Be(date.Date);
        result.Parent.Should().Be(parent);
        result.IsVAB.Should().Be(isVAB);
        result.SpecialStatus.Should().Be(specialStatus);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.ModifiedAt.Should().BeNull();

        // Verify it was saved to database
        var saved = await _context.DayAssignments.FirstOrDefaultAsync(d => d.Date.Date == date.Date);
        saved.Should().NotBeNull();
        saved!.Parent.Should().Be(parent);
    }

    [Fact]
    public async Task UpsertDay_ExistingDay_ShouldUpdateAssignment()
    {
        // Arrange
        var date = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);
        
        // Create initial assignment
        var initial = new DayAssignment
        {
            Date = date,
            Parent = "A",
            IsVAB = false,
            SpecialStatus = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.DayAssignments.Add(initial);
        await _context.SaveChangesAsync();
        var initialId = initial.Id;

        // Act: Update the assignment
        var result = await _service.UpsertDayAssignmentAsync(date, "B", true, "VAB Day");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(initialId, "Should update existing record, not create new one");
        result.Date.Date.Should().Be(date.Date);
        result.Parent.Should().Be("B", "Parent should be updated");
        result.IsVAB.Should().BeTrue("IsVAB should be updated");
        result.SpecialStatus.Should().Be("VAB Day", "SpecialStatus should be updated");
        result.ModifiedAt.Should().NotBeNull();
        result.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify database has only one record
        var count = await _context.DayAssignments.CountAsync(d => d.Date.Date == date.Date);
        count.Should().Be(1, "Should only have one record for the date");
    }

    [Fact]
    public async Task UpsertDay_UnspecifiedDateKind_ShouldConvertToUtc()
    {
        // Arrange
        var date = new DateTime(2026, 2, 15, 12, 30, 45, DateTimeKind.Unspecified);

        // Act
        var result = await _service.UpsertDayAssignmentAsync(date, "A", false, null);

        // Assert
        result.Date.Kind.Should().Be(DateTimeKind.Utc);
        result.Date.Hour.Should().Be(0);
        result.Date.Minute.Should().Be(0);
        result.Date.Second.Should().Be(0);
    }

    #endregion

    #region GetMonthAssignmentsAsync Tests

    [Fact]
    public async Task GetMonth_ShouldReturnAllDaysWithComments()
    {
        // Arrange
        var year = 2026;
        var month = 2;
        
        // Create assignments for Feb 1-5
        for (int day = 1; day <= 5; day++)
        {
            var assignment = new DayAssignment
            {
                Date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc),
                Parent = day % 2 == 1 ? "A" : "B",
                IsVAB = false,
                SpecialStatus = null,
                CreatedAt = DateTime.UtcNow
            };
            
            // Add comment to Feb 2
            if (day == 2)
            {
                assignment.Comments = new List<Comment>
                {
                    new Comment
                    {
                        Parent = "A",
                        CommentText = "Test comment",
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            
            _context.DayAssignments.Add(assignment);
        }
        await _context.SaveChangesAsync();

        // Act
        var results = await _service.GetMonthAssignmentsAsync(year, month);

        // Assert
        results.Should().HaveCount(5);
        results.Should().BeInAscendingOrder(a => a.Date);
        
        var feb2 = results.First(a => a.Date.Day == 2);
        feb2.Comments.Should().HaveCount(1);
        feb2.Comments.First().CommentText.Should().Be("Test comment");
    }

    [Fact]
    public async Task GetMonth_EmptyMonth_ShouldReturnEmptyList()
    {
        // Arrange
        var year = 2026;
        var month = 3;

        // Act
        var results = await _service.GetMonthAssignmentsAsync(year, month);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region GetYearStatisticsAsync Tests

    [Fact]
    public async Task GetYearStatistics_ShouldCountParentADaysCorrectly()
    {
        // Arrange
        var year = 2026;
        
        // Create 10 days for Parent A, 5 for Parent B
        for (int i = 1; i <= 10; i++)
        {
            _context.DayAssignments.Add(new DayAssignment
            {
                Date = new DateTime(year, 1, i, 0, 0, 0, DateTimeKind.Utc),
                Parent = "A",
                IsVAB = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        for (int i = 11; i <= 15; i++)
        {
            _context.DayAssignments.Add(new DayAssignment
            {
                Date = new DateTime(year, 1, i, 0, 0, 0, DateTimeKind.Utc),
                Parent = "B",
                IsVAB = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (parentA, parentB, vab, unassigned, withComments) = await _service.GetYearStatisticsAsync(year);

        // Assert
        parentA.Should().Be(10, "Should count 10 days for Parent A");
        parentB.Should().Be(5, "Should count 5 days for Parent B");
        unassigned.Should().Be(365 - 15, "2026 is not a leap year, so 365 - 15 assigned days");
    }

    [Fact]
    public async Task GetYearStatistics_ShouldCountVABDaysCorrectly()
    {
        // Arrange
        var year = 2026;
        
        // Create 3 VAB days
        for (int i = 1; i <= 3; i++)
        {
            _context.DayAssignments.Add(new DayAssignment
            {
                Date = new DateTime(year, 1, i, 0, 0, 0, DateTimeKind.Utc),
                Parent = "A",
                IsVAB = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Create 2 non-VAB days
        for (int i = 4; i <= 5; i++)
        {
            _context.DayAssignments.Add(new DayAssignment
            {
                Date = new DateTime(year, 1, i, 0, 0, 0, DateTimeKind.Utc),
                Parent = "B",
                IsVAB = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (parentA, parentB, vab, unassigned, withComments) = await _service.GetYearStatisticsAsync(year);

        // Assert
        vab.Should().Be(3, "Should count 3 VAB days");
    }

    [Fact]
    public async Task GetYearStatistics_ShouldCountDaysWithCommentsCorrectly()
    {
        // Arrange
        var year = 2026;
        
        // Create day with comment
        var dayWithComment = new DayAssignment
        {
            Date = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Parent = "A",
            IsVAB = false,
            CreatedAt = DateTime.UtcNow,
            Comments = new List<Comment>
            {
                new Comment { Parent = "A", CommentText = "Comment 1", CreatedAt = DateTime.UtcNow },
                new Comment { Parent = "B", CommentText = "Comment 2", CreatedAt = DateTime.UtcNow }
            }
        };
        _context.DayAssignments.Add(dayWithComment);
        
        // Create day without comment
        _context.DayAssignments.Add(new DayAssignment
        {
            Date = new DateTime(year, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            Parent = "B",
            IsVAB = false,
            CreatedAt = DateTime.UtcNow
        });
        
        await _context.SaveChangesAsync();

        // Act
        var (parentA, parentB, vab, unassigned, withComments) = await _service.GetYearStatisticsAsync(year);

        // Assert
        withComments.Should().Be(1, "Only one day has comments (even with multiple comments)");
    }

    [Fact]
    public async Task GetYearStatistics_LeapYear_ShouldUse366Days()
    {
        // Arrange
        var year = 2024; // Leap year
        
        // Create 10 assigned days
        for (int i = 1; i <= 10; i++)
        {
            _context.DayAssignments.Add(new DayAssignment
            {
                Date = new DateTime(year, 1, i, 0, 0, 0, DateTimeKind.Utc),
                Parent = "A",
                IsVAB = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (parentA, parentB, vab, unassigned, withComments) = await _service.GetYearStatisticsAsync(year);

        // Assert
        unassigned.Should().Be(366 - 10, "2024 is a leap year with 366 days");
    }

    [Fact]
    public async Task GetYearStatistics_EmptyYear_ShouldReturnAllUnassigned()
    {
        // Arrange
        var year = 2026;

        // Act (no assignments created)
        var (parentA, parentB, vab, unassigned, withComments) = await _service.GetYearStatisticsAsync(year);

        // Assert
        parentA.Should().Be(0);
        parentB.Should().Be(0);
        vab.Should().Be(0);
        withComments.Should().Be(0);
        unassigned.Should().Be(365, "2026 has 365 days");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper to call the private GetWeekNumber method using reflection
    /// </summary>
    private int CallGetWeekNumber(DateTime date)
    {
        var method = typeof(DayAssignmentService).GetMethod(
            "GetWeekNumber",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );
        
        if (method == null)
            throw new InvalidOperationException("GetWeekNumber method not found");
        
        var result = method.Invoke(null, new object[] { date });
        return (int)result!;
    }

    #endregion
}
