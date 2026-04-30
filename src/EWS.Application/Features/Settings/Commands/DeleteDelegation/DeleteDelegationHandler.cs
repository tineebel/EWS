using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Commands.DeleteDelegation;

public class DeleteDelegationHandler(IAppDbContext db, IDateTimeService clock)
    : IRequestHandler<DeleteDelegationCommand, Result>
{
    public async Task<Result> Handle(DeleteDelegationCommand request, CancellationToken ct)
    {
        var delegation = await db.Delegations
            .FirstOrDefaultAsync(d => d.DelegationId == request.DelegationId, ct);

        if (delegation is null)
            return Result.Fail("DELEGATION_NOT_FOUND", $"Delegation {request.DelegationId} not found.");

        delegation.IsActive = false;
        delegation.UpdatedAt = clock.Now;
        delegation.UpdatedBy = request.ChangedBy;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
