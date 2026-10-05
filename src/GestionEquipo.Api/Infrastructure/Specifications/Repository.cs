using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Infrastructure.Specifications;

public class Repository<T>(DbContext context) : IRepository<T> where T : class
{
    public async Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default) =>
        await SpecificationEvaluator.GetQuery(context.Set<T>().AsQueryable(), spec).ToListAsync(ct);

    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        var query = context.Set<T>().AsQueryable();
        if (spec.Criteria is not null) query = query.Where(spec.Criteria);
        return await query.CountAsync(ct);
    }

    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default) =>
        await context.Set<T>().FindAsync([id], ct);

    public void Add(T entity) => context.Set<T>().Add(entity);
    public void Remove(T entity) => context.Set<T>().Remove(entity);
    public async Task SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
