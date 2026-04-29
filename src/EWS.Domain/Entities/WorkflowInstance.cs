using EWS.Domain.Common;
using EWS.Domain.Enums;

namespace EWS.Domain.Entities;

/// <summary>
/// WorkflowInstance — เอกสารที่ถูก Submit เข้าสู่ระบบ (Runtime)
/// 1 Instance = 1 เอกสาร 1 ครั้ง
/// </summary>
public class WorkflowInstance : BaseEntity
{
    public Guid InstanceId { get; set; }

    public int TemplateId { get; set; }
    public WorkflowTemplate Template { get; set; } = null!;

    /// <summary>เลขที่เอกสาร (Running Number เช่น PCV-2026-00001)</summary>
    public string DocumentNo { get; set; } = string.Empty;

    /// <summary>Reference ไปยัง Document จริงในระบบต้นทาง (nullable)</summary>
    public string? ExternalDocRef { get; set; }

    /// <summary>ตำแหน่งผู้ Submit</summary>
    public int SubmitterPositionId { get; set; }
    public Position SubmitterPosition { get; set; } = null!;

    /// <summary>พนักงานที่กด Submit จริง (อาจเป็นคนรักษาการ)</summary>
    public Guid SubmitterEmployeeId { get; set; }
    public Employee SubmitterEmployee { get; set; } = null!;

    /// <summary>
    /// กรณี Delegation: ตำแหน่งจริงที่มีสิทธิ์ Submit
    /// ใช้สำหรับ Audit Trail
    /// </summary>
    public int? ActingAsPositionId { get; set; }
    public Position? ActingAsPosition { get; set; }

    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;

    // --- Secretary Pre-Approval (ก่อนเข้า Flow จริง) ---

    /// <summary>
    /// สถานะ Pre-Approval: ใช้เมื่อเลขาสร้างเอกสารแทน Chef
    /// Chef ต้อง Confirm ก่อน Flow เริ่ม (ไม่นับ Step)
    /// </summary>
    public PreApprovalStatus PreApprovalStatus { get; set; } = PreApprovalStatus.NotRequired;

    /// <summary>ตำแหน่งเลขาที่สร้างเอกสารแทน (ถ้ามี)</summary>
    public int? CreatedBySecretaryPositionId { get; set; }
    public Position? CreatedBySecretaryPosition { get; set; }

    /// <summary>ตำแหน่ง Chef ที่ต้อง Confirm Pre-Approval</summary>
    public int? PreApprovalChiefPositionId { get; set; }
    public Position? PreApprovalChiefPosition { get; set; }

    public DateTime? PreApprovalConfirmedAt { get; set; }

    // --- Document Data ---

    /// <summary>ยอดรวมเอกสาร (ใช้ match กับ wf_condition)</summary>
    public decimal? TotalAmount { get; set; }

    public bool IsSpecialItem { get; set; }
    public bool IsUrgent { get; set; }

    public string? Subject { get; set; }
    public string? Remark { get; set; }

    public DateTime SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public ICollection<WorkflowApproval> Approvals { get; set; } = [];
    public ICollection<WorkflowHistory> Histories { get; set; } = [];

    public byte[] RowVersion { get; set; } = [];
}
