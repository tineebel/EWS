using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Organization.Queries.GetEmployeeCurrentPosition;

public record GetEmployeeCurrentPositionQuery(string EmployeeCode)
    : IRequest<Result<EmployeeCurrentPositionDto>>;

public record EmployeeCurrentPositionDto(
    string EmployeeCode,
    string EmployeeName,
    string? Email,
    string PositionCode,
    string PositionName,
    string JobGrade,
    string WfScopeType,
    string SectCode,
    string SectName,
    string DeptCode,
    string DeptName,
    string DivisionCode,
    string DivisionName,
    DateTime AssignmentStart
);
