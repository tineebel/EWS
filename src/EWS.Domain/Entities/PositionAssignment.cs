using EWS.Domain.Common;

namespace EWS.Domain.Entities;

/// <summary>
/// PositionAssignment — การมอบหมายพนักงานให้ตำแหน่ง (Effective Date)
///
/// กฎ: พนักงานคนเดียวอยู่ได้หลายตำแหน่ง (ช่วงเวลาต่างกัน)
///     ตำแหน่งเดียวมีพนักงานได้หลายคน แต่ Active ได้แค่ 1 คน ณ เวลาเดียวกัน
///
/// Active condition: StartDate &lt;= NOW &lt;= EndDate (EndDate=null หมายถึงยังไม่สิ้นสุด)
/// </summary>
public class PositionAssignment : BaseEntity
{
    public int AssignmentId { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    /// <summary>วันเริ่มดำรงตำแหน่ง (UTC+7)</summary>
    public DateTime StartDate { get; set; }

    /// <summary>วันสิ้นสุดตำแหน่ง (null = ยังดำรงอยู่)</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>ตำแหน่งนี้ว่างอยู่หรือไม่ (ยังไม่มีพนักงาน)</summary>
    public bool IsVacant { get; set; } = false;

    public bool IsActive { get; set; } = true;
}
