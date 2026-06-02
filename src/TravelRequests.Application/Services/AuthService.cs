using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TravelRequests.Domain.Dto.Auth;
using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Repository;
using TravelRequests.Domain.Services;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<ResponsePackage<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
    {
        var response = new ResponsePackage<AuthResponseDto>();

        var existing = await _userRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
        {
            response.Errors = new ErrorResponse(400, "Email already registered");
            return response;
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            WorkspaceId = dto.WorkspaceId,
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Enum.TryParse<TravelRequests.Domain.Enums.Role>(dto.Role, true, out var r) ? r : TravelRequests.Domain.Enums.Role.Requester
        };

        await _userRepository.InsertAsync(user);
        await _userRepository.SaveAsync();

        var token = GenerateToken(user);
        response.Message = "OK";
        response.Result = new AuthResponseDto { Token = token, ExpiresAt = DateTime.UtcNow.AddHours(8) };
        return response;
    }

    public async Task<ResponsePackage<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var response = new ResponsePackage<AuthResponseDto>();
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            response.Errors = new ErrorResponse(401, "Invalid credentials");
            return response;
        }

        var token = GenerateToken(user);
        response.Message = "OK";
        response.Result = new AuthResponseDto { Token = token, ExpiresAt = DateTime.UtcNow.AddHours(8) };
        return response;
    }

    private string GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "travelrequests";
        var audience = _configuration["Jwt:Audience"] ?? "travelrequests_clients";
        var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var m) ? m : 480;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("UserId", user.UserId.ToString()),
            new Claim("workspaceId", user.WorkspaceId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes), signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
