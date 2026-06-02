using Microsoft.EntityFrameworkCore;
using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Repository;

namespace TravelRequests.Infrastructure.Repository;

public class UserRepository : BaseEfRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _entity.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }

    // Explicit implementation to satisfy interface
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _entity.FindAsync(id);
    }
}
