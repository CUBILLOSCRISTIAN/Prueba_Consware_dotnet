using TravelRequests.Domain.Dto.Travel;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Domain.Services;

public interface ITravelRequestService
{
    Task<ResponsePackage<TravelRequestResponseDto>> CreateAsync(CreateTravelRequestDto dto, Guid userId, Guid workspaceId);
    Task<ResponsePackage<List<TravelRequestResponseDto>>> GetByUserAsync(Guid userId);
    Task<ResponsePackage<TravelRequestResponseDto>> GetByIdAsync(Guid id);
}
