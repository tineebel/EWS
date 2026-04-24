using EWS.Domain.Common;
using EWS.Domain.Enums;

namespace EWS.Domain.Entities;

/// <summary>
/// WorkflowTemplate — กำหนด Flow สำหรับแต่ละ Document + เงื่อนไข
/// ตรงกับแต่ละ row ใน flowWF.xlsx (doc_code + flow_code = unique)
///
/// ตัวอย่าง: doc_code=2001, flow_code=1 → "PCV-BR ≤ 1,000"
///           doc_code=2001, flow_code=2 → "PCV-BR > 1,000"
/// </summary>
public class WorkflowTemplate : BaseEntity
{
    public int TemplateId { get; set; }

    public int DocumentTypeId { get; set; }
    public DocumentType DocumentType { get; set; } = null!;

    /// <summary>รหัส Flow ย่อย เช่น 1, 2, 3 (ต่างเงื่อนไขราคา)</summary>
    public int FlowCode { get; set; }

    public string FlowDesc { get; set; } = string.Empty;

    /// <summary>Branch / Ho / All — ขอบเขตผู้ใช้ที่ flow นี้ใช้ได้</summary>
    public WfScopeType WfScopeType { get; set; }

    /// <summary>มี Item พิเศษ (wf_item_special=1) → ใช้ Flow แตกต่าง</summary>
    public bool HasSpecialItem { get; set; }

    /// <summary>กรณีเร่งด่วน (wf_urgent=1) → ลัด Step บางขั้น</summary>
    public bool IsUrgent { get; set; }

    /// <summary>เงื่อนไข 1 เช่น "&gt; 1,000", "&lt;= 5,000", "NULL"</summary>
    public string? Condition1 { get; set; }
    public string? Condition2 { get; set; }
    public string? Condition3 { get; set; }
    public string? Condition4 { get; set; }
    public string? Condition5 { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<WorkflowStep> Steps { get; set; } = [];
    public ICollection<WorkflowInstance> Instances { get; set; } = [];
}
