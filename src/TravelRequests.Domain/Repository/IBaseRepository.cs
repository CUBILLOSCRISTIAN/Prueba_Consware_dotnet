namespace TravelRequests.Domain.Repository;

public interface IBaseRepository<T> where T : class
{
    Task InsertAsync(T entity);
    Task SaveAsync();
    void Update(T entity);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    void MarkAsModified(T entity);
}
