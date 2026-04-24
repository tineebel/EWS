using EWS.Domain.Common;

namespace EWS.Domain.Entities;

/// <summary>
/// Department — ระดับ 2 (e.g., Accounting Department, Cinema Business)
/// ตรงกับ dept_code/dept_name ใน employee data
/// </summary>
public class Department : BaseEntity
{
    public int DepartmentId { get; set; }

    /// <summary>e.g., J100, F200, B100, C200</summary>
    public string DeptCode { get; set; } = string.Empty;

    public string DeptName { get; set; } = string.Empty;
    public string? DeptNameEn { get; set; }

    public int DivisionId { get; set; }
    public Division Division { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Section> Sections { get; set; } = [];
}
