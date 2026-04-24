using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.GetEmployeeDetail;

public record GetEmployeeDetailQuery(string EmployeeCode)
    : IRequest<Result<EmployeeDetailDto>>;

public record EmployeeDetailDto(
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string? EmployeeNameEn,
    string? Nickname,
    string? Email,
    string? Tel,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsTest,
    List<PositionAssignmentDto> PositionAssignments);

public record PositionAssignmentDto(
    int AssignmentId,
    string PositionCode,
    string PositionName,
    string? PositionShortName,
    string JobGrade,
    string SectionName,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent);
