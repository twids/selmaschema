namespace CoParenting.Core.Entities;

/// <summary>
/// Represents a comment made by a parent on a specific day
/// Supports multiple comments per day per parent for conversation-style interaction
/// </summary>
public class Comment
{
    public int Id { get; set; }
    
    public int DayAssignmentId { get; set; }
    
    /// <summary>
    /// Which parent made the comment: 'A' or 'B'
    /// </summary>
    public string Parent { get; set; } = string.Empty;
    
    /// <summary>
    /// The comment text (max 1000 chars)
    /// </summary>
    public string CommentText { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    // Navigation property
    public DayAssignment? DayAssignment { get; set; }
}
