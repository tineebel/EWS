using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Commands.UpdateWorkflowTemplate;

public record UpdateWorkflowTemplateCommand(
    int TemplateId,
    string FlowDesc,
    string WfScopeType,
    bool HasSpecialItem,
    bool IsUrgent,
    string? Condition1,
    string? Condition2,
    string? Condition3,
    string? Condition4,
    string? Condition5,
    bool IsActive,
    List<UpdateStepDto> Steps,
    string? ChangeNote,
    string ChangedBy) : IRequest<Result<int>>;

public record UpdateStepDto(
    int? StepId,
    int StepOrder,
    string StepName,
    string ApproverType,
    string? SpecificPositionCode,
    int EscalationDays,
    bool IsRequired);
