namespace EWS.Domain.Enums;

public enum InfoRequestStatus
{
    Open      = 0,  // รอผู้รับตอบ
    Forwarded = 1,  // ผู้รับ forward ต่อไปยัง Step ก่อนหน้า รอ child ตอบกลับ
    Closed    = 2,  // ตอบแล้ว / chain สมบูรณ์
    Cancelled = 3   // ถูกยกเลิก (เช่น instance ถูก Reject)
}
