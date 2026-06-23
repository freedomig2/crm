using CRM.Domain.Common;

namespace backend.Entities;

public class ActivityComment : BaseEntity
{
    public Guid ActivityId { get; set; }
    public Activity Activity { get; set; } = default!;
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
