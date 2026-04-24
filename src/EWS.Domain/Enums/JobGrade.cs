namespace EWS.Domain.Enums;

/// <summary>
/// ระดับตำแหน่งงาน — ตรงกับ job_grade ใน employee data
/// </summary>
public enum JobGrade
{
    XX = 0, // Special / Contract
    A0 = 1, // CEO / C-Level
    A1 = 2, // Director Level
    A2 = 3, // Senior Director
    A3 = 4, // Executive Director
    B0 = 5, // Department Manager
    B1 = 6, // Section Manager
    B2 = 7, // Assistant Section Manager
    C1 = 8, // Senior / Supervisor
    D1 = 9  // Officer / Staff
}
