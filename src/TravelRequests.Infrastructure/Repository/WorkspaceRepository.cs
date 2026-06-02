using Microsoft.EntityFrameworkCore;
using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Repository;

namespace TravelRequests.Infrastructure.Repository;

public class WorkspaceRepository : BaseEfRepository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(AppDbContext context) : base(context) { }

    public async Task<Workspace?> GetByIdAsync(Guid id)
    {
        return await _entity.Include(w => w.Members).FirstOrDefaultAsync(w => w.WorkspaceId == id);
    }

    public async Task<List<User>> GetMembersAsync(Guid workspaceId)
    {
        return await _context.Users.AsNoTracking().Where(u => u.WorkspaceId == workspaceId).ToListAsync();
    }
}
