using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Queries.GetPendingInfoRequests;

public class GetPendingInfoRequestsHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<GetPendingInfoRequestsQuery, Result<List<PendingInfoRequestDto>>>
{
    public async Task<Result<List<PendingInfoRequestDto>>> Handle(
        GetPendingInfoRequestsQuery request, CancellationToken ct)
    {
        var now = clock.Now;

        // ดึง PositionId ของ position code นี้
        var positionId = await db.Positions
            .Where(p => p.PositionCode == request.PositionCode && p.IsActive)
            .Select(p => (int?)p.PositionId)
            .FirstOrDefaultAsync(ct);

        if (positionId == null)
            return Result<List<PendingInfoRequestDto>>.Fail("WF_POSITION_NOT_FOUND",
                $"Position '{request.PositionCode}' not found.");

        // ดึง Open + Forwarded info requests ที่ ToPosition = ตำแหน่งนี้
        var query = db.WorkflowInfoRequests
            .Include(r => r.Instance)
            .Include(r => r.FromPosition)
            .Include(r => r.ToPosition)
            .Include(r => r.ChildRequest)
                .ThenInclude(c => c!.ToPosition)
            .Where(r =>
                r.ToPositionId == positionId &&
                (r.Status == InfoRequestStatus.Open || r.Status == InfoRequestStatus.Forwarded));

        if (request.InstanceId.HasValue)
            query = query.Where(r => r.InstanceId == request.InstanceId.Value);

        var items = await query
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);

        var result = items.Select(r => new PendingInfoRequestDto(
            r.InfoRequestId,
            r.InstanceId,
            r.Instance.DocumentNo,
            r.Instance.Subject ?? r.Instance.DocumentNo,
            r.FromStepOrder,
            r.FromPosition.PositionCode,
            r.FromPosition.PositionName,
            r.ToStepOrder,
            r.ToPosition.PositionCode,
            r.Question,
            r.Status.ToString(),
            r.ChildRequest?.InfoRequestId,
            r.ChildRequest?.ToStepOrder,
            r.ChildRequest?.ToPosition.PositionCode,
            r.CreatedAt,
            r.ChildRequest?.Answer    // คำตอบจาก child (ถ้า Forwarded และ child ตอบแล้ว)
        )).ToList();

        return Result<List<PendingInfoRequestDto>>.Success(result);
    }
}
