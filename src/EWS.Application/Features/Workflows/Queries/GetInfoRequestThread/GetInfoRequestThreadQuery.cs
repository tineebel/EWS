using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Queries.GetInfoRequestThread;

/// <summary>
/// ดู Info Request thread ทั้งหมดของ Instance (สำหรับ timeline / tracking)
/// </summary>
public record GetInfoRequestThreadQuery(Guid InstanceId)
    : IRequest<Result<List<InfoRequestThreadDto>>>;

public record InfoRequestThreadDto(
    long InfoRequestId,
    int FromStepOrder,
    string FromPositionCode,
    string FromPositionName,
    int ToStepOrder,
    string ToPositionCode,
    string ToPositionName,
    string? ToOccupantName,
    string Question,
    string? Answer,
    string Status,
    long? ParentInfoRequestId,
    long? ChildInfoRequestId,
    DateTime CreatedAt,
    DateTime? AnsweredAt,
    int Depth    // 0 = root (ต้นทาง), 1 = forwarded once, 2 = forwarded twice...
);
