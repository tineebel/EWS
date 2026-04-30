using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Settings.Queries.GetDelegationDetail;

public record GetDelegationDetailQuery(int DelegationId)
    : IRequest<Result<DelegationDetailDto>>;

public record DelegationDetailDto(
    int DelegationId,
    string FromPositionCode,
    string FromPositionName,
    string ToPositionCode,
    string ToPositionName,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason,
    bool IsActive,
    bool IsCurrentlyActive,
    string CreatedAt,
    string? UpdatedAt);
