using EWS.Domain.Common;
using EWS.Domain.Enums;

namespace EWS.Domain.Entities;

/// <summary>
/// WorkflowStep — ลำดับขั้นตอนการอนุมัติใน Template
///
/// แต่ละ Step ระบุผู้อนุมัติด้วย ApproverType (relative) หรือ PositionCode (absolute)
///
/// ตัวอย่าง flow PCV-BR &gt; 1,000:
///   Step 1: DirectSupervisor  (Theater Manager)
///   Step 2: SectionManager    (Area Manager)
///   Step 3: DeptManager       (Operation Dept Manager)
///   Step 4: DivisionDirector  (COO)
/// </summary>
public class WorkflowStep : BaseEntity
{
    public int StepId { get; set; }

    public int TemplateId { get; set; }
    public WorkflowTemplate Template { get; set; } = null!;

    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;

    public ApproverType ApproverType { get; set; }

    /// <summary>
    /// กรณี ApproverType = SpecificPosition ระบุ PositionCode ตรงๆ
    /// เช่น "HOFIN01" (CFO Position)
    /// </summary>
    public string? SpecificPositionCode { get; set; }

    /// <summary>จำนวนวันที่รอได้ก่อน Auto-Escalate (0 = ไม่ Escalate)</summary>
    public int EscalationDays { get; set; } = 0;

    /// <summary>Step นี้บังคับหรือสามารถข้ามได้</summary>
    public bool IsRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;
}
