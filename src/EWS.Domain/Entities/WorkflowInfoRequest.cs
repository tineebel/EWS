using EWS.Domain.Common;
using EWS.Domain.Enums;

namespace EWS.Domain.Entities;

/// <summary>
/// Info Request — การขอข้อมูลระหว่าง Step ระหว่างรออนุมัติ
///
/// กฎ:
/// - ขอข้อมูลได้เฉพาะ Step ที่ StepOrder น้อยกว่าตัวเอง (ย้อนหลัง)
/// - ไม่สามารถขอข้อมูลไปข้างหน้าได้
/// - ไม่สามารถย้อนไปย้อนมาได้ (ตอบแล้ว request ปิด)
/// - Parent chain: 5→3→1 เมื่อ 1 ตอบ → แจ้ง 3 → เมื่อ 3 ตอบ → แจ้ง 5
/// </summary>
public class WorkflowInfoRequest : BaseEntity
{
    public long InfoRequestId { get; set; }

    public Guid InstanceId { get; set; }
    public WorkflowInstance Instance { get; set; } = null!;

    /// <summary>Step ที่ขอ (ผู้ถาม)</summary>
    public int FromStepOrder { get; set; }
    public int FromPositionId { get; set; }
    public Position FromPosition { get; set; } = null!;

    /// <summary>Step ที่ถูกขอ (ผู้ตอบ)</summary>
    public int ToStepOrder { get; set; }
    public int ToPositionId { get; set; }
    public Position ToPosition { get; set; } = null!;

    public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }

    public InfoRequestStatus Status { get; set; } = InfoRequestStatus.Open;

    /// <summary>
    /// ถ้าผู้รับ forward ต่อไป Step ก่อนหน้า → เก็บ Id ของ child request
    /// เมื่อ child Closed → request นี้กลับเป็น Open อีกครั้ง (รอผู้รับตอบ)
    /// </summary>
    public long? ChildInfoRequestId { get; set; }
    public WorkflowInfoRequest? ChildRequest { get; set; }

    /// <summary>Parent request ที่ forward มาให้ (null = ต้นทาง)</summary>
    public long? ParentInfoRequestId { get; set; }
    public WorkflowInfoRequest? ParentRequest { get; set; }

    public DateTime? AnsweredAt { get; set; }
}
