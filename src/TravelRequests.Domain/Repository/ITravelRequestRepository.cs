using TravelRequests.Domain.Entities;

namespace TravelRequests.Domain.Repository;

public interface ITravelRequestRepository : IBaseRepository<TravelRequest>
{
    Task<List<TravelRequest>> GetByUserAsync(Guid userId);
    Task<TravelRequest?> GetByIdAsync(Guid id);
}
