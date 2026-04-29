using EWS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Common;

public static class WorkflowActorVerifier
{
    public static Task<bool> HasActiveAssignmentAsync(
        IAppDbContext db,
        int positionId,
        Guid employeeId,
        DateTime now,
        CancellationToken ct = default)
    {
        return db.PositionAssignments.AnyAsync(a =>
            a.PositionId == positionId &&
            a.EmployeeId == employeeId &&
            a.IsActive &&
            !a.IsVacant &&
            a.StartDate <= now &&
            (a.EndDate == null || a.EndDate >= now), ct);
    }
}
