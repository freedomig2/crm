using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/contact-communications")]
public class ContactCommunicationsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ContactCommunicationsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("ContactCommunications.View")]
    public async Task<ActionResult<PagedResult<ContactCommunicationDto>>> GetCommunications([FromQuery] ContactCommunicationFilterDto query)
    {
        var communicationsQuery = _dbContext.ContactCommunications.AsQueryable();

        if (query.ContactId.HasValue)
        {
            communicationsQuery = communicationsQuery.Where(x => x.ContactId == query.ContactId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            communicationsQuery = communicationsQuery.Where(x =>
                x.Value.ToLower().Contains(search) ||
                (x.Notes ?? string.Empty).ToLower().Contains(search) ||
                x.Contact.FullName.ToLower().Contains(search) ||
                ((x.CommunicationType != null ? x.CommunicationType.Name : string.Empty).ToLower().Contains(search)));
        }

        communicationsQuery = communicationsQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            communicationsQuery = communicationsQuery
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Value);
        }

        return Ok(await ProjectCommunications(communicationsQuery).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("ContactCommunications.View")]
    public async Task<ActionResult<ContactCommunicationDto>> GetCommunication(Guid id)
    {
        var item = await ProjectCommunications(_dbContext.ContactCommunications.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("ContactCommunications.Create")]
    public async Task<ActionResult<ContactCommunicationDto>> CreateCommunication(UpsertContactCommunicationRequestDto dto)
    {
        if (!await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId))
        {
            return BadRequest("Contact is required.");
        }

        var item = new ContactCommunication
        {
            ContactId = dto.ContactId,
            CommunicationTypeId = dto.CommunicationTypeId,
            Value = dto.Value.Trim(),
            IsPrimary = dto.IsPrimary,
            IsVerified = dto.IsVerified,
            VerificationDate = dto.VerificationDate,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
        };

        _dbContext.ContactCommunications.Add(item);

        if (dto.IsPrimary)
        {
            await ClearOtherPrimariesAsync(item.ContactId, item.Id);
        }

        await _dbContext.SaveChangesAsync();

        var created = await ProjectCommunications(_dbContext.ContactCommunications.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Communication was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("ContactCommunications.Update")]
    public async Task<IActionResult> UpdateCommunication(Guid id, UpsertContactCommunicationRequestDto dto)
    {
        var item = await _dbContext.ContactCommunications.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (!await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId))
        {
            return BadRequest("Contact is required.");
        }

        item.ContactId = dto.ContactId;
        item.CommunicationTypeId = dto.CommunicationTypeId;
        item.Value = dto.Value.Trim();
        item.IsPrimary = dto.IsPrimary;
        item.IsVerified = dto.IsVerified;
        item.VerificationDate = dto.VerificationDate;
        item.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

        if (dto.IsPrimary)
        {
            await ClearOtherPrimariesAsync(item.ContactId, item.Id);
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("ContactCommunications.Delete")]
    public async Task<IActionResult> DeleteCommunication(Guid id)
    {
        var item = await _dbContext.ContactCommunications.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task ClearOtherPrimariesAsync(Guid contactId, Guid currentId)
    {
        var existing = await _dbContext.ContactCommunications
            .Where(x => x.ContactId == contactId && x.Id != currentId && x.IsPrimary)
            .ToListAsync();

        foreach (var communication in existing)
        {
            communication.IsPrimary = false;
        }
    }

    private static IQueryable<ContactCommunicationDto> ProjectCommunications(IQueryable<ContactCommunication> query)
    {
        return query.Select(x => new ContactCommunicationDto
        {
            Id = x.Id,
            ContactId = x.ContactId,
            ContactName = x.Contact.FullName,
            CommunicationTypeId = x.CommunicationTypeId,
            CommunicationTypeName = x.CommunicationType != null ? x.CommunicationType.Name : null,
            Value = x.Value,
            IsPrimary = x.IsPrimary,
            IsVerified = x.IsVerified,
            VerificationDate = x.VerificationDate,
            Notes = x.Notes
        });
    }
}
