using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.ListEmployees;

public record ListEmployeesQuery(string? Search, string? Status, string? DeptCode, string? SectionCode, int Page, int PageSize)
    : IRequest<Result<PaginatedList<EmployeeDto>>>;

public record EmployeeDto(
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
    List<string> PositionCodes);
