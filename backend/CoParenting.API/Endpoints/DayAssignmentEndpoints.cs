using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;

namespace CoParenting.API.Endpoints;

public static class DayAssignmentEndpoints
{
    public static void MapDayAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/days")
            .WithTags("Day Assignments")
            .RequireAuthorization();

        // GET /api/days/{year}/{month}
        group.MapGet("/{year}/{month}", async (int year, int month, IDayAssignmentService service) =>
        {
            var assignments = await service.GetMonthAssignmentsAsync(year, month);
            var dtos = assignments.Select(MapToDto).ToList();
            return Results.Ok(new MonthDataDto(year, month, dtos));
        })
        .WithName("GetMonthAssignments")
        .WithDescription("Get all day assignments for a specific month")
        .Produces<MonthDataDto>();

        // GET /api/days/{year}/{month}/{day}
        group.MapGet("/{year}/{month}/{day}", async (int year, int month, int day, IDayAssignmentService service) =>
        {
            var date = new DateOnly(year, month, day);
            var assignment = await service.GetDayAssignmentAsync(date);

            if (assignment == null)
                return Results.NotFound();

            return Results.Ok(MapToDto(assignment));
        })
        .WithName("GetDayAssignment")
        .WithDescription("Get a specific day assignment")
        .Produces<DayAssignmentDto>()
        .Produces(StatusCodes.Status404NotFound);

        // PUT /api/days/{year}/{month}/{day}
        group.MapPut("/{year}/{month}/{day}", async (int year, int month, int day, UpdateDayAssignmentDto dto, IDayAssignmentService service) =>
        {
            var date = new DateOnly(year, month, day);
            var assignment = await service.UpsertDayAssignmentAsync(date, dto.Parent, dto.IsVAB, dto.SpecialStatus);
            return Results.Ok(MapToDto(assignment));
        })
        .WithName("UpdateDayAssignment")
        .WithDescription("Create or update a day assignment")
        .Produces<DayAssignmentDto>();

        // POST /api/days/{year}/{month}/initialize
        group.MapPost("/{year}/{month}/initialize", async (int year, int month, IDayAssignmentService service) =>
        {
            var assignments = await service.InitializeMonthWithDefaultsAsync(year, month);
            var dtos = assignments.Select(MapToDto).ToList();
            return Results.Ok(new MonthDataDto(year, month, dtos));
        })
        .WithName("InitializeMonth")
        .WithDescription("Initialize month with default assignments (odd weeks Parent A, even weeks Parent B)")
        .Produces<MonthDataDto>();
    }

    private static DayAssignmentDto MapToDto(DayAssignment assignment)
    {
        var parentAComments = assignment.Comments
            .Where(c => c.Parent == "A")
            .Select(c => new CommentDto(c.Id, c.DayAssignmentId, c.Parent, c.CommentText, c.CreatedAt, c.ModifiedAt))
            .OrderBy(c => c.CreatedAt)
            .ToList();

        var parentBComments = assignment.Comments
            .Where(c => c.Parent == "B")
            .Select(c => new CommentDto(c.Id, c.DayAssignmentId, c.Parent, c.CommentText, c.CreatedAt, c.ModifiedAt))
            .OrderBy(c => c.CreatedAt)
            .ToList();

        return new DayAssignmentDto(
            assignment.Id,
            assignment.Date,
            assignment.Parent,
            assignment.IsVAB,
            assignment.SpecialStatus,
            parentAComments,
            parentBComments
        );
    }
}
