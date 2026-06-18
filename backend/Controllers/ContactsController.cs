using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/contacts")]
public class ContactsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ContactsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("Contacts.View")]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetContacts([FromQuery] ListQueryDto query)
    {
        var contactsQuery = _dbContext.Contacts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            contactsQuery = contactsQuery.Where(x =>
                x.FirstName.ToLower().Contains(search) ||
                x.LastName.ToLower().Contains(search) ||
                (x.Email ?? string.Empty).ToLower().Contains(search) ||
                (x.JobTitle ?? string.Empty).ToLower().Contains(search));
        }

        contactsQuery = contactsQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = contactsQuery.Select(x => new ContactDto
        {
            Id = x.Id,
            AccountId = x.AccountId,
            ContactRoleId = x.ContactRoleId,
            SalutationId = x.SalutationId,
            FirstName = x.FirstName,
            MiddleName = x.MiddleName,
            LastName = x.LastName,
            JobTitle = x.JobTitle,
            DepartmentName = x.DepartmentName,
            Email = x.Email,
            MobilePhone = x.MobilePhone,
            WorkPhone = x.WorkPhone,
            Extension = x.Extension,
            PreferredCommunicationId = x.PreferredCommunicationId,
            IsPrimaryContact = x.IsPrimaryContact,
            DateOfBirth = x.DateOfBirth,
            Notes = x.Notes,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Contacts.View")]
    public async Task<ActionResult<ContactDto>> GetContact(Guid id)
    {
        var contact = await _dbContext.Contacts
            .Where(x => x.Id == id)
            .Select(x => new ContactDto
            {
                Id = x.Id,
                AccountId = x.AccountId,
                ContactRoleId = x.ContactRoleId,
                SalutationId = x.SalutationId,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,
                JobTitle = x.JobTitle,
                DepartmentName = x.DepartmentName,
                Email = x.Email,
                MobilePhone = x.MobilePhone,
                WorkPhone = x.WorkPhone,
                Extension = x.Extension,
                PreferredCommunicationId = x.PreferredCommunicationId,
                IsPrimaryContact = x.IsPrimaryContact,
                DateOfBirth = x.DateOfBirth,
                Notes = x.Notes,
                IsActive = x.IsActive,
                OwnerUserId = x.OwnerUserId,
                OwnerTeamId = x.OwnerTeamId
            })
            .FirstOrDefaultAsync();

        return contact is null ? NotFound() : Ok(contact);
    }

    [HttpPost]
    [HasPermission("Contacts.Create")]
    public async Task<ActionResult<ContactDto>> CreateContact(UpsertContactRequestDto dto)
    {
        var item = new Contact
        {
            AccountId = dto.AccountId,
            ContactRoleId = dto.ContactRoleId,
            SalutationId = dto.SalutationId,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            JobTitle = dto.JobTitle,
            DepartmentName = dto.DepartmentName,
            Email = dto.Email,
            MobilePhone = dto.MobilePhone,
            WorkPhone = dto.WorkPhone,
            Extension = dto.Extension,
            PreferredCommunicationId = dto.PreferredCommunicationId,
            IsPrimaryContact = dto.IsPrimaryContact,
            DateOfBirth = dto.DateOfBirth,
            Notes = dto.Notes,
            IsActive = dto.IsActive,
            OwnerUserId = dto.OwnerUserId,
            OwnerTeamId = dto.OwnerTeamId
        };

        _dbContext.Contacts.Add(item);
        await _dbContext.SaveChangesAsync();

        return Ok(new ContactDto
        {
            Id = item.Id,
            AccountId = item.AccountId,
            ContactRoleId = item.ContactRoleId,
            SalutationId = item.SalutationId,
            FirstName = item.FirstName,
            MiddleName = item.MiddleName,
            LastName = item.LastName,
            JobTitle = item.JobTitle,
            DepartmentName = item.DepartmentName,
            Email = item.Email,
            MobilePhone = item.MobilePhone,
            WorkPhone = item.WorkPhone,
            Extension = item.Extension,
            PreferredCommunicationId = item.PreferredCommunicationId,
            IsPrimaryContact = item.IsPrimaryContact,
            DateOfBirth = item.DateOfBirth,
            Notes = item.Notes,
            IsActive = item.IsActive,
            OwnerUserId = item.OwnerUserId,
            OwnerTeamId = item.OwnerTeamId
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Contacts.Update")]
    public async Task<IActionResult> UpdateContact(Guid id, UpsertContactRequestDto dto)
    {
        var item = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.AccountId = dto.AccountId;
        item.ContactRoleId = dto.ContactRoleId;
        item.SalutationId = dto.SalutationId;
        item.FirstName = dto.FirstName;
        item.MiddleName = dto.MiddleName;
        item.LastName = dto.LastName;
        item.JobTitle = dto.JobTitle;
        item.DepartmentName = dto.DepartmentName;
        item.Email = dto.Email;
        item.MobilePhone = dto.MobilePhone;
        item.WorkPhone = dto.WorkPhone;
        item.Extension = dto.Extension;
        item.PreferredCommunicationId = dto.PreferredCommunicationId;
        item.IsPrimaryContact = dto.IsPrimaryContact;
        item.DateOfBirth = dto.DateOfBirth;
        item.Notes = dto.Notes;
        item.IsActive = dto.IsActive;
        item.OwnerUserId = dto.OwnerUserId;
        item.OwnerTeamId = dto.OwnerTeamId;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Contacts.Delete")]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
        var item = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}