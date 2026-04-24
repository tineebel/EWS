using EWS.Domain.Common;
using EWS.Domain.Enums;

namespace EWS.Domain.Entities;

/// <summary>
/// Position — หน่วยหลักของระบบ EWS (Position-Based)
/// ตรงกับ position_code/position_name ใน employee data (~1,020 ตำแหน่ง)
///
/// สิทธิ์และ Workflow ผูกกับ Position ไม่ใช่ตัวบุคคล
/// เมื่อสลับพนักงาน สิทธิ์ตามไปกับตำแหน่งทันที
/// </summary>
public class Position : BaseEntity
{
    public int PositionId { get; set; }

    /// <summary>รหัสตำแหน่ง เช่น HOFNA17, CBMBK008, HOMKT81</summary>
    public string PositionCode { get; set; } = string.Empty;

    public string PositionName { get; set; } = string.Empty;

    /// <summary>ชื่อย่อ เช่น JD-FNA-17, JD-CB-MBK-008</summary>
    public string? PositionShortName { get; set; }

    public JobGrade JobGrade { get; set; }

    /// <summary>ขอบเขต Workflow: Branch / Ho / All</summary>
    public WfScopeType WfScopeType { get; set; } = WfScopeType.All;

    public int SectionId { get; set; }
    public Section Section { get; set; } = null!;

    /// <summary>
    /// ตำแหน่งหัวหน้าโดยตรง (reportto_position_code)
    /// ใช้สำหรับ Auto-Escalation และ Approval Chain
    /// Self-referencing FK
    /// </summary>
    public int? ParentPositionId { get; set; }
    public Position? ParentPosition { get; set; }

    /// <summary>
    /// ตำแหน่งนี้เป็น "Chief Level" หรือไม่ (JobGrade A0-B0)
    /// ถ้าใช่ → ระบบจะต้องตรวจสอบ Secretary Review ก่อน Submit
    /// </summary>
    public bool IsChiefLevel { get; set; } = false;

    /// <summary>
    /// ตำแหน่งเลขา (Secretary) ของ Chief นี้
    /// เลขาทำหน้าที่ Review ก่อน Flow เริ่ม (ไม่นับเป็น Approval Step)
    /// </summary>
    public int? SecretaryPositionId { get; set; }
    public Position? SecretaryPosition { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Position> SubordinatePositions { get; set; } = [];
    public ICollection<PositionAssignment> Assignments { get; set; } = [];
    public ICollection<Delegation> DelegationsFrom { get; set; } = [];
    public ICollection<Delegation> DelegationsTo { get; set; } = [];
}
