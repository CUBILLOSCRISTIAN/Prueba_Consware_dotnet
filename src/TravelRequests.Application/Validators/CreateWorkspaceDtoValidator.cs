using FluentValidation;
using TravelRequests.Domain.Dto.Workspace;

namespace TravelRequests.Application.Validators;

public class CreateWorkspaceDtoValidator : AbstractValidator<CreateWorkspaceDto>
{
    public CreateWorkspaceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(120);
    }
}