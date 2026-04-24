using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Commands.PreApproveWorkflow;

public record PreApproveWorkflowCommand(
    Guid InstanceId,
    string ChiefPositionCode,
    Guid ChiefEmployeeId,
    bool IsConfirmed, // true = Confirm, false = Reject
    string? Comment
) : IRequest<Result<bool>>;
