using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/account-relationships")]
public class AccountRelationshipsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AccountRelationshipsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("AccountRelationships.View")]
    public async Task<ActionResult<PagedResult<AccountRelationshipDto>>> GetAccountRelationships([FromQuery] ListQueryDto query)
    {
        var relationshipsQuery = _dbContext.AccountRelationships.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            relationshipsQuery = relationshipsQuery.Where(x => (x.Notes ?? string.Empty).ToLower().Contains(search));
        }

        relationshipsQuery = relationshipsQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = relationshipsQuery.Select(x => new AccountRelationshipDto
        {
            Id = x.Id,
            SourceAccountId = x.SourceAccountId,
            TargetAccountId = x.TargetAccountId,
            RelationshipTypeId = x.RelationshipTypeId,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            StrengthId = x.StrengthId,
            Notes = x.Notes,
            IsActive = x.IsActive
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("AccountRelationships.View")]
    public async Task<ActionResult<AccountRelationshipDto>> GetAccountRelationship(Guid id)
    {
        var item = await _dbContext.AccountRelationships
            .Where(x => x.Id == id)
            .Select(x => new AccountRelationshipDto
            {
                Id = x.Id,
                SourceAccountId = x.SourceAccountId,
                TargetAccountId = x.TargetAccountId,
                RelationshipTypeId = x.RelationshipTypeId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                StrengthId = x.StrengthId,
                Notes = x.Notes,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("AccountRelationships.Create")]
    public async Task<ActionResult<AccountRelationshipDto>> CreateAccountRelationship(UpsertAccountRelationshipRequestDto dto)
    {
        var item = new AccountRelationship
        {
            SourceAccountId = dto.SourceAccountId,
            TargetAccountId = dto.TargetAccountId,
            RelationshipTypeId = dto.RelationshipTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StrengthId = dto.StrengthId,
            Notes = dto.Notes,
            IsActive = dto.IsActive
        };

        _dbContext.AccountRelationships.Add(item);
        await _dbContext.SaveChangesAsync();

        return Ok(new AccountRelationshipDto
        {
            Id = item.Id,
            SourceAccountId = item.SourceAccountId,
            TargetAccountId = item.TargetAccountId,
            RelationshipTypeId = item.RelationshipTypeId,
            StartDate = item.StartDate,
            EndDate = item.EndDate,
            StrengthId = item.StrengthId,
            Notes = item.Notes,
            IsActive = item.IsActive
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("AccountRelationships.Update")]
    public async Task<IActionResult> UpdateAccountRelationship(Guid id, UpsertAccountRelationshipRequestDto dto)
    {
        var item = await _dbContext.AccountRelationships.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.SourceAccountId = dto.SourceAccountId;
        item.TargetAccountId = dto.TargetAccountId;
        item.RelationshipTypeId = dto.RelationshipTypeId;
        item.StartDate = dto.StartDate;
        item.EndDate = dto.EndDate;
        item.StrengthId = dto.StrengthId;
        item.Notes = dto.Notes;
        item.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("AccountRelationships.Delete")]
    public async Task<IActionResult> DeleteAccountRelationship(Guid id)
    {
        var item = await _dbContext.AccountRelationships.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
