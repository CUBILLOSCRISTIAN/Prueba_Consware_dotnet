using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRequests.Domain.Dto.Workspace;
using TravelRequests.Domain.Services;

namespace TravelRequests.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceDto dto)
    {
        // OwnerUserId from token
        var ownerClaim = User.FindFirst("UserId")?.Value;
        if (!Guid.TryParse(ownerClaim, out var ownerId)) return Unauthorized();

        var res = await _workspaceService.CreateWorkspaceAsync(dto, ownerId);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }

    [HttpGet("{id}/members")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        var res = await _workspaceService.GetMembersAsync(id);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }

    [HttpGet("{id}/users")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> GetUsers(Guid id)
    {
        var res = await _workspaceService.GetMembersAsync(id);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }

    [HttpPost("{id}/members")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto dto)
    {
        var res = await _workspaceService.AddMemberAsync(id, dto);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }

    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var res = await _workspaceService.RemoveMemberAsync(id, userId);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }
}
