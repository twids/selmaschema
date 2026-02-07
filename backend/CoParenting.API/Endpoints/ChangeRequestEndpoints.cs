using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Application.Validators;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoParenting.API.Endpoints;

public static class ChangeRequestEndpoints
{
    public static void MapChangeRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/change-requests")
            .WithTags("Change Requests")
            .RequireAuthorization();

        // POST /api/change-requests - Create new change request(s)
        group.MapPost("/", async (
            CreateChangeRequestDto dto,
            IChangeRequestService service,
            HttpContext context) =>
        {
            // Validate
            var validator = new CreateChangeRequestDtoValidator();
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var changeRequests = await service.CreateChangeRequestsAsync(
                userId,
                dto.Dates,
                dto.RequestedParent,
                dto.Comment);

            var responseDtos = changeRequests.Select(MapToDto).ToList();
            return Results.Ok(responseDtos);
        })
        .WithName("CreateChangeRequests")
        .WithDescription("Create change request(s) for day assignment swaps")
        .Produces<List<ChangeRequestDto>>();

        // GET /api/change-requests - Get all my change requests
        group.MapGet("/", async (IChangeRequestService service, HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var changeRequests = await service.GetMyChangeRequestsAsync(userId);
            var dtos = changeRequests.Select(MapToDto).ToList();
            return Results.Ok(dtos);
        })
        .WithName("GetMyChangeRequests")
        .WithDescription("Get all change requests created by or affecting current user")
        .Produces<List<ChangeRequestDto>>();

        // GET /api/change-requests/pending - Get all pending change requests
        group.MapGet("/pending", async (IChangeRequestService service) =>
        {
            var changeRequests = await service.GetPendingChangeRequestsAsync();
            var dtos = changeRequests.Select(MapToDto).ToList();
            return Results.Ok(dtos);
        })
        .WithName("GetPendingChangeRequests")
        .WithDescription("Get all pending change requests")
        .Produces<List<ChangeRequestDto>>();

        // GET /api/change-requests/{id} - Get specific change request
        group.MapGet("/{id:int}", async (int id, IChangeRequestService service) =>
        {
            var changeRequest = await service.GetChangeRequestAsync(id);
            if (changeRequest == null)
                return Results.NotFound();

            return Results.Ok(MapToDto(changeRequest));
        })
        .WithName("GetChangeRequest")
        .WithDescription("Get a specific change request by ID")
        .Produces<ChangeRequestDto>()
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/change-requests/{id}/review - Review a change request
        group.MapPost("/{id:int}/review", async (
            int id,
            ReviewChangeRequestDto dto,
            IChangeRequestService service,
            HttpContext context) =>
        {
            // Validate
            var validator = new ReviewChangeRequestDtoValidator();
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            try
            {
                var changeRequest = await service.ReviewChangeRequestAsync(id, userId, dto.Approved, dto.Comment);
                if (changeRequest == null)
                    return Results.NotFound();

                return Results.Ok(MapToDto(changeRequest));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ReviewChangeRequest")
        .WithDescription("Approve or reject a change request")
        .Produces<ChangeRequestDto>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        // DELETE /api/change-requests/{id} - Cancel a change request
        group.MapDelete("/{id:int}", async (
            int id,
            IChangeRequestService service,
            HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            try
            {
                var success = await service.CancelChangeRequestAsync(id, userId);
                if (!success)
                    return Results.NotFound();

                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CancelChangeRequest")
        .WithDescription("Cancel a pending change request (only by creator)")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }

    private static ChangeRequestDto MapToDto(Core.Entities.ChangeRequest cr)
    {
        return new ChangeRequestDto
        {
            Id = cr.Id,
            RequestedByUserId = cr.RequestedByUserId,
            RequestedByName = cr.RequestedByUser?.DisplayName ?? "Unknown",
            RequestedForDate = cr.RequestedForDate,
            CurrentParent = cr.CurrentParent,
            RequestedParent = cr.RequestedParent,
            Status = cr.Status,
            CreatedAt = cr.CreatedAt,
            ReviewedAt = cr.ReviewedAt,
            ReviewedByName = cr.ReviewedByUser?.DisplayName,
            Comment = cr.Comment
        };
    }
}
