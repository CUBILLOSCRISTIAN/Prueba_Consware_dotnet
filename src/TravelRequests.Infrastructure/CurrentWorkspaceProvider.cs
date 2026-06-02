using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TravelRequests.Infrastructure;

public class CurrentWorkspaceProvider : ICurrentWorkspaceProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentWorkspaceProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid WorkspaceId
    {
        get
        {
            var http = _httpContextAccessor.HttpContext;
            if (http == null) return Guid.Empty;
            var claim = http.User.FindFirst("workspaceId") ?? http.User.FindFirst("workspaceid");
            if (claim == null) return Guid.Empty;
            return Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }
    }
}
