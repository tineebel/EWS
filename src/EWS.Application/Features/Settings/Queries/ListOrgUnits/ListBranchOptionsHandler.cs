using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListOrgUnits;

public class ListBranchOptionsHandler(IAppDbContext db)
    : IRequestHandler<ListBranchOptionsQuery, Result<List<BranchOptionDto>>>
{
    public async Task<Result<List<BranchOptionDto>>> Handle(ListBranchOptionsQuery req, CancellationToken ct)
    {
        var items = await db.Sections
            .Where(s => s.Positions.Any(p => p.WfScopeType == WfScopeType.Branch))
            .OrderBy(s => s.SectShortCode ?? s.SectCode)
            .Select(s => new BranchOptionDto(
                s.SectionId,
                s.SectCode,
                s.SectShortCode,
                s.SectName,
                s.Positions.Count(p => p.WfScopeType == WfScopeType.Branch)))
            .ToListAsync(ct);

        return Result<List<BranchOptionDto>>.Success(items);
    }
}
