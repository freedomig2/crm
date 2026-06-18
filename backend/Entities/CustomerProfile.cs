using CRM.Domain.Common;

namespace backend.Entities;

public class CustomerProfile : BaseEntity
{
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
