using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.ListPositions;

public record ListPositionsQuery(string? Search, bool? IsActive, string? DeptCode, string? SectionCode, int Page, int PageSize)
    : IRequest<Result<PaginatedList<PositionDto>>>;

public record PositionDto(
    int PositionId,
    string PositionCode,
    string PositionName,
    string? PositionShortName,
    string JobGrade,
    string WfScopeType,
    bool IsChiefLevel,
    bool IsActive,
    int SectionId,
    string SectionCode,
    string? SectionShortCode,
    string SectionName,
    string DeptCode,
    string? DeptShortCode,
    string DeptName,
    int? ParentPositionId,
    string? ParentPositionCode,
    int? SecretaryPositionId,
    string? SecretaryPositionCode);
