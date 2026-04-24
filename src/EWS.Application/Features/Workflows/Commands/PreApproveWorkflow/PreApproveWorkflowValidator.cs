using FluentValidation;

namespace EWS.Application.Features.Workflows.Commands.PreApproveWorkflow;

public class PreApproveWorkflowValidator : AbstractValidator<PreApproveWorkflowCommand>
{
    public PreApproveWorkflowValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty();
        RuleFor(x => x.ChiefPositionCode).NotEmpty();
        RuleFor(x => x.ChiefEmployeeId).NotEmpty();
    }
}
