using CoParenting.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using CoParenting.Infrastructure.Data;

namespace CoParenting.Tests;

/// <summary>
/// Unit tests for TestWebApplicationFactory that verify the factory code itself.
/// </summary>
public class TestWebApplicationFactoryUnitTests
{
    [Fact]
    public void TestWebApplicationFactory_ConstructorShouldAcceptDatabaseName()
    {
        // Arrange
        const string testDatabaseName = "TestDb_Unit";

        // Act
        var factory = new TestWebApplicationFactory(testDatabaseName);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void TestWebApplicationFactory_ShouldBeDisposable()
    {
        // Arrange
        const string testDatabaseName = "TestDb_Unit";
        var factory = new TestWebApplicationFactory(testDatabaseName);

        // Act & Assert - should not throw
        factory.Dispose();
    }
}
