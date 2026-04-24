using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Commands.ReEscalateWorkflow;

/// <summary>
/// Re-Escalate pending approval steps ของ Workflow Instance หนึ่ง
/// ใช้เมื่อผู้อนุมัติลาออก/โอนย้ายหลังจาก Submit แล้ว
/// ระบบจะ walk up hierarchy หาผู้อนุมัติใหม่แทนตำแหน่งที่ว่าง
/// </summary>
public record ReEscalateWorkflowCommand(
    Guid InstanceId,
    string RequestedByPositionCode  // Admin/HR ที่สั่ง re-escalate (สำหรับ Audit)
) : IRequest<Result<ReEscalateWorkflowDto>>;

public record ReEscalateWorkflowDto(
    Guid InstanceId,
    string DocumentNo,
    int StepsReEscalated,
    int StepsStuck,
    List<ReEscalatedStepDto> Changes
);

public record ReEscalatedStepDto(
    int StepOrder,
    string StepName,
    string OldPositionCode,
    string OldPositionName,
    string NewPositionCode,   // same as old when Stuck
    string NewPositionName,
    bool WasEscalated,
    bool IsStuck              // true = hierarchy exhausted, needs manual reassign
);
