namespace CRM.Domain.Common;

public abstract class ActivatableEntity : BaseEntity
{
    public bool IsActive { get; set; } = true;
}
