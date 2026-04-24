using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListDelegations;

public class ListDelegationsHandler(IAppDbContext db)
    : IRequestHandler<ListDelegationsQuery, Result<List<DelegationDto>>>
{
    public async Task<Result<List<DelegationDto>>> Handle(ListDelegationsQuery req, CancellationToken ct)
    {
        var now = DateTime.UtcNow.AddHours(7);

        var q = db.Delegations
            .Include(d => d.FromPosition)
            .Include(d => d.ToPosition)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.PositionCode))
            q = q.Where(d =>
                d.FromPosition.PositionCode == req.PositionCode ||
                d.ToPosition.PositionCode == req.PositionCode);

        if (req.ActiveOnly == true)
            q = q.Where(d => d.StartDate <= now && d.EndDate >= now);

        var result = await q.OrderByDescending(d => d.StartDate)
            .Select(d => new DelegationDto(
                d.DelegationId,
                d.FromPosition.PositionCode,
                d.FromPosition.PositionName,
                d.ToPosition.PositionCode,
                d.ToPosition.PositionName,
                d.StartDate,
                d.EndDate,
                d.Reason,
                d.StartDate <= now && d.EndDate >= now,
                d.CreatedAt.ToString("yyyy-MM-dd HH:mm")))
            .ToListAsync(ct);

        return Result<List<DelegationDto>>.Success(result);
    }
}
