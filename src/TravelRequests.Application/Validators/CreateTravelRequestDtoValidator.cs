using FluentValidation;
using TravelRequests.Domain.Dto.Travel;

namespace TravelRequests.Application.Validators;

public class CreateTravelRequestDtoValidator : AbstractValidator<CreateTravelRequestDto>
{
    public CreateTravelRequestDtoValidator()
    {
        RuleFor(x => x.OriginCity)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.DestinationCity)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
            .NotEqual(x => x.OriginCity)
            .WithMessage("DestinationCity must be different from OriginCity");

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("EndDate must be after StartDate");

        RuleFor(x => x.Justification)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(1000);
    }
}