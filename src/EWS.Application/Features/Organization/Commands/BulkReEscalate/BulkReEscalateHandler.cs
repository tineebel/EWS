using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Organization.Commands.BulkReEscalate;

public class BulkReEscalateHandler(
    IAppDbContext db,
    IApproverResolver resolver,
    IDateTimeService clock)
    : IRequestHandler<BulkReEscalateCommand, Result<BulkReEscalateDto>>
{
    public async Task<Result<BulkReEscalateDto>> Handle(
        BulkReEscalateCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        // ดึงข้อมูลพนักงาน
        var employee = await db.Employees
            .Where(e => e.EmployeeCode == request.EmployeeCode)
            .Select(e => new { e.EmployeeId, e.EmployeeName })
            .FirstOrDefaultAsync(ct);

        if (employee == null)
            return Result<BulkReEscalateDto>.Fail("EMP_NOT_FOUND",
                $"Employee '{request.EmployeeCode}' not found.");

        // Admin position สำหรับ Audit
        var adminPos = await db.Positions
            .Where(p => p.PositionCode == request.RequestedByPositionCode && p.IsActive)
            .Select(p => new { p.PositionId })
            .FirstOrDefaultAsync(ct);

        if (adminPos == null)
            return Result<BulkReEscalateDto>.Fail("WF_POSITION_NOT_FOUND",
                $"Position '{request.RequestedByPositionCode}' not found.");

        // หาตำแหน่งทั้งหมดที่พนักงานนี้ครองอยู่ (active assignments)
        var positionIds = await db.PositionAssignments
            .Where(a => a.EmployeeId == employee.EmployeeId && a.IsActive)
            .Select(a => a.PositionId)
            .ToListAsync(ct);

        if (positionIds.Count == 0)
            return Result<BulkReEscalateDto>.Success(new BulkReEscalateDto(
                request.EmployeeCode, employee.EmployeeName, 0, 0, 0, []));

        // หา Pending Approvals ทุกตัวที่ assign ให้ตำแหน่งของพนักงานคนนี้
        var pendingApprovals = await db.WorkflowApprovals
            .Where(a => positionIds.Contains(a.AssignedPositionId)
                && a.Status == ApprovalStatus.Pending)
            .Include(a => a.AssignedPosition)
            .Include(a => a.Instance)
            .Include(a => a.Step)
            .OrderBy(a => a.InstanceId).ThenBy(a => a.StepOrder)
            .ToListAsync(ct);

        if (pendingApprovals.Count == 0)
            return Result<BulkReEscalateDto>.Success(new BulkReEscalateDto(
                request.EmployeeCode, employee.EmployeeName, 0, 0, 0, []));

        var details          = new List<BulkReEscalateInstanceDto>();
        var instances        = new HashSet<Guid>();
        var blockedInstances = new HashSet<Guid>();

        foreach (var approval in pendingApprovals)
        {
            // Skip ถ้า instance ไม่ใช่ Pending หรือ Blocked
            if (approval.Instance.Status != WorkflowStatus.Pending &&
                approval.Instance.Status != WorkflowStatus.Blocked) continue;

            var oldPos   = approval.AssignedPosition;
            var oldCode  = oldPos.PositionCode;
            var resolved = await resolver.EscalateFromPositionAsync(approval.AssignedPositionId, ct);

            // Hierarchy หมดแล้ว → Stuck
            if (resolved == null)
            {
                approval.Status    = ApprovalStatus.Stuck;
                approval.UpdatedAt = now;
                approval.UpdatedBy = $"BULK-RE-ESCALATE:{request.RequestedByPositionCode}";

                blockedInstances.Add(approval.InstanceId);
                instances.Add(approval.InstanceId);

                details.Add(new BulkReEscalateInstanceDto(
                    approval.InstanceId,
                    approval.Instance.DocumentNo,
                    approval.StepOrder,
                    approval.Step?.StepName ?? $"Step {approval.StepOrder}",
                    oldCode, oldCode, oldPos.PositionName, IsStuck: true));

                approval.Instance.Status    = WorkflowStatus.Blocked;
                approval.Instance.UpdatedAt = now;
                approval.Instance.UpdatedBy = $"BULK-RE-ESCALATE:{request.EmployeeCode}";

                db.WorkflowHistories.Add(new WorkflowHistory
                {
                    InstanceId      = approval.InstanceId,
                    EventType       = "ReEscalate:Stuck",
                    ActorPositionId = adminPos.PositionId,
                    Comment         = $"Step {approval.StepOrder} STUCK — hierarchy exhausted at '{oldCode}'. " +
                                      $"Manual reassignment required. (Employee: '{request.EmployeeCode}')",
                    OccurredAt      = now
                });
                continue;
            }

            if (resolved.PositionId == approval.AssignedPositionId && !resolved.IsVacant)
                continue;

            approval.AssignedPositionId      = resolved.PositionId;
            approval.EscalatedFromPositionId = oldPos.PositionId;
            approval.UpdatedAt = now;
            approval.UpdatedBy = $"BULK-RE-ESCALATE:{request.RequestedByPositionCode}";

            instances.Add(approval.InstanceId);

            details.Add(new BulkReEscalateInstanceDto(
                approval.InstanceId,
                approval.Instance.DocumentNo,
                approval.StepOrder,
                approval.Step?.StepName ?? $"Step {approval.StepOrder}",
                oldCode, resolved.PositionCode, resolved.PositionName, IsStuck: false));

            approval.Instance.UpdatedAt = now;
            approval.Instance.UpdatedBy = $"BULK-RE-ESCALATE:{request.EmployeeCode}";

            db.WorkflowHistories.Add(new WorkflowHistory
            {
                InstanceId      = approval.InstanceId,
                EventType       = "ReEscalate",
                ActorPositionId = adminPos.PositionId,
                Comment         = $"Auto re-escalated due to employee '{request.EmployeeCode}' departure. " +
                                  $"Step {approval.StepOrder}: {oldCode} → {resolved.PositionCode}",
                OccurredAt      = now
            });
        }

        await db.SaveChangesAsync(ct);

        return Result<BulkReEscalateDto>.Success(new BulkReEscalateDto(
            request.EmployeeCode,
            employee.EmployeeName,
            instances.Count,
            details.Count(d => !d.IsStuck),
            details.Count(d => d.IsStuck),
            details));
    }
}
