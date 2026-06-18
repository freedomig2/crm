using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TradingName { get; set; }
    public Guid? AccountTypeId { get; set; }
    public Guid? IndustryId { get; set; }
    public Guid? OwnershipTypeId { get; set; }
    public Guid? CustomerStatusId { get; set; }
    public Guid? CustomerSegmentId { get; set; }
    public string? Website { get; set; }
    public string? MainPhone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Fax { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? Description { get; set; }
    public Guid? ParentAccountId { get; set; }
    public Guid? PrimaryContactId { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class UpsertAccountRequestDto
{
    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? LegalName { get; set; }
    public string? TradingName { get; set; }
    public Guid? AccountTypeId { get; set; }
    public Guid? IndustryId { get; set; }
    public Guid? OwnershipTypeId { get; set; }
    public Guid? CustomerStatusId { get; set; }
    public Guid? CustomerSegmentId { get; set; }
    public string? Website { get; set; }
    public string? MainPhone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Fax { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? Description { get; set; }
    public Guid? ParentAccountId { get; set; }
    public Guid? PrimaryContactId { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class ContactDto
{
    public Guid Id { get; set; }
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
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class UpsertContactRequestDto
{
    [Required]
    public Guid AccountId { get; set; }

    public Guid? ContactRoleId { get; set; }
    public Guid? SalutationId { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }
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
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class AccountAddressDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? AddressTypeId { get; set; }
    public string? AttentionTo { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? Landmark { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public Guid? CountryId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsBilling { get; set; }
    public bool IsShipping { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertAccountAddressRequestDto
{
    [Required]
    public Guid AccountId { get; set; }

    public Guid? AddressTypeId { get; set; }
    public string? AttentionTo { get; set; }

    [Required]
    public string Line1 { get; set; } = string.Empty;

    public string? Line2 { get; set; }
    public string? Landmark { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public Guid? CountryId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsBilling { get; set; }
    public bool IsShipping { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal? CreditLimit { get; set; }
    public Guid? PaymentTermsId { get; set; }
    public Guid? PreferredCurrencyId { get; set; }
    public Guid? PreferredLanguageId { get; set; }
    public Guid? TimeZoneId { get; set; }
    public Guid? RiskRatingId { get; set; }
    public Guid? LifecycleStageId { get; set; }
    public DateTime? CustomerSince { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public decimal? ChurnRiskScore { get; set; }
    public decimal? SatisfactionScore { get; set; }
    public string? Notes { get; set; }
}

public class UpsertCustomerProfileRequestDto
{
    [Required]
    public Guid AccountId { get; set; }

    public decimal? CreditLimit { get; set; }
    public Guid? PaymentTermsId { get; set; }
    public Guid? PreferredCurrencyId { get; set; }
    public Guid? PreferredLanguageId { get; set; }
    public Guid? TimeZoneId { get; set; }
    public Guid? RiskRatingId { get; set; }
    public Guid? LifecycleStageId { get; set; }
    public DateTime? CustomerSince { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public decimal? ChurnRiskScore { get; set; }
    public decimal? SatisfactionScore { get; set; }
    public string? Notes { get; set; }
}

public class AccountRelationshipDto
{
    public Guid Id { get; set; }
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public Guid? RelationshipTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? StrengthId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertAccountRelationshipRequestDto
{
    [Required]
    public Guid SourceAccountId { get; set; }

    [Required]
    public Guid TargetAccountId { get; set; }

    public Guid? RelationshipTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? StrengthId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AccountActivityDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? OutcomeId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsPrivate { get; set; }
    public bool FollowUpRequired { get; set; }
    public DateTime? FollowUpDate { get; set; }
}

public class UpsertAccountActivityRequestDto
{
    [Required]
    public Guid AccountId { get; set; }

    public Guid? ContactId { get; set; }
    public Guid? ActivityTypeId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? OutcomeId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsPrivate { get; set; }
    public bool FollowUpRequired { get; set; }
    public DateTime? FollowUpDate { get; set; }
}