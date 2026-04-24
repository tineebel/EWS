using EWS.Domain.Common;

namespace EWS.Domain.Entities;

/// <summary>
/// DocumentType — ประเภทเอกสาร (doc_code ใน flowWF.xlsx)
///
/// Categories:
///   1xxx = Memo
///   2xxx = การเงิน (PCV, PCR, ADV, ADC, EXP)
///   4xxx = IT Request
///   5xxx = SIS (Stock/Inventory System)
///   6xxx = สัญญา / ลูกค้า
/// </summary>
public class DocumentType : BaseEntity
{
    public int DocumentTypeId { get; set; }

    /// <summary>รหัสเอกสาร เช่น 1001, 2001, 4002</summary>
    public int DocCode { get; set; }

    public string DocName { get; set; } = string.Empty;
    public string? DocNameEn { get; set; }
    public string? Description { get; set; }

    /// <summary>หมวดหมู่ เช่น Memo, Finance, IT, SIS, Contract</summary>
    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<WorkflowTemplate> Templates { get; set; } = [];
}
