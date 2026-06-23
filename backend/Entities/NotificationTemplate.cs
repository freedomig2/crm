using CRM.Domain.Common;

namespace backend.Entities;

public class NotificationTemplate : ActivatableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public Guid ChannelId { get; set; }
    public LookupValue Channel { get; set; } = default!;
    public bool IsSystem { get; set; }
}
