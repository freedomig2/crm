namespace backend.Entities;

public class PasswordResetToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsValid => UsedAt is null && DateTime.UtcNow <= ExpiresAt && !IsDeleted;
}
