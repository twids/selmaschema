namespace CoParenting.Core.Entities;

/// <summary>
/// Represents a child in the co-parenting system
/// </summary>
public class Child
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// ID of the primary parent (creator)
    /// </summary>
    public int PrimaryParentId { get; set; }
    
    /// <summary>
    /// ID of the secondary parent (optional, added via invitation)
    /// </summary>
    public int? SecondaryParentId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    // Navigation properties
    public User PrimaryParent { get; set; } = null!;
    public User? SecondaryParent { get; set; }
}
