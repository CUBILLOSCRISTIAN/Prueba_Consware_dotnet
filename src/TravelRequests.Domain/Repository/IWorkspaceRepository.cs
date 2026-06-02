using TravelRequests.Domain.Entities;

namespace TravelRequests.Domain.Repository;

public interface IWorkspaceRepository : IBaseRepository<Workspace>
{
    Task<Workspace?> GetByIdAsync(Guid id);
    Task<List<User>> GetMembersAsync(Guid workspaceId);
}
