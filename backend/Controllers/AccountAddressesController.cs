using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/account-addresses")]
public class AccountAddressesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AccountAddressesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("AccountAddresses.View")]
    public async Task<ActionResult<PagedResult<AccountAddressDto>>> GetAccountAddresses([FromQuery] ListQueryDto query)
    {
        var addressesQuery = _dbContext.AccountAddresses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            addressesQuery = addressesQuery.Where(x =>
                x.Line1.ToLower().Contains(search) ||
                (x.Line2 ?? string.Empty).ToLower().Contains(search) ||
                (x.City ?? string.Empty).ToLower().Contains(search) ||
                (x.PostalCode ?? string.Empty).ToLower().Contains(search));
        }

        addressesQuery = addressesQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = addressesQuery.Select(x => new AccountAddressDto
        {
            Id = x.Id,
            AccountId = x.AccountId,
            AddressTypeId = x.AddressTypeId,
            AttentionTo = x.AttentionTo,
            Line1 = x.Line1,
            Line2 = x.Line2,
            Landmark = x.Landmark,
            City = x.City,
            StateProvince = x.StateProvince,
            PostalCode = x.PostalCode,
            CountryId = x.CountryId,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            IsPrimary = x.IsPrimary,
            IsBilling = x.IsBilling,
            IsShipping = x.IsShipping,
            IsActive = x.IsActive
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("AccountAddresses.View")]
    public async Task<ActionResult<AccountAddressDto>> GetAccountAddress(Guid id)
    {
        var item = await _dbContext.AccountAddresses
            .Where(x => x.Id == id)
            .Select(x => new AccountAddressDto
            {
                Id = x.Id,
                AccountId = x.AccountId,
                AddressTypeId = x.AddressTypeId,
                AttentionTo = x.AttentionTo,
                Line1 = x.Line1,
                Line2 = x.Line2,
                Landmark = x.Landmark,
                City = x.City,
                StateProvince = x.StateProvince,
                PostalCode = x.PostalCode,
                CountryId = x.CountryId,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                IsPrimary = x.IsPrimary,
                IsBilling = x.IsBilling,
                IsShipping = x.IsShipping,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("AccountAddresses.Create")]
    public async Task<ActionResult<AccountAddressDto>> CreateAccountAddress(UpsertAccountAddressRequestDto dto)
    {
        var item = new AccountAddress
        {
            AccountId = dto.AccountId,
            AddressTypeId = dto.AddressTypeId,
            AttentionTo = dto.AttentionTo,
            Line1 = dto.Line1,
            Line2 = dto.Line2,
            Landmark = dto.Landmark,
            City = dto.City,
            StateProvince = dto.StateProvince,
            PostalCode = dto.PostalCode,
            CountryId = dto.CountryId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsPrimary = dto.IsPrimary,
            IsBilling = dto.IsBilling,
            IsShipping = dto.IsShipping,
            IsActive = dto.IsActive
        };

        _dbContext.AccountAddresses.Add(item);
        await _dbContext.SaveChangesAsync();

        return Ok(new AccountAddressDto
        {
            Id = item.Id,
            AccountId = item.AccountId,
            AddressTypeId = item.AddressTypeId,
            AttentionTo = item.AttentionTo,
            Line1 = item.Line1,
            Line2 = item.Line2,
            Landmark = item.Landmark,
            City = item.City,
            StateProvince = item.StateProvince,
            PostalCode = item.PostalCode,
            CountryId = item.CountryId,
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            IsPrimary = item.IsPrimary,
            IsBilling = item.IsBilling,
            IsShipping = item.IsShipping,
            IsActive = item.IsActive
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("AccountAddresses.Update")]
    public async Task<IActionResult> UpdateAccountAddress(Guid id, UpsertAccountAddressRequestDto dto)
    {
        var item = await _dbContext.AccountAddresses.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.AccountId = dto.AccountId;
        item.AddressTypeId = dto.AddressTypeId;
        item.AttentionTo = dto.AttentionTo;
        item.Line1 = dto.Line1;
        item.Line2 = dto.Line2;
        item.Landmark = dto.Landmark;
        item.City = dto.City;
        item.StateProvince = dto.StateProvince;
        item.PostalCode = dto.PostalCode;
        item.CountryId = dto.CountryId;
        item.Latitude = dto.Latitude;
        item.Longitude = dto.Longitude;
        item.IsPrimary = dto.IsPrimary;
        item.IsBilling = dto.IsBilling;
        item.IsShipping = dto.IsShipping;
        item.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("AccountAddresses.Delete")]
    public async Task<IActionResult> DeleteAccountAddress(Guid id)
    {
        var item = await _dbContext.AccountAddresses.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
