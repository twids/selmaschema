namespace CoParenting.Application.Interfaces;

public interface IConfigurationService
{
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value);
    Task<(string parentAName, string parentBName)> GetParentNamesAsync();
    Task SetParentNamesAsync(string parentAName, string parentBName);
}
