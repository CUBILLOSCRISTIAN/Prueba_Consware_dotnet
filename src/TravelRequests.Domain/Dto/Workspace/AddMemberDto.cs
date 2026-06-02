namespace TravelRequests.Domain.Dto.Workspace;

public class AddMemberDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Requester";
}
