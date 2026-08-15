using CoParenting.API.Authentication;
using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoParenting.Tests.Fixtures;

public static class TestAuthentication
{
    public static async Task<string> AuthenticateClientAsync(
        this TestWebApplicationFactory factory,
        HttpClient client,
        string role,
        string email)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                Email = email,
                Role = role,
                DisplayName = role switch
                {
                    "ParentA" => "Parent A",
                    "ParentB" => "Parent B",
                    _ => role
                },
                CreatedAt = DateTime.UtcNow,
                IsLocalAdmin = role == "Admin"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var rawToken = TokenService.GenerateToken();
        context.Sessions.Add(new Session
        {
            TokenHash = TokenService.HashToken(rawToken),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true
        });
        await context.SaveChangesAsync();
        client.SetSessionCookie(rawToken);
        return rawToken;
    }

    public static void SetSessionCookie(this HttpClient client, string rawToken)
    {
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", $"{AuthSchemes.SessionCookie}={rawToken}");
    }
}
