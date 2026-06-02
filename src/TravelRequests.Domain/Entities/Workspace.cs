using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelRequests.Domain.Entities;

[Table("workspace")]
public class Workspace
{
    [Key]
    public Guid WorkspaceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid OwnerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public WorkspaceAIConfig? AIConfig { get; set; }

    public ICollection<User> Members { get; set; } = new List<User>();
    public ICollection<TravelRequest> TravelRequests { get; set; } = new List<TravelRequest>();
}

public class WorkspaceAIConfig
{
    // Simple placeholder for AI toggles
    public bool EnableRiskAssessment { get; set; } = true;
    public bool EnableClassifier { get; set; } = true;
}
