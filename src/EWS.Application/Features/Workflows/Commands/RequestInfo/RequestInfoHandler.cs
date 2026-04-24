using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Commands.RequestInfo;

public class RequestInfoHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<RequestInfoCommand, Result<InfoRequestDto>>
{
    public async Task<Result<InfoRequestDto>> Handle(RequestInfoCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        // ตรวจ Instance
        var instance = await db.WorkflowInstances
            .FirstOrDefaultAsync(x => x.InstanceId == request.InstanceId, ct);
        if (instance == null)
            return Result<InfoRequestDto>.Fail("WF_INSTANCE_NOT_FOUND", "Workflow instance not found.");
        if (instance.Status != WorkflowStatus.Pending)
            return Result<InfoRequestDto>.Fail("WF_INSTANCE_NOT_PENDING",
                $"Instance is '{instance.Status}'. Only Pending instances can have info requests.");

        // ตรวจว่า ToStep < FromStep
        if (request.ToStepOrder >= request.FromStepOrder)
            return Result<InfoRequestDto>.Fail("WF_INFO_INVALID_DIRECTION",
                $"ToStepOrder ({request.ToStepOrder}) must be less than FromStepOrder ({request.FromStepOrder}). Cannot request info from future steps.");

        // ตรวจ FromStep — ต้องเป็น Pending หรือ InfoRequested และเป็น step ที่ actor ครอง
        var fromApproval = await db.WorkflowApprovals
            .Include(a => a.AssignedPosition)
            .FirstOrDefaultAsync(a =>
                a.InstanceId  == request.InstanceId &&
                a.StepOrder   == request.FromStepOrder &&
                (a.Status == ApprovalStatus.Pending || a.Status == ApprovalStatus.InfoRequested), ct);

        if (fromApproval == null)
            return Result<InfoRequestDto>.Fail("WF_STEP_NOT_FOUND",
                $"Step {request.FromStepOrder} not found or not in Pending/InfoRequested state.");

        if (fromApproval.AssignedPosition.PositionCode != request.ActorPositionCode)
            return Result<InfoRequestDto>.Fail("WF_UNAUTHORIZED",
                $"Position '{request.ActorPositionCode}' is not assigned to step {request.FromStepOrder}.");

        // ตรวจ ToStep — ต้องเป็น Step ที่ผ่านไปแล้ว (Approved) หรือ Pending ก่อนหน้า
        var toApproval = await db.WorkflowApprovals
            .Include(a => a.AssignedPosition)
            .Include(a => a.ActorEmployee)
            .FirstOrDefaultAsync(a =>
                a.InstanceId == request.InstanceId &&
                a.StepOrder  == request.ToStepOrder, ct);

        if (toApproval == null)
            return Result<InfoRequestDto>.Fail("WF_STEP_NOT_FOUND",
                $"Step {request.ToStepOrder} not found in this instance.");

        // ตรวจ: ห้ามถาม Step เดิมซ้ำขณะที่ยังมี Open/Forwarded request ไปยัง Step นั้นค้างอยู่
        // (ถามหลาย Step พร้อมกันได้ — แค่ห้ามถาม Step เดิมซ้ำ)
        var duplicateToSameStep = await db.WorkflowInfoRequests.AnyAsync(r =>
            r.InstanceId    == request.InstanceId    &&
            r.FromStepOrder == request.FromStepOrder &&
            r.ToStepOrder   == request.ToStepOrder   &&
            (r.Status == InfoRequestStatus.Open || r.Status == InfoRequestStatus.Forwarded), ct);

        if (duplicateToSameStep)
            return Result<InfoRequestDto>.Fail("WF_INFO_DUPLICATE",
                $"There is already an open info request from Step {request.FromStepOrder} to Step {request.ToStepOrder}. Wait for it to be answered first.");

        // สร้าง Info Request
        var infoRequest = new WorkflowInfoRequest
        {
            InstanceId      = request.InstanceId,
            FromStepOrder   = request.FromStepOrder,
            FromPositionId  = fromApproval.AssignedPositionId,
            ToStepOrder     = request.ToStepOrder,
            ToPositionId    = toApproval.AssignedPositionId,
            Question        = request.Question,
            Status          = InfoRequestStatus.Open,
            CreatedAt       = now,
            CreatedBy       = request.ActorEmployeeId.ToString()
        };

        db.WorkflowInfoRequests.Add(infoRequest);
        // Step ยังคงเป็น Pending — info request ไม่ block การ Approve
        // เมื่อ Approve ระบบจะ auto-cancel info requests ที่ค้างทั้งหมด

        // Audit
        db.WorkflowHistories.Add(new WorkflowHistory
        {
            InstanceId      = request.InstanceId,
            EventType       = "InfoRequest",
            StepOrder       = request.FromStepOrder,
            ActorPositionId = fromApproval.AssignedPositionId,
            ActorEmployeeId = request.ActorEmployeeId,
            Comment         = $"Step {request.FromStepOrder} → Step {request.ToStepOrder}: {request.Question}",
            OccurredAt      = now
        });

        await db.SaveChangesAsync(ct);

        // ดึง ToPosition occupant name
        var toOccupant = await db.PositionAssignments
            .Where(a => a.PositionId == toApproval.AssignedPositionId && a.IsActive && !a.IsVacant
                     && a.StartDate <= now && (a.EndDate == null || a.EndDate >= now))
            .Select(a => a.Employee.EmployeeName)
            .FirstOrDefaultAsync(ct);

        return Result<InfoRequestDto>.Success(new InfoRequestDto(
            infoRequest.InfoRequestId,
            request.InstanceId,
            instance.DocumentNo,
            request.FromStepOrder,
            fromApproval.AssignedPosition.PositionCode,
            request.ToStepOrder,
            toApproval.AssignedPosition.PositionCode,
            toApproval.AssignedPosition.PositionName,
            toOccupant,
            request.Question,
            infoRequest.Status.ToString()));
    }
}
