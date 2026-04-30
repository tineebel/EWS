using EWS.Domain.Entities;
using EWS.Domain.Enums;

namespace EWS.Application.Common.Interfaces;

public record ResolvedApprover(
    int PositionId,
    string PositionCode,
    string PositionName,
    bool WasEscalated,
    int EscalationDepth,
    string? OccupantName,
    IReadOnlyList<string> OccupantNames,
    int OccupantCount,
    bool IsVacant,
    int? DelegatedToPositionId,
    string? DelegatedToPositionCode,
    int? EscalatedFromPositionId = null
);

public interface IApproverResolver
{
    /// <summary>
    /// หาตำแหน่งผู้อนุมัติจาก ApproverType โดยเดินขึ้น Hierarchy
    /// จาก submitterPositionId แล้ว Resolve ตำแหน่งที่ว่างด้วย Auto-Escalation
    /// </summary>
    Task<ResolvedApprover?> ResolveAsync(
        int submitterPositionId,
        ApproverType approverType,
        string? specificPositionCode,
        CancellationToken ct = default);

    /// <summary>
    /// ตรวจสอบว่าตำแหน่งนี้มีคนครองอยู่ไหม ถ้าว่างให้ Escalate ขึ้น hierarchy
    /// ใช้สำหรับ Re-Escalate เมื่อพนักงานลาออกหลัง Submit
    /// </summary>
    Task<ResolvedApprover?> EscalateFromPositionAsync(
        int positionId,
        CancellationToken ct = default);
}
