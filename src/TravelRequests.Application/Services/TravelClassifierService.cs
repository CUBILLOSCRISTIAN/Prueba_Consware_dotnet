using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Enums;
using TravelRequests.Domain.Services;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Application.Services;

public class TravelClassifierService : ITravelClassifierService
{
    public Task<ResponsePackage<TravelCategory>> ClassifyAsync(TravelRequest request)
    {
        TravelCategory category = TravelCategory.NACIONAL;
        if (request.OriginCity.Equals(request.DestinationCity, StringComparison.OrdinalIgnoreCase))
            category = TravelCategory.REGIONAL;
        else if (request.OriginCity.Contains("International", StringComparison.OrdinalIgnoreCase) || request.DestinationCity.Contains("International", StringComparison.OrdinalIgnoreCase))
            category = TravelCategory.INTERNACIONAL;
        else if ((request.EndDate - request.StartDate).TotalDays <= 1)
            category = TravelCategory.URGENTE;

        var resp = new ResponsePackage<TravelCategory> { Message = "OK", Result = category };
        return Task.FromResult(resp);
    }
}
