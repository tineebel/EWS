using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Commands.RejectWorkflow;

public record RejectWorkflowCommand(
    Guid InstanceId,
    string ActorPositionCode,
    Guid ActorEmployeeId,
    string Comment
) : IRequest<Result<bool>>;
