using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Organization.Commands.BulkReEscalate;

/// <summary>
/// Bulk Re-Escalate: เมื่อพนักงานลาออก/โอนย้าย
/// ค้นหาทุก Pending Approval ที่ assign ให้ตำแหน่งที่พนักงานคนนี้ครองอยู่
/// แล้ว Re-Escalate ทั้งหมดไปยัง hierarchy ถัดไป
/// </summary>
public record BulkReEscalateCommand(
    string EmployeeCode,            // พนักงานที่ลาออก/โอนย้าย
    string RequestedByPositionCode  // HR/Admin ที่สั่ง (สำหรับ Audit)
) : IRequest<Result<BulkReEscalateDto>>;

public record BulkReEscalateDto(
    string EmployeeCode,
    string EmployeeName,
    int AffectedInstances,
    int TotalStepsReEscalated,
    int TotalStepsStuck,
    List<BulkReEscalateInstanceDto> Details
);

public record BulkReEscalateInstanceDto(
    Guid InstanceId,
    string DocumentNo,
    int StepOrder,
    string StepName,
    string OldPositionCode,
    string NewPositionCode,
    string NewPositionName,
    bool IsStuck              // true = hierarchy exhausted, needs manual reassign
);
