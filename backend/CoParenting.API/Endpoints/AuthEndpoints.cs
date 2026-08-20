using System.Security.Claims;
using CoParenting.API.Authentication;
using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace CoParenting.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapGet("/login", (string? returnUrl) =>
        {
            var safeReturnUrl = SafeReturnUrl(returnUrl);
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/api/auth/oidc/complete?returnUrl={Uri.EscapeDataString(safeReturnUrl)}"
            };
            return Results.Challenge(properties, [AuthSchemes.Oidc]);
        }).AllowAnonymous();

        group.MapGet("/invitations/{token}", async (
            string token,
            IAuthService authService) =>
        {
            var invitation = await authService.GetValidInvitationByTokenAsync(token);
            if (invitation == null)
            {
                return Results.Redirect("/auth/invitation?error=invalid_invitation");
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/oidc/invitation-complete"
            };
            properties.Items["invitation_id"] = invitation.Id.ToString();
            return Results.Challenge(properties, [AuthSchemes.Oidc]);
        }).AllowAnonymous();

        group.MapGet("/oidc/complete", async (
            HttpContext context,
            string? returnUrl,
            IAuthService authService) =>
        {
            var external = await context.AuthenticateAsync(AuthSchemes.OidcTemporary);
            var login = ToExternalLogin(external.Principal);
            if (!external.Succeeded || login == null)
            {
                await context.SignOutAsync(AuthSchemes.OidcTemporary);
                return Results.Redirect("/login?error=oidc_failed");
            }

            var result = await authService.SignInExternalAsync(login);
            await context.SignOutAsync(AuthSchemes.OidcTemporary);
            if (!result.Success || result.RawToken == null || result.Session == null)
            {
                var error = result.Error == "Invitation required" ? "invitation_required" : "oidc_failed";
                return Results.Redirect($"/login?error={error}");
            }

            SetSessionCookie(context, result.RawToken, result.Session.ExpiresAt);
            return Results.Redirect(SafeReturnUrl(returnUrl));
        }).AllowAnonymous();

        group.MapGet("/oidc/invitation-complete", async (
            HttpContext context,
            IAuthService authService) =>
        {
            var external = await context.AuthenticateAsync(AuthSchemes.OidcTemporary);
            var invitationId = GetPendingInvitationId(external);
            var login = ToExternalLogin(external.Principal);
            if (!external.Succeeded || invitationId == null || login == null ||
                await authService.GetValidInvitationByIdAsync(invitationId.Value) == null)
            {
                await context.SignOutAsync(AuthSchemes.OidcTemporary);
                return Results.Redirect("/auth/invitation?error=invalid_invitation");
            }

            return Results.Redirect("/auth/invitation");
        }).AllowAnonymous();

        group.MapGet("/invitations/pending", async (
            HttpContext context,
            IAuthService authService) =>
        {
            var external = await context.AuthenticateAsync(AuthSchemes.OidcTemporary);
            var invitationId = GetPendingInvitationId(external);
            var login = ToExternalLogin(external.Principal);
            if (!external.Succeeded || invitationId == null || login == null)
            {
                return Results.Unauthorized();
            }

            var invitation = await authService.GetValidInvitationByIdAsync(invitationId.Value);
            if (invitation == null)
            {
                return Results.BadRequest(new { error = "Invitation is invalid, expired, or already used" });
            }

            return Results.Ok(new PendingInvitationDto
            {
                InvitationId = invitation.Id,
                EmailHint = invitation.EmailHint,
                VerifiedEmail = login.Email,
                DisplayName = login.DisplayName,
                Role = invitation.Role,
                ExpiresAt = invitation.ExpiresAt
            });
        }).AllowAnonymous();

        group.MapPost("/invitations/complete", async (
            HttpContext context,
            IAuthService authService) =>
        {
            var external = await context.AuthenticateAsync(AuthSchemes.OidcTemporary);
            var invitationId = GetPendingInvitationId(external);
            var login = ToExternalLogin(external.Principal);
            if (!external.Succeeded || invitationId == null || login == null)
            {
                return Results.Unauthorized();
            }

            var result = await authService.CompleteInvitationAsync(invitationId.Value, login);
            if (!result.Success || result.RawToken == null || result.Session == null || result.User == null)
            {
                return Results.Conflict(new { error = result.Error ?? "Invitation could not be completed" });
            }

            SetSessionCookie(context, result.RawToken, result.Session.ExpiresAt);
            await context.SignOutAsync(AuthSchemes.OidcTemporary);
            return Results.Ok(ToAuthResponse(result.User, result.Session.ExpiresAt));
        }).AllowAnonymous();

        group.MapPost("/invitations/cancel", async (HttpContext context) =>
        {
            await context.SignOutAsync(AuthSchemes.OidcTemporary);
            return Results.NoContent();
        }).AllowAnonymous();

        group.MapPost("/admin/login", async (
            [FromBody] AdminLoginRequest request,
            HttpContext context,
            IAuthService authService) =>
        {
            var result = await authService.ValidateAdminPasswordAsync(request.Password);
            if (!result.Success || result.RawToken == null || result.Session == null || result.User == null)
            {
                return Results.Unauthorized();
            }

            SetSessionCookie(context, result.RawToken, result.Session.ExpiresAt);
            return Results.Ok(ToAuthResponse(result.User, result.Session.ExpiresAt));
        }).AllowAnonymous().RequireRateLimiting("local-admin-login");

        group.MapGet("/me", (HttpContext context) =>
        {
            var user = UserFromClaims(context.User);
            return user == null ? Results.Unauthorized() : Results.Ok(user);
        }).RequireAuthorization();

        group.MapPost("/logout", async (
            HttpContext context,
            IAuthService authService) =>
        {
            if (context.Request.Cookies.TryGetValue(AuthSchemes.SessionCookie, out var token))
            {
                await authService.InvalidateSessionAsync(token);
            }

            DeleteSessionCookie(context);
            return Results.NoContent();
        }).RequireAuthorization();
    }

    private static int? GetPendingInvitationId(AuthenticateResult result)
    {
        return result.Properties?.Items.TryGetValue("invitation_id", out var value) == true &&
               int.TryParse(value, out var invitationId)
            ? invitationId
            : null;
    }

    private static ExternalLoginInfo? ToExternalLogin(ClaimsPrincipal? principal)
    {
        if (principal == null)
        {
            return null;
        }

        var issuer = principal.FindFirstValue("iss") ?? string.Empty;
        var subject = principal.FindFirstValue("sub") ?? string.Empty;
        var email = principal.FindFirstValue("email") ?? principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var displayName = principal.FindFirstValue("name") ?? principal.FindFirstValue(ClaimTypes.Name) ?? email;
        var emailVerifiedValue = principal.FindFirstValue("email_verified");
        var emailVerified = bool.TryParse(emailVerifiedValue, out var parsed) && parsed;
        return new ExternalLoginInfo(issuer, subject, email, displayName, emailVerified);
    }

    private static UserDto? UserFromClaims(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var role = principal.FindFirstValue(ClaimTypes.Role);
        if (!int.TryParse(userId, out var id) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return new UserDto
        {
            Id = id,
            Email = email,
            Role = role,
            DisplayName = principal.FindFirstValue(ClaimTypes.Name) ?? email
        };
    }

    internal static string SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !returnUrl.StartsWith('/') ||
            returnUrl.StartsWith("//", StringComparison.Ordinal) ||
            returnUrl.StartsWith("/\\", StringComparison.Ordinal) ||
            returnUrl.Contains('\\') ||
            returnUrl.Contains('\r') ||
            returnUrl.Contains('\n') ||
            !Uri.TryCreate(returnUrl, UriKind.Relative, out _))
        {
            return "/";
        }

        return returnUrl;
    }

    internal static AuthResponse ToAuthResponse(User user, DateTime expiresAt) => new()
    {
        User = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            DisplayName = user.DisplayName,
            LastLoginAt = user.LastLoginAt
        },
        ExpiresAt = expiresAt
    };

    internal static void SetSessionCookie(HttpContext context, string rawToken, DateTime expiresAt)
    {
        context.Response.Cookies.Append(AuthSchemes.SessionCookie, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = new DateTimeOffset(expiresAt)
        });
    }

    private static void DeleteSessionCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(AuthSchemes.SessionCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
    }
}

