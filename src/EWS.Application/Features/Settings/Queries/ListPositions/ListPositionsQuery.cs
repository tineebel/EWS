using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.ListPositions;

public record ListPositionsQuery(string? Search, bool? IsActive, int Page, int PageSize)
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
    string SectionName,
    int? ParentPositionId,
    string? ParentPositionCode,
    int? SecretaryPositionId,
    string? SecretaryPositionCode);
