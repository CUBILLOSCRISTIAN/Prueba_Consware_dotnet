using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelRequests.Domain.Enums;

namespace TravelRequests.Domain.Entities;

[Table("user")]
public class User
{
    [Key]
    public Guid UserId { get; set; }

    public Guid WorkspaceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; } = Role.Requester;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
