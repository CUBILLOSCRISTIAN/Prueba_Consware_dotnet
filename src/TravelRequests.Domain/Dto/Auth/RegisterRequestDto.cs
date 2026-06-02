namespace TravelRequests.Domain.Dto.Auth;

public class RegisterRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public string Role { get; set; } = "Requester";
}
