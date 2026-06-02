using TravelRequests.Domain.Dto.Workspace;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Domain.Services;

public interface IWorkspaceService
{
    Task<ResponsePackage<WorkspaceResponseDto>> CreateWorkspaceAsync(CreateWorkspaceDto dto, Guid ownerUserId);
    Task<ResponsePackage<WorkspaceResponseDto>> GetWorkspaceByIdAsync(Guid id);
    Task<ResponsePackage<List<WorkspaceMemberResponseDto>>> GetMembersAsync(Guid workspaceId);
    Task<ResponsePackage<bool>> AddMemberAsync(Guid workspaceId, AddMemberDto dto);
    Task<ResponsePackage<bool>> RemoveMemberAsync(Guid workspaceId, Guid userId);
}
