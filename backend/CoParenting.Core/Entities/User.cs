namespace CoParenting.Core.Entities;

/// <summary>
/// Represents a user authenticated via Google or demo mode
/// </summary>
public class User
{
    public int Id { get; set; }
    
    /// <summary>
    /// Google ID or "demo" for demo users
    /// </summary>
    public string GoogleId { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public bool IsDemo { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties
    public ICollection<Child> Children { get; set; } = new List<Child>();
    public ICollection<Invitation> SentInvitations { get; set; } = new List<Invitation>();
}
