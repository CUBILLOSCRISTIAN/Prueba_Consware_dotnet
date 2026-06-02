using TravelRequests.Domain.Dto.Travel;
using TravelRequests.Domain.Entities;
using TravelRequests.Domain.Repository;
using TravelRequests.Domain.Services;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Application.Services;

public class TravelRequestService : ITravelRequestService
{
    private readonly ITravelRequestRepository _repo;
    private readonly ITravelRiskService _riskService;
    private readonly ITravelClassifierService _classifier;

    public TravelRequestService(ITravelRequestRepository repo, ITravelRiskService riskService, ITravelClassifierService classifier)
    {
        _repo = repo;
        _riskService = riskService;
        _classifier = classifier;
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

    public async Task<ResponsePackage<List<TravelRequestResponseDto>>> GetByUserAsync(Guid userId)
    {
        var response = new ResponsePackage<List<TravelRequestResponseDto>>();
        var items = await _repo.GetByUserAsync(userId);
        response.Result = items.Select(tr => new TravelRequestResponseDto
        {
            Id = tr.Id,
            OriginCity = tr.OriginCity,
            DestinationCity = tr.DestinationCity,
            StartDate = tr.StartDate,
            EndDate = tr.EndDate,
            Justification = tr.Justification,
            Status = tr.Status,
            Category = tr.Category
        }).ToList();
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
        response.Result = new TravelRequestResponseDto
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
        return response;
    }
}
