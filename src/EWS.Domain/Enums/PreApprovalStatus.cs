namespace EWS.Domain.Enums;

/// <summary>
/// สถานะ Pre-Approval ก่อนเข้าสู่ Workflow จริง
/// ใช้เฉพาะกรณี: เลขาสร้างเอกสารให้ Chef → Chef ต้อง Pre-Approve ก่อน
/// ไม่นับเป็น Approval Step ใน Flow
/// </summary>
public enum PreApprovalStatus
{
    /// <summary>ไม่ต้องผ่าน Pre-Approval (เอกสารไม่ได้สร้างโดยเลขา)</summary>
    NotRequired = 0,

    /// <summary>รอ Chef ยืนยัน (เลขาสร้างให้แล้ว รอ Chef กด Confirm)</summary>
    Pending = 1,

    /// <summary>Chef ยืนยันแล้ว → Flow เริ่มได้</summary>
    Confirmed = 2,

    /// <summary>Chef ปฏิเสธ → เอกสารถูก Cancel</summary>
    Rejected = 3
}
