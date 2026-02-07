using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Tests.Fixtures;

/// <summary>
/// Fixture for managing an in-memory database for integration tests.
/// Each test gets an isolated database instance.
/// </summary>
public class DatabaseFixture
{
    /// <summary>
    /// Gets the unique database name for this test fixture instance.
    /// </summary>
    public string DatabaseName { get; }

    public DatabaseFixture()
    {
        // Create a unique database name to ensure test isolation
        DatabaseName = $"TestDb_{Guid.NewGuid()}";
    }

    /// <summary>
    /// Creates a new DbContext configured with an in-memory database.
    /// </summary>
    public CoParentingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CoParentingDbContext>()
            .UseInMemoryDatabase(DatabaseName)
            .Options;

        var context = new CoParentingDbContext(options);
        return context;
    }
}
