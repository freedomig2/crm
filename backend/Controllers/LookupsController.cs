using backend.Authorization;
using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public LookupsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{categoryCode}/values")]
    [HasPermission("ReferenceData.View")]
    public async Task<ActionResult<IReadOnlyCollection<LookupOptionDto>>> GetLookupValuesByCategory(
        string categoryCode,
        [FromQuery] string? search,
        [FromQuery] int limit = 250)
    {
        if (string.IsNullOrWhiteSpace(categoryCode))
        {
            return BadRequest("Lookup category code is required.");
        }

        var normalizedCode = categoryCode.Trim().ToUpperInvariant();
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLowerInvariant();
        var pageSize = Math.Clamp(limit, 1, 1000);

        var query = _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == normalizedCode && x.IsActive);

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(x => x.Name.ToLower().Contains(normalizedSearch));
        }

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Take(pageSize)
            .Select(x => new LookupOptionDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .ToListAsync();

        return Ok(items);
    }
}
