using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.ListWorkflowTemplates;

public record ListWorkflowTemplatesQuery(int? DocCode, bool? IsActive)
    : IRequest<Result<List<WorkflowTemplateDto>>>;

public record WorkflowTemplateDto(
    int TemplateId,
    int DocCode,
    string DocName,
    int FlowCode,
    string FlowDesc,
    string WfScopeType,
    bool HasSpecialItem,
    bool IsUrgent,
    string? Condition1,
    string? Condition2,
    bool IsActive,
    List<WorkflowStepDto> Steps);

public record WorkflowStepDto(
    int StepId,
    int StepOrder,
    string StepName,
    string ApproverType,
    string? SpecificPositionCode,
    bool IsRequired);
