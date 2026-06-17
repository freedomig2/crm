namespace backend.Entities;

public class UserProfile : AuditableEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
}
