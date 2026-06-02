using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TravelRequests.Infrastructure;

public static class DBServerConfig
{
    public static IServiceCollection AddDBServer(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(conn));
        return services;
    }
}
