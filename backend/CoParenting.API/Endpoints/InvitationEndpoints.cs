using Microsoft.EntityFrameworkCore;
using CoParenting.Infrastructure.Data;
using CoParenting.Core.Entities;
using System.Security.Claims;

namespace CoParenting.API.Endpoints;

public static class InvitationEndpoints
{
    public static void MapInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        // Get pending invitations for current user
        app.MapGet("/api/invitations/pending", async (HttpContext context, CoParentingDbContext db) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Results.Unauthorized();
            }

            var invitations = await db.Invitations
                .Include(i => i.Inviter)
                .Include(i => i.Child)
                .Where(i => i.InviteeEmail == email && i.Status == "Pending")
                .Select(i => new
                {
                    i.Id,
                    i.ChildId,
                    ChildName = i.Child.Name,
                    InviterName = i.Inviter.Name,
                    i.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(invitations);
        })
        .RequireAuthorization()
        .WithName("GetPendingInvitations")
        .WithTags("Invitations");

        // Send an invitation
        app.MapPost("/api/invitations", async (
            HttpContext context,
            CoParentingDbContext db,
            InvitationRequest request) =>
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

            // Verify the child belongs to the user
            var child = await db.Children
                .FirstOrDefaultAsync(c => c.Id == request.ChildId && c.PrimaryParentId == user.Id);

            if (child == null)
            {
                return Results.BadRequest("Child not found or you don't have permission");
            }

            // Check if child already has a secondary parent
            if (child.SecondaryParentId.HasValue)
            {
                return Results.BadRequest("Child already has a secondary parent");
            }

            // Check if invitation already exists
            var existingInvitation = await db.Invitations
                .FirstOrDefaultAsync(i => i.ChildId == request.ChildId 
                    && i.InviteeEmail == request.InviteeEmail 
                    && i.Status == "Pending");

            if (existingInvitation != null)
            {
                return Results.BadRequest("An invitation for this email already exists");
            }

            var invitation = new Invitation
            {
                InviterId = user.Id,
                InviteeEmail = request.InviteeEmail,
                ChildId = request.ChildId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            db.Invitations.Add(invitation);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Invitation sent successfully", invitationId = invitation.Id });
        })
        .RequireAuthorization()
        .WithName("SendInvitation")
        .WithTags("Invitations");

        // Accept an invitation
        app.MapPost("/api/invitations/{id}/accept", async (
            int id,
            HttpContext context,
            CoParentingDbContext db) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            var googleId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            {
                return Results.Unauthorized();
            }

            var invitation = await db.Invitations
                .Include(i => i.Child)
                .FirstOrDefaultAsync(i => i.Id == id && i.InviteeEmail == email && i.Status == "Pending");

            if (invitation == null)
            {
                return Results.NotFound("Invitation not found");
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user == null)
            {
                return Results.NotFound("User not found");
            }

            // Update invitation status
            invitation.Status = "Accepted";
            invitation.RespondedAt = DateTime.UtcNow;

            // Update child's secondary parent
            invitation.Child.SecondaryParentId = user.Id;
            invitation.Child.ModifiedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Invitation accepted successfully" });
        })
        .RequireAuthorization()
        .WithName("AcceptInvitation")
        .WithTags("Invitations");

        // Decline an invitation
        app.MapPost("/api/invitations/{id}/decline", async (
            int id,
            HttpContext context,
            CoParentingDbContext db) =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Results.Unauthorized();
            }

            var invitation = await db.Invitations
                .FirstOrDefaultAsync(i => i.Id == id && i.InviteeEmail == email && i.Status == "Pending");

            if (invitation == null)
            {
                return Results.NotFound("Invitation not found");
            }

            invitation.Status = "Declined";
            invitation.RespondedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Invitation declined" });
        })
        .RequireAuthorization()
        .WithName("DeclineInvitation")
        .WithTags("Invitations");
    }

    public record InvitationRequest(int ChildId, string InviteeEmail);
}
