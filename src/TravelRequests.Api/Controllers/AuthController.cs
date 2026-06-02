using Microsoft.AspNetCore.Mvc;
using TravelRequests.Domain.Dto.Auth;
using TravelRequests.Domain.Services;

namespace TravelRequests.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var res = await _authService.RegisterAsync(dto);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var res = await _authService.LoginAsync(dto);
        return res.Errors is null ? Ok(res) : StatusCode(res.Errors.StatusCode, res);
    }
}
