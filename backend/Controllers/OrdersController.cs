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
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private const string OrderStatusCategoryCode = "ORDER_STATUS";
    private const string OrderApprovalStatusCategoryCode = "ORDER_APPROVAL_STATUS";
    private const string OrderDeliveryStatusCategoryCode = "ORDER_DELIVERY_STATUS";
    private const string OrderBillingStatusCategoryCode = "ORDER_BILLING_STATUS";
    private const string CurrencyCategoryCode = "CURRENCY";

    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly ICurrentUserContext _currentUserContext;

    public OrdersController(
        AppDbContext dbContext,
        INumberSequenceService numberSequenceService,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [HasPermission("Orders.View")]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders([FromQuery] OrderFilterDto query)
    {
        var orders = _dbContext.Orders.AsQueryable();

        if (query.AccountId.HasValue)
        {
            orders = orders.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (query.OpportunityId.HasValue)
        {
            orders = orders.Where(x => x.OpportunityId == query.OpportunityId.Value);
        }

        if (query.OrderStatusId.HasValue)
        {
            orders = orders.Where(x => x.OrderStatusId == query.OrderStatusId.Value);
        }

        if (query.ApprovalStatusId.HasValue)
        {
            orders = orders.Where(x => x.ApprovalStatusId == query.ApprovalStatusId.Value);
        }

        if (query.DeliveryStatusId.HasValue)
        {
            orders = orders.Where(x => x.DeliveryStatusId == query.DeliveryStatusId.Value);
        }

        if (query.BillingStatusId.HasValue)
        {
            orders = orders.Where(x => x.BillingStatusId == query.BillingStatusId.Value);
        }

        if (query.IsActive.HasValue)
        {
            orders = orders.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            orders = orders.Where(x =>
                x.OrderNumber.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search) ||
                (x.Contact != null && x.Contact.FullName.ToLower().Contains(search)) ||
                (x.Opportunity != null && x.Opportunity.Topic.ToLower().Contains(search)) ||
                (x.Quote != null && x.Quote.QuoteNumber.ToLower().Contains(search)));
        }

        orders = orders.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            orders = orders.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectOrders(orders).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Orders.View")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await ProjectOrders(_dbContext.Orders.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [HasPermission("Orders.Create")]
    public async Task<ActionResult<OrderDto>> CreateOrder(UpsertOrderRequestDto dto)
    {
        var validationError = await ValidateOrderAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var order = new Order();
        ApplyOrderValues(order, dto);
        if (string.IsNullOrWhiteSpace(order.OrderNumber))
        {
            order.OrderNumber = await _numberSequenceService.GenerateNextAsync("ORDER");
        }

        if (await _dbContext.Orders.AnyAsync(x => x.OrderNumber == order.OrderNumber))
        {
            return BadRequest("Order number already exists.");
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectOrders(_dbContext.Orders.Where(x => x.Id == order.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Order was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Orders.Update")]
    public async Task<IActionResult> UpdateOrder(Guid id, UpsertOrderRequestDto dto)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        var validationError = await ValidateOrderAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyOrderValues(order, dto);
        if (string.IsNullOrWhiteSpace(order.OrderNumber))
        {
            order.OrderNumber = await _numberSequenceService.GenerateNextAsync("ORDER");
        }

        if (await _dbContext.Orders.AnyAsync(x => x.Id != id && x.OrderNumber == order.OrderNumber))
        {
            return BadRequest("Order number already exists.");
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Orders.Delete")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        order.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("lookup")]
    [HasPermission("Orders.View")]
    public async Task<ActionResult<object>> GetLookup()
    {
        return Ok(new
        {
            OrderStatuses = await GetLookupOptionsAsync(OrderStatusCategoryCode),
            ApprovalStatuses = await GetLookupOptionsAsync(OrderApprovalStatusCategoryCode),
            DeliveryStatuses = await GetLookupOptionsAsync(OrderDeliveryStatusCategoryCode),
            BillingStatuses = await GetLookupOptionsAsync(OrderBillingStatusCategoryCode),
            Currencies = await GetLookupOptionsAsync(CurrencyCategoryCode)
        });
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission("Orders.Approve")]
    public async Task<IActionResult> ApproveOrder(Guid id)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        var approvedStatusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == OrderApprovalStatusCategoryCode && x.Code == "APPROVED")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!approvedStatusId.HasValue)
        {
            return BadRequest("Approved status lookup is missing.");
        }

        order.ApprovalStatusId = approvedStatusId.Value;
        order.ApprovedAt = DateTime.UtcNow;
        order.ApprovedById = _currentUserContext.UserId;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{orderId:guid}/lines")]
    [HasPermission("OrderLines.View")]
    public async Task<ActionResult<PagedResult<OrderLineDto>>> GetOrderLines(Guid orderId, [FromQuery] OrderLineFilterDto query)
    {
        if (!await _dbContext.Orders.AnyAsync(x => x.Id == orderId))
        {
            return NotFound();
        }

        query.OrderId = orderId;
        var lines = _dbContext.OrderLines.AsQueryable();

        if (query.OrderId.HasValue)
        {
            lines = lines.Where(x => x.OrderId == query.OrderId.Value);
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

        return Ok(await ProjectOrderLines(lines).ToPagedAsync(query));
    }

    [HttpPost("{orderId:guid}/lines")]
    [HasPermission("OrderLines.Create")]
    public async Task<ActionResult<OrderLineDto>> CreateOrderLine(Guid orderId, UpsertOrderLineRequestDto dto)
    {
        if (!await _dbContext.Orders.AnyAsync(x => x.Id == orderId))
        {
            return NotFound();
        }

        var validationError = await ValidateOrderLineAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var line = new OrderLine { OrderId = orderId };
        ApplyOrderLineValues(line, dto);

        _dbContext.OrderLines.Add(line);
        await _dbContext.SaveChangesAsync();
        await RecalculateOrderTotalsAsync(orderId);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectOrderLines(_dbContext.OrderLines.Where(x => x.Id == line.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Order line was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("lines/{id:guid}")]
    [HasPermission("OrderLines.Update")]
    public async Task<IActionResult> UpdateOrderLine(Guid id, UpsertOrderLineRequestDto dto)
    {
        var line = await _dbContext.OrderLines.FirstOrDefaultAsync(x => x.Id == id);
        if (line is null)
        {
            return NotFound();
        }

        var validationError = await ValidateOrderLineAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyOrderLineValues(line, dto);
        await _dbContext.SaveChangesAsync();
        await RecalculateOrderTotalsAsync(line.OrderId);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("lines/{id:guid}")]
    [HasPermission("OrderLines.Delete")]
    public async Task<IActionResult> DeleteOrderLine(Guid id)
    {
        var line = await _dbContext.OrderLines.FirstOrDefaultAsync(x => x.Id == id);
        if (line is null)
        {
            return NotFound();
        }

        var orderId = line.OrderId;
        line.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        await RecalculateOrderTotalsAsync(orderId);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/generate-invoice")]
    [HasPermission("Orders.GenerateInvoice")]
    public async Task<ActionResult<object>> GenerateInvoice(Guid id)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.ConvertedInvoiceId.HasValue)
        {
            return BadRequest("Order has already been marked as converted to invoice.");
        }

        order.ConvertedInvoiceAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            OrderId = order.Id,
            order.OrderNumber,
            order.ConvertedInvoiceAt,
            Message = "Order conversion marker recorded. Invoice creation will be finalized in Module 10."
        });
    }

    private async Task<string?> ValidateOrderAsync(Guid? orderId, UpsertOrderRequestDto dto)
    {
        if (!await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId))
        {
            return "Account is invalid.";
        }

        if (dto.QuoteId.HasValue && !await _dbContext.Quotes.AnyAsync(x => x.Id == dto.QuoteId.Value))
        {
            return "Quote is invalid.";
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

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.OrderStatusId && x.LookupCategory.Code == OrderStatusCategoryCode))
        {
            return "Order status is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ApprovalStatusId && x.LookupCategory.Code == OrderApprovalStatusCategoryCode))
        {
            return "Approval status is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.DeliveryStatusId && x.LookupCategory.Code == OrderDeliveryStatusCategoryCode))
        {
            return "Delivery status is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.BillingStatusId && x.LookupCategory.Code == OrderBillingStatusCategoryCode))
        {
            return "Billing status is invalid.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value))
        {
            return "Owner user is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        if (dto.ExpectedDeliveryDate.HasValue && dto.OrderDate.HasValue && dto.ExpectedDeliveryDate.Value < dto.OrderDate.Value)
        {
            return "Expected delivery date must be greater than or equal to order date.";
        }

        if (dto.DeliveryDate.HasValue && dto.OrderDate.HasValue && dto.DeliveryDate.Value < dto.OrderDate.Value)
        {
            return "Delivery date must be greater than or equal to order date.";
        }

        if (dto.BillingDate.HasValue && dto.OrderDate.HasValue && dto.BillingDate.Value < dto.OrderDate.Value)
        {
            return "Billing date must be greater than or equal to order date.";
        }

        if (!string.IsNullOrWhiteSpace(dto.OrderNumber))
        {
            var number = dto.OrderNumber.Trim();
            var exists = await _dbContext.Orders.AnyAsync(x => x.Id != orderId && x.OrderNumber == number);
            if (exists)
            {
                return "Order number already exists.";
            }
        }

        return null;
    }

    private async Task<string?> ValidateOrderLineAsync(UpsertOrderLineRequestDto dto)
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

    private static void ApplyOrderValues(Order order, UpsertOrderRequestDto dto)
    {
        order.OrderNumber = TrimToNull(dto.OrderNumber) ?? string.Empty;
        order.QuoteId = dto.QuoteId;
        order.AccountId = dto.AccountId;
        order.ContactId = dto.ContactId;
        order.OpportunityId = dto.OpportunityId;
        order.CurrencyId = dto.CurrencyId;
        order.OrderStatusId = dto.OrderStatusId;
        order.ApprovalStatusId = dto.ApprovalStatusId;
        order.DeliveryStatusId = dto.DeliveryStatusId;
        order.BillingStatusId = dto.BillingStatusId;
        order.OrderDate = dto.OrderDate;
        order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        order.DeliveryDate = dto.DeliveryDate;
        order.BillingDate = dto.BillingDate;
        order.Notes = TrimToNull(dto.Notes);
        order.IsActive = dto.IsActive;
        order.OwnerUserId = dto.OwnerUserId;
        order.OwnerTeamId = dto.OwnerTeamId;
    }

    private static void ApplyOrderLineValues(OrderLine line, UpsertOrderLineRequestDto dto)
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

    private async Task RecalculateOrderTotalsAsync(Guid orderId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
        if (order is null)
        {
            return;
        }

        var lines = await _dbContext.OrderLines.Where(x => x.OrderId == orderId).ToListAsync();
        var subtotal = lines.Sum(x => x.Quantity * x.UnitPrice);
        var discount = lines.Sum(x => x.DiscountAmount);
        var tax = lines.Sum(x => x.TaxAmount);

        order.SubtotalAmount = Math.Round(subtotal, 2);
        order.DiscountAmount = Math.Round(discount, 2);
        order.TaxAmount = Math.Round(tax, 2);
        order.TotalAmount = Math.Round(order.SubtotalAmount - order.DiscountAmount + order.TaxAmount, 2);
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

    private static IQueryable<OrderDto> ProjectOrders(IQueryable<Order> query)
    {
        return query.Select(x => new OrderDto
        {
            Id = x.Id,
            OrderNumber = x.OrderNumber,
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
            OrderStatusId = x.OrderStatusId,
            OrderStatusName = x.OrderStatus.Name,
            ApprovalStatusId = x.ApprovalStatusId,
            ApprovalStatusName = x.ApprovalStatus.Name,
            DeliveryStatusId = x.DeliveryStatusId,
            DeliveryStatusName = x.DeliveryStatus.Name,
            BillingStatusId = x.BillingStatusId,
            BillingStatusName = x.BillingStatus.Name,
            OrderDate = x.OrderDate,
            ExpectedDeliveryDate = x.ExpectedDeliveryDate,
            DeliveryDate = x.DeliveryDate,
            BillingDate = x.BillingDate,
            SubtotalAmount = x.SubtotalAmount,
            DiscountAmount = x.DiscountAmount,
            TaxAmount = x.TaxAmount,
            TotalAmount = x.TotalAmount,
            Notes = x.Notes,
            ApprovedById = x.ApprovedById,
            ApprovedAt = x.ApprovedAt,
            ConvertedInvoiceId = x.ConvertedInvoiceId,
            ConvertedInvoiceAt = x.ConvertedInvoiceAt,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private static IQueryable<OrderLineDto> ProjectOrderLines(IQueryable<OrderLine> query)
    {
        return query.Select(x => new OrderLineDto
        {
            Id = x.Id,
            OrderId = x.OrderId,
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
