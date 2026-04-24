namespace EWS.Domain.Entities;

/// <summary>
/// WorkflowTemplateAuditLog — บันทึกประวัติการเปลี่ยนแปลง WorkflowTemplate (Insert-Only / Immutable)
/// </summary>
public class WorkflowTemplateAuditLog
{
    public long AuditId { get; set; }
    public int TemplateId { get; set; }

    /// <summary>ลำดับ Version ต่อ Template เริ่มที่ 1</summary>
    public int Version { get; set; }

    /// <summary>"Updated" | "Deactivated" | "Activated"</summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>username ที่ทำการแก้ไข</summary>
    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    /// <summary>JSON Snapshot ของ Template + Steps ก่อนเปลี่ยนแปลง</summary>
    public string SnapshotJson { get; set; } = string.Empty;

    public string? ChangeNote { get; set; }

    // Navigation
    public WorkflowTemplate Template { get; set; } = null!;
}
