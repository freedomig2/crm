using CRM.Domain.Common;

namespace backend.Entities;

public class ContactCommunication : BaseEntity
{
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = default!;
    public Guid? CommunicationTypeId { get; set; }
    public LookupValue? CommunicationType { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? Notes { get; set; }
}
