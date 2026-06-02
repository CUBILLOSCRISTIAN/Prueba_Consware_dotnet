using TravelRequests.Domain.Entities;

namespace TravelRequests.Domain.Repository;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
}
