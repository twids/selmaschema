using CoParenting.Tests.Fixtures;

namespace CoParenting.Tests;

/// <summary>
/// Unit tests for DatabaseFixture that verify the fixture code itself.
/// </summary>
public class DatabaseFixtureUnitTests
{
    [Fact]
    public void DatabaseFixture_ShouldBeConstructable()
    {
        // Act
        var fixture = new DatabaseFixture();

        // Assert
        fixture.Should().NotBeNull();
    }

    [Fact]
    public void DatabaseFixture_ShouldGenerateUniqueDatabaseName()
    {
        // Act
        var fixture = new DatabaseFixture();

        // Assert
        fixture.DatabaseName.Should().NotBeNullOrWhiteSpace();
        fixture.DatabaseName.Should().StartWith("TestDb_");
    }

    [Fact]
    public void DatabaseFixture_ShouldAllowContextCreation()
    {
        // Arrange
        var fixture = new DatabaseFixture();

        // Act
        var act = () => fixture.CreateDbContext();

        // Assert
        act.Should().NotThrow();
    }
}
