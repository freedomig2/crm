using CRM.Domain.Common;

namespace backend.Entities;

public class Contact : OwnedEntity
{
    public Guid AccountId { get; set; }
    public Guid? ContactRoleId { get; set; }
    public Guid? SalutationId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? DepartmentName { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? Extension { get; set; }
    public Guid? PreferredCommunicationId { get; set; }
    public bool IsPrimaryContact { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
}
