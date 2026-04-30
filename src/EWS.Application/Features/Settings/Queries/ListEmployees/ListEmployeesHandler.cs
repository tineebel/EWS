using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListEmployees;

public class ListEmployeesHandler(IAppDbContext db, IDateTimeService clock)
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

        var now = clock.Now;
        EmployeeStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(req.Status) &&
            Enum.TryParse<EmployeeStatus>(req.Status, out var parsedStatus))
        {
            statusFilter = parsedStatus;
        }

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var searchTerms = req.Search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var term in searchTerms)
            {
                q = q.Where(e =>
                    e.EmployeeCode.Contains(term) ||
                    e.EmployeeName.Contains(term) ||
                    (e.Email != null && e.Email.Contains(term)) ||
                    e.PositionAssignments.Any(pa =>
                        (!statusFilter.HasValue ||
                         (!pa.IsVacant &&
                          pa.StartDate <= now &&
                          (pa.EndDate == null || pa.EndDate >= now))) &&
                        (pa.Position.PositionCode.Contains(term) ||
                         pa.Position.PositionName.Contains(term) ||
                         pa.Position.Section.SectCode.Contains(term) ||
                         (pa.Position.Section.SectShortCode != null && pa.Position.Section.SectShortCode.Contains(term)) ||
                         pa.Position.Section.SectName.Contains(term) ||
                         pa.Position.Section.Department.DeptCode.Contains(term) ||
                         (pa.Position.Section.Department.DeptShortCode != null && pa.Position.Section.Department.DeptShortCode.Contains(term)) ||
                         pa.Position.Section.Department.DeptName.Contains(term))));
            }
        }

        if (statusFilter == EmployeeStatus.Active)
            q = q.Where(e => e.EndDate == null || e.EndDate >= now);
        else if (statusFilter == EmployeeStatus.Resigned)
            q = q.Where(e => e.EndDate != null && e.EndDate < now);

        if (!string.IsNullOrWhiteSpace(req.BranchCode))
        {
            var branchCode = req.BranchCode.Trim().ToUpper();

            if (branchCode == "HO")
            {
                q = q.Where(e => e.PositionAssignments.Any(pa =>
                    !pa.IsVacant &&
                    pa.StartDate <= now &&
                    (pa.EndDate == null || pa.EndDate >= now) &&
                    pa.Position.WfScopeType == WfScopeType.Ho));
            }
            else
            {
                q = q.Where(e => e.PositionAssignments.Any(pa =>
                    !pa.IsVacant &&
                    pa.StartDate <= now &&
                    (pa.EndDate == null || pa.EndDate >= now) &&
                    pa.Position.WfScopeType == WfScopeType.Branch &&
                    (pa.Position.Section.SectCode.ToUpper() == branchCode ||
                     (pa.Position.Section.SectShortCode != null && pa.Position.Section.SectShortCode.ToUpper() == branchCode))));
            }
        }

        if (!string.IsNullOrWhiteSpace(req.DeptCode))
            q = q.Where(e => e.PositionAssignments.Any(pa =>
                !pa.IsVacant &&
                pa.StartDate <= now &&
                (pa.EndDate == null || pa.EndDate >= now) &&
                pa.Position.Section.Department.DeptCode == req.DeptCode));

        if (!string.IsNullOrWhiteSpace(req.SectionCode))
            q = q.Where(e => e.PositionAssignments.Any(pa =>
                !pa.IsVacant &&
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
                e.EndDate == null || e.EndDate >= now
                    ? EmployeeStatus.Active.ToString()
                    : EmployeeStatus.Resigned.ToString(),
                e.StartDate,
                e.EndDate,
                e.IsTest,
                e.PositionAssignments
                    .Where(pa => !statusFilter.HasValue ||
                        (!pa.IsVacant &&
                         pa.StartDate <= now &&
                         (pa.EndDate == null || pa.EndDate >= now)))
                    .Select(pa => pa.Position.PositionCode)
                    .ToList()))
            .ToList();

        return Result<PaginatedList<EmployeeDto>>.Success(
            new PaginatedList<EmployeeDto>(items, total, req.Page, req.PageSize));
    }
}
