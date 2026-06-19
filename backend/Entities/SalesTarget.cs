using CRM.Domain.Common;

namespace backend.Entities;

public class SalesTarget : OwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TargetTypeId { get; set; }
    public LookupValue TargetType { get; set; } = default!;
    public Guid TargetPeriodId { get; set; }
    public LookupValue TargetPeriod { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal AchievementPercentage { get; set; }
    public Guid? AssignedUserId { get; set; }
    public AppUser? AssignedUser { get; set; }
    public Guid? AssignedTeamId { get; set; }
    public Team? AssignedTeam { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
}
