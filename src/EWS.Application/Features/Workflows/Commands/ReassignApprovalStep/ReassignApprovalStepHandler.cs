using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Commands.ReassignApprovalStep;

public class ReassignApprovalStepHandler(
    IAppDbContext db,
    IDateTimeService clock)
    : IRequestHandler<ReassignApprovalStepCommand, Result<ReassignApprovalStepDto>>
{
    public async Task<Result<ReassignApprovalStepDto>> Handle(
        ReassignApprovalStepCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        // ดึง Instance
        var instance = await db.WorkflowInstances
            .FirstOrDefaultAsync(x => x.InstanceId == request.InstanceId, ct);

        if (instance == null)
            return Result<ReassignApprovalStepDto>.Fail("WF_INSTANCE_NOT_FOUND",
                "Workflow instance not found.");

        if (instance.Status != WorkflowStatus.Blocked && instance.Status != WorkflowStatus.Pending)
            return Result<ReassignApprovalStepDto>.Fail("WF_INSTANCE_NOT_ACTIONABLE",
                $"Instance is '{instance.Status}'. Only Blocked or Pending instances can be reassigned.");

        // ดึง Approval Step ที่ต้องการ reassign
        var approval = await db.WorkflowApprovals
            .Include(a => a.AssignedPosition)
            .Include(a => a.Step)
            .FirstOrDefaultAsync(a =>
                a.InstanceId == request.InstanceId &&
                a.StepOrder  == request.StepOrder  &&
                (a.Status == ApprovalStatus.Stuck || a.Status == ApprovalStatus.Pending), ct);

        if (approval == null)
            return Result<ReassignApprovalStepDto>.Fail("WF_STEP_NOT_FOUND",
                $"Step {request.StepOrder} not found or is not Stuck/Pending.");

        // ดึงตำแหน่งใหม่ที่ Admin กำหนด
        var newPos = await db.Positions
            .Where(p => p.PositionCode == request.TargetPositionCode && p.IsActive)
            .Select(p => new { p.PositionId, p.PositionCode, p.PositionName })
            .FirstOrDefaultAsync(ct);

        if (newPos == null)
            return Result<ReassignApprovalStepDto>.Fail("WF_POSITION_NOT_FOUND",
                $"Position '{request.TargetPositionCode}' not found or inactive.");

        // ตรวจ Admin position สำหรับ Audit
        var adminPos = await db.Positions
            .Where(p => p.PositionCode == request.RequestedByPositionCode && p.IsActive)
            .Select(p => new { p.PositionId })
            .FirstOrDefaultAsync(ct);

        if (adminPos == null)
            return Result<ReassignApprovalStepDto>.Fail("WF_POSITION_NOT_FOUND",
                $"Admin position '{request.RequestedByPositionCode}' not found.");

        var oldPos  = approval.AssignedPosition;
        var oldCode = oldPos.PositionCode;

        // Reassign step
        approval.EscalatedFromPositionId = approval.AssignedPositionId;
        approval.AssignedPositionId      = newPos.PositionId;
        approval.Status    = ApprovalStatus.Pending; // unblock
        approval.UpdatedAt = now;
        approval.UpdatedBy = $"REASSIGN:{request.RequestedByPositionCode}";

        // ถ้า Instance เป็น Blocked ให้เช็คว่ายังมี Stuck step อื่นอยู่ไหม
        if (instance.Status == WorkflowStatus.Blocked)
        {
            var hasOtherStuck = await db.WorkflowApprovals
                .AnyAsync(a => a.InstanceId == request.InstanceId &&
                               a.StepOrder  != request.StepOrder  &&
                               a.Status     == ApprovalStatus.Stuck, ct);

            instance.Status = hasOtherStuck ? WorkflowStatus.Blocked : WorkflowStatus.Pending;
        }

        instance.UpdatedAt = now;
        instance.UpdatedBy = $"REASSIGN:{request.RequestedByPositionCode}";

        // Audit
        db.WorkflowHistories.Add(new WorkflowHistory
        {
            InstanceId      = instance.InstanceId,
            EventType       = "Reassign",
            ActorPositionId = adminPos.PositionId,
            Comment         = $"Step {request.StepOrder} manually reassigned: {oldCode} → {newPos.PositionCode}." +
                              (string.IsNullOrWhiteSpace(request.Reason) ? "" : $" Reason: {request.Reason}"),
            OccurredAt      = now
        });

        await db.SaveChangesAsync(ct);

        return Result<ReassignApprovalStepDto>.Success(new ReassignApprovalStepDto(
            instance.InstanceId,
            instance.DocumentNo,
            approval.StepOrder,
            approval.Step?.StepName ?? $"Step {approval.StepOrder}",
            oldCode,
            newPos.PositionCode,
            newPos.PositionName));
    }
}
