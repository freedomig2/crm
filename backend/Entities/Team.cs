namespace backend.Entities;

public class Team : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? OwnerUserId { get; set; }
    public AppUser? OwnerUser { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
}
