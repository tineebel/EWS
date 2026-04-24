using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Organization.Queries.GetPositionHierarchy;

public record GetPositionHierarchyQuery(string PositionCode) : IRequest<Result<List<PositionHierarchyDto>>>;

public record PositionHierarchyDto(
    int PositionId,
    string PositionCode,
    string PositionName,
    string JobGrade,
    string WfScopeType,
    bool IsChiefLevel,
    bool IsVacant,
    string? CurrentOccupant,
    int Level // 0 = ตำแหน่งที่ query, 1 = parent, 2 = grandparent...
);
