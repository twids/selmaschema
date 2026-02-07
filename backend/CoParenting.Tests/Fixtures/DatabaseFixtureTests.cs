using CoParenting.Tests.Fixtures;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Tests;

/// <summary>
/// Tests to verify DatabaseFixture works correctly - providing in-memory database
/// </summary>
public class DatabaseFixtureTests
{
    [Fact]
    public void DatabaseFixture_ShouldProvideUniqueDatabaseName()
    {
        // Arrange & Act
        var fixture1 = new DatabaseFixture();
        var fixture2 = new DatabaseFixture();

        // Assert
        fixture1.DatabaseName.Should().NotBeNullOrWhiteSpace();
        fixture2.DatabaseName.Should().NotBeNullOrWhiteSpace();
        fixture1.DatabaseName.Should().NotBe(fixture2.DatabaseName);
    }

    [Fact]
    public void DatabaseFixture_ShouldCreateDbContext()
    {
        // Arrange
        var fixture = new DatabaseFixture();

        // Act
        using var dbContext = fixture.CreateDbContext();

        // Assert
        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public async Task DatabaseFixture_ShouldAllowDatabaseOperations()
    {
        // Arrange
        var fixture = new DatabaseFixture();
        using var dbContext = fixture.CreateDbContext();

        // Act - Try to add and retrieve a configuration
        var config = new CoParenting.Core.Entities.Configuration
        {
            Key = "test_key",
            Value = "test_value"
        };
        dbContext.Configurations.Add(config);
        await dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await dbContext.Configurations
            .FirstOrDefaultAsync(c => c.Key == "test_key");
        retrieved.Should().NotBeNull();
        retrieved!.Value.Should().Be("test_value");
    }

    [Fact]
    public void DatabaseFixture_ShouldIsolateDatabases()
    {
        // Arrange
        var fixture1 = new DatabaseFixture();
        var fixture2 = new DatabaseFixture();

        // Act - Add data to first database
        using (var dbContext1 = fixture1.CreateDbContext())
        {
            dbContext1.Configurations.Add(new CoParenting.Core.Entities.Configuration
            {
                Key = "test_key",
                Value = "value1"
            });
            dbContext1.SaveChanges();
        }

        // Assert - Second database should not have the data
        using (var dbContext2 = fixture2.CreateDbContext())
        {
            var count = dbContext2.Configurations.Count();
            count.Should().Be(0);
        }
    }
}
