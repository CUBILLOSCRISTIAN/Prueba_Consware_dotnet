using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelRequests.Domain.Enums;

namespace TravelRequests.Domain.Entities;

[Table("travel_request")]
public class TravelRequest
{
    [Key]
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public Guid UserId { get; set; }

    public string OriginCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string Justification { get; set; } = string.Empty;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // AI fields placeholder
    public string? RiskReportJson { get; set; }
    public string? Category { get; set; }
}
