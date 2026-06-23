using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/product-categories")]
public class ProductCategoriesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductCategoriesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("ProductCategories.View")]
    public async Task<ActionResult<PagedResult<ProductCategoryDto>>> GetCategories([FromQuery] ProductCategoryFilterDto query)
    {
        var categories = _dbContext.ProductCategories.AsQueryable();

        if (query.ParentCategoryId.HasValue)
        {
            categories = categories.Where(x => x.ParentCategoryId == query.ParentCategoryId.Value);
        }

        if (query.IsActive.HasValue)
        {
            categories = categories.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            categories = categories.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        categories = categories.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            categories = categories.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
        }

        return Ok(await ProjectCategories(categories).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("ProductCategories.View")]
    public async Task<ActionResult<ProductCategoryDto>> GetCategory(Guid id)
    {
        var category = await ProjectCategories(_dbContext.ProductCategories.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [HasPermission("ProductCategories.Create")]
    public async Task<ActionResult<ProductCategoryDto>> CreateCategory(UpsertProductCategoryRequestDto dto)
    {
        var validationError = await ValidateCategoryAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var category = new ProductCategory();
        ApplyValues(category, dto);

        _dbContext.ProductCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectCategories(_dbContext.ProductCategories.Where(x => x.Id == category.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Product category was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("ProductCategories.Update")]
    public async Task<IActionResult> UpdateCategory(Guid id, UpsertProductCategoryRequestDto dto)
    {
        var category = await _dbContext.ProductCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        var validationError = await ValidateCategoryAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyValues(category, dto);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("ProductCategories.Delete")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _dbContext.ProductCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (await _dbContext.Products.AnyAsync(x => x.ProductCategoryId == id))
        {
            return BadRequest("Cannot delete a category that has products.");
        }

        if (await _dbContext.ProductCategories.AnyAsync(x => x.ParentCategoryId == id))
        {
            return BadRequest("Cannot delete a category that has child categories.");
        }

        category.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateCategoryAsync(Guid? id, UpsertProductCategoryRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Name is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            return "Code is required.";
        }

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        if (await _dbContext.ProductCategories.AnyAsync(x => x.Id != id && x.Code == normalizedCode))
        {
            return "Code already exists.";
        }

        if (dto.ParentCategoryId.HasValue)
        {
            if (id.HasValue && dto.ParentCategoryId.Value == id.Value)
            {
                return "Category cannot reference itself as parent.";
            }

            var parentExists = await _dbContext.ProductCategories.AnyAsync(x => x.Id == dto.ParentCategoryId.Value);
            if (!parentExists)
            {
                return "Parent category is invalid.";
            }

            if (id.HasValue && await CreatesCycleAsync(id.Value, dto.ParentCategoryId.Value))
            {
                return "Circular category hierarchy is not allowed.";
            }
        }

        return null;
    }

    private async Task<bool> CreatesCycleAsync(Guid categoryId, Guid newParentId)
    {
        var currentParentId = newParentId;
        while (true)
        {
            if (currentParentId == categoryId)
            {
                return true;
            }

            var parent = await _dbContext.ProductCategories
                .Where(x => x.Id == currentParentId)
                .Select(x => x.ParentCategoryId)
                .FirstOrDefaultAsync();

            if (!parent.HasValue)
            {
                return false;
            }

            currentParentId = parent.Value;
        }
    }

    private static void ApplyValues(ProductCategory item, UpsertProductCategoryRequestDto dto)
    {
        item.Name = dto.Name.Trim();
        item.Code = dto.Code.Trim().ToUpperInvariant();
        item.Description = TrimToNull(dto.Description);
        item.ParentCategoryId = dto.ParentCategoryId;
        item.SortOrder = dto.SortOrder;
        item.IsActive = dto.IsActive;
    }

    private IQueryable<ProductCategoryDto> ProjectCategories(IQueryable<ProductCategory> query)
    {
        return query.Select(x => new ProductCategoryDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Description = x.Description,
            ParentCategoryId = x.ParentCategoryId,
            ParentCategoryName = x.ParentCategory != null ? x.ParentCategory.Name : null,
            SortOrder = x.SortOrder,
            ProductCount = x.Products.Count,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
