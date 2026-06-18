using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/customer-profiles")]
public class CustomerProfilesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CustomerProfilesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("CustomerProfiles.View")]
    public async Task<ActionResult<PagedResult<CustomerProfileDto>>> GetCustomerProfiles([FromQuery] ListQueryDto query)
    {
        var profilesQuery = _dbContext.CustomerProfiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            profilesQuery = profilesQuery.Where(x => (x.Notes ?? string.Empty).ToLower().Contains(search));
        }

        profilesQuery = profilesQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = profilesQuery.Select(x => new CustomerProfileDto
        {
            Id = x.Id,
            AccountId = x.AccountId,
            CreditLimit = x.CreditLimit,
            PaymentTermsId = x.PaymentTermsId,
            PreferredCurrencyId = x.PreferredCurrencyId,
            PreferredLanguageId = x.PreferredLanguageId,
            TimeZoneId = x.TimeZoneId,
            RiskRatingId = x.RiskRatingId,
            LifecycleStageId = x.LifecycleStageId,
            CustomerSince = x.CustomerSince,
            LastReviewDate = x.LastReviewDate,
            NextReviewDate = x.NextReviewDate,
            ChurnRiskScore = x.ChurnRiskScore,
            SatisfactionScore = x.SatisfactionScore,
            Notes = x.Notes
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("CustomerProfiles.View")]
    public async Task<ActionResult<CustomerProfileDto>> GetCustomerProfile(Guid id)
    {
        var item = await _dbContext.CustomerProfiles
            .Where(x => x.Id == id)
            .Select(x => new CustomerProfileDto
            {
                Id = x.Id,
                AccountId = x.AccountId,
                CreditLimit = x.CreditLimit,
                PaymentTermsId = x.PaymentTermsId,
                PreferredCurrencyId = x.PreferredCurrencyId,
                PreferredLanguageId = x.PreferredLanguageId,
                TimeZoneId = x.TimeZoneId,
                RiskRatingId = x.RiskRatingId,
                LifecycleStageId = x.LifecycleStageId,
                CustomerSince = x.CustomerSince,
                LastReviewDate = x.LastReviewDate,
                NextReviewDate = x.NextReviewDate,
                ChurnRiskScore = x.ChurnRiskScore,
                SatisfactionScore = x.SatisfactionScore,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("CustomerProfiles.Create")]
    public async Task<ActionResult<CustomerProfileDto>> CreateCustomerProfile(UpsertCustomerProfileRequestDto dto)
    {
        var item = new CustomerProfile
        {
            AccountId = dto.AccountId,
            CreditLimit = dto.CreditLimit,
            PaymentTermsId = dto.PaymentTermsId,
            PreferredCurrencyId = dto.PreferredCurrencyId,
            PreferredLanguageId = dto.PreferredLanguageId,
            TimeZoneId = dto.TimeZoneId,
            RiskRatingId = dto.RiskRatingId,
            LifecycleStageId = dto.LifecycleStageId,
            CustomerSince = dto.CustomerSince,
            LastReviewDate = dto.LastReviewDate,
            NextReviewDate = dto.NextReviewDate,
            ChurnRiskScore = dto.ChurnRiskScore,
            SatisfactionScore = dto.SatisfactionScore,
            Notes = dto.Notes
        };

        _dbContext.CustomerProfiles.Add(item);
        await _dbContext.SaveChangesAsync();

        return Ok(new CustomerProfileDto
        {
            Id = item.Id,
            AccountId = item.AccountId,
            CreditLimit = item.CreditLimit,
            PaymentTermsId = item.PaymentTermsId,
            PreferredCurrencyId = item.PreferredCurrencyId,
            PreferredLanguageId = item.PreferredLanguageId,
            TimeZoneId = item.TimeZoneId,
            RiskRatingId = item.RiskRatingId,
            LifecycleStageId = item.LifecycleStageId,
            CustomerSince = item.CustomerSince,
            LastReviewDate = item.LastReviewDate,
            NextReviewDate = item.NextReviewDate,
            ChurnRiskScore = item.ChurnRiskScore,
            SatisfactionScore = item.SatisfactionScore,
            Notes = item.Notes
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("CustomerProfiles.Update")]
    public async Task<IActionResult> UpdateCustomerProfile(Guid id, UpsertCustomerProfileRequestDto dto)
    {
        var item = await _dbContext.CustomerProfiles.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.AccountId = dto.AccountId;
        item.CreditLimit = dto.CreditLimit;
        item.PaymentTermsId = dto.PaymentTermsId;
        item.PreferredCurrencyId = dto.PreferredCurrencyId;
        item.PreferredLanguageId = dto.PreferredLanguageId;
        item.TimeZoneId = dto.TimeZoneId;
        item.RiskRatingId = dto.RiskRatingId;
        item.LifecycleStageId = dto.LifecycleStageId;
        item.CustomerSince = dto.CustomerSince;
        item.LastReviewDate = dto.LastReviewDate;
        item.NextReviewDate = dto.NextReviewDate;
        item.ChurnRiskScore = dto.ChurnRiskScore;
        item.SatisfactionScore = dto.SatisfactionScore;
        item.Notes = dto.Notes;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("CustomerProfiles.Delete")]
    public async Task<IActionResult> DeleteCustomerProfile(Guid id)
    {
        var item = await _dbContext.CustomerProfiles.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
