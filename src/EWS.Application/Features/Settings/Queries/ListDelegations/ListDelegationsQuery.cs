using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.ListDelegations;

public record ListDelegationsQuery(string? PositionCode, bool? ActiveOnly)
    : IRequest<Result<List<DelegationDto>>>;

public record DelegationDto(
    int DelegationId,
    string FromPositionCode,
    string FromPositionName,
    string ToPositionCode,
    string ToPositionName,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason,
    bool IsCurrentlyActive,
    string CreatedAt);
