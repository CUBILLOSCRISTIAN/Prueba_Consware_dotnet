using TravelRequests.Domain.Enums;

namespace TravelRequests.Domain.Dto.Travel;

public class TravelRequestResponseDto
{
    public Guid Id { get; set; }
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Justification { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public decimal? RiskScore { get; set; }
    public string? RiskLevel { get; set; }
    public string? Category { get; set; }
}
