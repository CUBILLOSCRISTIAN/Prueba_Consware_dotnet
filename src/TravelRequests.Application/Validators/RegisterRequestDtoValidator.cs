using FluentValidation;
using TravelRequests.Domain.Dto.Auth;
using TravelRequests.Domain.Enums;

namespace TravelRequests.Application.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => Enum.TryParse<Role>(role, true, out _))
            .WithMessage("Role must be Requester, Approver, Admin or Owner");
    }
}