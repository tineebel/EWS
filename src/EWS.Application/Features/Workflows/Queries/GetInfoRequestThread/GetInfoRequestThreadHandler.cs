using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Queries.GetInfoRequestThread;

public class GetInfoRequestThreadHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<GetInfoRequestThreadQuery, Result<List<InfoRequestThreadDto>>>
{
    public async Task<Result<List<InfoRequestThreadDto>>> Handle(
        GetInfoRequestThreadQuery request, CancellationToken ct)
    {
        var now = clock.Now;

        var instance = await db.WorkflowInstances
            .AnyAsync(x => x.InstanceId == request.InstanceId, ct);
        if (!instance)
            return Result<List<InfoRequestThreadDto>>.Fail("WF_INSTANCE_NOT_FOUND",
                "Workflow instance not found.");

        var allRequests = await db.WorkflowInfoRequests
            .Include(r => r.FromPosition)
            .Include(r => r.ToPosition)
            .Where(r => r.InstanceId == request.InstanceId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);

        // คำนวณ Depth โดย traverse parent chain
        var depthMap = new Dictionary<long, int>();
        foreach (var r in allRequests)
        {
            int depth = 0;
            var current = r;
            while (current.ParentInfoRequestId.HasValue)
            {
                depth++;
                current = allRequests.First(x => x.InfoRequestId == current.ParentInfoRequestId.Value);
            }
            depthMap[r.InfoRequestId] = depth;
        }

        // ดึง occupant names
        var positionIds = allRequests.Select(r => r.ToPositionId).Distinct().ToList();
        var occupants = await db.PositionAssignments
            .Where(a => positionIds.Contains(a.PositionId) && a.IsActive && !a.IsVacant
                     && a.StartDate <= now && (a.EndDate == null || a.EndDate >= now))
            .Select(a => new { a.PositionId, a.Employee.EmployeeName })
            .ToListAsync(ct);
        var occupantMap = occupants.ToDictionary(o => o.PositionId, o => o.EmployeeName);

        var result = allRequests.Select(r => new InfoRequestThreadDto(
            r.InfoRequestId,
            r.FromStepOrder,
            r.FromPosition.PositionCode,
            r.FromPosition.PositionName,
            r.ToStepOrder,
            r.ToPosition.PositionCode,
            r.ToPosition.PositionName,
            occupantMap.GetValueOrDefault(r.ToPositionId),
            r.Question,
            r.Answer,
            r.Status.ToString(),
            r.ParentInfoRequestId,
            r.ChildInfoRequestId,
            r.CreatedAt,
            r.AnsweredAt,
            depthMap[r.InfoRequestId]
        )).ToList();

        return Result<List<InfoRequestThreadDto>>.Success(result);
    }
}
