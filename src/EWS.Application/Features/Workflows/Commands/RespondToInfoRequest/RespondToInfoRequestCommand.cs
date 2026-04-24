using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Commands.RespondToInfoRequest;

/// <summary>
/// ตอบ Info Request — ผู้รับคำถาม (ToStep) ตอบกลับ
///
/// สองรูปแบบ:
/// 1. ตอบตรง: ระบุ Answer → request ปิด, FromStep กลับเป็น Pending
/// 2. Forward ต่อ: ระบุ ForwardToStepOrder + ForwardQuestion → สร้าง child request,
///    request นี้เปลี่ยนเป็น Forwarded (รอ child ตอบก่อน จึงค่อยตอบ FromStep)
/// </summary>
public record RespondToInfoRequestCommand(
    long InfoRequestId,
    string ActorPositionCode,   // ตำแหน่งของผู้ตอบ (ต้องตรงกับ ToPosition ของ request)
    Guid ActorEmployeeId,
    string? Answer,             // คำตอบ (จำเป็นถ้าไม่ Forward)
    int? ForwardToStepOrder,    // optional: ถ้าต้องการ forward ต่อไปยัง Step ก่อนหน้า
    string? ForwardQuestion     // คำถามที่จะส่งต่อ (จำเป็นถ้า ForwardToStepOrder มีค่า)
) : IRequest<Result<RespondToInfoRequestDto>>;

public record RespondToInfoRequestDto(
    long InfoRequestId,
    string Action,              // "Answered" | "Forwarded"
    string? ForwardedToPositionCode,
    string? ForwardedToOccupantName,
    long? ChildInfoRequestId
);
