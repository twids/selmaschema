using CoParenting.API.Services;
using CoParenting.API.DTOs;
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
        group.MapGet("/me", async (
            HttpContext httpContext,
            IAuthService authService) =>
        {
            var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                return Results.Unauthorized();
            }

            var (success, user) = await authService.ValidateSessionAsync(token);
            
            if (!success || user == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                DisplayName = user.DisplayName
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
            .RequireAuthorization();

        // Create magic link
        group.MapPost("/magic-links", async (
            [FromBody] CreateMagicLinkRequest request,
            HttpContext httpContext,
            IAuthService authService) =>
        {
            // Verify admin role
            var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var (success, user) = await authService.ValidateSessionAsync(token);
            
            if (!success || user?.Role != "Admin")
            {
                return Results.Forbid();
            }

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
            HttpContext httpContext,
            IAuthService authService) =>
        {
            var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var (success, user) = await authService.ValidateSessionAsync(token);
            
            if (!success || user?.Role != "Admin")
            {
                return Results.Forbid();
            }

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
            var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var (success, user) = await authService.ValidateSessionAsync(token);
            
            if (!success || user?.Role != "Admin")
            {
                return Results.Forbid();
            }

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
