using CRM.Domain.Common;

namespace backend.Entities;

public class CaseComment : BaseEntity
{
    public Guid CaseId { get; set; }
    public ServiceCase Case { get; set; } = default!;
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
