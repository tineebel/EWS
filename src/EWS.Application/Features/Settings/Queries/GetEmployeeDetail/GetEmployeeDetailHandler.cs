using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.GetEmployeeDetail;

public class GetEmployeeDetailHandler(IAppDbContext db)
    : IRequestHandler<GetEmployeeDetailQuery, Result<EmployeeDetailDto>>
{
    public async Task<Result<EmployeeDetailDto>> Handle(GetEmployeeDetailQuery req, CancellationToken ct)
    {
        var employee = await db.Employees
            .Include(e => e.PositionAssignments)
                .ThenInclude(pa => pa.Position)
                    .ThenInclude(p => p.Section)
                        .ThenInclude(s => s.Department)
            .FirstOrDefaultAsync(e => e.EmployeeCode == req.EmployeeCode, ct);

        if (employee is null)
            return Result<EmployeeDetailDto>.Fail("EMPLOYEE_NOT_FOUND", $"Employee '{req.EmployeeCode}' not found.");

        var now = DateTime.UtcNow.AddHours(7);

        var assignments = employee.PositionAssignments
            .OrderByDescending(pa => pa.StartDate)
            .Select(pa => new PositionAssignmentDto(
                pa.AssignmentId,
                pa.Position.PositionCode,
                pa.Position.PositionName,
                pa.Position.PositionShortName,
                pa.Position.JobGrade.ToString(),
                pa.Position.Section.SectCode,
                pa.Position.Section.SectShortCode,
                pa.Position.Section.SectName,
                pa.Position.Section.Department.DeptCode,
                pa.Position.Section.Department.DeptShortCode,
                pa.Position.Section.Department.DeptName,
                pa.StartDate,
                pa.EndDate,
                pa.IsActive && pa.StartDate <= now && (pa.EndDate == null || pa.EndDate >= now)))
            .ToList();

        var dto = new EmployeeDetailDto(
            employee.EmployeeId,
            employee.EmployeeCode,
            employee.EmployeeName,
            employee.EmployeeNameEn,
            employee.Nickname,
            employee.Email,
            employee.Tel,
            employee.Status.ToString(),
            employee.StartDate,
            employee.EndDate,
            employee.IsTest,
            assignments);

        return Result<EmployeeDetailDto>.Success(dto);
    }
}
