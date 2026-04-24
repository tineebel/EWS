using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Organization.Queries.GetOrgChart;

public class GetOrgChartHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<GetOrgChartQuery, Result<List<OrgChartNodeDto>>>
{
    public async Task<Result<List<OrgChartNodeDto>>> Handle(GetOrgChartQuery request, CancellationToken ct)
    {
        var now = clock.Now;

        var positions = await db.Positions
            .Where(p => p.IsActive)
            .Select(p => new
            {
                p.PositionId,
                p.PositionCode,
                p.PositionName,
                p.JobGrade,
                p.IsChiefLevel,
                p.ParentPositionId,
                p.SecretaryPositionId
            })
            .ToListAsync(ct);

        var occupants = await db.PositionAssignments
            .Where(a => a.IsActive && !a.IsVacant
                && a.StartDate <= now
                && (a.EndDate == null || a.EndDate >= now))
            .Join(db.Employees, a => a.EmployeeId, e => e.EmployeeId,
                (a, e) => new { a.PositionId, e.EmployeeName })
            .ToListAsync(ct);

        var occupantMap = occupants
            .GroupBy(x => x.PositionId)
            .ToDictionary(g => g.Key, g => g.First().EmployeeName);

        var posCodeMap = positions.ToDictionary(p => p.PositionId, p => p.PositionCode);

        var childrenMap = positions
            .Where(p => p.ParentPositionId.HasValue)
            .GroupBy(p => p.ParentPositionId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        OrgChartNodeDto BuildNode(int posId)
        {
            var p = positions.First(x => x.PositionId == posId);
            var secretaryCode = p.SecretaryPositionId.HasValue
                ? posCodeMap.GetValueOrDefault(p.SecretaryPositionId.Value)
                : null;

            var children = childrenMap.TryGetValue(posId, out var ch)
                ? ch.Select(c => BuildNode(c.PositionId)).ToList()
                : new List<OrgChartNodeDto>();

            return new OrgChartNodeDto(
                p.PositionId,
                p.PositionCode,
                p.PositionName,
                p.JobGrade.ToString(),
                p.IsChiefLevel,
                !occupantMap.ContainsKey(p.PositionId),
                occupantMap.GetValueOrDefault(p.PositionId),
                secretaryCode,
                children
            );
        }

        if (!string.IsNullOrEmpty(request.RootCode))
        {
            var root = positions.FirstOrDefault(p => p.PositionCode == request.RootCode);
            if (root == null)
                return Result<List<OrgChartNodeDto>>.Fail("ORG_NOT_FOUND", $"Position '{request.RootCode}' not found.");

            return Result<List<OrgChartNodeDto>>.Success([BuildNode(root.PositionId)]);
        }

        // Top-level: positions whose parent is null or parent is not in the active set
        var activeIds = positions.Select(p => p.PositionId).ToHashSet();
        var topLevel = positions
            .Where(p => p.ParentPositionId == null || !activeIds.Contains(p.ParentPositionId.Value))
            .Select(p => BuildNode(p.PositionId))
            .ToList();

        return Result<List<OrgChartNodeDto>>.Success(topLevel);
    }
}
