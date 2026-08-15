using CoParenting.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Tests;

/// <summary>
/// Tests to verify TestWebApplicationFactory works correctly with in-memory database
/// </summary>
public class TestWebApplicationFactoryTests
{
    [Fact]
    public void TestWebApplicationFactory_ShouldCreateApplication()
    {
        // Arrange
        var databaseName = $"TestDb_{Guid.NewGuid()}";

        // Act
        using var factory = new TestWebApplicationFactory(databaseName);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void TestWebApplicationFactory_ShouldCreateHttpClient()
    {
        // Arrange
        var databaseName = $"TestDb_{Guid.NewGuid()}";
        using var factory = new TestWebApplicationFactory(databaseName);

        // Act
        using var client = factory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        client.BaseAddress.Should().NotBeNull();
    }

    [Fact]
    public void TestWebApplicationFactory_ShouldProvideAuthenticatedClient()
    {
        // Arrange
        var databaseName = $"TestDb_{Guid.NewGuid()}";
        using var factory = new TestWebApplicationFactory(databaseName);

        // Act
        using var client = factory.CreateAuthenticatedClient("test-session-token");

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Should().NotBeNull();
        var cookieHeader = client.DefaultRequestHeaders.GetValues("Cookie").FirstOrDefault();
        cookieHeader.Should().Contain("__Host-selma-session=test-session-token");
    }

    [Fact]
    public void TestWebApplicationFactory_ShouldConfigureDbContext()
    {
        // Arrange
        var databaseName = $"TestDb_{Guid.NewGuid()}";

        // Act
        using var factory = new TestWebApplicationFactory(databaseName);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();

        // Assert
        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public async Task TestWebApplicationFactory_ShouldRespondToHealthCheck()
    {
        // Arrange
        var databaseName = $"TestDb_{Guid.NewGuid()}";
        using var factory = new TestWebApplicationFactory(databaseName);
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert - should get some response (not 404)
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task TestWebApplicationFactory_ShouldAllowDatabaseOperations()
    {
        // Arrange
        var databaseName = $"TestDb_{Guid.NewGuid()}";
        using var factory = new TestWebApplicationFactory(databaseName);

        // Act - Add data through DbContext
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            dbContext.Configurations.Add(new CoParenting.Core.Entities.Configuration
            {
                Key = "test_factory_key",
                Value = "test_factory_value"
            });
            await dbContext.SaveChangesAsync();
        }

        // Assert - Retrieve data to confirm it was saved
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CoParentingDbContext>();
            var config = await dbContext.Configurations
                .FirstOrDefaultAsync(c => c.Key == "test_factory_key");
            config.Should().NotBeNull();
            config!.Value.Should().Be("test_factory_value");
        }
    }
}
