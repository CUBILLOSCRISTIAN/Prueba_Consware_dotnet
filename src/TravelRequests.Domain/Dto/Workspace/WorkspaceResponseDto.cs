namespace TravelRequests.Domain.Dto.Workspace;

public class WorkspaceResponseDto
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
