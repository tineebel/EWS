using FluentValidation;

namespace EWS.Application.Features.Workflows.Commands.RejectWorkflow;

public class RejectWorkflowValidator : AbstractValidator<RejectWorkflowCommand>
{
    public RejectWorkflowValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty();
        RuleFor(x => x.ActorPositionCode).NotEmpty();
        RuleFor(x => x.ActorEmployeeId).NotEmpty();
        RuleFor(x => x.Comment).NotEmpty().WithMessage("กรุณาระบุเหตุผลการปฏิเสธ");
    }
}
