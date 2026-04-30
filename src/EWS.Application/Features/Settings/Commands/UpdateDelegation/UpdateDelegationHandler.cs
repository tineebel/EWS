using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Commands.UpdateDelegation;

public class UpdateDelegationHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<UpdateDelegationCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateDelegationCommand request, CancellationToken ct)
    {
        var delegation = await db.Delegations
            .FirstOrDefaultAsync(d => d.DelegationId == request.DelegationId, ct);

        if (delegation is null)
            return Result<int>.Fail("DELEGATION_NOT_FOUND", $"Delegation {request.DelegationId} not found.");

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
                d.DelegationId != request.DelegationId &&
                d.FromPositionId == fromPosition.PositionId &&
                d.IsActive &&
                d.StartDate <= request.EndDate &&
                d.EndDate >= request.StartDate, ct);

            if (hasFromOverlap)
                return Result<int>.Fail("DELEGATION_OVERLAP",
                    $"Position '{request.FromPositionCode}' already has an overlapping active delegation.");

            var hasToOverlap = await db.Delegations.AnyAsync(d =>
                d.DelegationId != request.DelegationId &&
                d.ToPositionId == toPosition.PositionId &&
                d.IsActive &&
                d.StartDate <= request.EndDate &&
                d.EndDate >= request.StartDate, ct);

            if (hasToOverlap)
                return Result<int>.Fail("DELEGATION_TARGET_OVERLAP",
                    $"Position '{request.ToPositionCode}' already receives an overlapping active delegation.");
        }

        delegation.FromPositionId = fromPosition.PositionId;
        delegation.ToPositionId = toPosition.PositionId;
        delegation.StartDate = request.StartDate;
        delegation.EndDate = request.EndDate;
        delegation.Reason = request.Reason;
        delegation.IsActive = request.IsActive;
        delegation.UpdatedAt = clock.Now;
        delegation.UpdatedBy = request.ChangedBy;

        await db.SaveChangesAsync(ct);
        return Result<int>.Success(delegation.DelegationId);
    }
}
