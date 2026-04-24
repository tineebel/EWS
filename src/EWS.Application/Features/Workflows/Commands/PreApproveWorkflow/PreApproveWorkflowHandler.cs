using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Commands.PreApproveWorkflow;

public class PreApproveWorkflowHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<PreApproveWorkflowCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PreApproveWorkflowCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        var instance = await db.WorkflowInstances
            .FirstOrDefaultAsync(x => x.InstanceId == request.InstanceId, ct);

        if (instance == null)
            return Result<bool>.Fail("WF_INSTANCE_NOT_FOUND", "Workflow instance not found.");

        if (instance.PreApprovalStatus != PreApprovalStatus.Pending)
            return Result<bool>.Fail("WF_PREAPPROVAL_NOT_PENDING",
                $"Pre-approval status is '{instance.PreApprovalStatus}', not Pending.");

        var chiefPos = await db.Positions
            .Where(p => p.PositionCode == request.ChiefPositionCode && p.IsActive)
            .Select(p => new { p.PositionId })
            .FirstOrDefaultAsync(ct);

        if (chiefPos == null)
            return Result<bool>.Fail("WF_POSITION_NOT_FOUND",
                $"Chief position '{request.ChiefPositionCode}' not found.");

        if (instance.PreApprovalChiefPositionId != chiefPos.PositionId)
            return Result<bool>.Fail("WF_UNAUTHORIZED_APPROVER",
                "This position is not the designated chief for pre-approval.");

        // ตรวจสอบว่า ChiefEmployeeId ครอง ChiefPosition จริง
        var hasAssignment = await db.PositionAssignments
            .AnyAsync(a => a.PositionId == chiefPos.PositionId
                && a.EmployeeId == request.ChiefEmployeeId
                && a.IsActive && !a.IsVacant
                && a.StartDate <= now && (a.EndDate == null || a.EndDate >= now), ct);

        if (!hasAssignment)
            return Result<bool>.Fail("WF_UNAUTHORIZED",
                "Employee does not hold the specified chief position.");

        if (request.IsConfirmed)
        {
            // Chief ยืนยัน → Flow จริงเริ่มได้
            instance.PreApprovalStatus = PreApprovalStatus.Confirmed;
            instance.PreApprovalConfirmedAt = now;

            // ตรวจว่ามี Approval Steps รออยู่หรือไม่
            var hasPendingSteps = await db.WorkflowApprovals
                .AnyAsync(a => a.InstanceId == instance.InstanceId
                    && a.Status == ApprovalStatus.Pending, ct);

            if (hasPendingSteps)
            {
                // มี Steps → รอ Approver ต่อไปตามปกติ
                instance.Status = WorkflowStatus.Pending;
            }
            else
            {
                // ไม่มี Steps เพิ่มเติม (Chief is highest authority) → Auto-Approved
                instance.Status = WorkflowStatus.Approved;
                instance.CompletedAt = now;

                db.WorkflowHistories.Add(new WorkflowHistory
                {
                    InstanceId = instance.InstanceId,
                    EventType = "AutoApproved",
                    ActorPositionId = chiefPos.PositionId,
                    ActorEmployeeId = request.ChiefEmployeeId,
                    Comment = "Auto-approved: Chief pre-approval is the final authority (no further approval steps).",
                    OccurredAt = now
                });
            }
        }
        else
        {
            // Chief ปฏิเสธ → Rejected (ไม่ใช่ Cancelled — Cancelled สงวนไว้สำหรับผู้ยื่นถอน)
            instance.PreApprovalStatus = PreApprovalStatus.Rejected;
            instance.Status = WorkflowStatus.Rejected;
            instance.CompletedAt = now;
        }

        instance.UpdatedAt = now;
        instance.UpdatedBy = request.ChiefEmployeeId.ToString();

        db.WorkflowHistories.Add(new WorkflowHistory
        {
            InstanceId = instance.InstanceId,
            EventType = request.IsConfirmed ? "PreApproveConfirm" : "PreApproveReject",
            ActorPositionId = chiefPos.PositionId,
            ActorEmployeeId = request.ChiefEmployeeId,
            Comment = request.Comment,
            OccurredAt = now
        });

        await db.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
