namespace EWS.Domain.Enums;

/// <summary>
/// วิธีระบุผู้อนุมัติในแต่ละ Step
/// </summary>
public enum ApproverType
{
    /// <summary>ผู้บังคับบัญชาโดยตรง (reportto_position)</summary>
    DirectSupervisor = 1,

    /// <summary>Section Manager ของ Dept เดียวกัน</summary>
    SectionManager = 2,

    /// <summary>Department Manager ของ Dept เดียวกัน</summary>
    DeptManager = 3,

    /// <summary>Division Director ของ Division เดียวกัน</summary>
    DivisionDirector = 4,

    /// <summary>C-Level ที่ดูแล Division นั้น (CMO/COO/CFO/CPO/CTO)</summary>
    CLevel = 5,

    /// <summary>CEO</summary>
    Ceo = 6,

    /// <summary>ระบุตำแหน่งเฉพาะเจาะจง (ใช้ SpecificPositionCode)</summary>
    SpecificPosition = 7,

    /// <summary>Area Manager (Grade B0) — ใช้สำหรับ Branch DOA เท่านั้น</summary>
    AreaManager = 8
}
