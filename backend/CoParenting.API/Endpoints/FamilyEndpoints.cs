using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoParenting.API.Endpoints;

public static class FamilyEndpoints
{
    public static void MapFamilyEndpoints(this WebApplication app)
    {
        var families = app.MapGroup("/api/families")
            .WithTags("Families")
            .RequireAuthorization();

        families.MapGet("/", async (HttpContext context, IFamilyService service) =>
        {
            var accountId = AuthEndpoints.AccountId(context.User);
            return accountId == null ? Results.Unauthorized() : Results.Ok(await service.GetFamiliesAsync(accountId.Value));
        });

        families.MapPost("/", async (CreateFamilyRequest request, HttpContext context, IFamilyService service) =>
        {
            var accountId = AuthEndpoints.AccountId(context.User);
            if (accountId == null)
            {
                return Results.Unauthorized();
            }

            var family = await service.CreateFamilyAsync(accountId.Value, request);
            return Results.Created($"/api/families/{family.Id}", family);
        });

        families.MapGet("/{familyId:guid}", async (Guid familyId, HttpContext context, IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.GetFamilyAsync(accountId, familyId));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPut("/{familyId:guid}", async (
            Guid familyId,
            UpdateFamilyRequest request,
            HttpContext context,
            IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.UpdateFamilyAsync(accountId, familyId, request));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapGet("/{familyId:guid}/members", async (Guid familyId, HttpContext context, IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.GetMembersAsync(accountId, familyId));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPut("/{familyId:guid}/members/{memberId:guid}", async (
            Guid familyId,
            Guid memberId,
            UpdateMemberRequest request,
            HttpContext context,
            IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.UpdateMemberAsync(accountId, familyId, memberId, request));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPost("/{familyId:guid}/ownership-transfer", async (
            Guid familyId,
            TransferOwnershipRequest request,
            HttpContext context,
            IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.TransferOwnershipAsync(accountId, familyId, request));
            return value ? Results.NoContent() : Results.NotFound();
        });

        families.MapGet("/{familyId:guid}/children", async (Guid familyId, HttpContext context, IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.GetChildrenAsync(accountId, familyId));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPost("/{familyId:guid}/children", async (
            Guid familyId,
            CreateChildRequest request,
            HttpContext context,
            IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.CreateChildAsync(accountId, familyId, request));
            return value == null ? Results.NotFound() : Results.Created($"/api/families/{familyId}/children/{value.Id}", value);
        });

        families.MapPut("/{familyId:guid}/children/{childId:guid}", async (
            Guid familyId,
            Guid childId,
            UpdateChildRequest request,
            HttpContext context,
            IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.UpdateChildAsync(accountId, familyId, childId, request));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapGet("/{familyId:guid}/calendars", async (Guid familyId, HttpContext context, IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.GetCalendarsAsync(accountId, familyId));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPost("/{familyId:guid}/calendars", async (
            Guid familyId,
            CreateCalendarRequest request,
            HttpContext context,
            IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.CreateCalendarAsync(accountId, familyId, request));
            return value == null ? Results.NotFound() : Results.Created($"/api/families/{familyId}/calendars/{value.Id}", value);
        });

        families.MapPut("/{familyId:guid}/calendars/{calendarId:guid}", async (
            Guid familyId,
            Guid calendarId,
            UpdateCalendarRequest request,
            HttpContext context,
            IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.UpdateCalendarAsync(accountId, familyId, calendarId, request));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapGet("/{familyId:guid}/invitations", async (Guid familyId, HttpContext context, IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.GetInvitationsAsync(accountId, familyId));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPost("/{familyId:guid}/invitations", async (
            Guid familyId,
            CreateInvitationRequest request,
            HttpContext context,
            IFamilyService service) =>
        {
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var value = await WithAccount(context, accountId => service.CreateInvitationAsync(accountId, familyId, request, baseUrl));
            return value == null ? Results.NotFound() : Results.Created($"/api/families/{familyId}/invitations/{value.Invitation.Id}", value);
        });

        families.MapDelete("/{familyId:guid}/invitations/{invitationId:guid}", async (
            Guid familyId,
            Guid invitationId,
            HttpContext context,
            IFamilyService service) =>
        {
            var value = await WithAccount(context, accountId => service.RevokeInvitationAsync(accountId, familyId, invitationId));
            return value ? Results.NoContent() : Results.NotFound();
        });

        MapCalendarEndpoints(families);
    }

    private static void MapCalendarEndpoints(RouteGroupBuilder families)
    {
        families.MapPost("/{familyId:guid}/calendars/{calendarId:guid}/schedule/preview", async (
            Guid familyId,
            Guid calendarId,
            ScheduleDraftRequest request,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.PreviewAsync(accountId, familyId, calendarId, request));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPost("/{familyId:guid}/calendars/{calendarId:guid}/schedule/versions", async (
            Guid familyId,
            Guid calendarId,
            ScheduleDraftRequest request,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.CreateVersionAsync(accountId, familyId, calendarId, request));
            return value == null ? Results.NotFound() : Results.Created($"/api/families/{familyId}/calendars/{calendarId}/schedule/versions/{value.Id}", value);
        });

        families.MapGet("/{familyId:guid}/calendars/{calendarId:guid}/schedule/versions", async (
            Guid familyId,
            Guid calendarId,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.GetVersionsAsync(accountId, familyId, calendarId));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapGet("/{familyId:guid}/calendars/{calendarId:guid}/months/{year:int}/{month:int}", async (
            Guid familyId,
            Guid calendarId,
            int year,
            int month,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.GetMonthAsync(accountId, familyId, calendarId, year, month));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPut("/{familyId:guid}/calendars/{calendarId:guid}/days/{date}", async (
            Guid familyId,
            Guid calendarId,
            DateOnly date,
            DayOverrideRequest request,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.SetOverrideAsync(accountId, familyId, calendarId, date, request));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapDelete("/{familyId:guid}/calendars/{calendarId:guid}/days/{date}", async (
            Guid familyId,
            Guid calendarId,
            DateOnly date,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.ClearOverrideAsync(accountId, familyId, calendarId, date));
            return value ? Results.NoContent() : Results.NotFound();
        });

        families.MapPost("/{familyId:guid}/calendars/{calendarId:guid}/days/{date}/comments", async (
            Guid familyId,
            Guid calendarId,
            DateOnly date,
            CreateCommentRequest request,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.AddCommentAsync(accountId, familyId, calendarId, date, request));
            return value == null ? Results.NotFound() : Results.Created($"/api/families/{familyId}/comments/{value.Id}", value);
        });

        families.MapPut("/{familyId:guid}/comments/{commentId:guid}", async (
            Guid familyId,
            Guid commentId,
            UpdateCommentRequest request,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.UpdateCommentAsync(accountId, familyId, commentId, request));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapDelete("/{familyId:guid}/comments/{commentId:guid}", async (
            Guid familyId,
            Guid commentId,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.DeleteCommentAsync(accountId, familyId, commentId));
            return value ? Results.NoContent() : Results.NotFound();
        });

        families.MapGet("/{familyId:guid}/change-requests", async (
            Guid familyId,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.GetChangeRequestsAsync(accountId, familyId));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });

        families.MapPost("/{familyId:guid}/calendars/{calendarId:guid}/change-requests", async (
            Guid familyId,
            Guid calendarId,
            CreateChangeRequestRequest request,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.CreateChangeRequestAsync(accountId, familyId, calendarId, request));
            return value == null ? Results.NotFound() : Results.Created($"/api/families/{familyId}/change-requests/{value.Id}", value);
        });

        families.MapPost("/{familyId:guid}/change-requests/{requestId:guid}/review", async (
            Guid familyId,
            Guid requestId,
            ReviewChangeRequestRequest request,
            HttpContext context,
            IScheduleService service) =>
        {
            var value = await WithAccount(context, accountId => service.ReviewChangeRequestAsync(accountId, familyId, requestId, request));
            return value == null ? Results.NotFound() : Results.Ok(value);
        });
    }

    private static async Task<T?> WithAccount<T>(HttpContext context, Func<Guid, Task<T?>> action)
    {
        var accountId = AuthEndpoints.AccountId(context.User);
        return accountId == null ? default : await action(accountId.Value);
    }
}
