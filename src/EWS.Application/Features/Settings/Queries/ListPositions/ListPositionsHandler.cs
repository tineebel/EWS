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
            .Include(p => p.ParentPosition)
            .Include(p => p.SecretaryPosition)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            q = q.Where(p => p.PositionCode.Contains(req.Search) || p.PositionName.Contains(req.Search));

        if (req.IsActive.HasValue)
            q = q.Where(p => p.IsActive == req.IsActive.Value);

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
                p.Section.SectName,
                p.ParentPositionId,
                p.ParentPosition != null ? p.ParentPosition.PositionCode : null,
                p.SecretaryPositionId,
                p.SecretaryPosition != null ? p.SecretaryPosition.PositionCode : null))
            .ToListAsync(ct);

        return Result<PaginatedList<PositionDto>>.Success(
            new PaginatedList<PositionDto>(items, total, req.Page, req.PageSize));
    }
}
