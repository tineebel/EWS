using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Settings.Queries.ListWorkflowTemplates;

public class ListWorkflowTemplatesHandler(IAppDbContext db)
    : IRequestHandler<ListWorkflowTemplatesQuery, Result<List<WorkflowTemplateDto>>>
{
    public async Task<Result<List<WorkflowTemplateDto>>> Handle(ListWorkflowTemplatesQuery req, CancellationToken ct)
    {
        var q = db.WorkflowTemplates
            .Include(t => t.DocumentType)
            .Include(t => t.Steps)
            .AsQueryable();

        if (req.DocCode.HasValue)
            q = q.Where(t => t.DocumentType.DocCode == req.DocCode.Value);

        if (req.IsActive.HasValue)
            q = q.Where(t => t.IsActive == req.IsActive.Value);

        var result = await q.OrderBy(t => t.DocumentType.DocCode).ThenBy(t => t.FlowCode)
            .Select(t => new WorkflowTemplateDto(
                t.TemplateId,
                t.DocumentType.DocCode,
                t.DocumentType.DocName,
                t.FlowCode,
                t.FlowDesc,
                t.WfScopeType.ToString(),
                t.HasSpecialItem,
                t.IsUrgent,
                t.Condition1,
                t.Condition2,
                t.IsActive,
                t.Steps.OrderBy(s => s.StepOrder).Select(s => new WorkflowStepDto(
                    s.StepId,
                    s.StepOrder,
                    s.StepName,
                    s.ApproverType.ToString(),
                    s.SpecificPositionCode,
                    s.IsRequired)).ToList()))
            .ToListAsync(ct);

        return Result<List<WorkflowTemplateDto>>.Success(result);
    }
}
