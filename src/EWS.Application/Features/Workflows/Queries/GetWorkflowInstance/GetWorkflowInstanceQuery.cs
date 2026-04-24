using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Queries.GetWorkflowInstance;

public record GetWorkflowInstanceQuery(Guid InstanceId) : IRequest<Result<WorkflowInstanceDetailDto>>;

public record WorkflowInstanceDetailDto(
    Guid InstanceId,
    string DocumentNo,
    int DocCode,
    string DocName,
    int FlowCode,
    string FlowDesc,
    string Status,
    string PreApprovalStatus,
    decimal? TotalAmount,
    bool IsSpecialItem,
    bool IsUrgent,
    string? Subject,
    string SubmitterPositionCode,
    string SubmitterPositionName,
    DateTime SubmittedAt,
    DateTime? CompletedAt,
    int CurrentStepOrder,
    string? CurrentApproverPositionCode,
    List<WorkflowApprovalStepDto> Steps
);

public record WorkflowApprovalStepDto(
    int StepOrder,
    string StepName,
    string ApproverPositionCode,
    string ApproverPositionName,
    string? OccupantName,
    string Status,
    string? ActorName,
    DateTime? ActionAt,
    string? Comment
);
