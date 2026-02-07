using CoParenting.API.DTOs;
using CoParenting.API.Services;

namespace CoParenting.API.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comments")
            .WithTags("Comments")
            .RequireAuthorization();

        // POST /api/comments/{dayAssignmentId}
        group.MapPost("/{dayAssignmentId}", async (int dayAssignmentId, CreateCommentDto dto, CommentService service) =>
        {
            var comment = await service.AddCommentAsync(dayAssignmentId, dto.Parent, dto.CommentText);
            return Results.Ok(new CommentDto(
                comment.Id,
                comment.DayAssignmentId,
                comment.Parent,
                comment.CommentText,
                comment.CreatedAt,
                comment.ModifiedAt
            ));
        })
        .WithName("AddComment")
        .WithDescription("Add a comment to a day assignment")
        .Produces<CommentDto>();

        // GET /api/comments/{dayAssignmentId}
        group.MapGet("/{dayAssignmentId}", async (int dayAssignmentId, CommentService service) =>
        {
            var comments = await service.GetCommentsForDayAsync(dayAssignmentId);
            var dtos = comments.Select(c => new CommentDto(
                c.Id,
                c.DayAssignmentId,
                c.Parent,
                c.CommentText,
                c.CreatedAt,
                c.ModifiedAt
            )).ToList();
            return Results.Ok(dtos);
        })
        .WithName("GetComments")
        .WithDescription("Get all comments for a day assignment")
        .Produces<List<CommentDto>>();

        // PUT /api/comments/{commentId}
        group.MapPut("/{commentId}", async (int commentId, UpdateCommentDto dto, CommentService service) =>
        {
            var comment = await service.UpdateCommentAsync(commentId, dto.CommentText);
            if (comment == null)
                return Results.NotFound();

            return Results.Ok(new CommentDto(
                comment.Id,
                comment.DayAssignmentId,
                comment.Parent,
                comment.CommentText,
                comment.CreatedAt,
                comment.ModifiedAt
            ));
        })
        .WithName("UpdateComment")
        .WithDescription("Update a comment")
        .Produces<CommentDto>()
        .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/comments/{commentId}
        group.MapDelete("/{commentId}", async (int commentId, CommentService service) =>
        {
            var deleted = await service.DeleteCommentAsync(commentId);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteComment")
        .WithDescription("Delete a comment")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
