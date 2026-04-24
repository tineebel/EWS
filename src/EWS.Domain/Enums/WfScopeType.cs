namespace EWS.Domain.Enums;

/// <summary>
/// ขอบเขตของ Workflow — ตรงกับ wf_type ใน flowWF.xlsx
/// </summary>
public enum WfScopeType
{
    All = 0,    // ใช้ได้ทุกประเภท (Branch + HO)
    Branch = 1, // สาขา Cinema เท่านั้น
    Ho = 2      // Head Office เท่านั้น
}
