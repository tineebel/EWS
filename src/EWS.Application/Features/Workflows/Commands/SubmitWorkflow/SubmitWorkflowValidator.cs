using FluentValidation;

namespace EWS.Application.Features.Workflows.Commands.SubmitWorkflow;

public class SubmitWorkflowValidator : AbstractValidator<SubmitWorkflowCommand>
{
    public SubmitWorkflowValidator()
    {
        RuleFor(x => x.DocCode).GreaterThan(0).WithMessage("DocCode ต้องมากกว่า 0");
        RuleFor(x => x.SubmitterPositionCode).NotEmpty().WithMessage("SubmitterPositionCode ห้ามว่าง");
        RuleFor(x => x.SubmitterEmployeeId).NotEmpty().WithMessage("SubmitterEmployeeId ห้ามว่าง");
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0).When(x => x.TotalAmount.HasValue)
            .WithMessage("TotalAmount ต้อง >= 0");
    }
}
