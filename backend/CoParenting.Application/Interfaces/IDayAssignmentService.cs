using CoParenting.Core.Entities;

namespace CoParenting.Application.Interfaces;

public interface IDayAssignmentService
{
    Task<List<DayAssignment>> GetMonthAssignmentsAsync(int year, int month);
    Task<DayAssignment?> GetDayAssignmentAsync(DateTime date);
    Task<DayAssignment> UpsertDayAssignmentAsync(DateTime date, string? parent, bool isVAB, string? specialStatus);
    Task<List<DayAssignment>> InitializeMonthWithDefaultsAsync(int year, int month);
    Task<(int parentA, int parentB, int vab, int unassigned, int withComments)> GetYearStatisticsAsync(int year);
}
