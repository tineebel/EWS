using EWS.Domain.Common;

namespace EWS.Domain.Entities;

/// <summary>
/// Delegation — การรักษาการแทน (ตำแหน่ง A มอบสิทธิ์ให้ตำแหน่ง B ชั่วคราว)
///
/// Audit: บันทึกทั้ง OriginalPositionId (ตำแหน่งเดิม)
///        และ ActingPositionId (ตำแหน่งที่ทำแทน)
/// </summary>
public class Delegation : BaseEntity
{
    public int DelegationId { get; set; }

    /// <summary>ตำแหน่งที่มอบสิทธิ์ออก (ผู้ขาด/ลา)</summary>
    public int FromPositionId { get; set; }
    public Position FromPosition { get; set; } = null!;

    /// <summary>ตำแหน่งที่รับสิทธิ์มาทำแทน</summary>
    public int ToPositionId { get; set; }
    public Position ToPosition { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;
}
