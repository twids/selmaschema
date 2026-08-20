using CoParenting.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CoParenting.API.Authentication;

namespace CoParenting.Tests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Configures the application to use an in-memory database and provides helper methods for creating authenticated clients.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    /// <summary>
    /// Initializes a new instance of TestWebApplicationFactory with a unique in-memory database.
    /// </summary>
    /// <param name="databaseName">The unique name for the in-memory database.</param>
    public TestWebApplicationFactory(string databaseName)
    {
        _databaseName = databaseName;
    }

    /// <summary>
    /// Configures the web host to use an in-memory test database.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddDebug();
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddDataProtection().UseEphemeralDataProtectionProvider();

            // Remove the existing DbContext registration
            services.RemoveAll<DbContextOptions<CoParentingDbContext>>();
            services.RemoveAll<CoParentingDbContext>();

            // Add DbContext with in-memory database
            services.AddDbContext<CoParentingDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            services.PostConfigure<OpenIdConnectOptions>(AuthSchemes.Oidc, options =>
            {
                options.Configuration = new OpenIdConnectConfiguration
                {
                    AuthorizationEndpoint = "https://id.test/authorize",
                    TokenEndpoint = "https://id.test/token",
                    Issuer = "https://id.test"
                };
                options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(
                    options.Configuration);
            });

            // Ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            dbContext.Database.EnsureCreated();
        });

        // Configure admin password hash for testing
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:LocalAdmin:Enabled"] = "true",
                ["Auth:LocalAdmin:PasswordHash"] = BCrypt.Net.BCrypt.HashPassword("admin123"),
                ["Oidc:Authority"] = "https://id.test",
                ["Oidc:ClientId"] = "selma-tests",
                ["Oidc:ClientSecret"] = "test-secret",
                ["Oidc:CallbackPath"] = "/signin-oidc"
            });
        });

        builder.UseEnvironment("Testing");
    }

    public new HttpClient CreateClient()
    {
        return base.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    /// <summary>
    /// Creates an HTTP client with authentication headers for the specified session token.
    /// </summary>
    /// <param name="sessionToken">The session token to use for authentication.</param>
    /// <returns>An HttpClient with authentication headers set.</returns>
    public HttpClient CreateAuthenticatedClient(string sessionToken)
    {
        var client = CreateClient();

        // Add session cookie
        client.DefaultRequestHeaders.Add("Cookie", $"{AuthSchemes.SessionCookie}={sessionToken}");

        return client;
    }

    public HttpClient CreateOidcCallbackClient(
        int invitationId,
        string email,
        string subject,
        bool emailVerified = true)
    {
        var cookieOptions = Services
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(AuthSchemes.OidcTemporary);
        var claims = new[]
        {
            new Claim("sub", subject),
            new Claim("email", email),
            new Claim("name", email),
            new Claim("email_verified", emailVerified.ToString().ToLowerInvariant())
        };
        var properties = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
        };
        properties.Items[AuthSchemes.OidcIssuerProperty] = "https://id.test";
        properties.Items["invitation_id"] = invitationId.ToString();
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(claims, AuthSchemes.OidcTemporary)),
            properties,
            AuthSchemes.OidcTemporary);
        var protectedTicket = cookieOptions.TicketDataFormat.Protect(ticket);

        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{AuthSchemes.OidcCookie}={protectedTicket}");
        return client;
    }
}
