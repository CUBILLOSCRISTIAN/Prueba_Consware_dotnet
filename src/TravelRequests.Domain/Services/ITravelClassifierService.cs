using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Enums;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Domain.Services;

public interface ITravelClassifierService
{
    Task<ResponsePackage<TravelCategory>> ClassifyAsync(TravelRequest request);
}
