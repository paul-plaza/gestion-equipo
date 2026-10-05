using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Infrastructure.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> spec) where T : class
    {
        if (spec.Criteria is not null) query = query.Where(spec.Criteria);
        query = spec.Includes.Aggregate(query, (q, include) => q.Include(include));
        if (spec.OrderBy is not null) query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending is not null) query = query.OrderByDescending(spec.OrderByDescending);
        if (spec.Skip.HasValue) query = query.Skip(spec.Skip.Value);
        if (spec.Take.HasValue) query = query.Take(spec.Take.Value);
        return query;
    }
}
