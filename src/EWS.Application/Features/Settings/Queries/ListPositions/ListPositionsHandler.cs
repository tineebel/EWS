using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListPositions;

public class ListPositionsHandler(IAppDbContext db)
    : IRequestHandler<ListPositionsQuery, Result<PaginatedList<PositionDto>>>
{
    public async Task<Result<PaginatedList<PositionDto>>> Handle(ListPositionsQuery req, CancellationToken ct)
    {
        var q = db.Positions
            .Include(p => p.Section)
                .ThenInclude(s => s.Department)
            .Include(p => p.ParentPosition)
            .Include(p => p.SecretaryPosition)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            q = q.Where(p =>
                p.PositionCode.Contains(req.Search) ||
                p.PositionName.Contains(req.Search) ||
                (p.PositionShortName != null && p.PositionShortName.Contains(req.Search)) ||
                p.Section.SectCode.Contains(req.Search) ||
                p.Section.SectName.Contains(req.Search) ||
                p.Section.Department.DeptCode.Contains(req.Search) ||
                p.Section.Department.DeptName.Contains(req.Search));

        if (req.IsActive.HasValue)
            q = q.Where(p => p.IsActive == req.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(req.DeptCode))
            q = q.Where(p => p.Section.Department.DeptCode == req.DeptCode);

        if (!string.IsNullOrWhiteSpace(req.SectionCode))
            q = q.Where(p => p.Section.SectCode == req.SectionCode);

        var total = await q.CountAsync(ct);

        var items = await q.OrderBy(p => p.PositionCode)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new PositionDto(
                p.PositionId,
                p.PositionCode,
                p.PositionName,
                p.PositionShortName,
                p.JobGrade.ToString(),
                p.WfScopeType.ToString(),
                p.IsChiefLevel,
                p.IsActive,
                p.SectionId,
                p.Section.SectCode,
                p.Section.SectName,
                p.Section.Department.DeptCode,
                p.Section.Department.DeptName,
                p.ParentPositionId,
                p.ParentPosition != null ? p.ParentPosition.PositionCode : null,
                p.SecretaryPositionId,
                p.SecretaryPosition != null ? p.SecretaryPosition.PositionCode : null))
            .ToListAsync(ct);

        return Result<PaginatedList<PositionDto>>.Success(
            new PaginatedList<PositionDto>(items, total, req.Page, req.PageSize));
    }
}
