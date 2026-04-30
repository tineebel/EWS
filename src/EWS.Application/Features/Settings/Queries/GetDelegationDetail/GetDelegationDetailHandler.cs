using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.GetDelegationDetail;

public class GetDelegationDetailHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<GetDelegationDetailQuery, Result<DelegationDetailDto>>
{
    public async Task<Result<DelegationDetailDto>> Handle(GetDelegationDetailQuery request, CancellationToken ct)
    {
        var now = clock.Now;

        var delegation = await db.Delegations
            .Where(d => d.DelegationId == request.DelegationId)
            .Select(d => new DelegationDetailDto(
                d.DelegationId,
                d.FromPosition.PositionCode,
                d.FromPosition.PositionName,
                d.ToPosition.PositionCode,
                d.ToPosition.PositionName,
                d.StartDate,
                d.EndDate,
                d.Reason,
                d.IsActive,
                d.IsActive && d.StartDate <= now && d.EndDate >= now,
                d.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                d.UpdatedAt.HasValue ? d.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm") : null))
            .FirstOrDefaultAsync(ct);

        return delegation is null
            ? Result<DelegationDetailDto>.Fail("DELEGATION_NOT_FOUND", $"Delegation {request.DelegationId} not found.")
            : Result<DelegationDetailDto>.Success(delegation);
    }
}
