using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Organization.Queries.GetEmployeeCurrentPosition;

public class GetEmployeeCurrentPositionHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<GetEmployeeCurrentPositionQuery, Result<EmployeeCurrentPositionDto>>
{
    public async Task<Result<EmployeeCurrentPositionDto>> Handle(
        GetEmployeeCurrentPositionQuery request, CancellationToken ct)
    {
        var now = clock.Now;
        var result = await db.PositionAssignments
            .Where(a => a.IsActive && !a.IsVacant
                && a.StartDate <= now && (a.EndDate == null || a.EndDate >= now)
                && a.Employee.EmployeeCode == request.EmployeeCode)
            .Select(a => new EmployeeCurrentPositionDto(
                a.Employee.EmployeeCode,
                a.Employee.EmployeeName,
                a.Employee.Email,
                a.Position.PositionCode,
                a.Position.PositionName,
                a.Position.JobGrade.ToString(),
                a.Position.WfScopeType.ToString(),
                a.Position.Section.SectCode,
                a.Position.Section.SectName,
                a.Position.Section.Department.DeptCode,
                a.Position.Section.Department.DeptName,
                a.Position.Section.Department.Division.DivisionCode,
                a.Position.Section.Department.Division.DivisionName,
                a.StartDate
            ))
            .FirstOrDefaultAsync(ct);

        if (result == null)
            return Result<EmployeeCurrentPositionDto>.Fail("EMP_NOT_FOUND",
                $"Active position assignment for employee '{request.EmployeeCode}' not found.");

        return Result<EmployeeCurrentPositionDto>.Success(result);
    }
}
