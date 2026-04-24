using EWS.Domain.Common;

namespace EWS.Domain.Entities;

/// <summary>
/// Division — ระดับสูงสุด (e.g., Operation Division, Marketing Division)
/// ตรงกับ division_code/division_name ใน employee data
/// </summary>
public class Division : BaseEntity
{
    public int DivisionId { get; set; }

    /// <summary>e.g., F000, C000, J000, B000</summary>
    public string DivisionCode { get; set; } = string.Empty;

    public string DivisionName { get; set; } = string.Empty;
    public string? DivisionNameEn { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Department> Departments { get; set; } = [];
}
