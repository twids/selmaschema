using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoParenting.Tests.Unit.Services;

public class ConfigurationServiceTests : IDisposable
{
    private readonly CoParentingDbContext _context;
    private readonly ConfigurationService _service;

    public ConfigurationServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<CoParentingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CoParentingDbContext(options);

        // Create service
        _service = new ConfigurationService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetValueAsync Tests

    [Fact]
    public async Task GetValueAsync_ExistingKey_ShouldReturnValue()
    {
        // Arrange
        var key = "TestKey";
        var value = "TestValue";
        _context.Configurations.Add(new Configuration
        {
            Key = key,
            Value = value,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetValueAsync(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task GetValueAsync_NonExistentKey_ShouldReturnNull()
    {
        // Arrange
        var nonExistentKey = "NonExistentKey";

        // Act
        var result = await _service.GetValueAsync(nonExistentKey);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValueAsync_CaseSensitiveKey_ShouldReturnCorrectValue()
    {
        // Arrange
        _context.Configurations.Add(new Configuration
        {
            Key = "MyKey",
            Value = "Value1",
            CreatedAt = DateTime.UtcNow
        });
        _context.Configurations.Add(new Configuration
        {
            Key = "mykey",
            Value = "Value2",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result1 = await _service.GetValueAsync("MyKey");
        var result2 = await _service.GetValueAsync("mykey");

        // Assert
        result1.Should().Be("Value1");
        result2.Should().Be("Value2");
    }

    #endregion

    #region SetValueAsync Tests

    [Fact]
    public async Task SetValueAsync_NewKey_ShouldCreateConfiguration()
    {
        // Arrange
        var key = "NewKey";
        var value = "NewValue";

        // Act
        await _service.SetValueAsync(key, value);

        // Assert
        var config = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == key);
        config.Should().NotBeNull();
        config!.Value.Should().Be(value);
        config.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        config.ModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task SetValueAsync_ExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var key = "ExistingKey";
        var originalValue = "OriginalValue";
        var newValue = "NewValue";

        _context.Configurations.Add(new Configuration
        {
            Key = key,
            Value = originalValue,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.SetValueAsync(key, newValue);

        // Assert
        var config = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == key);
        config.Should().NotBeNull();
        config!.Value.Should().Be(newValue);
        config.ModifiedAt.Should().NotBeNull();
        config.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SetValueAsync_ExistingKey_ShouldNotChangeCreatedAt()
    {
        // Arrange
        var key = "TestKey";
        var originalCreatedAt = DateTime.UtcNow.AddDays(-7);

        _context.Configurations.Add(new Configuration
        {
            Key = key,
            Value = "OriginalValue",
            CreatedAt = originalCreatedAt
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.SetValueAsync(key, "NewValue");

        // Assert
        var config = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == key);
        config!.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public async Task SetValueAsync_MultipleUpdates_ShouldUpdateModifiedAt()
    {
        // Arrange
        var key = "UpdateKey";
        await _service.SetValueAsync(key, "Value1");
        await Task.Delay(10);

        // Act
        await _service.SetValueAsync(key, "Value2");

        // Assert
        var config = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == key);
        config!.Value.Should().Be("Value2");
        config.ModifiedAt.Should().NotBeNull();
    }

    #endregion

    #region GetParentNamesAsync Tests

    [Fact]
    public async Task GetParentNamesAsync_NoConfiguration_ShouldReturnDefaults()
    {
        // Act
        var (parentAName, parentBName) = await _service.GetParentNamesAsync();

        // Assert
        parentAName.Should().Be("Parent A");
        parentBName.Should().Be("Parent B");
    }

    [Fact]
    public async Task GetParentNamesAsync_WithCustomNames_ShouldReturnConfiguredNames()
    {
        // Arrange
        await _service.SetValueAsync("ParentAName", "Alice");
        await _service.SetValueAsync("ParentBName", "Bob");

        // Act
        var (parentAName, parentBName) = await _service.GetParentNamesAsync();

        // Assert
        parentAName.Should().Be("Alice");
        parentBName.Should().Be("Bob");
    }

    [Fact]
    public async Task GetParentNamesAsync_OnlyParentAConfigured_ShouldReturnMixed()
    {
        // Arrange
        await _service.SetValueAsync("ParentAName", "Charlie");

        // Act
        var (parentAName, parentBName) = await _service.GetParentNamesAsync();

        // Assert
        parentAName.Should().Be("Charlie");
        parentBName.Should().Be("Parent B"); // default
    }

    [Fact]
    public async Task GetParentNamesAsync_OnlyParentBConfigured_ShouldReturnMixed()
    {
        // Arrange
        await _service.SetValueAsync("ParentBName", "Dana");

        // Act
        var (parentAName, parentBName) = await _service.GetParentNamesAsync();

        // Assert
        parentAName.Should().Be("Parent A"); // default
        parentBName.Should().Be("Dana");
    }

    #endregion

    #region SetParentNamesAsync Tests

    [Fact]
    public async Task SetParentNamesAsync_ShouldSetBothNames()
    {
        // Arrange
        var newParentAName = "Emma";
        var newParentBName = "Frank";

        // Act
        await _service.SetParentNamesAsync(newParentAName, newParentBName);

        // Assert
        var parentAValue = await _service.GetValueAsync("ParentAName");
        var parentBValue = await _service.GetValueAsync("ParentBName");

        parentAValue.Should().Be(newParentAName);
        parentBValue.Should().Be(newParentBName);
    }

    [Fact]
    public async Task SetParentNamesAsync_OverwriteExisting_ShouldUpdateBoth()
    {
        // Arrange
        await _service.SetParentNamesAsync("OldA", "OldB");

        // Act
        await _service.SetParentNamesAsync("NewA", "NewB");

        // Assert
        var (parentAName, parentBName) = await _service.GetParentNamesAsync();
        parentAName.Should().Be("NewA");
        parentBName.Should().Be("NewB");
    }

    [Fact]
    public async Task SetParentNamesAsync_ShouldPersistToDatabase()
    {
        // Arrange & Act
        await _service.SetParentNamesAsync("Grace", "Henry");

        // Assert
        var parentAConfig = await _context.Configurations
            .FirstOrDefaultAsync(c => c.Key == "ParentAName");
        var parentBConfig = await _context.Configurations
            .FirstOrDefaultAsync(c => c.Key == "ParentBName");

        parentAConfig.Should().NotBeNull();
        parentAConfig!.Value.Should().Be("Grace");

        parentBConfig.Should().NotBeNull();
        parentBConfig!.Value.Should().Be("Henry");
    }

    #endregion
}
