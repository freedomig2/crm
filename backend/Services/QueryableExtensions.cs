using System.Linq.Expressions;
using backend.DTOs;

namespace backend.Services;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(this IQueryable<T> query, ListQueryDto dto, CancellationToken cancellationToken = default)
    {
        var total = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query, cancellationToken);
        var items = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            query.Skip((dto.Page - 1) * dto.PageSize).Take(dto.PageSize),
            cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            Page = dto.Page,
            PageSize = dto.PageSize
        };
    }

    public static IQueryable<T> OrderByPropertyName<T>(this IQueryable<T> query, string? sortBy, string sortDir)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var property = typeof(T).GetProperty(sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (property is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);
        var methodName = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "OrderByDescending" : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}
