namespace TravelRequests.Domain.Dto.Auth;

public class PasswordRecoveryResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}