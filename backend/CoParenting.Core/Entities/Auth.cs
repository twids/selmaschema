namespace CoParenting.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin, ParentA, ParentB
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsLocalAdmin { get; set; }
}

public class Session
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class ExternalIdentity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Issuer { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string NormalizedIssuer { get; set; } = string.Empty;
    public string NormalizedSubject { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
}

public class Invitation
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? EmailHint { get; set; }
    public string Role { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int? RedeemedByUserId { get; set; }
    public User? RedeemedByUser { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public Guid ConcurrencyToken { get; set; }
}

public class ChangeRequest
{
    public int Id { get; set; }
    public int RequestedByUserId { get; set; }
    public User RequestedByUser { get; set; } = null!;
    public DateOnly RequestedForDate { get; set; }
    public string CurrentParent { get; set; } = string.Empty;
    public string RequestedParent { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public string? Comment { get; set; }
}
