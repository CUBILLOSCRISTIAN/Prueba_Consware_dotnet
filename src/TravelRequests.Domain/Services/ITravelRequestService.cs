using TravelRequests.Domain.Dto.Travel;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Domain.Services;

public interface ITravelRequestService
{
    Task<ResponsePackage<TravelRequestResponseDto>> CreateAsync(CreateTravelRequestDto dto, Guid userId, Guid workspaceId);
    Task<ResponsePackage<IEnumerable<TravelRequestResponseDto>>> GetByUserAsync(Guid userId);
    Task<ResponsePackage<TravelRequestResponseDto>> GetByIdAsync(Guid id);
    Task<ResponsePackage<TravelRequestResponseDto>> ApproveAsync(Guid id, Guid approverUserId, Guid workspaceId);
    Task<ResponsePackage<TravelRequestResponseDto>> RejectAsync(Guid id, Guid approverUserId, Guid workspaceId);
}