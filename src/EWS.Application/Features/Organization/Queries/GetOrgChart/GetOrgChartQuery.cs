using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Organization.Queries.GetOrgChart;

public record GetOrgChartQuery(string? RootCode, string? BranchCode = null, string? DeptCode = null, string? SectionCode = null) : IRequest<Result<List<OrgChartNodeDto>>>;

public record OrgChartNodeDto(
    int PositionId,
    string PositionCode,
    string PositionName,
    string JobGrade,
    bool IsChiefLevel,
    bool IsVacant,
    string? OccupantName,
    string? SecretaryCode,
    List<OrgChartNodeDto> Children
);
