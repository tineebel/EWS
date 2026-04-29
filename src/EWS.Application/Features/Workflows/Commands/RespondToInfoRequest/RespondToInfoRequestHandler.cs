using EWS.Application.Common;
using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Commands.RespondToInfoRequest;

public class RespondToInfoRequestHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<RespondToInfoRequestCommand, Result<RespondToInfoRequestDto>>
{
    public async Task<Result<RespondToInfoRequestDto>> Handle(
        RespondToInfoRequestCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        // ดึง InfoRequest
        var infoRequest = await db.WorkflowInfoRequests
            .Include(r => r.FromPosition)
            .Include(r => r.ToPosition)
            .FirstOrDefaultAsync(r => r.InfoRequestId == request.InfoRequestId, ct);

        if (infoRequest == null)
            return Result<RespondToInfoRequestDto>.Fail("WF_INFO_NOT_FOUND",
                "Info request not found.");

        if (infoRequest.Status != InfoRequestStatus.Open)
            return Result<RespondToInfoRequestDto>.Fail("WF_INFO_NOT_OPEN",
                $"Info request is '{infoRequest.Status}'. Only Open requests can be responded to.");

        // ตรวจว่า actor เป็น ToPosition
        if (infoRequest.ToPosition.PositionCode != request.ActorPositionCode)
            return Result<RespondToInfoRequestDto>.Fail("WF_UNAUTHORIZED",
                $"Position '{request.ActorPositionCode}' is not the recipient of this info request.");

        var hasActorAssignment = await WorkflowActorVerifier.HasActiveAssignmentAsync(
            db, infoRequest.ToPositionId, request.ActorEmployeeId, now, ct);

        if (!hasActorAssignment)
            return Result<RespondToInfoRequestDto>.Fail("WF_UNAUTHORIZED",
                "Employee does not have an active assignment for the actor position.");

        var instance = await db.WorkflowInstances
            .FirstOrDefaultAsync(x => x.InstanceId == infoRequest.InstanceId, ct);

        // --- กรณี 1: Forward ต่อไปยัง Step ก่อนหน้า ---
        if (request.ForwardToStepOrder.HasValue)
        {
            if (string.IsNullOrWhiteSpace(request.ForwardQuestion))
                return Result<RespondToInfoRequestDto>.Fail("WF_INFO_FORWARD_QUESTION_REQUIRED",
                    "ForwardQuestion is required when forwarding.");

            if (request.ForwardToStepOrder.Value >= infoRequest.ToStepOrder)
                return Result<RespondToInfoRequestDto>.Fail("WF_INFO_INVALID_DIRECTION",
                    $"ForwardToStepOrder ({request.ForwardToStepOrder}) must be less than ToStepOrder ({infoRequest.ToStepOrder}).");

            // ตรวจ target step
            var targetApproval = await db.WorkflowApprovals
                .Include(a => a.AssignedPosition)
                .FirstOrDefaultAsync(a =>
                    a.InstanceId == infoRequest.InstanceId &&
                    a.StepOrder  == request.ForwardToStepOrder.Value, ct);

            if (targetApproval == null)
                return Result<RespondToInfoRequestDto>.Fail("WF_STEP_NOT_FOUND",
                    $"Step {request.ForwardToStepOrder} not found in this instance.");

            // ตรวจ: ห้าม forward ถ้ามี open child request อยู่แล้ว
            var hasOpenChild = await db.WorkflowInfoRequests.AnyAsync(r =>
                r.ParentInfoRequestId == infoRequest.InfoRequestId &&
                (r.Status == InfoRequestStatus.Open || r.Status == InfoRequestStatus.Forwarded), ct);
            if (hasOpenChild)
                return Result<RespondToInfoRequestDto>.Fail("WF_INFO_CHILD_OPEN",
                    "There is already an open forwarded request. Wait for it to be answered.");

            // สร้าง child request
            var childRequest = new WorkflowInfoRequest
            {
                InstanceId          = infoRequest.InstanceId,
                FromStepOrder       = infoRequest.ToStepOrder,    // คนที่ forward คือ FromStep ของ child
                FromPositionId      = infoRequest.ToPositionId,
                ToStepOrder         = request.ForwardToStepOrder.Value,
                ToPositionId        = targetApproval.AssignedPositionId,
                Question            = request.ForwardQuestion!,
                Status              = InfoRequestStatus.Open,
                ParentInfoRequestId = infoRequest.InfoRequestId,
                CreatedAt           = now,
                CreatedBy           = request.ActorEmployeeId.ToString()
            };
            db.WorkflowInfoRequests.Add(childRequest);

            // parent เปลี่ยนเป็น Forwarded
            infoRequest.Status    = InfoRequestStatus.Forwarded;
            infoRequest.UpdatedAt = now;
            infoRequest.UpdatedBy = $"FORWARD:{request.ActorPositionCode}";

            db.WorkflowHistories.Add(new WorkflowHistory
            {
                InstanceId      = infoRequest.InstanceId,
                EventType       = "InfoRequest:Forward",
                StepOrder       = infoRequest.ToStepOrder,
                ActorPositionId = infoRequest.ToPositionId,
                ActorEmployeeId = request.ActorEmployeeId,
                Comment         = $"Step {infoRequest.ToStepOrder} forwarded to Step {request.ForwardToStepOrder}: {request.ForwardQuestion}",
                OccurredAt      = now
            });

            await db.SaveChangesAsync(ct);

            var toOccupant = await db.PositionAssignments
                .Where(a => a.PositionId == targetApproval.AssignedPositionId && a.IsActive && !a.IsVacant
                         && a.StartDate <= now && (a.EndDate == null || a.EndDate >= now))
                .Select(a => a.Employee.EmployeeName)
                .FirstOrDefaultAsync(ct);

            return Result<RespondToInfoRequestDto>.Success(new RespondToInfoRequestDto(
                infoRequest.InfoRequestId,
                "Forwarded",
                targetApproval.AssignedPosition.PositionCode,
                toOccupant,
                childRequest.InfoRequestId));
        }

        // --- กรณี 2: ตอบตรง ---
        if (string.IsNullOrWhiteSpace(request.Answer))
            return Result<RespondToInfoRequestDto>.Fail("WF_INFO_ANSWER_REQUIRED",
                "Answer is required when not forwarding.");

        infoRequest.Answer     = request.Answer;
        infoRequest.Status     = InfoRequestStatus.Closed;
        infoRequest.AnsweredAt = now;
        infoRequest.UpdatedAt  = now;
        infoRequest.UpdatedBy  = $"ANSWER:{request.ActorPositionCode}";

        db.WorkflowHistories.Add(new WorkflowHistory
        {
            InstanceId      = infoRequest.InstanceId,
            EventType       = "InfoRequest:Answered",
            StepOrder       = infoRequest.ToStepOrder,
            ActorPositionId = infoRequest.ToPositionId,
            ActorEmployeeId = request.ActorEmployeeId,
            Comment         = $"Step {infoRequest.ToStepOrder} answered Step {infoRequest.FromStepOrder}: {request.Answer}",
            OccurredAt      = now
        });

        // ถ้านี่เป็น child request → parent ต้องกลับมา Open (รอ parent ตอบ FromStep)
        if (infoRequest.ParentInfoRequestId.HasValue)
        {
            var parent = await db.WorkflowInfoRequests
                .FirstOrDefaultAsync(r => r.InfoRequestId == infoRequest.ParentInfoRequestId, ct);
            if (parent != null && parent.Status == InfoRequestStatus.Forwarded)
            {
                parent.Status    = InfoRequestStatus.Open; // กลับมา Open รอ parent ตอบ
                parent.UpdatedAt = now;
                parent.UpdatedBy = $"CHILD-ANSWERED:{request.ActorPositionCode}";

                db.WorkflowHistories.Add(new WorkflowHistory
                {
                    InstanceId      = infoRequest.InstanceId,
                    EventType       = "InfoRequest:ChainResume",
                    StepOrder       = parent.ToStepOrder,
                    ActorPositionId = parent.ToPositionId,
                    Comment         = $"Step {parent.ToStepOrder} received answer from Step {infoRequest.ToStepOrder}. Resume answering Step {parent.FromStepOrder}.",
                    OccurredAt      = now
                });
            }
        }
        // Step ยังคงเป็น Pending ตลอด — info requests ไม่ block การ Approve
        // (เมื่อ Approve ระบบจะ auto-cancel info requests ที่ค้างทั้งหมด)

        await db.SaveChangesAsync(ct);

        return Result<RespondToInfoRequestDto>.Success(new RespondToInfoRequestDto(
            infoRequest.InfoRequestId,
            "Answered",
            null, null, null));
    }
}
