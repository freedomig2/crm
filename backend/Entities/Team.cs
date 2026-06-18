using CRM.Domain.Common;

namespace backend.Entities;

public class Team : OwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AppUser? OwnerUser { get; set; }

    public ICollection<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
}
