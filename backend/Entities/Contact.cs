using CRM.Domain.Common;

namespace backend.Entities;

public class Contact : OwnedEntity
{
    public string ContactNumber { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid? ContactRoleId { get; set; }
    public LookupValue? ContactRole { get; set; }
    public Guid? SalutationLookupId { get; set; }
    public LookupValue? Salutation { get; set; }
    public Guid? GenderLookupId { get; set; }
    public LookupValue? Gender { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? PreferredName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? AlternateEmail { get; set; }
    public string? WorkPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? HomePhone { get; set; }
    public string? Fax { get; set; }
    public Guid? PreferredContactMethodId { get; set; }
    public LookupValue? PreferredContactMethod { get; set; }
    public Guid? PreferredLanguageId { get; set; }
    public LookupValue? PreferredLanguage { get; set; }
    public Guid? PreferredTimeZoneId { get; set; }
    public LookupValue? PreferredTimeZone { get; set; }
    public bool MarketingConsent { get; set; }
    public bool EmailOptIn { get; set; }
    public bool SMSOptIn { get; set; }
    public bool PhoneOptIn { get; set; }
    public bool IsPrimaryContact { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public ICollection<ContactCommunication> Communications { get; set; } = new List<ContactCommunication>();
    public ICollection<ContactInteraction> Interactions { get; set; } = new List<ContactInteraction>();
}
