namespace EWS.Domain.Entities;

/// <summary>
/// WorkflowHistory — Audit Trail แบบ Immutable (Insert-Only, ห้าม Update/Delete)
/// บันทึกทุก Event ที่เกิดขึ้นใน Workflow
/// </summary>
public class WorkflowHistory
{
    public long HistoryId { get; set; }

    public Guid InstanceId { get; set; }
    public WorkflowInstance Instance { get; set; } = null!;

    /// <summary>Submit, Approve, Reject, Escalate, Recall, Delegate, Cancel</summary>
    public string EventType { get; set; } = string.Empty;

    public int? StepOrder { get; set; }

    /// <summary>ตำแหน่งที่ทำ Action</summary>
    public int? ActorPositionId { get; set; }
    public Position? ActorPosition { get; set; }

    public Guid? ActorEmployeeId { get; set; }
    public Employee? ActorEmployee { get; set; }

    public string? Comment { get; set; }

    /// <summary>Snapshot ข้อมูลก่อน/หลัง (JSON) สำหรับ Full Audit</summary>
    public string? DataSnapshot { get; set; }

    public DateTime OccurredAt { get; set; }
}
