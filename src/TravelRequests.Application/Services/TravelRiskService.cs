using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Services;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Application.Services;

public class TravelRiskService : ITravelRiskService
{
    public Task<ResponsePackage<RiskReport>> AssessRiskAsync(TravelRequest request)
    {
        var report = new RiskReport();
        var days = (request.EndDate - request.StartDate).TotalDays;
        report.Score = (decimal)Math.Min(100, Math.Max(0, days * 2));
        if (report.Score > 60) report.RiskLevel = "High";
        else if (report.Score > 30) report.RiskLevel = "Medium";
        else report.RiskLevel = "Low";
        report.Reasons.Add($"Duración: {days} días");
        report.Suggestions.Add("Revisar itinerario y reducción de duración si es posible.");

        var response = new ResponsePackage<RiskReport> { Message = "OK", Result = report };
        return Task.FromResult(response);
    }
}
