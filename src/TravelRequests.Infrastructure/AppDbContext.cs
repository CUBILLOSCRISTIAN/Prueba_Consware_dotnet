using Microsoft.EntityFrameworkCore;
using TravelRequests.Domain.Entities;

namespace TravelRequests.Infrastructure;

public interface ICurrentWorkspaceProvider
{
    Guid WorkspaceId { get; }
}

public class AppDbContext : DbContext
{
    private readonly ICurrentWorkspaceProvider? _workspaceProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentWorkspaceProvider? workspaceProvider = null)
        : base(options)
    {
        _workspaceProvider = workspaceProvider;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Workspace> Workspaces { get; set; } = null!;
    public DbSet<TravelRequest> TravelRequests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("user", "dbo");
        modelBuilder.Entity<Workspace>().ToTable("workspace", "dbo");
        modelBuilder.Entity<TravelRequest>().ToTable("travel_request", "dbo");

        // Global query filter for multi-tenant isolation (skip when provider is not set or WorkspaceId is empty)
        if (_workspaceProvider != null)
        {
            var wsId = _workspaceProvider.WorkspaceId;
            if (wsId != Guid.Empty)
            {
                modelBuilder.Entity<User>().HasQueryFilter(u => u.WorkspaceId == wsId);
                modelBuilder.Entity<TravelRequest>().HasQueryFilter(t => t.WorkspaceId == wsId);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
