using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CoParenting.Tests.Fixtures;

public sealed class V2WebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"selma-v2-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:ApplyMigrations"] = "false",
            ["Auth:BreakGlass:Enabled"] = "true",
            ["Auth:BreakGlass:PasswordHash"] = BCrypt.Net.BCrypt.HashPassword("correct horse battery staple"),
            ["AdminOidc:RequiredGroup"] = "selma-platform-admins",
            ["Oidc:Authority"] = "https://id.test.invalid/application/o/selma/",
            ["Oidc:ClientId"] = "selma-test",
            ["Oidc:ClientSecret"] = "test-secret",
            ["AdminOidc:Authority"] = "https://id.test.invalid/application/o/selma-admin/",
            ["AdminOidc:ClientId"] = "selma-admin-test",
            ["AdminOidc:ClientSecret"] = "test-secret",
            ["EndToEnd:LoginSecret"] = "integration-login-secret"
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CoParentingDbContext>>();
            services.RemoveAll<CoParentingDbContext>();
            services.AddDbContext<CoParentingDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
        });
    }

    public HttpClient CreateSecureClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    public async Task<(Account Account, string SessionToken, string CsrfToken)> AddAccountAsync(string email = "person@example.test")
    {
        var account = new Account
        {
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            DisplayName = "Testperson",
            CreatedAt = DateTime.UtcNow
        };
        var sessionToken = TokenService.GenerateToken();
        var csrfToken = TokenService.GenerateToken();
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
        db.Accounts.Add(account);
        db.AccountSessions.Add(new AccountSession
        {
            Account = account,
            TokenHash = TokenService.HashToken(sessionToken),
            CsrfTokenHash = TokenService.HashToken(csrfToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });
        await db.SaveChangesAsync();
        return (account, sessionToken, csrfToken);
    }

    public static void Authenticate(HttpClient client, string sessionToken, string csrfToken)
    {
        client.DefaultRequestHeaders.Add("Cookie", $"__Host-selma-session={sessionToken}; __Host-selma-csrf={csrfToken}");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrfToken);
    }
}
