using FluentValidation;

namespace EWS.Application.Features.Workflows.Commands.ApproveWorkflow;

public class ApproveWorkflowValidator : AbstractValidator<ApproveWorkflowCommand>
{
    public ApproveWorkflowValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty();
        RuleFor(x => x.ActorPositionCode).NotEmpty();
        RuleFor(x => x.ActorEmployeeId).NotEmpty();
    }
}
