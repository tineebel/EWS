using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Commands.ReEscalateWorkflow;

public class ReEscalateWorkflowHandler(
    IAppDbContext db,
    IApproverResolver resolver,
    IDateTimeService clock)
    : IRequestHandler<ReEscalateWorkflowCommand, Result<ReEscalateWorkflowDto>>
{
    public async Task<Result<ReEscalateWorkflowDto>> Handle(
        ReEscalateWorkflowCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        var instance = await db.WorkflowInstances
            .FirstOrDefaultAsync(x => x.InstanceId == request.InstanceId, ct);

        if (instance == null)
            return Result<ReEscalateWorkflowDto>.Fail("WF_INSTANCE_NOT_FOUND",
                "Workflow instance not found.");

        if (instance.Status != WorkflowStatus.Pending)
            return Result<ReEscalateWorkflowDto>.Fail("WF_INSTANCE_NOT_PENDING",
                $"Instance is '{instance.Status}'. Only Pending instances can be re-escalated.");

        // ดึง Admin position สำหรับ Audit
        var adminPos = await db.Positions
            .Where(p => p.PositionCode == request.RequestedByPositionCode && p.IsActive)
            .Select(p => new { p.PositionId })
            .FirstOrDefaultAsync(ct);

        if (adminPos == null)
            return Result<ReEscalateWorkflowDto>.Fail("WF_POSITION_NOT_FOUND",
                $"Position '{request.RequestedByPositionCode}' not found.");

        // ดึง Pending approvals ทุก step
        var pendingApprovals = await db.WorkflowApprovals
            .Where(a => a.InstanceId == request.InstanceId && a.Status == ApprovalStatus.Pending)
            .Include(a => a.AssignedPosition)
            .OrderBy(a => a.StepOrder)
            .ToListAsync(ct);

        if (pendingApprovals.Count == 0)
            return Result<ReEscalateWorkflowDto>.Fail("WF_NO_PENDING_STEP",
                "No pending approval steps to re-escalate.");

        var changes  = new List<ReEscalatedStepDto>();
        var stuckSteps = new List<ReEscalatedStepDto>();

        foreach (var approval in pendingApprovals)
        {
            var oldPos = approval.AssignedPosition;

            // Re-resolve: ถ้าตำแหน่งยังมีคนครองอยู่ ไม่ต้องเปลี่ยน
            var resolved = await resolver.EscalateFromPositionAsync(approval.AssignedPositionId, ct);

            var oldCode = oldPos.PositionCode;
            var oldName = oldPos.PositionName;

            // Hierarchy หมดแล้ว — ไม่มีผู้อนุมัติในสาย → mark Stuck
            if (resolved == null)
            {
                approval.Status    = ApprovalStatus.Stuck;
                approval.UpdatedAt = now;
                approval.UpdatedBy = $"RE-ESCALATE:{request.RequestedByPositionCode}";

                stuckSteps.Add(new ReEscalatedStepDto(
                    approval.StepOrder,
                    approval.Step?.StepName ?? $"Step {approval.StepOrder}",
                    oldCode, oldName, oldCode, oldName,
                    WasEscalated: false, IsStuck: true));
                continue;
            }

            if (resolved.PositionId == approval.AssignedPositionId && !resolved.IsVacant)
                continue; // ตำแหน่งเดิมยังมีคน ไม่ต้องเปลี่ยน

            // Update AssignedPosition → ตำแหน่งใหม่ที่ escalate ไป
            approval.AssignedPositionId      = resolved.PositionId;
            approval.EscalatedFromPositionId = oldPos.PositionId;
            approval.UpdatedAt = now;
            approval.UpdatedBy = $"RE-ESCALATE:{request.RequestedByPositionCode}";

            changes.Add(new ReEscalatedStepDto(
                approval.StepOrder,
                approval.Step?.StepName ?? $"Step {approval.StepOrder}",
                oldCode, oldName,
                resolved.PositionCode, resolved.PositionName,
                resolved.WasEscalated, IsStuck: false));
        }

        var allChanges = changes.Concat(stuckSteps).OrderBy(c => c.StepOrder).ToList();

        if (allChanges.Count == 0)
            return Result<ReEscalateWorkflowDto>.Success(new ReEscalateWorkflowDto(
                instance.InstanceId, instance.DocumentNo, 0, 0, allChanges));
            // ไม่มีการเปลี่ยน → ตำแหน่งทั้งหมดยังมีคนครองอยู่

        // ถ้ามี Stuck step → instance เป็น Blocked
        if (stuckSteps.Count > 0)
        {
            instance.Status    = WorkflowStatus.Blocked;
            instance.UpdatedAt = now;
            instance.UpdatedBy = $"RE-ESCALATE:{request.RequestedByPositionCode}";
        }
        else
        {
            instance.UpdatedAt = now;
            instance.UpdatedBy = request.RequestedByPositionCode;
        }

        // บันทึก History
        var comment = new System.Text.StringBuilder();
        if (changes.Count > 0)
            comment.Append($"Re-escalated {changes.Count} step(s): " +
                string.Join(", ", changes.Select(c => $"Step{c.StepOrder} {c.OldPositionCode}→{c.NewPositionCode}")));
        if (stuckSteps.Count > 0)
        {
            if (comment.Length > 0) comment.Append(". ");
            comment.Append($"STUCK {stuckSteps.Count} step(s) (hierarchy exhausted): " +
                string.Join(", ", stuckSteps.Select(s => $"Step{s.StepOrder} {s.OldPositionCode}")));
        }

        db.WorkflowHistories.Add(new WorkflowHistory
        {
            InstanceId      = instance.InstanceId,
            EventType       = stuckSteps.Count > 0 ? "ReEscalate:Blocked" : "ReEscalate",
            ActorPositionId = adminPos.PositionId,
            Comment         = comment.ToString(),
            OccurredAt      = now
        });

        await db.SaveChangesAsync(ct);

        return Result<ReEscalateWorkflowDto>.Success(new ReEscalateWorkflowDto(
            instance.InstanceId, instance.DocumentNo, changes.Count, stuckSteps.Count, allChanges));
    }
}
