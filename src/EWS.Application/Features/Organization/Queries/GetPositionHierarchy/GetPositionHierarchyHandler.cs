using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Organization.Queries.GetPositionHierarchy;

public class GetPositionHierarchyHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<GetPositionHierarchyQuery, Result<List<PositionHierarchyDto>>>
{
    public async Task<Result<List<PositionHierarchyDto>>> Handle(
        GetPositionHierarchyQuery request, CancellationToken ct)
    {
        var now = clock.Now;
        var result = new List<PositionHierarchyDto>();

        var pos = await db.Positions
            .Where(p => p.PositionCode == request.PositionCode && p.IsActive)
            .Select(p => new { p.PositionId, p.PositionCode, p.PositionName, p.JobGrade, p.WfScopeType, p.IsChiefLevel, p.ParentPositionId })
            .FirstOrDefaultAsync(ct);

        if (pos == null)
            return Result<List<PositionHierarchyDto>>.Fail("ORG_POSITION_NOT_FOUND", $"Position '{request.PositionCode}' not found.");

        int level = 0;
        int? currentId = pos.PositionId;

        while (currentId != null && level <= 10)
        {
            var p = await db.Positions
                .Where(x => x.PositionId == currentId && x.IsActive)
                .Select(x => new { x.PositionId, x.PositionCode, x.PositionName, x.JobGrade, x.WfScopeType, x.IsChiefLevel, x.ParentPositionId })
                .FirstOrDefaultAsync(ct);

            if (p == null) break;

            var occupant = await db.PositionAssignments
                .Where(a => a.PositionId == p.PositionId && a.IsActive && !a.IsVacant
                    && a.StartDate <= now && (a.EndDate == null || a.EndDate >= now))
                .Join(db.Employees, a => a.EmployeeId, e => e.EmployeeId, (_, e) => e.EmployeeName)
                .FirstOrDefaultAsync(ct);

            result.Add(new PositionHierarchyDto(
                p.PositionId, p.PositionCode, p.PositionName,
                p.JobGrade.ToString(), p.WfScopeType.ToString(),
                p.IsChiefLevel, occupant == null, occupant, level));

            currentId = p.ParentPositionId;
            level++;
        }

        return Result<List<PositionHierarchyDto>>.Success(result);
    }
}
