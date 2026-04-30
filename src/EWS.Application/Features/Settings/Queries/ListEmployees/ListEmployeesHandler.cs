using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListEmployees;

public class ListEmployeesHandler(IAppDbContext db)
    : IRequestHandler<ListEmployeesQuery, Result<PaginatedList<EmployeeDto>>>
{
    public async Task<Result<PaginatedList<EmployeeDto>>> Handle(ListEmployeesQuery req, CancellationToken ct)
    {
        var q = db.Employees
            .Include(e => e.PositionAssignments)
                .ThenInclude(pa => pa.Position)
                    .ThenInclude(p => p.Section)
                        .ThenInclude(s => s.Department)
            .AsQueryable();

        var now = DateTime.UtcNow.AddHours(7);

        if (!string.IsNullOrWhiteSpace(req.Search))
            q = q.Where(e =>
                e.EmployeeCode.Contains(req.Search) ||
                e.EmployeeName.Contains(req.Search) ||
                (e.Email != null && e.Email.Contains(req.Search)) ||
                e.PositionAssignments.Any(pa =>
                    pa.Position.PositionCode.Contains(req.Search) ||
                    pa.Position.PositionName.Contains(req.Search) ||
                    pa.Position.Section.SectCode.Contains(req.Search) ||
                    (pa.Position.Section.SectShortCode != null && pa.Position.Section.SectShortCode.Contains(req.Search)) ||
                    pa.Position.Section.SectName.Contains(req.Search) ||
                    pa.Position.Section.Department.DeptCode.Contains(req.Search) ||
                    (pa.Position.Section.Department.DeptShortCode != null && pa.Position.Section.Department.DeptShortCode.Contains(req.Search)) ||
                    pa.Position.Section.Department.DeptName.Contains(req.Search)));

        if (!string.IsNullOrWhiteSpace(req.Status) &&
            Enum.TryParse<EmployeeStatus>(req.Status, out var statusEnum))
            q = q.Where(e => e.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(req.DeptCode))
            q = q.Where(e => e.PositionAssignments.Any(pa =>
                pa.StartDate <= now &&
                (pa.EndDate == null || pa.EndDate >= now) &&
                pa.Position.Section.Department.DeptCode == req.DeptCode));

        if (!string.IsNullOrWhiteSpace(req.SectionCode))
            q = q.Where(e => e.PositionAssignments.Any(pa =>
                pa.StartDate <= now &&
                (pa.EndDate == null || pa.EndDate >= now) &&
                pa.Position.Section.SectCode == req.SectionCode));

        var total = await q.CountAsync(ct);

        var employees = await q.OrderBy(e => e.EmployeeCode)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync(ct);

        var items = employees.Select(e => new EmployeeDto(
                e.EmployeeId,
                e.EmployeeCode,
                e.EmployeeName,
                e.EmployeeNameEn,
                e.Nickname,
                e.Email,
                e.Tel,
                e.Status.ToString(),
                e.StartDate,
                e.EndDate,
                e.IsTest,
                e.PositionAssignments
                    .Where(pa => pa.StartDate <= now && (pa.EndDate == null || pa.EndDate >= now))
                    .Select(pa => pa.Position.PositionCode)
                    .ToList()))
            .ToList();

        return Result<PaginatedList<EmployeeDto>>.Success(
            new PaginatedList<EmployeeDto>(items, total, req.Page, req.PageSize));
    }
}
