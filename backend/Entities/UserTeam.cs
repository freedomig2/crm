namespace backend.Entities;

public class UserTeam
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
