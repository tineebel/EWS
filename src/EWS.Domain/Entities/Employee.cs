using EWS.Domain.Common;
using EWS.Domain.Enums;

namespace EWS.Domain.Entities;

/// <summary>
/// Employee — ข้อมูลพนักงาน (8,176 records, 2,163 active)
/// ตัวบุคคลไม่มีสิทธิ์โดยตรง — สิทธิ์มาจาก PositionAssignment
/// </summary>
public class Employee : BaseEntity
{
    /// <summary>GUID — ตรงกับ id ใน employee data</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>รหัสพนักงาน เช่น 100004044, 201400368</summary>
    public string EmployeeCode { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeNameEn { get; set; }
    public string? Nickname { get; set; }
    public string? Tel { get; set; }
    public string? Email { get; set; }
    public string? ImagePath { get; set; }

    public EmployeeStatus Status { get; set; }

    /// <summary>วันที่เริ่มงาน (UTC+7)</summary>
    public DateTime StartDate { get; set; }

    /// <summary>วันสิ้นสุดการทำงาน (null = ยังทำงานอยู่)</summary>
    public DateTime? EndDate { get; set; }

    public bool IsTest { get; set; } = false;

    // Navigation
    public ICollection<PositionAssignment> PositionAssignments { get; set; } = [];
}
