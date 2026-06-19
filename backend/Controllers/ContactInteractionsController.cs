using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/contact-interactions")]
public class ContactInteractionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ContactInteractionsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("ContactInteractions.View")]
    public async Task<ActionResult<PagedResult<ContactInteractionDto>>> GetInteractions([FromQuery] ContactInteractionFilterDto query)
    {
        var interactionsQuery = _dbContext.ContactInteractions.AsQueryable();

        if (query.ContactId.HasValue)
        {
            interactionsQuery = interactionsQuery.Where(x => x.ContactId == query.ContactId.Value);
        }

        if (query.AccountId.HasValue)
        {
            interactionsQuery = interactionsQuery.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            interactionsQuery = interactionsQuery.Where(x =>
                x.Subject.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                (x.Outcome ?? string.Empty).ToLower().Contains(search) ||
                x.Contact.FullName.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search) ||
                ((x.InteractionType != null ? x.InteractionType.Name : string.Empty).ToLower().Contains(search)));
        }

        interactionsQuery = interactionsQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            interactionsQuery = interactionsQuery.OrderByDescending(x => x.InteractionDate);
        }

        return Ok(await ProjectInteractions(interactionsQuery).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("ContactInteractions.View")]
    public async Task<ActionResult<ContactInteractionDto>> GetInteraction(Guid id)
    {
        var item = await ProjectInteractions(_dbContext.ContactInteractions.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("ContactInteractions.Create")]
    public async Task<ActionResult<ContactInteractionDto>> CreateInteraction(UpsertContactInteractionRequestDto dto)
    {
        var contact = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == dto.ContactId);
        if (contact is null)
        {
            return BadRequest("Contact is required.");
        }

        if (contact.AccountId != dto.AccountId)
        {
            return BadRequest("Interaction account must match the contact account.");
        }

        var item = new ContactInteraction
        {
            ContactId = dto.ContactId,
            AccountId = dto.AccountId,
            InteractionTypeId = dto.InteractionTypeId,
            Subject = dto.Subject.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            InteractionDate = dto.InteractionDate == default ? DateTime.UtcNow : dto.InteractionDate,
            Outcome = string.IsNullOrWhiteSpace(dto.Outcome) ? null : dto.Outcome.Trim(),
            FollowUpDate = dto.FollowUpDate,
            AssignedToUserId = dto.AssignedToUserId
        };

        _dbContext.ContactInteractions.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectInteractions(_dbContext.ContactInteractions.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Interaction was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("ContactInteractions.Update")]
    public async Task<IActionResult> UpdateInteraction(Guid id, UpsertContactInteractionRequestDto dto)
    {
        var item = await _dbContext.ContactInteractions.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var contact = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == dto.ContactId);
        if (contact is null)
        {
            return BadRequest("Contact is required.");
        }

        if (contact.AccountId != dto.AccountId)
        {
            return BadRequest("Interaction account must match the contact account.");
        }

        item.ContactId = dto.ContactId;
        item.AccountId = dto.AccountId;
        item.InteractionTypeId = dto.InteractionTypeId;
        item.Subject = dto.Subject.Trim();
        item.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        item.InteractionDate = dto.InteractionDate == default ? DateTime.UtcNow : dto.InteractionDate;
        item.Outcome = string.IsNullOrWhiteSpace(dto.Outcome) ? null : dto.Outcome.Trim();
        item.FollowUpDate = dto.FollowUpDate;
        item.AssignedToUserId = dto.AssignedToUserId;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("ContactInteractions.Delete")]
    public async Task<IActionResult> DeleteInteraction(Guid id)
    {
        var item = await _dbContext.ContactInteractions.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static IQueryable<ContactInteractionDto> ProjectInteractions(IQueryable<ContactInteraction> query)
    {
        return query.Select(x => new ContactInteractionDto
        {
            Id = x.Id,
            ContactId = x.ContactId,
            ContactName = x.Contact.FullName,
            AccountId = x.AccountId,
            AccountName = x.Account.Name,
            InteractionTypeId = x.InteractionTypeId,
            InteractionTypeName = x.InteractionType != null ? x.InteractionType.Name : null,
            Subject = x.Subject,
            Description = x.Description,
            InteractionDate = x.InteractionDate,
            Outcome = x.Outcome,
            FollowUpDate = x.FollowUpDate,
            AssignedToUserId = x.AssignedToUserId,
            AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.Email : null
        });
    }
}
