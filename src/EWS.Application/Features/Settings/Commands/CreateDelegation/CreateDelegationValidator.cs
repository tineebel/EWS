using FluentValidation;

namespace EWS.Application.Features.Settings.Commands.CreateDelegation;

public class CreateDelegationValidator : AbstractValidator<CreateDelegationCommand>
{
    public CreateDelegationValidator()
    {
        RuleFor(x => x.FromPositionCode)
            .NotEmpty().MaximumLength(30);

        RuleFor(x => x.ToPositionCode)
            .NotEmpty().MaximumLength(30);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}
