using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Middleware;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/quotes")]
public class QuotesController : ControllerBase
{
    private const string QuoteStatusCategoryCode = "QUOTE_STATUS";
    private const string QuoteApprovalStatusCategoryCode = "QUOTE_APPROVAL_STATUS";
    private const string CurrencyCategoryCode = "CURRENCY";
    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly ICurrentUserContext _currentUserContext;

    public QuotesController(
        AppDbContext dbContext,
        INumberSequenceService numberSequenceService,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [HasPermission("Quotes.View")]
    public async Task<ActionResult<PagedResult<QuoteDto>>> GetQuotes([FromQuery] QuoteFilterDto query)
    {
        var quotes = _dbContext.Quotes.AsQueryable();

        if (query.AccountId.HasValue)
        {
            quotes = quotes.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (query.OpportunityId.HasValue)
        {
            quotes = quotes.Where(x => x.OpportunityId == query.OpportunityId.Value);
        }

        if (query.QuoteStatusId.HasValue)
        {
            quotes = quotes.Where(x => x.QuoteStatusId == query.QuoteStatusId.Value);
        }

        if (query.ApprovalStatusId.HasValue)
        {
            quotes = quotes.Where(x => x.ApprovalStatusId == query.ApprovalStatusId.Value);
        }

        if (query.IsActive.HasValue)
        {
            quotes = quotes.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            quotes = quotes.Where(x =>
                x.QuoteNumber.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search) ||
                (x.Contact != null && x.Contact.FullName.ToLower().Contains(search)) ||
                (x.Opportunity != null && x.Opportunity.Topic.ToLower().Contains(search)) ||
                x.PriceList.Name.ToLower().Contains(search));
        }

        quotes = quotes.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            quotes = quotes.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectQuotes(quotes).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Quotes.View")]
    public async Task<ActionResult<QuoteDto>> GetQuote(Guid id)
    {
        var quote = await ProjectQuotes(_dbContext.Quotes.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return quote is null ? NotFound() : Ok(quote);
    }

    [HttpPost]
    [HasPermission("Quotes.Create")]
    public async Task<ActionResult<QuoteDto>> CreateQuote(UpsertQuoteRequestDto dto)
    {
        var validationError = await ValidateQuoteAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var quote = new Quote();
        ApplyQuoteValues(quote, dto);
        if (string.IsNullOrWhiteSpace(quote.QuoteNumber))
        {
            quote.QuoteNumber = await _numberSequenceService.GenerateNextAsync("QUOTE");
        }

        if (await _dbContext.Quotes.AnyAsync(x => x.QuoteNumber == quote.QuoteNumber))
        {
            return BadRequest("Quote number already exists.");
        }

        _dbContext.Quotes.Add(quote);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectQuotes(_dbContext.Quotes.Where(x => x.Id == quote.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Quote was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Quotes.Update")]
    public async Task<IActionResult> UpdateQuote(Guid id, UpsertQuoteRequestDto dto)
    {
        var quote = await _dbContext.Quotes.FirstOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        var validationError = await ValidateQuoteAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyQuoteValues(quote, dto);
        if (string.IsNullOrWhiteSpace(quote.QuoteNumber))
        {
            quote.QuoteNumber = await _numberSequenceService.GenerateNextAsync("QUOTE");
        }

        if (await _dbContext.Quotes.AnyAsync(x => x.Id != id && x.QuoteNumber == quote.QuoteNumber))
        {
            return BadRequest("Quote number already exists.");
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Quotes.Delete")]
    public async Task<IActionResult> DeleteQuote(Guid id)
    {
        var quote = await _dbContext.Quotes.FirstOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        quote.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission("Quotes.Approve")]
    public async Task<IActionResult> ApproveQuote(Guid id)
    {
        var quote = await _dbContext.Quotes.FirstOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        var approvedStatusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == QuoteApprovalStatusCategoryCode && x.Code == "APPROVED")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!approvedStatusId.HasValue)
        {
            return BadRequest("Approved status lookup is missing.");
        }

        quote.ApprovalStatusId = approvedStatusId.Value;
        quote.ApprovedAt = DateTime.UtcNow;
        quote.ApprovedById = _currentUserContext.UserId;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/convert-to-order")]
    [HasPermission("Quotes.ConvertToOrder")]
    public async Task<ActionResult<object>> ConvertQuoteToOrder(Guid id)
    {
        var quote = await _dbContext.Quotes.FirstOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.ConvertedOrderId.HasValue)
        {
            return BadRequest("Quote has already been converted.");
        }

        quote.ConvertedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            QuoteId = quote.Id,
            quote.QuoteNumber,
            quote.ConvertedAt,
            Message = "Quote conversion marker recorded. Order creation will be finalized in Module 9."
        });
    }

    [HttpGet("lookup")]
    [HasPermission("Quotes.View")]
    public async Task<ActionResult<object>> GetLookup()
    {
        return Ok(new
        {
            QuoteStatuses = await GetLookupOptionsAsync(QuoteStatusCategoryCode),
            ApprovalStatuses = await GetLookupOptionsAsync(QuoteApprovalStatusCategoryCode),
            Currencies = await GetLookupOptionsAsync(CurrencyCategoryCode)
        });
    }

    [HttpGet("{quoteId:guid}/lines")]
    [HasPermission("QuoteLines.View")]
    public async Task<ActionResult<PagedResult<QuoteLineDto>>> GetQuoteLines(Guid quoteId, [FromQuery] QuoteLineFilterDto query)
    {
        if (!await _dbContext.Quotes.AnyAsync(x => x.Id == quoteId))
        {
            return NotFound();
        }

        query.QuoteId = quoteId;
        var lines = _dbContext.QuoteLines.AsQueryable();

        if (query.QuoteId.HasValue)
        {
            lines = lines.Where(x => x.QuoteId == query.QuoteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            lines = lines.Where(x =>
                x.ProductName.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        lines = lines.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            lines = lines.OrderBy(x => x.SortOrder).ThenBy(x => x.ProductName);
        }

        return Ok(await ProjectQuoteLines(lines).ToPagedAsync(query));
    }

    [HttpPost("{quoteId:guid}/lines")]
    [HasPermission("QuoteLines.Create")]
    public async Task<ActionResult<QuoteLineDto>> CreateQuoteLine(Guid quoteId, UpsertQuoteLineRequestDto dto)
    {
        if (!await _dbContext.Quotes.AnyAsync(x => x.Id == quoteId))
        {
            return NotFound();
        }

        var validationError = await ValidateQuoteLineAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var line = new QuoteLine { QuoteId = quoteId };
        ApplyQuoteLineValues(line, dto);

        _dbContext.QuoteLines.Add(line);
        await _dbContext.SaveChangesAsync();
        await RecalculateQuoteTotalsAsync(quoteId);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectQuoteLines(_dbContext.QuoteLines.Where(x => x.Id == line.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Quote line was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("lines/{id:guid}")]
    [HasPermission("QuoteLines.Update")]
    public async Task<IActionResult> UpdateQuoteLine(Guid id, UpsertQuoteLineRequestDto dto)
    {
        var line = await _dbContext.QuoteLines.FirstOrDefaultAsync(x => x.Id == id);
        if (line is null)
        {
            return NotFound();
        }

        var validationError = await ValidateQuoteLineAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyQuoteLineValues(line, dto);
        await _dbContext.SaveChangesAsync();
        await RecalculateQuoteTotalsAsync(line.QuoteId);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("lines/{id:guid}")]
    [HasPermission("QuoteLines.Delete")]
    public async Task<IActionResult> DeleteQuoteLine(Guid id)
    {
        var line = await _dbContext.QuoteLines.FirstOrDefaultAsync(x => x.Id == id);
        if (line is null)
        {
            return NotFound();
        }

        var quoteId = line.QuoteId;
        line.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        await RecalculateQuoteTotalsAsync(quoteId);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateQuoteAsync(Guid? quoteId, UpsertQuoteRequestDto dto)
    {
        if (!await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId))
        {
            return "Account is invalid.";
        }

        if (dto.ContactId.HasValue && !await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId.Value && x.AccountId == dto.AccountId))
        {
            return "Contact is invalid for the selected account.";
        }

        if (dto.OpportunityId.HasValue && !await _dbContext.Opportunities.AnyAsync(x => x.Id == dto.OpportunityId.Value && x.AccountId == dto.AccountId))
        {
            return "Opportunity is invalid for the selected account.";
        }

        if (!await _dbContext.PriceLists.AnyAsync(x => x.Id == dto.PriceListId))
        {
            return "Price list is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.CurrencyId && x.LookupCategory.Code == CurrencyCategoryCode))
        {
            return "Currency is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.QuoteStatusId && x.LookupCategory.Code == QuoteStatusCategoryCode))
        {
            return "Quote status is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ApprovalStatusId && x.LookupCategory.Code == QuoteApprovalStatusCategoryCode))
        {
            return "Approval status is invalid.";
        }

        if (dto.ValidFrom.HasValue && dto.ValidTo.HasValue && dto.ValidTo.Value < dto.ValidFrom.Value)
        {
            return "Valid To date must be greater than or equal to Valid From date.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value))
        {
            return "Owner user is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        if (!string.IsNullOrWhiteSpace(dto.QuoteNumber))
        {
            var number = dto.QuoteNumber.Trim();
            var exists = await _dbContext.Quotes.AnyAsync(x => x.Id != quoteId && x.QuoteNumber == number);
            if (exists)
            {
                return "Quote number already exists.";
            }
        }

        return null;
    }

    private async Task<string?> ValidateQuoteLineAsync(UpsertQuoteLineRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ProductName))
        {
            return "Product name is required.";
        }

        if (dto.Quantity <= 0)
        {
            return "Quantity must be greater than zero.";
        }

        if (dto.UnitPrice < 0)
        {
            return "Unit price cannot be negative.";
        }

        if (dto.DiscountPercent is < 0 or > 100)
        {
            return "Discount percent must be between 0 and 100.";
        }

        if (dto.TaxRate < 0)
        {
            return "Tax rate cannot be negative.";
        }

        if (dto.ProductId.HasValue && !await _dbContext.Products.AnyAsync(x => x.Id == dto.ProductId.Value))
        {
            return "Product is invalid.";
        }

        if (dto.ProductBundleId.HasValue && !await _dbContext.ProductBundles.AnyAsync(x => x.Id == dto.ProductBundleId.Value))
        {
            return "Product bundle is invalid.";
        }

        if (dto.UnitOfMeasureId.HasValue && !await _dbContext.UnitOfMeasures.AnyAsync(x => x.Id == dto.UnitOfMeasureId.Value))
        {
            return "Unit of measure is invalid.";
        }

        return null;
    }

    private static void ApplyQuoteValues(Quote quote, UpsertQuoteRequestDto dto)
    {
        quote.QuoteNumber = TrimToNull(dto.QuoteNumber) ?? string.Empty;
        quote.AccountId = dto.AccountId;
        quote.ContactId = dto.ContactId;
        quote.OpportunityId = dto.OpportunityId;
        quote.PriceListId = dto.PriceListId;
        quote.CurrencyId = dto.CurrencyId;
        quote.QuoteStatusId = dto.QuoteStatusId;
        quote.ApprovalStatusId = dto.ApprovalStatusId;
        quote.ValidFrom = dto.ValidFrom;
        quote.ValidTo = dto.ValidTo;
        quote.Notes = TrimToNull(dto.Notes);
        quote.TermsAndConditions = TrimToNull(dto.TermsAndConditions);
        quote.IsActive = dto.IsActive;
        quote.OwnerUserId = dto.OwnerUserId;
        quote.OwnerTeamId = dto.OwnerTeamId;
    }

    private static void ApplyQuoteLineValues(QuoteLine line, UpsertQuoteLineRequestDto dto)
    {
        line.ProductId = dto.ProductId;
        line.ProductBundleId = dto.ProductBundleId;
        line.ProductName = dto.ProductName.Trim();
        line.Description = TrimToNull(dto.Description);
        line.UnitOfMeasureId = dto.UnitOfMeasureId;
        line.Quantity = dto.Quantity;
        line.UnitPrice = dto.UnitPrice;
        line.DiscountPercent = dto.DiscountPercent;
        line.TaxRate = dto.TaxRate;
        line.SortOrder = dto.SortOrder;

        var gross = dto.Quantity * dto.UnitPrice;
        var discountAmount = Math.Round(gross * (dto.DiscountPercent / 100m), 2);
        var taxable = gross - discountAmount;
        var taxAmount = Math.Round(taxable * (dto.TaxRate / 100m), 2);
        var lineTotal = taxable + taxAmount;

        line.DiscountAmount = discountAmount;
        line.TaxAmount = taxAmount;
        line.LineTotal = Math.Round(lineTotal, 2);
    }

    private async Task RecalculateQuoteTotalsAsync(Guid quoteId)
    {
        var quote = await _dbContext.Quotes.FirstOrDefaultAsync(x => x.Id == quoteId);
        if (quote is null)
        {
            return;
        }

        var lines = await _dbContext.QuoteLines.Where(x => x.QuoteId == quoteId).ToListAsync();
        var subtotal = lines.Sum(x => x.Quantity * x.UnitPrice);
        var discount = lines.Sum(x => x.DiscountAmount);
        var tax = lines.Sum(x => x.TaxAmount);

        quote.SubtotalAmount = Math.Round(subtotal, 2);
        quote.DiscountAmount = Math.Round(discount, 2);
        quote.TaxAmount = Math.Round(tax, 2);
        quote.TotalAmount = Math.Round(quote.SubtotalAmount - quote.DiscountAmount + quote.TaxAmount, 2);
    }

    private async Task<List<LookupOptionDto>> GetLookupOptionsAsync(string categoryCode)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new LookupOptionDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .ToListAsync();
    }

    private static IQueryable<QuoteDto> ProjectQuotes(IQueryable<Quote> query)
    {
        return query.Select(x => new QuoteDto
        {
            Id = x.Id,
            QuoteNumber = x.QuoteNumber,
            AccountId = x.AccountId,
            AccountName = x.Account.Name,
            ContactId = x.ContactId,
            ContactName = x.Contact != null ? x.Contact.FullName : null,
            OpportunityId = x.OpportunityId,
            OpportunityTopic = x.Opportunity != null ? x.Opportunity.Topic : null,
            PriceListId = x.PriceListId,
            PriceListName = x.PriceList.Name,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            QuoteStatusId = x.QuoteStatusId,
            QuoteStatusName = x.QuoteStatus.Name,
            ApprovalStatusId = x.ApprovalStatusId,
            ApprovalStatusName = x.ApprovalStatus.Name,
            ValidFrom = x.ValidFrom,
            ValidTo = x.ValidTo,
            SubtotalAmount = x.SubtotalAmount,
            DiscountAmount = x.DiscountAmount,
            TaxAmount = x.TaxAmount,
            TotalAmount = x.TotalAmount,
            Notes = x.Notes,
            TermsAndConditions = x.TermsAndConditions,
            ApprovedById = x.ApprovedById,
            ApprovedAt = x.ApprovedAt,
            ConvertedOrderId = x.ConvertedOrderId,
            ConvertedAt = x.ConvertedAt,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private static IQueryable<QuoteLineDto> ProjectQuoteLines(IQueryable<QuoteLine> query)
    {
        return query.Select(x => new QuoteLineDto
        {
            Id = x.Id,
            QuoteId = x.QuoteId,
            ProductId = x.ProductId,
            ProductBundleId = x.ProductBundleId,
            ProductName = x.ProductName,
            Description = x.Description,
            UnitOfMeasureId = x.UnitOfMeasureId,
            UnitOfMeasureName = x.UnitOfMeasure != null ? x.UnitOfMeasure.Name : null,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            DiscountPercent = x.DiscountPercent,
            DiscountAmount = x.DiscountAmount,
            TaxRate = x.TaxRate,
            TaxAmount = x.TaxAmount,
            LineTotal = x.LineTotal,
            SortOrder = x.SortOrder,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
