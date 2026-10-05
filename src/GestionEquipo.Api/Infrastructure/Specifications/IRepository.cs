namespace GestionEquipo.Api.Infrastructure.Specifications;

public interface IRepository<T> where T : class
{
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    void Add(T entity);
    void Remove(T entity);
    Task SaveChangesAsync(CancellationToken ct = default);
}
