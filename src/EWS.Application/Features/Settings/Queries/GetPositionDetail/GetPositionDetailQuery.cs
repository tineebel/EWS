using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.GetPositionDetail;

public record GetPositionDetailQuery(string PositionCode)
    : IRequest<Result<PositionDetailDto>>;

public record PositionDetailDto(
    int PositionId,
    string PositionCode,
    string PositionName,
    string? PositionShortName,
    string JobGrade,
    string WfScopeType,
    string SectionCode,
    string? SectionShortCode,
    string SectionName,
    string DeptCode,
    string? DeptShortCode,
    string DeptName,
    string? ParentPositionCode,
    string? ParentPositionName,
    string? SecretaryPositionCode,
    bool IsChiefLevel,
    bool IsActive,
    List<PositionOccupantDto> Occupants);

public record PositionOccupantDto(
    int AssignmentId,
    string EmployeeCode,
    string EmployeeName,
    string? EmployeeNameEn,
    string? Email,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent);
