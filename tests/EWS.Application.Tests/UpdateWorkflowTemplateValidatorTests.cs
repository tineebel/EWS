using EWS.Application.Features.Settings.Commands.UpdateWorkflowTemplate;

namespace EWS.Application.Tests;

public class UpdateWorkflowTemplateValidatorTests
{
    [Fact]
    public void Validate_InvalidCondition_ReturnsValidationError()
    {
        var validator = new UpdateWorkflowTemplateValidator();
        var command = ValidCommand() with { Condition1 = "> amount" };

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateWorkflowTemplateCommand.Condition1));
    }

    [Fact]
    public void Validate_ValidAmountCondition_PassesConditionValidation()
    {
        var validator = new UpdateWorkflowTemplateValidator();
        var command = ValidCommand() with { Condition1 = "> 1000" };

        var result = validator.Validate(command);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(UpdateWorkflowTemplateCommand.Condition1));
    }

    private static UpdateWorkflowTemplateCommand ValidCommand()
    {
        return new UpdateWorkflowTemplateCommand(
            TemplateId: 1,
            FlowDesc: "PO approval",
            WfScopeType: "Ho",
            HasSpecialItem: false,
            IsUrgent: false,
            Condition1: null,
            Condition2: null,
            Condition3: null,
            Condition4: null,
            Condition5: null,
            IsActive: true,
            Steps:
            [
                new UpdateStepDto(
                    StepId: null,
                    StepOrder: 1,
                    StepName: "Manager",
                    ApproverType: "DirectSupervisor",
                    SpecificPositionCode: null,
                    EscalationDays: 0,
                    IsRequired: true)
            ],
            ChangeNote: null,
            ChangedBy: "test");
    }
}
