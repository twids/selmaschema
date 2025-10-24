namespace CoParenting.Core.Entities;

/// <summary>
/// Represents a single day assignment in the co-parenting calendar
/// </summary>
public class DayAssignment
{
    public int Id { get; set; }
    
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Which parent has the child: 'A', 'B', or null for unassigned
    /// </summary>
    public string? Parent { get; set; }
    
    /// <summary>
    /// Whether this is a VAB (Vård av Barn - child care leave) day
    /// </summary>
    public bool IsVAB { get; set; }
    
    /// <summary>
    /// Optional comment for the day
    /// </summary>
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
}
