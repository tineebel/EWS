using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Commands.ApproveWorkflow;

public class ApproveWorkflowHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<ApproveWorkflowCommand, Result<ApproveWorkflowDto>>
{
    public async Task<Result<ApproveWorkflowDto>> Handle(ApproveWorkflowCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        var instance = await db.WorkflowInstances
            .FirstOrDefaultAsync(x => x.InstanceId == request.InstanceId, ct);

        if (instance == null)
            return Result<ApproveWorkflowDto>.Fail("WF_INSTANCE_NOT_FOUND", "Workflow instance not found.");

        if (instance.Status != WorkflowStatus.Pending)
            return Result<ApproveWorkflowDto>.Fail("WF_INSTANCE_NOT_PENDING",
                $"Instance is '{instance.Status}', cannot approve.");

        if (instance.PreApprovalStatus == PreApprovalStatus.Pending)
            return Result<ApproveWorkflowDto>.Fail("WF_PREAPPROVAL_REQUIRED",
                "Chief must confirm the document before approval flow starts.");

        // หา current pending step (step เล็กสุดที่ยัง Pending)
        var currentApproval = await db.WorkflowApprovals
            .Where(a => a.InstanceId == request.InstanceId && a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.StepOrder)
            .FirstOrDefaultAsync(ct);

        if (currentApproval == null)
            return Result<ApproveWorkflowDto>.Fail("WF_NO_PENDING_STEP", "No pending approval step found.");

        // ตรวจสอบ Actor Position
        var actorPos = await db.Positions
            .Where(p => p.PositionCode == request.ActorPositionCode && p.IsActive)
            .Select(p => new { p.PositionId })
            .FirstOrDefaultAsync(ct);

        if (actorPos == null)
            return Result<ApproveWorkflowDto>.Fail("WF_POSITION_NOT_FOUND",
                $"Actor position '{request.ActorPositionCode}' not found.");

        // ตรวจสอบว่า Actor มีสิทธิ์: ตรงกับ AssignedPosition หรือมี Delegation
        bool isAssigned = actorPos.PositionId == currentApproval.AssignedPositionId;
        int? actingAsPositionId = null;

        if (!isAssigned)
        {
            var delegation = await db.Delegations
                .Where(d => d.ToPositionId == actorPos.PositionId
                    && d.FromPositionId == currentApproval.AssignedPositionId
                    && d.IsActive && d.StartDate <= now && d.EndDate >= now)
                .FirstOrDefaultAsync(ct);

            if (delegation == null)
                return Result<ApproveWorkflowDto>.Fail("WF_UNAUTHORIZED_APPROVER",
                    "Actor does not have permission to approve this step.");

            actingAsPositionId = actorPos.PositionId;
        }

        // Auto-cancel info requests ที่ค้างอยู่ของ Step นี้ (ไม่ว่าจะถามใครไว้ก็ตาม)
        var openInfoRequests = await db.WorkflowInfoRequests
            .Where(r => r.InstanceId    == request.InstanceId
                     && r.FromStepOrder == currentApproval.StepOrder
                     && (r.Status == InfoRequestStatus.Open || r.Status == InfoRequestStatus.Forwarded))
            .ToListAsync(ct);

        foreach (var ir in openInfoRequests)
        {
            ir.Status    = InfoRequestStatus.Cancelled;
            ir.UpdatedAt = now;
            ir.UpdatedBy = $"AUTO-CANCEL:APPROVED:{request.ActorPositionCode}";

            // Cancel child ที่ยังค้างด้วย (กรณี forward chain)
            if (ir.ChildInfoRequestId.HasValue)
            {
                var child = await db.WorkflowInfoRequests
                    .FirstOrDefaultAsync(c => c.InfoRequestId == ir.ChildInfoRequestId, ct);
                if (child != null &&
                    (child.Status == InfoRequestStatus.Open || child.Status == InfoRequestStatus.Forwarded))
                {
                    child.Status    = InfoRequestStatus.Cancelled;
                    child.UpdatedAt = now;
                    child.UpdatedBy = $"AUTO-CANCEL:PARENT-APPROVED:{request.ActorPositionCode}";
                }
            }
        }

        // Approve this step
        currentApproval.Status = ApprovalStatus.Approved;
        currentApproval.ActorEmployeeId = request.ActorEmployeeId;
        currentApproval.ActorActingAsPositionId = actingAsPositionId;
        currentApproval.Comment = request.Comment;
        currentApproval.ActionAt = now;
        currentApproval.UpdatedAt = now;
        currentApproval.UpdatedBy = request.ActorEmployeeId.ToString();

        // Check next step
        var nextApproval = await db.WorkflowApprovals
            .Where(a => a.InstanceId == request.InstanceId
                && a.StepOrder > currentApproval.StepOrder
                && a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.StepOrder)
            .Select(a => new { a.AssignedPositionId, PositionCode = a.AssignedPosition.PositionCode })
            .FirstOrDefaultAsync(ct);

        bool isCompleted = nextApproval == null;
        if (isCompleted)
        {
            instance.Status = WorkflowStatus.Approved;
            instance.CompletedAt = now;
        }

        instance.UpdatedAt = now;
        instance.UpdatedBy = request.ActorEmployeeId.ToString();

        // Insert History
        db.WorkflowHistories.Add(new WorkflowHistory
        {
            InstanceId = instance.InstanceId,
            EventType = isCompleted ? "Complete" : "Approve",
            StepOrder = currentApproval.StepOrder,
            ActorPositionId = actorPos.PositionId,
            ActorEmployeeId = request.ActorEmployeeId,
            Comment = request.Comment,
            OccurredAt = now
        });

        await db.SaveChangesAsync(ct);

        return Result<ApproveWorkflowDto>.Success(new ApproveWorkflowDto(
            instance.InstanceId,
            instance.DocumentNo,
            currentApproval.StepOrder,
            isCompleted,
            instance.Status.ToString(),
            nextApproval?.PositionCode));
    }
}
