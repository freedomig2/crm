namespace backend.Entities;

public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedFromIp { get; set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt && !IsDeleted;
}
