using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using CoParenting.Infrastructure.Data;
using CoParenting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoParenting.API.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        // Get current user info
        app.MapGet("/api/auth/user", async (HttpContext context, CoParentingDbContext db) =>
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

            var user = await db.Users
                .Include(u => u.Children)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);

            if (user == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                user.Id,
                user.Email,
                user.Name,
                user.IsDemo,
                Children = user.Children.Select(c => new { c.Id, c.Name }).ToList()
            });
        })
        .WithName("GetCurrentUser")
        .WithTags("Authentication");

        // Login with Google
        app.MapGet("/api/auth/login", () =>
        {
            return Results.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = "/"
                },
                authenticationSchemes: new[] { GoogleDefaults.AuthenticationScheme }
            );
        })
        .WithName("Login")
        .WithTags("Authentication")
        .AllowAnonymous();

        // Google callback
        app.MapGet("/api/auth/google-callback", async (HttpContext context, CoParentingDbContext db) =>
        {
            var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            if (!result.Succeeded)
            {
                return Results.Redirect("/");
            }

            var googleId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
            {
                return Results.Redirect("/");
            }

            // Find or create user
            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            
            if (user == null)
            {
                user = new User
                {
                    GoogleId = googleId,
                    Email = email,
                    Name = name ?? email,
                    IsDemo = false,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                db.Users.Add(user);
            }
            else
            {
                user.LastLoginAt = DateTime.UtcNow;
                user.Name = name ?? user.Name;
            }

            await db.SaveChangesAsync();

            return Results.Redirect("/");
        })
        .WithName("GoogleCallback")
        .WithTags("Authentication")
        .AllowAnonymous();

        // Demo login
        app.MapPost("/api/auth/demo", async (HttpContext context, CoParentingDbContext db) =>
        {
            // Use a secure random GUID for demo user identification
            var demoId = "demo-" + Guid.NewGuid().ToString("N");
            var demoEmail = $"demo-{Guid.NewGuid().ToString("N").Substring(0, 8)}@demo.local";
            
            // Create demo user
            var user = new User
            {
                GoogleId = demoId,
                Email = demoEmail,
                Name = "Demo User",
                IsDemo = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Create demo child
            var child = new Child
            {
                Name = "Demo Child",
                PrimaryParentId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Children.Add(child);
            await db.SaveChangesAsync();

            // Sign in the demo user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, demoId),
                new Claim(ClaimTypes.Email, demoEmail),
                new Claim(ClaimTypes.Name, "Demo User"),
                new Claim("IsDemo", "true")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false
            };

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Results.Ok(new { message = "Demo login successful" });
        })
        .WithName("DemoLogin")
        .WithTags("Authentication")
        .AllowAnonymous();

        // Logout
        app.MapPost("/api/auth/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { message = "Logged out successfully" });
        })
        .WithName("Logout")
        .WithTags("Authentication");
    }
}
