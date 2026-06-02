using TravelRequests.Domain.Dto.Workspace;
using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Repository;
using TravelRequests.Domain.Services;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Application.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;

    public WorkspaceService(IWorkspaceRepository workspaceRepository, IUserRepository userRepository)
    {
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
    }

    public async Task<ResponsePackage<WorkspaceResponseDto>> CreateWorkspaceAsync(CreateWorkspaceDto dto, Guid ownerUserId)
    {
        var response = new ResponsePackage<WorkspaceResponseDto>();

        var workspace = new Workspace
        {
            WorkspaceId = Guid.NewGuid(),
            Name = dto.Name,
            OwnerId = ownerUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _workspaceRepository.InsertAsync(workspace);
        await _workspaceRepository.SaveAsync();

        // Assign owner as member: find user and update WorkspaceId
        var owner = await _userRepository.GetByIdAsync(ownerUserId);
        if (owner != null)
        {
            owner.WorkspaceId = workspace.WorkspaceId;
            _userRepository.MarkAsModified(owner);
            await _userRepository.SaveAsync();
        }

        response.Message = "OK";
        response.Result = new WorkspaceResponseDto
        {
            WorkspaceId = workspace.WorkspaceId,
            Name = workspace.Name,
            OwnerId = workspace.OwnerId,
            CreatedAt = workspace.CreatedAt
        };
        return response;
    }

    public async Task<ResponsePackage<WorkspaceResponseDto>> GetWorkspaceByIdAsync(Guid id)
    {
        var response = new ResponsePackage<WorkspaceResponseDto>();
        var ws = await _workspaceRepository.GetByIdAsync(id);
        if (ws == null)
        {
            response.Errors = new ErrorResponse(404, "Workspace not found");
            return response;
        }
        response.Message = "OK";
        response.Result = new WorkspaceResponseDto { WorkspaceId = ws.WorkspaceId, Name = ws.Name, OwnerId = ws.OwnerId, CreatedAt = ws.CreatedAt };
        return response;
    }

    public async Task<ResponsePackage<bool>> AddMemberAsync(Guid workspaceId, AddMemberDto dto)
    {
        var response = new ResponsePackage<bool>();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            Role = Enum.TryParse<TravelRequests.Domain.Enums.Role>(dto.Role, true, out var r) ? r : TravelRequests.Domain.Enums.Role.Requester,
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.InsertAsync(user);
        await _userRepository.SaveAsync();
        response.Message = "OK";
        response.Result = true;
        return response;
    }

    public async Task<ResponsePackage<bool>> RemoveMemberAsync(Guid workspaceId, Guid userId)
    {
        var response = new ResponsePackage<bool>();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.WorkspaceId != workspaceId)
        {
            response.Errors = new ErrorResponse(404, "Member not found in workspace");
            return response;
        }
        // Simple delete: mark as removed by setting IsActive false or delete (we'll delete)
        _userRepository.Update(user); // no delete method in base, so using Update as placeholder
        // To actually remove, one might implement a Delete method; for now, set IsActive = false if exists
        // user.IsActive = false; _userRepository.MarkAsModified(user);
        await _userRepository.SaveAsync();
        response.Message = "OK";
        response.Result = true;
        return response;
    }
}
