using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AccountsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("Accounts.View")]
    public async Task<ActionResult<PagedResult<AccountDto>>> GetAccounts([FromQuery] ListQueryDto query)
    {
        var accountsQuery = _dbContext.Accounts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            accountsQuery = accountsQuery.Where(x =>
                x.AccountNumber.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.LegalName ?? string.Empty).ToLower().Contains(search) ||
                (x.Email ?? string.Empty).ToLower().Contains(search));
        }

        accountsQuery = accountsQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = accountsQuery.Select(x => new AccountDto
        {
            Id = x.Id,
            AccountNumber = x.AccountNumber,
            Name = x.Name,
            LegalName = x.LegalName,
            TradingName = x.TradingName,
            AccountTypeId = x.AccountTypeId,
            IndustryId = x.IndustryId,
            OwnershipTypeId = x.OwnershipTypeId,
            CustomerStatusId = x.CustomerStatusId,
            CustomerSegmentId = x.CustomerSegmentId,
            Website = x.Website,
            MainPhone = x.MainPhone,
            AlternatePhone = x.AlternatePhone,
            Email = x.Email,
            Fax = x.Fax,
            TaxNumber = x.TaxNumber,
            RegistrationNumber = x.RegistrationNumber,
            AnnualRevenue = x.AnnualRevenue,
            NumberOfEmployees = x.NumberOfEmployees,
            Description = x.Description,
            ParentAccountId = x.ParentAccountId,
            PrimaryContactId = x.PrimaryContactId,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Accounts.View")]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid id)
    {
        var account = await _dbContext.Accounts
            .Where(x => x.Id == id)
            .Select(x => new AccountDto
            {
                Id = x.Id,
                AccountNumber = x.AccountNumber,
                Name = x.Name,
                LegalName = x.LegalName,
                TradingName = x.TradingName,
                AccountTypeId = x.AccountTypeId,
                IndustryId = x.IndustryId,
                OwnershipTypeId = x.OwnershipTypeId,
                CustomerStatusId = x.CustomerStatusId,
                CustomerSegmentId = x.CustomerSegmentId,
                Website = x.Website,
                MainPhone = x.MainPhone,
                AlternatePhone = x.AlternatePhone,
                Email = x.Email,
                Fax = x.Fax,
                TaxNumber = x.TaxNumber,
                RegistrationNumber = x.RegistrationNumber,
                AnnualRevenue = x.AnnualRevenue,
                NumberOfEmployees = x.NumberOfEmployees,
                Description = x.Description,
                ParentAccountId = x.ParentAccountId,
                PrimaryContactId = x.PrimaryContactId,
                IsActive = x.IsActive,
                OwnerUserId = x.OwnerUserId,
                OwnerTeamId = x.OwnerTeamId
            })
            .FirstOrDefaultAsync();

        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    [HasPermission("Accounts.Create")]
    public async Task<ActionResult<AccountDto>> CreateAccount(UpsertAccountRequestDto dto)
    {
        var item = new Account
        {
            AccountNumber = dto.AccountNumber,
            Name = dto.Name,
            LegalName = dto.LegalName,
            TradingName = dto.TradingName,
            AccountTypeId = dto.AccountTypeId,
            IndustryId = dto.IndustryId,
            OwnershipTypeId = dto.OwnershipTypeId,
            CustomerStatusId = dto.CustomerStatusId,
            CustomerSegmentId = dto.CustomerSegmentId,
            Website = dto.Website,
            MainPhone = dto.MainPhone,
            AlternatePhone = dto.AlternatePhone,
            Email = dto.Email,
            Fax = dto.Fax,
            TaxNumber = dto.TaxNumber,
            RegistrationNumber = dto.RegistrationNumber,
            AnnualRevenue = dto.AnnualRevenue,
            NumberOfEmployees = dto.NumberOfEmployees,
            Description = dto.Description,
            ParentAccountId = dto.ParentAccountId,
            PrimaryContactId = dto.PrimaryContactId,
            IsActive = dto.IsActive,
            OwnerUserId = dto.OwnerUserId,
            OwnerTeamId = dto.OwnerTeamId
        };

        _dbContext.Accounts.Add(item);
        await _dbContext.SaveChangesAsync();

        return Ok(new AccountDto
        {
            Id = item.Id,
            AccountNumber = item.AccountNumber,
            Name = item.Name,
            LegalName = item.LegalName,
            TradingName = item.TradingName,
            AccountTypeId = item.AccountTypeId,
            IndustryId = item.IndustryId,
            OwnershipTypeId = item.OwnershipTypeId,
            CustomerStatusId = item.CustomerStatusId,
            CustomerSegmentId = item.CustomerSegmentId,
            Website = item.Website,
            MainPhone = item.MainPhone,
            AlternatePhone = item.AlternatePhone,
            Email = item.Email,
            Fax = item.Fax,
            TaxNumber = item.TaxNumber,
            RegistrationNumber = item.RegistrationNumber,
            AnnualRevenue = item.AnnualRevenue,
            NumberOfEmployees = item.NumberOfEmployees,
            Description = item.Description,
            ParentAccountId = item.ParentAccountId,
            PrimaryContactId = item.PrimaryContactId,
            IsActive = item.IsActive,
            OwnerUserId = item.OwnerUserId,
            OwnerTeamId = item.OwnerTeamId
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Accounts.Update")]
    public async Task<IActionResult> UpdateAccount(Guid id, UpsertAccountRequestDto dto)
    {
        var item = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.AccountNumber = dto.AccountNumber;
        item.Name = dto.Name;
        item.LegalName = dto.LegalName;
        item.TradingName = dto.TradingName;
        item.AccountTypeId = dto.AccountTypeId;
        item.IndustryId = dto.IndustryId;
        item.OwnershipTypeId = dto.OwnershipTypeId;
        item.CustomerStatusId = dto.CustomerStatusId;
        item.CustomerSegmentId = dto.CustomerSegmentId;
        item.Website = dto.Website;
        item.MainPhone = dto.MainPhone;
        item.AlternatePhone = dto.AlternatePhone;
        item.Email = dto.Email;
        item.Fax = dto.Fax;
        item.TaxNumber = dto.TaxNumber;
        item.RegistrationNumber = dto.RegistrationNumber;
        item.AnnualRevenue = dto.AnnualRevenue;
        item.NumberOfEmployees = dto.NumberOfEmployees;
        item.Description = dto.Description;
        item.ParentAccountId = dto.ParentAccountId;
        item.PrimaryContactId = dto.PrimaryContactId;
        item.IsActive = dto.IsActive;
        item.OwnerUserId = dto.OwnerUserId;
        item.OwnerTeamId = dto.OwnerTeamId;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Accounts.Delete")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var item = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}