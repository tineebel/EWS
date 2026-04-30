using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Commands.CreateDelegation;

public class CreateDelegationHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<CreateDelegationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateDelegationCommand request, CancellationToken ct)
    {
        var fromPosition = await db.Positions
            .Where(p => p.PositionCode == request.FromPositionCode && p.IsActive)
            .Select(p => new { p.PositionId, p.PositionCode })
            .FirstOrDefaultAsync(ct);

        if (fromPosition is null)
            return Result<int>.Fail("DELEGATION_FROM_POSITION_NOT_FOUND", $"From position '{request.FromPositionCode}' not found.");

        var toPosition = await db.Positions
            .Where(p => p.PositionCode == request.ToPositionCode && p.IsActive)
            .Select(p => new { p.PositionId, p.PositionCode })
            .FirstOrDefaultAsync(ct);

        if (toPosition is null)
            return Result<int>.Fail("DELEGATION_TO_POSITION_NOT_FOUND", $"To position '{request.ToPositionCode}' not found.");

        if (fromPosition.PositionId == toPosition.PositionId)
            return Result<int>.Fail("DELEGATION_INVALID_TARGET", "From position and to position must be different.");

        if (request.IsActive)
        {
            var hasFromOverlap = await db.Delegations.AnyAsync(d =>
                d.FromPositionId == fromPosition.PositionId &&
                d.IsActive &&
                d.StartDate <= request.EndDate &&
                d.EndDate >= request.StartDate, ct);

            if (hasFromOverlap)
                return Result<int>.Fail("DELEGATION_OVERLAP",
                    $"Position '{request.FromPositionCode}' already has an overlapping active delegation.");

            var hasToOverlap = await db.Delegations.AnyAsync(d =>
                d.ToPositionId == toPosition.PositionId &&
                d.IsActive &&
                d.StartDate <= request.EndDate &&
                d.EndDate >= request.StartDate, ct);

            if (hasToOverlap)
                return Result<int>.Fail("DELEGATION_TARGET_OVERLAP",
                    $"Position '{request.ToPositionCode}' already receives an overlapping active delegation.");
        }

        var now = clock.Now;
        var delegation = new Delegation
        {
            FromPositionId = fromPosition.PositionId,
            ToPositionId = toPosition.PositionId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            IsActive = request.IsActive,
            CreatedAt = now,
            CreatedBy = request.ChangedBy
        };

        db.Delegations.Add(delegation);
        await db.SaveChangesAsync(ct);

        return Result<int>.Success(delegation.DelegationId);
    }
}
