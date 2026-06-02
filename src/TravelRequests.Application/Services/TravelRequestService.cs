using TravelRequests.Domain.Dto.Travel;
using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Repository;
using TravelRequests.Domain.Services;
using TravelRequests.Domain.Shared;
using System.Text.Json;

namespace TravelRequests.Application.Services;

public class TravelRequestService : ITravelRequestService
{
    private readonly ITravelRequestRepository _repo;
    private readonly ITravelRiskService _riskService;
    private readonly ITravelClassifierService _classifier;
    private readonly IUserRepository _userRepository;

    public TravelRequestService(ITravelRequestRepository repo, ITravelRiskService riskService, ITravelClassifierService classifier, IUserRepository userRepository)
    {
        _repo = repo;
        _riskService = riskService;
        _classifier = classifier;
        _userRepository = userRepository;
    }

    public async Task<ResponsePackage<TravelRequestResponseDto>> CreateAsync(CreateTravelRequestDto dto, Guid userId, Guid workspaceId)
    {
        var response = new ResponsePackage<TravelRequestResponseDto>();

        if (dto.EndDate <= dto.StartDate)
        {
            response.Errors = new ErrorResponse(400, "EndDate must be after StartDate");
            return response;
        }
        if (dto.OriginCity == dto.DestinationCity)
        {
            response.Errors = new ErrorResponse(400, "Origin and destination must differ");
            return response;
        }

        var tr = new TravelRequest
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = userId,
            OriginCity = dto.OriginCity,
            DestinationCity = dto.DestinationCity,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Justification = dto.Justification,
            CreatedAt = DateTime.UtcNow,
            Status = TravelRequests.Domain.Enums.RequestStatus.Pending
        };

        await _repo.InsertAsync(tr);
        await _repo.SaveAsync();

        // AI services
        var risk = await _riskService.AssessRiskAsync(tr);
        var cat = await _classifier.ClassifyAsync(tr);

        tr.RiskReportJson = System.Text.Json.JsonSerializer.Serialize(risk.Result);
        tr.Category = cat.Result.ToString();
        _repo.MarkAsModified(tr);
        await _repo.SaveAsync();

        response.Message = "OK";
        response.Result = new TravelRequestResponseDto
        {
            Id = tr.Id,
            OriginCity = tr.OriginCity,
            DestinationCity = tr.DestinationCity,
            StartDate = tr.StartDate,
            EndDate = tr.EndDate,
            Justification = tr.Justification,
            Status = tr.Status,
            RiskScore = risk.Result.Score,
            RiskLevel = risk.Result.RiskLevel,
            Category = tr.Category
        };
        return response;
    }

    public async Task<ResponsePackage<IEnumerable<TravelRequestResponseDto>>> GetByUserAsync(Guid userId)
    {
        var response = new ResponsePackage<IEnumerable<TravelRequestResponseDto>>();
        var items = await _repo.GetByUserAsync(userId);
        response.Result = items.Select(MapToResponse).ToList();
        response.Message = "OK";
        return response;
    }

    public async Task<ResponsePackage<TravelRequestResponseDto>> GetByIdAsync(Guid id)
    {
        var response = new ResponsePackage<TravelRequestResponseDto>();
        var tr = await _repo.GetByIdAsync(id);
        if (tr == null)
        {
            response.Errors = new ErrorResponse(404, "Not found");
            return response;
        }
        response.Message = "OK";
        response.Result = MapToResponse(tr);
        return response;
    }

    public async Task<ResponsePackage<TravelRequestResponseDto>> ApproveAsync(Guid id, Guid approverId, Guid workspaceId)
    {
        return await ChangeStatusAsync(id, approverId, workspaceId, TravelRequests.Domain.Enums.RequestStatus.Approved);
    }

    public async Task<ResponsePackage<TravelRequestResponseDto>> RejectAsync(Guid id, Guid approverId, Guid workspaceId)
    {
        return await ChangeStatusAsync(id, approverId, workspaceId, TravelRequests.Domain.Enums.RequestStatus.Rejected);
    }

    private async Task<ResponsePackage<TravelRequestResponseDto>> ChangeStatusAsync(Guid id, Guid approverId, Guid workspaceId, TravelRequests.Domain.Enums.RequestStatus targetStatus)
    {
        var response = new ResponsePackage<TravelRequestResponseDto>();

        var approver = await _userRepository.GetByIdAsync(approverId);
        if (approver == null || approver.WorkspaceId != workspaceId || approver.Role != TravelRequests.Domain.Enums.Role.Approver)
        {
            response.Errors = new ErrorResponse(403, "Only Approver users from the same workspace can change request status");
            return response;
        }

        var tr = await _repo.GetByIdAsync(id);
        if (tr == null || tr.WorkspaceId != workspaceId)
        {
            response.Errors = new ErrorResponse(404, "Travel request not found");
            return response;
        }

        if (tr.Status != TravelRequests.Domain.Enums.RequestStatus.Pending)
        {
            response.Errors = new ErrorResponse(400, "Only pending requests can be approved or rejected");
            return response;
        }

        tr.Status = targetStatus;
        _repo.MarkAsModified(tr);
        await _repo.SaveAsync();

        response.Message = "OK";
        response.Result = MapToResponse(tr);
        return response;
    }

    private static TravelRequestResponseDto MapToResponse(TravelRequest tr)
    {
        var dto = new TravelRequestResponseDto
        {
            Id = tr.Id,
            OriginCity = tr.OriginCity,
            DestinationCity = tr.DestinationCity,
            StartDate = tr.StartDate,
            EndDate = tr.EndDate,
            Justification = tr.Justification,
            Status = tr.Status,
            Category = tr.Category
        };

        if (!string.IsNullOrWhiteSpace(tr.RiskReportJson))
        {
            try
            {
                var risk = JsonSerializer.Deserialize<RiskReport>(tr.RiskReportJson);
                if (risk != null)
                {
                    dto.RiskScore = risk.Score;
                    dto.RiskLevel = risk.RiskLevel;
                }
            }
            catch
            {
            }
        }

        return dto;
    }
}
