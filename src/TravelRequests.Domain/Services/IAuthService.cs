using TravelRequests.Domain.Dto.Auth;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Domain.Services;

public interface IAuthService
{
    Task<ResponsePackage<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
    Task<ResponsePackage<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
}
