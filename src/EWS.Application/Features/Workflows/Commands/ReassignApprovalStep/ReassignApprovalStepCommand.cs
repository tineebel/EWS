using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Commands.ReassignApprovalStep;

/// <summary>
/// Admin manual reassign — ใช้เมื่อ Step ติด Stuck (hierarchy หมดแล้ว)
/// กำหนดตำแหน่งใหม่โดยตรง ไม่ผ่าน hierarchy auto-resolve
/// </summary>
public record ReassignApprovalStepCommand(
    Guid InstanceId,
    int StepOrder,
    string TargetPositionCode,        // ตำแหน่งใหม่ที่ต้องการให้อนุมัติ
    string RequestedByPositionCode,   // Admin/HR ที่สั่ง (สำหรับ Audit)
    string? Reason                    // เหตุผล (optional)
) : IRequest<Result<ReassignApprovalStepDto>>;

public record ReassignApprovalStepDto(
    Guid InstanceId,
    string DocumentNo,
    int StepOrder,
    string StepName,
    string OldPositionCode,
    string NewPositionCode,
    string NewPositionName
);
