using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Application.Services;

/// <summary>
/// Service for managing application configuration
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly CoParentingDbContext _context;

    public ConfigurationService(CoParentingDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var config = await _context.Configurations
            .FirstOrDefaultAsync(c => c.Key == key);
        return config?.Value;
    }

    public async Task SetValueAsync(string key, string value)
    {
        var config = await _context.Configurations
            .FirstOrDefaultAsync(c => c.Key == key);

        if (config != null)
        {
            config.Value = value;
            config.ModifiedAt = DateTime.UtcNow;
        }
        else
        {
            config = new Configuration
            {
                Key = key,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };
            _context.Configurations.Add(config);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<(string parentAName, string parentBName)> GetParentNamesAsync()
    {
        var parentA = await GetValueAsync("ParentAName") ?? "Parent A";
        var parentB = await GetValueAsync("ParentBName") ?? "Parent B";
        return (parentA, parentB);
    }

    public async Task SetParentNamesAsync(string parentAName, string parentBName)
    {
        await SetValueAsync("ParentAName", parentAName);
        await SetValueAsync("ParentBName", parentBName);
    }
}
