using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/sales-targets")]
public class SalesTargetsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public SalesTargetsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("SalesTargets.View")]
    public async Task<ActionResult<PagedResult<SalesTargetDto>>> GetTargets([FromQuery] SalesTargetFilterDto query)
    {
        var targets = _dbContext.SalesTargets.AsQueryable();

        if (query.TargetTypeId.HasValue) targets = targets.Where(x => x.TargetTypeId == query.TargetTypeId.Value);
        if (query.TargetPeriodId.HasValue) targets = targets.Where(x => x.TargetPeriodId == query.TargetPeriodId.Value);
        if (query.AssignedUserId.HasValue) targets = targets.Where(x => x.AssignedUserId == query.AssignedUserId.Value);
        if (query.AssignedTeamId.HasValue) targets = targets.Where(x => x.AssignedTeamId == query.AssignedTeamId.Value);
        if (query.IsActive.HasValue) targets = targets.Where(x => x.IsActive == query.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            targets = targets.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                x.TargetType.Name.ToLower().Contains(search) ||
                x.TargetPeriod.Name.ToLower().Contains(search));
        }

        targets = targets.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            targets = targets.OrderByDescending(x => x.StartDate).ThenBy(x => x.Name);
        }

        return Ok(await ProjectTargets(targets).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("SalesTargets.View")]
    public async Task<ActionResult<SalesTargetDto>> GetTarget(Guid id)
    {
        var target = await ProjectTargets(_dbContext.SalesTargets.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return target is null ? NotFound() : Ok(target);
    }

    [HttpPost]
    [HasPermission("SalesTargets.Create")]
    public async Task<ActionResult<SalesTargetDto>> CreateTarget(UpsertSalesTargetRequestDto dto)
    {
        var validationError = await ValidateTargetAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var target = new SalesTarget();
        ApplyTargetValues(target, dto);
        _dbContext.SalesTargets.Add(target);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectTargets(_dbContext.SalesTargets.Where(x => x.Id == target.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Sales target was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("SalesTargets.Update")]
    public async Task<IActionResult> UpdateTarget(Guid id, UpsertSalesTargetRequestDto dto)
    {
        var target = await _dbContext.SalesTargets.FirstOrDefaultAsync(x => x.Id == id);
        if (target is null)
        {
            return NotFound();
        }

        var validationError = await ValidateTargetAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyTargetValues(target, dto);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("SalesTargets.Delete")]
    public async Task<IActionResult> DeleteTarget(Guid id)
    {
        var target = await _dbContext.SalesTargets.FirstOrDefaultAsync(x => x.Id == id);
        if (target is null)
        {
            return NotFound();
        }

        target.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateTargetAsync(UpsertSalesTargetRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Name is required.";
        }

        if (dto.EndDate < dto.StartDate)
        {
            return "End date must be on or after start date.";
        }

        if (dto.TargetAmount < 0 || dto.ActualAmount < 0)
        {
            return "Target and actual amounts cannot be negative.";
        }

        if (dto.AssignedUserId.HasValue == dto.AssignedTeamId.HasValue)
        {
            return "Assign the target to either a user or a team.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.TargetTypeId && x.LookupCategory.Code == "SALES_TARGET_TYPE"))
        {
            return "Target type is required.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.TargetPeriodId && x.LookupCategory.Code == "SALES_TARGET_PERIOD"))
        {
            return "Target period is required.";
        }

        if (dto.AssignedUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.AssignedUserId.Value && !x.IsDeleted))
        {
            return "Assigned user is invalid.";
        }

        if (dto.AssignedTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.AssignedTeamId.Value))
        {
            return "Assigned team is invalid.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value && !x.IsDeleted))
        {
            return "Owner user is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        return null;
    }

    private static void ApplyTargetValues(SalesTarget target, UpsertSalesTargetRequestDto dto)
    {
        target.Name = dto.Name.Trim();
        target.Description = TrimToNull(dto.Description);
        target.TargetTypeId = dto.TargetTypeId;
        target.TargetPeriodId = dto.TargetPeriodId;
        target.StartDate = dto.StartDate;
        target.EndDate = dto.EndDate;
        target.TargetAmount = dto.TargetAmount;
        target.ActualAmount = dto.ActualAmount;
        target.AchievementPercentage = dto.TargetAmount <= 0m ? 0m : Math.Round(dto.ActualAmount / dto.TargetAmount * 100m, 1);
        target.AssignedUserId = dto.AssignedUserId;
        target.AssignedTeamId = dto.AssignedTeamId;
        target.OwnerUserId = dto.OwnerUserId ?? dto.AssignedUserId;
        target.OwnerTeamId = dto.OwnerTeamId ?? dto.AssignedTeamId;
        target.IsActive = dto.IsActive;
    }

    private static IQueryable<SalesTargetDto> ProjectTargets(IQueryable<SalesTarget> query)
    {
        return query.Select(x => new SalesTargetDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            TargetTypeId = x.TargetTypeId,
            TargetTypeName = x.TargetType.Name,
            TargetPeriodId = x.TargetPeriodId,
            TargetPeriodName = x.TargetPeriod.Name,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            TargetAmount = x.TargetAmount,
            ActualAmount = x.ActualAmount,
            AchievementPercentage = x.AchievementPercentage,
            AssignedUserId = x.AssignedUserId,
            AssignedUserName = x.AssignedUser != null ? x.AssignedUser.Email : null,
            AssignedTeamId = x.AssignedTeamId,
            AssignedTeamName = x.AssignedTeam != null ? x.AssignedTeam.Name : null,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerUserName = x.OwnerUser != null ? x.OwnerUser.Email : null,
            OwnerTeamId = x.OwnerTeamId,
            OwnerTeamName = x.OwnerTeam != null ? x.OwnerTeam.Name : null,
            CreatedAt = x.CreatedAt
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
