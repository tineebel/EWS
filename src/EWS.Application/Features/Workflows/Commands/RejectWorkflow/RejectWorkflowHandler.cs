using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Commands.RejectWorkflow;

public class RejectWorkflowHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<RejectWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RejectWorkflowCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        var instance = await db.WorkflowInstances
            .FirstOrDefaultAsync(x => x.InstanceId == request.InstanceId, ct);

        if (instance == null)
            return Result<bool>.Fail("WF_INSTANCE_NOT_FOUND", "Workflow instance not found.");

        if (instance.Status != WorkflowStatus.Pending)
            return Result<bool>.Fail("WF_INSTANCE_NOT_PENDING",
                $"Instance is '{instance.Status}', cannot reject.");

        var actorPos = await db.Positions
            .Where(p => p.PositionCode == request.ActorPositionCode && p.IsActive)
            .Select(p => new { p.PositionId })
            .FirstOrDefaultAsync(ct);

        if (actorPos == null)
            return Result<bool>.Fail("WF_POSITION_NOT_FOUND",
                $"Actor position '{request.ActorPositionCode}' not found.");

        var currentApproval = await db.WorkflowApprovals
            .Where(a => a.InstanceId == request.InstanceId && a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.StepOrder)
            .FirstOrDefaultAsync(ct);

        if (currentApproval == null)
            return Result<bool>.Fail("WF_NO_PENDING_STEP", "No pending approval step found.");

        bool isAssigned = actorPos.PositionId == currentApproval.AssignedPositionId;
        if (!isAssigned)
        {
            var delegation = await db.Delegations
                .AnyAsync(d => d.ToPositionId == actorPos.PositionId
                    && d.FromPositionId == currentApproval.AssignedPositionId
                    && d.IsActive && d.StartDate <= now && d.EndDate >= now, ct);

            if (!delegation)
                return Result<bool>.Fail("WF_UNAUTHORIZED_APPROVER",
                    "Actor does not have permission to reject this step.");
        }

        currentApproval.Status = ApprovalStatus.Rejected;
        currentApproval.ActorEmployeeId = request.ActorEmployeeId;
        currentApproval.Comment = request.Comment;
        currentApproval.ActionAt = now;
        currentApproval.UpdatedAt = now;
        currentApproval.UpdatedBy = request.ActorEmployeeId.ToString();

        instance.Status = WorkflowStatus.Rejected;
        instance.CompletedAt = now;
        instance.UpdatedAt = now;
        instance.UpdatedBy = request.ActorEmployeeId.ToString();

        db.WorkflowHistories.Add(new WorkflowHistory
        {
            InstanceId = instance.InstanceId,
            EventType = "Reject",
            StepOrder = currentApproval.StepOrder,
            ActorPositionId = actorPos.PositionId,
            ActorEmployeeId = request.ActorEmployeeId,
            Comment = request.Comment,
            OccurredAt = now
        });

        await db.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
