using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListOrgUnits;

public class ListSectionsHandler(IAppDbContext db)
    : IRequestHandler<ListSectionsQuery, Result<List<SectionOptionDto>>>
{
    public async Task<Result<List<SectionOptionDto>>> Handle(ListSectionsQuery req, CancellationToken ct)
    {
        var q = db.Sections
            .Include(s => s.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            q = q.Where(s =>
                s.SectCode.Contains(req.Search) ||
                (s.SectShortCode != null && s.SectShortCode.Contains(req.Search)) ||
                s.SectName.Contains(req.Search) ||
                (s.SectNameEn != null && s.SectNameEn.Contains(req.Search)) ||
                s.Department.DeptCode.Contains(req.Search) ||
                (s.Department.DeptShortCode != null && s.Department.DeptShortCode.Contains(req.Search)) ||
                s.Department.DeptName.Contains(req.Search));

        if (!string.IsNullOrWhiteSpace(req.DeptCode))
            q = q.Where(s => s.Department.DeptCode == req.DeptCode);

        if (req.IsActive.HasValue)
            q = q.Where(s => s.IsActive == req.IsActive.Value);

        var items = await q
            .OrderBy(s => s.SectCode)
            .Select(s => new SectionOptionDto(
                s.SectionId,
                s.SectCode,
                s.SectShortCode,
                s.SectName,
                s.SectNameEn,
                s.Department.DeptCode,
                s.Department.DeptName,
                s.IsActive))
            .ToListAsync(ct);

        return Result<List<SectionOptionDto>>.Success(items);
    }
}
