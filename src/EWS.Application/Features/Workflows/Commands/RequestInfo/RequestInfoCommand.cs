using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Commands.RequestInfo;

/// <summary>
/// Step ปัจจุบัน (FromStepOrder) ขอข้อมูลจาก Step ก่อนหน้า (ToStepOrder)
/// กฎ:
/// - ToStepOrder ต้องน้อยกว่า FromStepOrder (ขอข้อมูลย้อนหลังเท่านั้น)
/// - ถามหลาย Step พร้อมกันได้
/// - ห้ามถาม Step เดิมซ้ำขณะที่ยังมี Open request ไปยัง Step นั้นค้างอยู่
/// - Info requests ค้างอยู่จนกว่าจะ Approve — เมื่อ Approve ระบบ auto-cancel ทั้งหมด
/// </summary>
public record RequestInfoCommand(
    Guid InstanceId,
    int FromStepOrder,              // Step ที่ถาม (ต้องเป็น Step ที่ InfoRequested/Pending)
    int ToStepOrder,                // Step ที่ถูกถาม (ต้องน้อยกว่า From)
    string ActorPositionCode,       // ตำแหน่งของคนถาม
    Guid ActorEmployeeId,
    string Question
) : IRequest<Result<InfoRequestDto>>;

public record InfoRequestDto(
    long InfoRequestId,
    Guid InstanceId,
    string DocumentNo,
    int FromStepOrder,
    string FromPositionCode,
    int ToStepOrder,
    string ToPositionCode,
    string ToPositionName,
    string? ToOccupantName,
    string Question,
    string Status
);
