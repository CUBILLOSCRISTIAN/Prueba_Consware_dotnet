using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Domain.Services;

public interface ITravelRiskService
{
    Task<ResponsePackage<RiskReport>> AssessRiskAsync(TravelRequest request);
}

public class RiskReport
{
    public string RiskLevel { get; set; } = "Low";
    public List<string> Reasons { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public decimal Score { get; set; }
}
