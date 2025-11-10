namespace CoParenting.Core.Entities;

/// <summary>
/// Stores configuration settings like parent names
/// </summary>
public class Configuration
{
    public int Id { get; set; }
    
    public string Key { get; set; } = string.Empty;
    
    public string Value { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
}
