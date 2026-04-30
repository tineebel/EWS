using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.GetPositionDetail;

public class GetPositionDetailHandler(IAppDbContext db)
    : IRequestHandler<GetPositionDetailQuery, Result<PositionDetailDto>>
{
    public async Task<Result<PositionDetailDto>> Handle(GetPositionDetailQuery req, CancellationToken ct)
    {
        var position = await db.Positions
            .Include(p => p.Section)
                .ThenInclude(s => s.Department)
            .Include(p => p.ParentPosition)
            .Include(p => p.SecretaryPosition)
            .Include(p => p.Assignments)
                .ThenInclude(pa => pa.Employee)
            .FirstOrDefaultAsync(p => p.PositionCode == req.PositionCode, ct);

        if (position is null)
            return Result<PositionDetailDto>.Fail("POSITION_NOT_FOUND", $"Position '{req.PositionCode}' not found.");

        var now = DateTime.UtcNow.AddHours(7);

        var occupants = position.Assignments
            .OrderByDescending(pa => pa.StartDate)
            .Select(pa => new PositionOccupantDto(
                pa.AssignmentId,
                pa.Employee.EmployeeCode,
                pa.Employee.EmployeeName,
                pa.Employee.EmployeeNameEn,
                pa.Employee.Email,
                pa.StartDate,
                pa.EndDate,
                pa.IsActive && pa.StartDate <= now && (pa.EndDate == null || pa.EndDate >= now)))
            .ToList();

        var dto = new PositionDetailDto(
            position.PositionId,
            position.PositionCode,
            position.PositionName,
            position.PositionShortName,
            position.JobGrade.ToString(),
            position.WfScopeType.ToString(),
            position.Section.SectCode,
            position.Section.SectShortCode,
            position.Section.SectName,
            position.Section.Department.DeptCode,
            position.Section.Department.DeptShortCode,
            position.Section.Department.DeptName,
            position.ParentPosition?.PositionCode,
            position.ParentPosition?.PositionName,
            position.SecretaryPosition?.PositionCode,
            position.IsChiefLevel,
            position.IsActive,
            occupants);

        return Result<PositionDetailDto>.Success(dto);
    }
}
