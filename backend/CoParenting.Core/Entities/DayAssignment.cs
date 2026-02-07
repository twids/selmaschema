namespace CoParenting.Core.Entities;

/// <summary>
/// Represents a single day assignment in the co-parenting calendar
/// </summary>
public class DayAssignment
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>
    /// Which parent has the child: 'A', 'B', or null for unassigned
    /// </summary>
    public string? Parent { get; set; }

    /// <summary>
    /// Whether this is a VAB (Vård av Barn - child care leave) day
    /// </summary>
    public bool IsVAB { get; set; }

    /// <summary>
    /// Special status: NULL, 'PreschoolClosed', 'Holiday', etc.
    /// </summary>
    public string? SpecialStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    // Navigation property
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
