namespace TravelRequests.Domain.Dto.Travel;

public class CreateTravelRequestDto
{
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Justification { get; set; } = string.Empty;
}
