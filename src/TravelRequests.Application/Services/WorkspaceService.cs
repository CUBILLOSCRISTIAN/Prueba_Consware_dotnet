using TravelRequests.Domain.Dto.Workspace;
using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Enums;
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

    public async Task<ResponsePackage<List<WorkspaceMemberResponseDto>>> GetMembersAsync(Guid workspaceId)
    {
        var response = new ResponsePackage<List<WorkspaceMemberResponseDto>>();
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        if (workspace == null)
        {
            response.Errors = new ErrorResponse(404, "Workspace not found");
            return response;
        }

        var members = await _workspaceRepository.GetMembersAsync(workspaceId);
        response.Message = "OK";
        response.Result = members.Select(member => new WorkspaceMemberResponseDto
        {
            UserId = member.UserId,
            WorkspaceId = member.WorkspaceId,
            Name = member.Name,
            Email = member.Email,
            Role = member.Role,
            CreatedAt = member.CreatedAt
        }).ToList();
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

        if (user.Role == Role.Owner)
        {
            response.Errors = new ErrorResponse(400, "Owner cannot be removed from the workspace");
            return response;
        }

        await _userRepository.DeleteAsync(user);
        response.Message = "OK";
        response.Result = true;
        return response;
    }
}
