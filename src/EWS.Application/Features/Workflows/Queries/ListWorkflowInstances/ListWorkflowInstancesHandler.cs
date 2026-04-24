using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Queries.ListWorkflowInstances;

public class ListWorkflowInstancesHandler(IAppDbContext db)
    : IRequestHandler<ListWorkflowInstancesQuery, Result<PaginatedList<WorkflowInstanceSummaryDto>>>
{
    public async Task<Result<PaginatedList<WorkflowInstanceSummaryDto>>> Handle(
        ListWorkflowInstancesQuery request, CancellationToken ct)
    {
        var query = db.WorkflowInstances.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<WorkflowStatus>(request.Status, true, out var status))
            query = query.Where(x => x.Status == status);

        if (request.DocCode.HasValue)
            query = query.Where(x => x.Template.DocumentType.DocCode == request.DocCode);

        if (!string.IsNullOrEmpty(request.PositionCode))
            query = query.Where(x =>
                x.SubmitterPosition.PositionCode == request.PositionCode ||
                x.Approvals.Any(a => a.AssignedPosition.PositionCode == request.PositionCode));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.SubmittedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new WorkflowInstanceSummaryDto(
                x.InstanceId, x.DocumentNo,
                x.Template.DocumentType.DocName,
                x.Template.FlowDesc,
                x.Status.ToString(),
                x.SubmitterPosition.PositionCode,
                x.TotalAmount,
                x.SubmittedAt,
                x.Approvals.Where(a => a.Status == ApprovalStatus.Pending)
                    .OrderBy(a => a.StepOrder)
                    .Select(a => a.StepOrder)
                    .FirstOrDefault(),
                x.Approvals.Where(a => a.Status == ApprovalStatus.Pending)
                    .OrderBy(a => a.StepOrder)
                    .Select(a => a.AssignedPosition.PositionCode)
                    .FirstOrDefault()
            ))
            .ToListAsync(ct);

        return Result<PaginatedList<WorkflowInstanceSummaryDto>>.Success(
            new PaginatedList<WorkflowInstanceSummaryDto>(items, total, request.Page, request.PageSize));
    }
}
