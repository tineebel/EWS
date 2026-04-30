using EWS.Domain.Common;

namespace EWS.Domain.Entities;

/// <summary>
/// Section — ระดับ 3 (e.g., Accounting (AR), SF MBK Center (MBK))
/// ตรงกับ sect_code/sect_name ใน employee data
/// </summary>
public class Section : BaseEntity
{
    public int SectionId { get; set; }

    /// <summary>e.g., J101, F201, F202, B104</summary>
    public string SectCode { get; set; } = string.Empty;

    /// <summary>Short display/search code, e.g., AR, ERP, MBK</summary>
    public string? SectShortCode { get; set; }

    public string SectName { get; set; } = string.Empty;
    public string? SectNameEn { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Position> Positions { get; set; } = [];
}