public static class InvitationEndpoints
{
    public static void MapInvitationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/invitations")
            .WithTags("Invitations")
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateInvitationRequest request,
            HttpContext context,
            IAuthService authService) =>
        {
            if (!TryGetUser(context.User, out var userId, out var role))
            {
                return Results.Unauthorized();
            }

            var result = await authService.CreateInvitationAsync(userId, role, request.Role, request.EmailHint);
            if (!result.Success || result.Invitation == null || result.RawToken == null)
            {
                return Results.BadRequest(new { error = result.Error ?? "Invitation could not be created" });
            }

            var creatorName = context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            return Results.Ok(new CreatedInvitationDto
            {
                Invitation = ToDto(result.Invitation, creatorName),
                InvitationUrl = $"{baseUrl}/api/auth/invitations/{result.RawToken}"
            });
        });

        group.MapGet("/", async (
            HttpContext context,
            IAuthService authService) =>
        {
            if (!TryGetUser(context.User, out var userId, out var role))
            {
                return Results.Unauthorized();
            }

            var invitations = await authService.GetInvitationsAsync(userId, role == "Admin");
            return Results.Ok(invitations.Select(i => ToDto(i, i.CreatedByUser.DisplayName)));
        });
    }

    private static bool TryGetUser(ClaimsPrincipal principal, out int userId, out string role)
    {
        role = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        return int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId) &&
               !string.IsNullOrWhiteSpace(role);
    }

    private static InvitationDto ToDto(Invitation invitation, string creatorName)
    {
        var status = invitation.ConsumedAt != null
            ? "Consumed"
            : invitation.ExpiresAt <= DateTime.UtcNow ? "Expired" : "Pending";
        return new InvitationDto
        {
            Id = invitation.Id,
            EmailHint = invitation.EmailHint,
            Role = invitation.Role,
            CreatedByUserId = invitation.CreatedByUserId,
            CreatedByName = creatorName,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            ConsumedAt = invitation.ConsumedAt,
            Status = status
        };
    }
}

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("RequireAdminRole");

        group.MapGet("/users", async (IAuthService authService) =>
        {
            var users = await authService.GetAllUsersAsync();
            return Results.Ok(users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                DisplayName = u.DisplayName,
                LastLoginAt = u.LastLoginAt
            }));
        });
    }
}
