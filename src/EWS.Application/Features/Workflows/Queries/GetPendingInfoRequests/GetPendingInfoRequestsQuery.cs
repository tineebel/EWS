using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Queries.GetPendingInfoRequests;

/// <summary>
/// ดู Info Requests ที่ตำแหน่งนี้ต้องตอบ (Inbox ของผู้ถูกถาม)
/// </summary>
public record GetPendingInfoRequestsQuery(
    string PositionCode,    // ตำแหน่งที่ต้องการดู inbox
    Guid? InstanceId = null // กรองเฉพาะ instance (optional)
) : IRequest<Result<List<PendingInfoRequestDto>>>;

public record PendingInfoRequestDto(
    long InfoRequestId,
    Guid InstanceId,
    string DocumentNo,
    string Subject,
    int FromStepOrder,
    string FromPositionCode,
    string FromPositionName,
    int ToStepOrder,
    string ToPositionCode,
    string Question,
    string Status,              // Open | Forwarded (if Forwarded, waiting for child)
    long? ChildInfoRequestId,   // ถ้า Forwarded: child request ที่รออยู่
    int? ForwardedToStepOrder,
    string? ForwardedToPositionCode,
    DateTime CreatedAt,
    // ถ้า Forwarded: คำตอบจาก child (ถ้ามี)
    string? ChildAnswer
);
