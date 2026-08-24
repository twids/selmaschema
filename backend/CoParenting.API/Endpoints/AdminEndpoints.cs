using System.Security.Claims;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;

namespace CoParenting.API.Endpoints;

public static class AdminEndpoints
{
    public static void MapPlatformAdminEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("/api/admin")
            .WithTags("Platform administration")
            .RequireAuthorization(AuthEndpoints.AdminPolicy());

        admin.MapGet("/families", async (IAdminService service) => Results.Ok(await service.GetFamiliesAsync()));

        admin.MapGet("/families/{familyId:guid}", async (Guid familyId, IAdminService service) =>
        {
            var value = await service.GetFamilyAsync(familyId);
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        admin.MapPut("/families/{familyId:guid}", async (
            Guid familyId,
            AdminUpdateFamilyRequest request,
            HttpContext context,
            IAdminService service) =>
        {
            var updated = await service.UpdateFamilyAsync(ActorAccountId(context.User), familyId, request);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        admin.MapPut("/families/{familyId:guid}/members/{memberId:guid}", async (
            Guid familyId,
            Guid memberId,
            AdminUpdateMemberRequest request,
            HttpContext context,
            IAdminService service) =>
        {
            var updated = await service.UpdateMemberAsync(ActorAccountId(context.User), familyId, memberId, request);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        admin.MapPost("/families/{familyId:guid}/ownership-transfer", async (
            Guid familyId,
            AdminTransferOwnershipRequest request,
            HttpContext context,
            IAdminService service) =>
        {
            var updated = await service.TransferOwnershipAsync(ActorAccountId(context.User), familyId, request);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        admin.MapPost("/families/{familyId:guid}/invitations/{invitationId:guid}/revoke", async (
            Guid familyId,
            Guid invitationId,
            AdminReasonRequest request,
            HttpContext context,
            IAdminService service) =>
        {
            var updated = await service.RevokeInvitationAsync(
                ActorAccountId(context.User), familyId, invitationId, request.Reason);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        admin.MapPost("/accounts/{accountId:guid}/sessions/revoke", async (
            Guid accountId,
            AdminReasonRequest request,
            HttpContext context,
            IAdminService service) => Results.Ok(new
        {
            revoked = await service.RevokeAccountSessionsAsync(ActorAccountId(context.User), accountId, request.Reason)
        }));

        admin.MapGet("/audit", async (Guid? familyId, int? take, IAdminService service) =>
            Results.Ok(await service.GetAuditAsync(familyId, take ?? 100)));
    }

    private static Guid? ActorAccountId(ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
