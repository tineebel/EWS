using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.GetTemplateHistory;

public class GetTemplateHistoryHandler(IAppDbContext db)
    : IRequestHandler<GetTemplateHistoryQuery, Result<List<TemplateAuditDto>>>
{
    public async Task<Result<List<TemplateAuditDto>>> Handle(GetTemplateHistoryQuery req, CancellationToken ct)
    {
        var history = await db.WorkflowTemplateAuditLogs
            .Where(a => a.TemplateId == req.TemplateId)
            .OrderByDescending(a => a.Version)
            .Select(a => new TemplateAuditDto(
                a.AuditId,
                a.Version,
                a.ChangeType,
                a.ChangedBy,
                a.ChangedAt,
                a.SnapshotJson,
                a.ChangeNote))
            .ToListAsync(ct);

        return Result<List<TemplateAuditDto>>.Success(history);
    }
}
