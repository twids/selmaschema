using Microsoft.EntityFrameworkCore;
using CoParenting.Infrastructure.Data;
using CoParenting.Core.Entities;
using System.Security.Claims;

namespace CoParenting.API.Endpoints;

public static class ChildEndpoints
{
    public static void MapChildEndpoints(this IEndpointRouteBuilder app)
    {
        // Get all children for current user
        app.MapGet("/api/children", async (HttpContext context, CoParentingDbContext db) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var googleId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user == null)
            {
                return Results.NotFound("User not found");
            }

            var children = await db.Children
                .Include(c => c.PrimaryParent)
                .Include(c => c.SecondaryParent)
                .Where(c => c.PrimaryParentId == user.Id || c.SecondaryParentId == user.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.DateOfBirth,
                    PrimaryParent = new { c.PrimaryParent.Id, c.PrimaryParent.Name },
                    SecondaryParent = c.SecondaryParent != null 
                        ? new { c.SecondaryParent.Id, c.SecondaryParent.Name }
                        : null
                })
                .ToListAsync();

            return Results.Ok(children);
        })
        .RequireAuthorization()
        .WithName("GetChildren")
        .WithTags("Children");

        // Create a new child
        app.MapPost("/api/children", async (
            HttpContext context,
            CoParentingDbContext db,
            ChildRequest request) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var googleId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user == null)
            {
                return Results.NotFound("User not found");
            }

            var child = new Child
            {
                Name = request.Name,
                DateOfBirth = request.DateOfBirth,
                PrimaryParentId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            db.Children.Add(child);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Child created successfully", childId = child.Id });
        })
        .RequireAuthorization()
        .WithName("CreateChild")
        .WithTags("Children");

        // Update a child
        app.MapPut("/api/children/{id}", async (
            int id,
            HttpContext context,
            CoParentingDbContext db,
            ChildRequest request) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var googleId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user == null)
            {
                return Results.NotFound("User not found");
            }

            var child = await db.Children
                .FirstOrDefaultAsync(c => c.Id == id && c.PrimaryParentId == user.Id);

            if (child == null)
            {
                return Results.NotFound("Child not found or you don't have permission");
            }

            child.Name = request.Name;
            child.DateOfBirth = request.DateOfBirth;
            child.ModifiedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Child updated successfully" });
        })
        .RequireAuthorization()
        .WithName("UpdateChild")
        .WithTags("Children");
    }

    public record ChildRequest(string Name, DateTime? DateOfBirth);
}
