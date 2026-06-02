using Microsoft.EntityFrameworkCore;
using TravelRequests.Domain.Repository;

namespace TravelRequests.Infrastructure.Repository;

public class BaseEfRepository<T> : TravelRequests.Domain.Repository.IBaseRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _entity;

    public BaseEfRepository(AppDbContext context)
    {
        _context = context;
        _entity = _context.Set<T>();
    }

    public virtual async Task InsertAsync(T entity)
    {
        await _entity.AddAsync(entity);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();

    public virtual void Update(T entity) => _context.Update(entity);

    public virtual async Task DeleteAsync(T entity)
    {
        _entity.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _entity.ToListAsync();

    public async Task<T?> GetByIdAsync(Guid id) => await _entity.FindAsync(id);

    public void MarkAsModified(T entity) => _context.Entry(entity).State = EntityState.Modified;
}
