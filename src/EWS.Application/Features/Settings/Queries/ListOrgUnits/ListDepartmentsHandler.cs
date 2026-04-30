using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListOrgUnits;

public class ListDepartmentsHandler(IAppDbContext db)
    : IRequestHandler<ListDepartmentsQuery, Result<List<DepartmentOptionDto>>>
{
    public async Task<Result<List<DepartmentOptionDto>>> Handle(ListDepartmentsQuery req, CancellationToken ct)
    {
        var q = db.Departments
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            q = q.Where(d =>
                d.DeptCode.Contains(req.Search) ||
                d.DeptName.Contains(req.Search) ||
                (d.DeptNameEn != null && d.DeptNameEn.Contains(req.Search)));

        if (req.IsActive.HasValue)
            q = q.Where(d => d.IsActive == req.IsActive.Value);

        var items = await q
            .OrderBy(d => d.DeptCode)
            .Select(d => new DepartmentOptionDto(
                d.DepartmentId,
                d.DeptCode,
                d.DeptName,
                d.DeptNameEn,
                null,
                null,
                d.IsActive))
            .ToListAsync(ct);

        return Result<List<DepartmentOptionDto>>.Success(items);
    }
}
