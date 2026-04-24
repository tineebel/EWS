using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Queries.GetWorkflowAudit;

public class GetWorkflowAuditHandler(IAppDbContext db)
    : IRequestHandler<GetWorkflowAuditQuery, Result<List<WorkflowAuditDto>>>
{
    public async Task<Result<List<WorkflowAuditDto>>> Handle(
        GetWorkflowAuditQuery request, CancellationToken ct)
    {
        var exists = await db.WorkflowInstances.AnyAsync(x => x.InstanceId == request.InstanceId, ct);
        if (!exists)
            return Result<List<WorkflowAuditDto>>.Fail("WF_INSTANCE_NOT_FOUND", "Instance not found.");

        var history = await db.WorkflowHistories
            .Where(h => h.InstanceId == request.InstanceId)
            .OrderBy(h => h.OccurredAt)
            .Select(h => new WorkflowAuditDto(
                h.HistoryId, h.EventType, h.StepOrder,
                h.ActorPosition != null ? h.ActorPosition.PositionCode : null,
                h.ActorPosition != null ? h.ActorPosition.PositionName : null,
                h.ActorEmployee != null ? h.ActorEmployee.EmployeeName : null,
                h.Comment, h.OccurredAt
            ))
            .ToListAsync(ct);

        return Result<List<WorkflowAuditDto>>.Success(history);
    }
}
