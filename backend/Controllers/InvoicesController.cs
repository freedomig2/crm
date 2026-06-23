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
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private const string InvoiceStatusCategoryCode = "INVOICE_STATUS";
    private const string InvoicePaymentStatusCategoryCode = "INVOICE_PAYMENT_STATUS";
    private const string CurrencyCategoryCode = "CURRENCY";

    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;

    public InvoicesController(AppDbContext dbContext, INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
    }

    [HttpGet]
    [HasPermission("Invoices.View")]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetInvoices([FromQuery] InvoiceFilterDto query)
    {
        var invoices = _dbContext.Invoices.AsQueryable();

        if (query.AccountId.HasValue)
        {
            invoices = invoices.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (query.OpportunityId.HasValue)
        {
            invoices = invoices.Where(x => x.OpportunityId == query.OpportunityId.Value);
        }

        if (query.InvoiceStatusId.HasValue)
        {
            invoices = invoices.Where(x => x.InvoiceStatusId == query.InvoiceStatusId.Value);
        }

        if (query.PaymentStatusId.HasValue)
        {
            invoices = invoices.Where(x => x.PaymentStatusId == query.PaymentStatusId.Value);
        }

        if (query.IsActive.HasValue)
        {
            invoices = invoices.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            invoices = invoices.Where(x =>
                x.InvoiceNumber.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search) ||
                (x.Contact != null && x.Contact.FullName.ToLower().Contains(search)) ||
                (x.Opportunity != null && x.Opportunity.Topic.ToLower().Contains(search)) ||
                (x.Order != null && x.Order.OrderNumber.ToLower().Contains(search)) ||
                (x.Quote != null && x.Quote.QuoteNumber.ToLower().Contains(search)));
        }

        invoices = invoices.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            invoices = invoices.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectInvoices(invoices).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Invoices.View")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
    {
        var invoice = await ProjectInvoices(_dbContext.Invoices.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    [HasPermission("Invoices.Create")]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice(UpsertInvoiceRequestDto dto)
    {
        var validationError = await ValidateInvoiceAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var invoice = new Invoice();
        ApplyInvoiceValues(invoice, dto);
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            invoice.InvoiceNumber = await _numberSequenceService.GenerateNextAsync("INVOICE");
        }

        if (await _dbContext.Invoices.AnyAsync(x => x.InvoiceNumber == invoice.InvoiceNumber))
        {
            return BadRequest("Invoice number already exists.");
        }

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectInvoices(_dbContext.Invoices.Where(x => x.Id == invoice.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Invoice was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Invoices.Update")]
    public async Task<IActionResult> UpdateInvoice(Guid id, UpsertInvoiceRequestDto dto)
    {
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        var validationError = await ValidateInvoiceAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyInvoiceValues(invoice, dto);
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            invoice.InvoiceNumber = await _numberSequenceService.GenerateNextAsync("INVOICE");
        }

        if (await _dbContext.Invoices.AnyAsync(x => x.Id != id && x.InvoiceNumber == invoice.InvoiceNumber))
        {
            return BadRequest("Invoice number already exists.");
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Invoices.Delete")]
    public async Task<IActionResult> DeleteInvoice(Guid id)
    {
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        invoice.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("lookup")]
    [HasPermission("Invoices.View")]
    public async Task<ActionResult<object>> GetLookup()
    {
        return Ok(new
        {
            InvoiceStatuses = await GetLookupOptionsAsync(InvoiceStatusCategoryCode),
            PaymentStatuses = await GetLookupOptionsAsync(InvoicePaymentStatusCategoryCode),
            Currencies = await GetLookupOptionsAsync(CurrencyCategoryCode)
        });
    }

    [HttpGet("{invoiceId:guid}/lines")]
    [HasPermission("InvoiceLines.View")]
    public async Task<ActionResult<PagedResult<InvoiceLineDto>>> GetInvoiceLines(Guid invoiceId, [FromQuery] InvoiceLineFilterDto query)
    {
        if (!await _dbContext.Invoices.AnyAsync(x => x.Id == invoiceId))
        {
            return NotFound();
        }

        query.InvoiceId = invoiceId;
        var lines = _dbContext.InvoiceLines.AsQueryable();

        if (query.InvoiceId.HasValue)
        {
            lines = lines.Where(x => x.InvoiceId == query.InvoiceId.Value);
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

        return Ok(await ProjectInvoiceLines(lines).ToPagedAsync(query));
    }

    [HttpPost("{invoiceId:guid}/lines")]
    [HasPermission("InvoiceLines.Create")]
    public async Task<ActionResult<InvoiceLineDto>> CreateInvoiceLine(Guid invoiceId, UpsertInvoiceLineRequestDto dto)
    {
        if (!await _dbContext.Invoices.AnyAsync(x => x.Id == invoiceId))
        {
            return NotFound();
        }

        var validationError = await ValidateInvoiceLineAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var line = new InvoiceLine { InvoiceId = invoiceId };
        ApplyInvoiceLineValues(line, dto);

        _dbContext.InvoiceLines.Add(line);
        await _dbContext.SaveChangesAsync();
        await RecalculateInvoiceTotalsAsync(invoiceId);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectInvoiceLines(_dbContext.InvoiceLines.Where(x => x.Id == line.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Invoice line was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("lines/{id:guid}")]
    [HasPermission("InvoiceLines.Update")]
    public async Task<IActionResult> UpdateInvoiceLine(Guid id, UpsertInvoiceLineRequestDto dto)
    {
        var line = await _dbContext.InvoiceLines.FirstOrDefaultAsync(x => x.Id == id);
        if (line is null)
        {
            return NotFound();
        }

        var validationError = await ValidateInvoiceLineAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyInvoiceLineValues(line, dto);
        await _dbContext.SaveChangesAsync();
        await RecalculateInvoiceTotalsAsync(line.InvoiceId);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("lines/{id:guid}")]
    [HasPermission("InvoiceLines.Delete")]
    public async Task<IActionResult> DeleteInvoiceLine(Guid id)
    {
        var line = await _dbContext.InvoiceLines.FirstOrDefaultAsync(x => x.Id == id);
        if (line is null)
        {
            return NotFound();
        }

        var invoiceId = line.InvoiceId;
        line.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        await RecalculateInvoiceTotalsAsync(invoiceId);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/mark-paid")]
    [HasPermission("Invoices.MarkPaid")]
    public async Task<IActionResult> MarkPaid(Guid id, MarkInvoicePaidRequestDto dto)
    {
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (dto.PaidAmount <= 0)
        {
            return BadRequest("Paid amount must be greater than zero.");
        }

        if (invoice.TotalAmount <= 0)
        {
            return BadRequest("Invoice total must be greater than zero before marking as paid.");
        }

        invoice.PaidAmount = Math.Round(Math.Min(invoice.TotalAmount, invoice.PaidAmount + dto.PaidAmount), 2);
        invoice.PaidDate = dto.PaidDate ?? DateTime.UtcNow;

        var fullPaidStatusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == InvoicePaymentStatusCategoryCode && x.Code == "PAID")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
        var partialPaidStatusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == InvoicePaymentStatusCategoryCode && x.Code == "PARTIALLY_PAID")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!fullPaidStatusId.HasValue || !partialPaidStatusId.HasValue)
        {
            return BadRequest("Invoice payment status lookups are missing.");
        }

        invoice.PaymentStatusId = invoice.PaidAmount >= invoice.TotalAmount ? fullPaidStatusId.Value : partialPaidStatusId.Value;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("from-order/{orderId:guid}")]
    [HasPermission("Invoices.Create")]
    public async Task<ActionResult<InvoiceDto>> CreateFromOrder(Guid orderId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
        if (order is null)
        {
            return NotFound();
        }

        if (order.ConvertedInvoiceId.HasValue)
        {
            return BadRequest("Order has already been converted to an invoice.");
        }

        var defaultInvoiceStatusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == InvoiceStatusCategoryCode && x.Code == "ISSUED")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
        var defaultPaymentStatusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == InvoicePaymentStatusCategoryCode && x.Code == "UNPAID")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!defaultInvoiceStatusId.HasValue || !defaultPaymentStatusId.HasValue)
        {
            return BadRequest("Invoice default status lookup values are missing.");
        }

        var invoice = new Invoice
        {
            InvoiceNumber = await _numberSequenceService.GenerateNextAsync("INVOICE"),
            OrderId = order.Id,
            QuoteId = order.QuoteId,
            AccountId = order.AccountId,
            ContactId = order.ContactId,
            OpportunityId = order.OpportunityId,
            CurrencyId = order.CurrencyId,
            InvoiceStatusId = defaultInvoiceStatusId.Value,
            PaymentStatusId = defaultPaymentStatusId.Value,
            InvoiceDate = DateTime.UtcNow,
            DueDate = order.BillingDate ?? DateTime.UtcNow.AddDays(30),
            Notes = order.Notes,
            IsActive = order.IsActive,
            OwnerUserId = order.OwnerUserId,
            OwnerTeamId = order.OwnerTeamId
        };

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var orderLines = await _dbContext.OrderLines.Where(x => x.OrderId == order.Id).OrderBy(x => x.SortOrder).ToListAsync();
        foreach (var orderLine in orderLines)
        {
            _dbContext.InvoiceLines.Add(new InvoiceLine
            {
                InvoiceId = invoice.Id,
                ProductId = orderLine.ProductId,
                ProductBundleId = orderLine.ProductBundleId,
                ProductName = orderLine.ProductName,
                Description = orderLine.Description,
                UnitOfMeasureId = orderLine.UnitOfMeasureId,
                Quantity = orderLine.Quantity,
                UnitPrice = orderLine.UnitPrice,
                DiscountPercent = orderLine.DiscountPercent,
                DiscountAmount = orderLine.DiscountAmount,
                TaxRate = orderLine.TaxRate,
                TaxAmount = orderLine.TaxAmount,
                LineTotal = orderLine.LineTotal,
                SortOrder = orderLine.SortOrder
            });
        }

        await _dbContext.SaveChangesAsync();
        await RecalculateInvoiceTotalsAsync(invoice.Id);

        order.ConvertedInvoiceId = invoice.Id;
        order.ConvertedInvoiceAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var created = await ProjectInvoices(_dbContext.Invoices.Where(x => x.Id == invoice.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Invoice was created but could not be loaded.") : Ok(created);
    }

    private async Task<string?> ValidateInvoiceAsync(Guid? invoiceId, UpsertInvoiceRequestDto dto)
    {
        if (!await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId))
        {
            return "Account is invalid.";
        }

        if (dto.OrderId.HasValue && !await _dbContext.Orders.AnyAsync(x => x.Id == dto.OrderId.Value && x.AccountId == dto.AccountId))
        {
            return "Order is invalid for the selected account.";
        }

        if (dto.QuoteId.HasValue && !await _dbContext.Quotes.AnyAsync(x => x.Id == dto.QuoteId.Value && x.AccountId == dto.AccountId))
        {
            return "Quote is invalid for the selected account.";
        }

        if (dto.ContactId.HasValue && !await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId.Value && x.AccountId == dto.AccountId))
        {
            return "Contact is invalid for the selected account.";
        }

        if (dto.OpportunityId.HasValue && !await _dbContext.Opportunities.AnyAsync(x => x.Id == dto.OpportunityId.Value && x.AccountId == dto.AccountId))
        {
            return "Opportunity is invalid for the selected account.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.CurrencyId && x.LookupCategory.Code == CurrencyCategoryCode))
        {
            return "Currency is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.InvoiceStatusId && x.LookupCategory.Code == InvoiceStatusCategoryCode))
        {
            return "Invoice status is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.PaymentStatusId && x.LookupCategory.Code == InvoicePaymentStatusCategoryCode))
        {
            return "Payment status is invalid.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value))
        {
            return "Owner user is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        if (dto.PaidAmount < 0)
        {
            return "Paid amount cannot be negative.";
        }

        if (dto.PaidDate.HasValue && dto.InvoiceDate.HasValue && dto.PaidDate.Value < dto.InvoiceDate.Value)
        {
            return "Paid date must be greater than or equal to invoice date.";
        }

        if (dto.DueDate.HasValue && dto.InvoiceDate.HasValue && dto.DueDate.Value < dto.InvoiceDate.Value)
        {
            return "Due date must be greater than or equal to invoice date.";
        }

        if (!string.IsNullOrWhiteSpace(dto.InvoiceNumber))
        {
            var number = dto.InvoiceNumber.Trim();
            var exists = await _dbContext.Invoices.AnyAsync(x => x.Id != invoiceId && x.InvoiceNumber == number);
            if (exists)
            {
                return "Invoice number already exists.";
            }
        }

        return null;
    }

    private async Task<string?> ValidateInvoiceLineAsync(UpsertInvoiceLineRequestDto dto)
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

    private static void ApplyInvoiceValues(Invoice invoice, UpsertInvoiceRequestDto dto)
    {
        invoice.InvoiceNumber = TrimToNull(dto.InvoiceNumber) ?? string.Empty;
        invoice.OrderId = dto.OrderId;
        invoice.QuoteId = dto.QuoteId;
        invoice.AccountId = dto.AccountId;
        invoice.ContactId = dto.ContactId;
        invoice.OpportunityId = dto.OpportunityId;
        invoice.CurrencyId = dto.CurrencyId;
        invoice.InvoiceStatusId = dto.InvoiceStatusId;
        invoice.PaymentStatusId = dto.PaymentStatusId;
        invoice.DueDate = dto.DueDate;
        invoice.InvoiceDate = dto.InvoiceDate;
        invoice.PaidDate = dto.PaidDate;
        invoice.PaidAmount = Math.Round(dto.PaidAmount, 2);
        invoice.Notes = TrimToNull(dto.Notes);
        invoice.IsActive = dto.IsActive;
        invoice.OwnerUserId = dto.OwnerUserId;
        invoice.OwnerTeamId = dto.OwnerTeamId;
    }

    private static void ApplyInvoiceLineValues(InvoiceLine line, UpsertInvoiceLineRequestDto dto)
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

    private async Task RecalculateInvoiceTotalsAsync(Guid invoiceId)
    {
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId);
        if (invoice is null)
        {
            return;
        }

        var lines = await _dbContext.InvoiceLines.Where(x => x.InvoiceId == invoiceId).ToListAsync();
        var subtotal = lines.Sum(x => x.Quantity * x.UnitPrice);
        var discount = lines.Sum(x => x.DiscountAmount);
        var tax = lines.Sum(x => x.TaxAmount);

        invoice.SubtotalAmount = Math.Round(subtotal, 2);
        invoice.DiscountAmount = Math.Round(discount, 2);
        invoice.TaxAmount = Math.Round(tax, 2);
        invoice.TotalAmount = Math.Round(invoice.SubtotalAmount - invoice.DiscountAmount + invoice.TaxAmount, 2);

        if (invoice.PaidAmount > invoice.TotalAmount)
        {
            invoice.PaidAmount = invoice.TotalAmount;
        }
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

    private static IQueryable<InvoiceDto> ProjectInvoices(IQueryable<Invoice> query)
    {
        return query.Select(x => new InvoiceDto
        {
            Id = x.Id,
            InvoiceNumber = x.InvoiceNumber,
            OrderId = x.OrderId,
            OrderNumber = x.Order != null ? x.Order.OrderNumber : null,
            QuoteId = x.QuoteId,
            QuoteNumber = x.Quote != null ? x.Quote.QuoteNumber : null,
            AccountId = x.AccountId,
            AccountName = x.Account.Name,
            ContactId = x.ContactId,
            ContactName = x.Contact != null ? x.Contact.FullName : null,
            OpportunityId = x.OpportunityId,
            OpportunityTopic = x.Opportunity != null ? x.Opportunity.Topic : null,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            InvoiceStatusId = x.InvoiceStatusId,
            InvoiceStatusName = x.InvoiceStatus.Name,
            PaymentStatusId = x.PaymentStatusId,
            PaymentStatusName = x.PaymentStatus.Name,
            DueDate = x.DueDate,
            InvoiceDate = x.InvoiceDate,
            PaidDate = x.PaidDate,
            SubtotalAmount = x.SubtotalAmount,
            DiscountAmount = x.DiscountAmount,
            TaxAmount = x.TaxAmount,
            TotalAmount = x.TotalAmount,
            PaidAmount = x.PaidAmount,
            Notes = x.Notes,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private static IQueryable<InvoiceLineDto> ProjectInvoiceLines(IQueryable<InvoiceLine> query)
    {
        return query.Select(x => new InvoiceLineDto
        {
            Id = x.Id,
            InvoiceId = x.InvoiceId,
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