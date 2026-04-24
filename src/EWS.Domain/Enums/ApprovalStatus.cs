namespace EWS.Domain.Enums;

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Delegated = 3,
    Escalated = 4,
    Skipped = 5,
    Stuck         = 6,  // Escalation chain exhausted — requires manual admin reassignment
    InfoRequested = 7   // Step กำลังรอข้อมูลจาก Step ก่อนหน้า (ชั่วคราว)
}
