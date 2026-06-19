using CRM.Domain.Common;

namespace backend.Entities;

public class Lead : OwnedEntity
{
    public string LeadNumber { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? Website { get; set; }
    public Guid? LeadSourceId { get; set; }
    public LookupValue? LeadSource { get; set; }
    public Guid LeadStatusId { get; set; }
    public LookupValue LeadStatus { get; set; } = default!;
    public Guid? QualificationStatusId { get; set; }
    public LookupValue? QualificationStatus { get; set; }
    public Guid? RatingId { get; set; }
    public LookupValue? Rating { get; set; }
    public Guid? IndustryId { get; set; }
    public LookupValue? Industry { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public int Score { get; set; }
    public string? ScoreGrade { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public AppUser? AssignedToUser { get; set; }
    public Guid? AssignedToTeamId { get; set; }
    public Team? AssignedToTeam { get; set; }
    public Guid? ConvertedAccountId { get; set; }
    public Account? ConvertedAccount { get; set; }
    public Guid? ConvertedContactId { get; set; }
    public Contact? ConvertedContact { get; set; }
    public Guid? ConvertedOpportunityId { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public Guid? ConvertedById { get; set; }
    public AppUser? ConvertedBy { get; set; }
    public Guid? DisqualifiedReasonId { get; set; }
    public LookupValue? DisqualifiedReason { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
    public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
}
