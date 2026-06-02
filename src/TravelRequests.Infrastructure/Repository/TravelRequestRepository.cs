using Microsoft.EntityFrameworkCore;
using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Repository;

namespace TravelRequests.Infrastructure.Repository;

public class TravelRequestRepository : BaseEfRepository<TravelRequest>, ITravelRequestRepository
{
    public TravelRequestRepository(AppDbContext context) : base(context) { }

    public async Task<List<TravelRequest>> GetByUserAsync(Guid userId)
    {
        return await _entity.AsNoTracking().Where(t => t.UserId == userId).ToListAsync();
    }

    public async Task<TravelRequest?> GetByIdAsync(Guid id)
    {
        return await _entity.FindAsync(id);
    }
}
