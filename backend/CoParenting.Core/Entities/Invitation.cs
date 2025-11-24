namespace CoParenting.Core.Entities;

/// <summary>
/// Represents an invitation to be a secondary parent for a child
/// </summary>
public class Invitation
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the user sending the invitation
    /// </summary>
    public int InviterId { get; set; }
    
    /// <summary>
    /// Email address of the person being invited
    /// </summary>
    public string InviteeEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the child this invitation is for
    /// </summary>
    public int ChildId { get; set; }
    
    /// <summary>
    /// Status: Pending, Accepted, Declined
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? RespondedAt { get; set; }
    
    // Navigation properties
    public User Inviter { get; set; } = null!;
    public Child Child { get; set; } = null!;
}
