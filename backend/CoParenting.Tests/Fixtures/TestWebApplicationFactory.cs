using CoParenting.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;

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
        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll<DbContextOptions<CoParentingDbContext>>();
            services.RemoveAll<CoParentingDbContext>();

            // Add DbContext with in-memory database
            services.AddDbContext<CoParentingDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            dbContext.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
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
        client.DefaultRequestHeaders.Add("Cookie", $"session_token={sessionToken}");

        return client;
    }
}
