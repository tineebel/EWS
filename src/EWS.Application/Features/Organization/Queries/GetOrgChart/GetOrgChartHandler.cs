using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Organization.Queries.GetOrgChart;

public class GetOrgChartHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<GetOrgChartQuery, Result<List<OrgChartNodeDto>>>
{
    public async Task<Result<List<OrgChartNodeDto>>> Handle(GetOrgChartQuery request, CancellationToken ct)
    {
        var now = clock.Now;

        var positionsQuery = db.Positions
            .Include(p => p.Section)
                .ThenInclude(s => s.Department)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.BranchCode))
        {
            var branch = request.BranchCode.Trim().ToUpper();
            if (branch == "HO")
            {
                positionsQuery = positionsQuery.Where(p => p.WfScopeType == WfScopeType.Ho);
            }
            else
            {
                positionsQuery = positionsQuery.Where(p =>
                    p.Section.SectCode.ToUpper() == branch ||
                    (p.Section.SectShortCode != null && p.Section.SectShortCode.ToUpper() == branch));
            }
        }

        if (!string.IsNullOrEmpty(request.DeptCode))
        {
            positionsQuery = positionsQuery.Where(p =>
                p.Section.Department.DeptCode == request.DeptCode);
        }

        if (!string.IsNullOrEmpty(request.SectionCode))
        {
            positionsQuery = positionsQuery.Where(p =>
                p.Section.SectCode == request.SectionCode);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerms = request.Search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var term in searchTerms)
            {
                positionsQuery = positionsQuery.Where(p =>
                    p.PositionCode.Contains(term) ||
                    p.PositionName.Contains(term) ||
                    (p.PositionShortName != null && p.PositionShortName.Contains(term)) ||
                    p.Section.SectCode.Contains(term) ||
                    (p.Section.SectShortCode != null && p.Section.SectShortCode.Contains(term)) ||
                    p.Section.SectName.Contains(term) ||
                    p.Section.Department.DeptCode.Contains(term) ||
                    (p.Section.Department.DeptShortCode != null && p.Section.Department.DeptShortCode.Contains(term)) ||
                    p.Section.Department.DeptName.Contains(term) ||
                    p.Assignments.Any(a =>
                        !a.IsVacant &&
                        a.StartDate <= now &&
                        (a.EndDate == null || a.EndDate >= now) &&
                        (a.Employee.EndDate == null || a.Employee.EndDate >= now) &&
                        !a.Employee.IsTest &&
                        a.Employee.EmployeeName.Contains(term)));
            }
        }

        var positions = await positionsQuery
            .Select(p => new
            {
                p.PositionId,
                p.PositionCode,
                p.PositionName,
                p.JobGrade,
                p.IsChiefLevel,
                p.ParentPositionId,
                p.SecretaryPositionId,
                SectionCode = p.Section.SectCode,
                SectionShortCode = p.Section.SectShortCode
            })
            .ToListAsync(ct);

        var occupants = await db.PositionAssignments
            .Where(a => !a.IsVacant
                && a.StartDate <= now
                && (a.EndDate == null || a.EndDate >= now)
                && (a.Employee.EndDate == null || a.Employee.EndDate >= now))
            .Join(db.Employees.Where(e => !e.IsTest), a => a.EmployeeId, e => e.EmployeeId,
                (a, e) => new { a.PositionId, e.EmployeeName })
            .ToListAsync(ct);

        var occupantMap = occupants
            .GroupBy(x => x.PositionId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.EmployeeName).OrderBy(name => name).ToList());

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

            var occupantNames = occupantMap.GetValueOrDefault(p.PositionId) ?? [];
            var occupantName = occupantNames.Count switch
            {
                0 => null,
                1 => occupantNames[0],
                _ => $"{occupantNames.Count} occupants"
            };

            return new OrgChartNodeDto(
                p.PositionId,
                p.PositionCode,
                p.PositionName,
                p.JobGrade.ToString(),
                p.IsChiefLevel,
                occupantNames.Count == 0,
                occupantName,
                occupantNames,
                occupantNames.Count,
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
