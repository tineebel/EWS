using EWS.Domain.Common;
using EWS.Domain.Enums;

namespace EWS.Domain.Entities;

/// <summary>
/// WorkflowApproval — การตัดสินใจอนุมัติ/ปฏิเสธ ต่อ Step ต่อ Instance
/// Insert-Only (ห้าม Update หลังจาก Action แล้ว)
/// </summary>
public class WorkflowApproval : BaseEntity
{
    public int ApprovalId { get; set; }

    public Guid InstanceId { get; set; }
    public WorkflowInstance Instance { get; set; } = null!;

    public int StepId { get; set; }
    public WorkflowStep Step { get; set; } = null!;

    public int StepOrder { get; set; }

    /// <summary>ตำแหน่งที่ควรอนุมัติ (Resolved จาก ApproverType)</summary>
    public int AssignedPositionId { get; set; }
    public Position AssignedPosition { get; set; } = null!;

    /// <summary>พนักงานที่กดอนุมัติจริง</summary>
    public Guid? ActorEmployeeId { get; set; }
    public Employee? ActorEmployee { get; set; }

    /// <summary>
    /// กรณี Delegation: บันทึกตำแหน่งจริงของคนกด
    /// (OriginalPositionId = AssignedPositionId, ActorActingAs = ตำแหน่งคนกด)
    /// </summary>
    public int? ActorActingAsPositionId { get; set; }
    public Position? ActorActingAsPosition { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public string? Comment { get; set; }

    public DateTime? ActionAt { get; set; }

    /// <summary>ถ้า Escalate → บันทึก Original PositionId ก่อน Escalate</summary>
    public int? EscalatedFromPositionId { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
