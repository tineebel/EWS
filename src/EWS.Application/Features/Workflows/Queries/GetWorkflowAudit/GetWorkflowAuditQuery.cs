using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Queries.GetWorkflowAudit;

public record GetWorkflowAuditQuery(Guid InstanceId) : IRequest<Result<List<WorkflowAuditDto>>>;

public record WorkflowAuditDto(
    long HistoryId,
    string EventType,
    int? StepOrder,
    string? ActorPositionCode,
    string? ActorPositionName,
    string? ActorEmployeeName,
    string? Comment,
    DateTime OccurredAt
);
