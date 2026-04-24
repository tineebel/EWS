using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Commands.ApproveWorkflow;

public record ApproveWorkflowCommand(
    Guid InstanceId,
    string ActorPositionCode,
    Guid ActorEmployeeId,
    string? Comment
) : IRequest<Result<ApproveWorkflowDto>>;

public record ApproveWorkflowDto(
    Guid InstanceId,
    string DocumentNo,
    int CompletedStep,
    bool IsCompleted,
    string Status,
    string? NextApproverPositionCode
);
