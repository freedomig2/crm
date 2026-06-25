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
    private const string SetPrimaryPermission = "Contacts.SetPrimary";
    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;

    public ContactsController(AppDbContext dbContext, INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
    }

    [HttpGet]
    [HasPermission("Contacts.View")]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetContacts([FromQuery] ContactFilterDto query)
    {
        var contactsQuery = _dbContext.Contacts.AsQueryable();

        if (query.AccountId.HasValue)
        {
            contactsQuery = contactsQuery.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (query.ContactRoleId.HasValue)
        {
            contactsQuery = contactsQuery.Where(x => x.ContactRoleId == query.ContactRoleId.Value);
        }

        if (query.PreferredContactMethodId.HasValue)
        {
            contactsQuery = contactsQuery.Where(x => x.PreferredContactMethodId == query.PreferredContactMethodId.Value);
        }

        if (query.IsActive.HasValue)
        {
            contactsQuery = contactsQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            contactsQuery = contactsQuery.Where(x =>
                x.ContactNumber.ToLower().Contains(search) ||
                x.FullName.ToLower().Contains(search) ||
                x.FirstName.ToLower().Contains(search) ||
                x.LastName.ToLower().Contains(search) ||
                (x.Email ?? string.Empty).ToLower().Contains(search) ||
                (x.MobilePhone ?? string.Empty).ToLower().Contains(search) ||
                (x.JobTitle ?? string.Empty).ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search));
        }

        contactsQuery = contactsQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            contactsQuery = contactsQuery.OrderBy(x => x.FullName);
        }

        return Ok(await ProjectContacts(contactsQuery).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Contacts.View")]
    public async Task<ActionResult<ContactDto>> GetContact(Guid id)
    {
        var contact = await GetContactDtoAsync(id);
        return contact is null ? NotFound() : Ok(contact);
    }

    [HttpPost]
    [HasPermission("Contacts.Create")]
    public async Task<ActionResult<ContactDto>> CreateContact(UpsertContactRequestDto dto)
    {
        if (dto.IsPrimaryContact && !CanSetPrimary())
        {
            return Forbid();
        }

        if (!await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId))
        {
            return BadRequest("Account is required.");
        }

        var lookupValidation = await ValidateLookupAssignmentsAsync(dto);
        if (lookupValidation is not null)
        {
            return BadRequest(lookupValidation);
        }

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            ContactNumber = await _numberSequenceService.GenerateNextAsync("CONTACT")
        };
        await ApplyContactValuesAsync(contact, dto);
        contact.IsPrimaryContact = false;

        _dbContext.Contacts.Add(contact);

        if (dto.IsPrimaryContact)
        {
            await MarkPrimaryAsync(contact);
        }

        await _dbContext.SaveChangesAsync();

        var created = await GetContactDtoAsync(contact.Id);
        return created is null ? Problem("Contact was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Contacts.Update")]
    public async Task<IActionResult> UpdateContact(Guid id, UpsertContactRequestDto dto)
    {
        var contact = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id);
        if (contact is null)
        {
            return NotFound();
        }

        if (dto.IsPrimaryContact && !contact.IsPrimaryContact && !CanSetPrimary())
        {
            return Forbid();
        }

        if (!await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId))
        {
            return BadRequest("Account is required.");
        }

        var lookupValidation = await ValidateLookupAssignmentsAsync(dto);
        if (lookupValidation is not null)
        {
            return BadRequest(lookupValidation);
        }

        await ApplyContactValuesAsync(contact, dto);
        if (string.IsNullOrWhiteSpace(contact.ContactNumber))
        {
            contact.ContactNumber = await _numberSequenceService.GenerateNextAsync("CONTACT");
        }

        if (dto.IsPrimaryContact)
        {
            await MarkPrimaryAsync(contact);
        }
        else if (contact.IsPrimaryContact)
        {
            await ClearPrimaryAsync(contact);
        }

        contact.IsPrimaryContact = dto.IsPrimaryContact;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/set-primary")]
    [HasPermission(SetPrimaryPermission)]
    public async Task<IActionResult> SetPrimary(Guid id)
    {
        var contact = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id);
        if (contact is null)
        {
            return NotFound();
        }

        await MarkPrimaryAsync(contact);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Contacts.Delete")]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
        var contact = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id);
        if (contact is null)
        {
            return NotFound();
        }

        await ClearPrimaryAsync(contact);
        contact.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private bool CanSetPrimary()
    {
        return User.HasClaim("permission", SetPrimaryPermission);
    }

    private async Task<string?> ValidateLookupAssignmentsAsync(UpsertContactRequestDto dto)
    {
        var checks = new (Guid? LookupId, string CategoryCode, string Label)[]
        {
            (dto.ContactRoleId, "CONTACT_ROLE", "Contact role"),
            (dto.SalutationLookupId, "SALUTATION", "Contact title"),
            (dto.GenderLookupId, "GENDER", "Gender"),
            (dto.PreferredContactMethodId, "CONTACT_METHOD", "Preferred communication method"),
            (dto.PreferredLanguageId, "LANGUAGE", "Preferred language"),
            (dto.PreferredTimeZoneId, "TIME_ZONE", "Preferred time zone")
        };

        foreach (var (lookupId, categoryCode, label) in checks)
        {
            if (!lookupId.HasValue)
            {
                continue;
            }

            var exists = await _dbContext.LookupValues
                .AnyAsync(x => x.Id == lookupId.Value && x.LookupCategory.Code == categoryCode && x.IsActive);
            if (!exists)
            {
                return $"{label} is invalid.";
            }
        }

        return null;
    }

    private async Task ApplyContactValuesAsync(Contact contact, UpsertContactRequestDto dto)
    {
        contact.AccountId = dto.AccountId;
        contact.ContactRoleId = dto.ContactRoleId;
        contact.SalutationLookupId = dto.SalutationLookupId;
        contact.GenderLookupId = dto.GenderLookupId;
        contact.FirstName = dto.FirstName.Trim();
        contact.MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
        contact.LastName = dto.LastName.Trim();
        contact.PreferredName = string.IsNullOrWhiteSpace(dto.PreferredName) ? null : dto.PreferredName.Trim();
        contact.JobTitle = string.IsNullOrWhiteSpace(dto.JobTitle) ? null : dto.JobTitle.Trim();
        contact.Department = string.IsNullOrWhiteSpace(dto.Department) ? null : dto.Department.Trim();
        contact.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        contact.AlternateEmail = string.IsNullOrWhiteSpace(dto.AlternateEmail) ? null : dto.AlternateEmail.Trim();
        contact.WorkPhone = string.IsNullOrWhiteSpace(dto.WorkPhone) ? null : dto.WorkPhone.Trim();
        contact.MobilePhone = string.IsNullOrWhiteSpace(dto.MobilePhone) ? null : dto.MobilePhone.Trim();
        contact.HomePhone = string.IsNullOrWhiteSpace(dto.HomePhone) ? null : dto.HomePhone.Trim();
        contact.Fax = string.IsNullOrWhiteSpace(dto.Fax) ? null : dto.Fax.Trim();
        contact.PreferredContactMethodId = dto.PreferredContactMethodId;
        contact.PreferredLanguageId = dto.PreferredLanguageId;
        contact.PreferredTimeZoneId = dto.PreferredTimeZoneId;
        contact.MarketingConsent = dto.MarketingConsent;
        contact.EmailOptIn = dto.EmailOptIn;
        contact.SMSOptIn = dto.SMSOptIn;
        contact.PhoneOptIn = dto.PhoneOptIn;
        contact.DateOfBirth = dto.DateOfBirth;
        contact.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        contact.IsActive = dto.IsActive;
        contact.OwnerUserId = dto.OwnerUserId;
        contact.OwnerTeamId = dto.OwnerTeamId;

        var salutationName = dto.SalutationLookupId.HasValue
            ? await _dbContext.LookupValues
                .Where(x => x.Id == dto.SalutationLookupId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync()
            : null;

        contact.FullName = BuildFullName(salutationName, contact.PreferredName, contact.FirstName, contact.MiddleName, contact.LastName);
    }

    private async Task MarkPrimaryAsync(Contact contact)
    {
        var siblingPrimaries = await _dbContext.Contacts
            .Where(x => x.AccountId == contact.AccountId && x.Id != contact.Id && x.IsPrimaryContact)
            .ToListAsync();

        foreach (var sibling in siblingPrimaries)
        {
            sibling.IsPrimaryContact = false;
        }

        contact.IsPrimaryContact = true;

        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == contact.AccountId);
        if (account is not null)
        {
            account.PrimaryContactId = contact.Id;
        }
    }

    private async Task ClearPrimaryAsync(Contact contact)
    {
        if (!contact.IsPrimaryContact)
        {
            return;
        }

        contact.IsPrimaryContact = false;
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == contact.AccountId);
        if (account?.PrimaryContactId == contact.Id)
        {
            account.PrimaryContactId = null;
        }
    }

    private async Task<ContactDto?> GetContactDtoAsync(Guid id)
    {
        return await ProjectContacts(_dbContext.Contacts.Where(x => x.Id == id)).FirstOrDefaultAsync();
    }

    private static string BuildFullName(string? salutation, string? preferredName, string firstName, string? middleName, string lastName)
    {
        var displayFirstName = string.IsNullOrWhiteSpace(preferredName) ? firstName : preferredName;
        return string.Join(' ', new[] { salutation, displayFirstName, middleName, lastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim()));
    }

    private static IQueryable<ContactDto> ProjectContacts(IQueryable<Contact> query)
    {
        return query.Select(x => new ContactDto
        {
            Id = x.Id,
            ContactNumber = x.ContactNumber,
            AccountId = x.AccountId,
            AccountName = x.Account.Name,
            ContactRoleId = x.ContactRoleId,
            ContactRoleName = x.ContactRole != null ? x.ContactRole.Name : null,
            SalutationLookupId = x.SalutationLookupId,
            SalutationName = x.Salutation != null ? x.Salutation.Name : null,
            GenderLookupId = x.GenderLookupId,
            GenderName = x.Gender != null ? x.Gender.Name : null,
            FirstName = x.FirstName,
            MiddleName = x.MiddleName,
            LastName = x.LastName,
            PreferredName = x.PreferredName,
            FullName = x.FullName,
            JobTitle = x.JobTitle,
            Department = x.Department,
            Email = x.Email,
            AlternateEmail = x.AlternateEmail,
            WorkPhone = x.WorkPhone,
            MobilePhone = x.MobilePhone,
            HomePhone = x.HomePhone,
            Fax = x.Fax,
            PreferredContactMethodId = x.PreferredContactMethodId,
            PreferredContactMethodName = x.PreferredContactMethod != null ? x.PreferredContactMethod.Name : null,
            PreferredLanguageId = x.PreferredLanguageId,
            PreferredLanguageName = x.PreferredLanguage != null ? x.PreferredLanguage.Name : null,
            PreferredTimeZoneId = x.PreferredTimeZoneId,
            PreferredTimeZoneName = x.PreferredTimeZone != null ? x.PreferredTimeZone.Name : null,
            MarketingConsent = x.MarketingConsent,
            EmailOptIn = x.EmailOptIn,
            SMSOptIn = x.SMSOptIn,
            PhoneOptIn = x.PhoneOptIn,
            IsPrimaryContact = x.IsPrimaryContact,
            DateOfBirth = x.DateOfBirth,
            Notes = x.Notes,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId
        });
    }
}
