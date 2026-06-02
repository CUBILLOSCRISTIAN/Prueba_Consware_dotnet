using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelRequests.Domain.Dto.Travel;
using TravelRequests.Domain.Services;

namespace TravelRequests.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TravelRequestsController : ControllerBase
{
    private readonly ITravelRequestService _service;

    public TravelRequestsController(ITravelRequestService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateTravelRequestDto dto)
    {
        var userClaim = User.FindFirst("UserId")?.Value;
        var workspaceClaim = User.FindFirst("workspaceId")?.Value;
        if (!Guid.TryParse(userClaim, out var userId)) return Unauthorized();
        if (!Guid.TryParse(workspaceClaim, out var workspaceId)) return Unauthorized();

        var res = await _service.CreateAsync(dto, userId, workspaceId);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> MyRequests()
    {
        var userClaim = User.FindFirst("UserId")?.Value;
        if (!Guid.TryParse(userClaim, out var userId)) return Unauthorized();
        var res = await _service.GetByUserAsync(userId);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Get(Guid id)
    {
        var res = await _service.GetByIdAsync(id);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }
}
