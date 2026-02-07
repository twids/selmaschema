using CoParenting.Application.DTOs;
using CoParenting.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoParenting.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        // Admin login
        group.MapPost("/admin/login", async (
            [FromBody] AdminLoginRequest request,
            IAuthService authService) =>
        {
            var (success, session, user) = await authService.ValidateAdminPasswordAsync(request.Password);

            if (!success || session == null || user == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new AuthResponse
            {
                Token = session.Token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role,
                    DisplayName = user.DisplayName
                },
                ExpiresAt = session.ExpiresAt
            });
        }).AllowAnonymous();

        // Exchange magic token for session
        group.MapPost("/magic", async (
            [FromBody] MagicTokenRequest request,
            IAuthService authService) =>
        {
            var (success, session, user, error) = await authService.ExchangeMagicTokenAsync(request.Token);

            if (!success || session == null || user == null)
            {
                return Results.BadRequest(new { error = error ?? "Invalid or expired token" });
            }

            return Results.Ok(new AuthResponse
            {
                Token = session.Token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role,
                    DisplayName = user.DisplayName
                },
                ExpiresAt = session.ExpiresAt
            });
        }).AllowAnonymous();

        // Get current user info
        group.MapGet("/me", (HttpContext httpContext) =>
        {
            // Get user info from claims set by SessionAuthenticationHandler
            // No need to re-validate the session token - authentication middleware already did it
            var user = httpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var displayName = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new UserDto
            {
                Id = int.Parse(userId),
                Email = email,
                Role = role,
                DisplayName = displayName
            });
        }).RequireAuthorization();

        // Logout (invalidate session)
        group.MapPost("/logout", async (
            HttpContext httpContext,
            IAuthService authService) =>
        {
            var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return Results.BadRequest(new { error = "No token provided" });
            }

            var success = await authService.InvalidateSessionAsync(token);

            return success
                ? Results.Ok(new { message = "Logged out successfully" })
                : Results.BadRequest(new { error = "Invalid session" });
        }).RequireAuthorization();
    }
}

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("RequireAdminRole"); // Apply admin policy to all endpoints in this group

        // Create magic link
        group.MapPost("/magic-links", async (
            [FromBody] CreateMagicLinkRequest request,
            HttpContext httpContext,
            IAuthService authService) =>
        {
            // No need to check role manually - authorization policy handles it
            var (linkSuccess, magicToken) = await authService.CreateMagicLinkAsync(
                request.Email,
                request.Role,
                request.DisplayName);

            if (!linkSuccess || magicToken == null)
            {
                return Results.BadRequest(new { error = "Failed to create magic link" });
            }

            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            var magicLink = $"{baseUrl}/auth/magic?token={magicToken.Token}";

            return Results.Ok(new MagicLinkResponse
            {
                Token = magicToken.Token,
                MagicLink = magicLink,
                Email = magicToken.User.Email,
                Role = magicToken.User.Role,
                DisplayName = magicToken.User.DisplayName,
                ExpiresAt = magicToken.ExpiresAt
            });
        });

        // Get all users
        group.MapGet("/users", async (
            IAuthService authService) =>
        {
            // No need to check role manually - authorization policy handles it
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

        // Get pending magic links
        group.MapGet("/magic-links", async (
            HttpContext httpContext,
            IAuthService authService) =>
        {
            // No need to check role manually - authorization policy handles it
            var magicLinks = await authService.GetPendingMagicLinksAsync();
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            return Results.Ok(magicLinks.Select(mt => new MagicLinkResponse
            {
                Token = mt.Token,
                MagicLink = $"{baseUrl}/auth/magic?token={mt.Token}",
                Email = mt.User.Email,
                Role = mt.User.Role,
                DisplayName = mt.User.DisplayName,
                ExpiresAt = mt.ExpiresAt,
                CreatedAt = mt.CreatedAt
            }));
        });
    }
}
