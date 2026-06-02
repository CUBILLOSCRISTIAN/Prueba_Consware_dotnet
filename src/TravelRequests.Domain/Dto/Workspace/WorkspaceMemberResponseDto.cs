using TravelRequests.Domain.Enums;

namespace TravelRequests.Domain.Dto.Workspace;

public class WorkspaceMemberResponseDto
{
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; }
}