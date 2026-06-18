namespace CRM.Domain.Common;

public abstract class OwnedEntity : ActivatableEntity
{
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}
