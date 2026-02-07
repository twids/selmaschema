using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;

namespace CoParenting.API.Endpoints;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/statistics")
            .WithTags("Statistics")
            .RequireAuthorization();

        // GET /api/statistics/{year}
        group.MapGet("/{year}", async (int year, IDayAssignmentService service) =>
        {
            var (parentA, parentB, vab, unassigned, withComments) = await service.GetYearStatisticsAsync(year);
            return Results.Ok(new StatisticsDto(parentA, parentB, vab, unassigned, withComments));
        })
        .WithName("GetYearStatistics")
        .WithDescription("Get statistics for a specific year")
        .Produces<StatisticsDto>();
    }
}
