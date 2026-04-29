using EWS.Application.Common;
using EWS.Domain.Enums;
using FluentValidation;

namespace EWS.Application.Features.Settings.Commands.UpdateWorkflowTemplate;

public class UpdateWorkflowTemplateValidator : AbstractValidator<UpdateWorkflowTemplateCommand>
{
    public UpdateWorkflowTemplateValidator()
    {
        RuleFor(x => x.FlowDesc)
            .NotEmpty().WithMessage("FlowDesc is required.")
            .MaximumLength(200).WithMessage("FlowDesc must not exceed 200 characters.");

        RuleFor(x => x.WfScopeType)
            .NotEmpty()
            .Must(v => Enum.TryParse<WfScopeType>(v, out _))
            .WithMessage("WfScopeType must be a valid value: All, Branch, Ho.");

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("At least one step is required.");

        RuleFor(x => x.Condition1)
            .Must(WorkflowConditionEvaluator.IsValid)
            .WithMessage("Condition1 must be empty, NULL, or a valid amount condition such as > 1000, <= 5000, or = 0.");
        RuleFor(x => x.Condition2)
            .Must(WorkflowConditionEvaluator.IsValid)
            .WithMessage("Condition2 must be empty, NULL, or a valid amount condition such as > 1000, <= 5000, or = 0.");
        RuleFor(x => x.Condition3)
            .Must(WorkflowConditionEvaluator.IsValid)
            .WithMessage("Condition3 must be empty, NULL, or a valid amount condition such as > 1000, <= 5000, or = 0.");
        RuleFor(x => x.Condition4)
            .Must(WorkflowConditionEvaluator.IsValid)
            .WithMessage("Condition4 must be empty, NULL, or a valid amount condition such as > 1000, <= 5000, or = 0.");
        RuleFor(x => x.Condition5)
            .Must(WorkflowConditionEvaluator.IsValid)
            .WithMessage("Condition5 must be empty, NULL, or a valid amount condition such as > 1000, <= 5000, or = 0.");

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.StepName)
                .NotEmpty().WithMessage("StepName is required.");

            step.RuleFor(s => s.StepOrder)
                .GreaterThan(0).WithMessage("StepOrder must be greater than 0.");

            step.RuleFor(s => s.ApproverType)
                .NotEmpty()
                .Must(v => Enum.TryParse<ApproverType>(v, out _))
                .WithMessage("ApproverType must be a valid enum value.");

            step.RuleFor(s => s.SpecificPositionCode)
                .NotEmpty()
                .When(s => s.ApproverType == nameof(ApproverType.SpecificPosition))
                .WithMessage("SpecificPositionCode is required when ApproverType is SpecificPosition.");

            step.RuleFor(s => s.EscalationDays)
                .GreaterThanOrEqualTo(0).WithMessage("EscalationDays must be >= 0.");
        });

        RuleFor(x => x.Steps)
            .Must(steps => steps.Select(s => s.StepOrder).Distinct().Count() == steps.Count)
            .WithMessage("StepOrder must be unique within the step list.");
    }
}
