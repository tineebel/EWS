using EWS.Application.Common;
using EWS.Application.Common.Interfaces;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using EWS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EWS.Infrastructure.Services;

public class WorkflowEngine(AppDbContext db, IApproverResolver approverResolver) : IWorkflowEngine
{
    public async Task<(WorkflowTemplate? Template, string? ErrorCode, string? ErrorMessage)> SelectTemplateAsync(
        TemplateSelectionRequest request,
        CancellationToken ct = default)
    {
        var docType = await db.DocumentTypes
            .Where(d => d.DocCode == request.DocCode && d.IsActive)
            .Select(d => d.DocumentTypeId)
            .FirstOrDefaultAsync(ct);

        if (docType == 0)
            return (null, "WF_TEMPLATE_NOT_FOUND", $"Document type {request.DocCode} not found.");

        var candidates = await db.WorkflowTemplates
            .Include(t => t.Steps.Where(s => s.IsActive).OrderBy(s => s.StepOrder))
            .Where(t => t.DocumentTypeId == docType
                && t.IsActive
                && t.HasSpecialItem == request.IsSpecialItem
                && t.IsUrgent == request.IsUrgent)
            .ToListAsync(ct);

        // กรอง Scope: Branch/Ho/All
        var scopeMatches = candidates.Where(t =>
            t.WfScopeType == WfScopeType.All ||
            t.WfScopeType.ToString() == request.WfScopeType
        ).ToList();

        // กรอง Condition Amount
        var conditionMatches = scopeMatches.Where(t =>
            WorkflowConditionEvaluator.Evaluate(t.Condition1, request.Amount) &&
            WorkflowConditionEvaluator.Evaluate(t.Condition2, request.Amount) &&
            WorkflowConditionEvaluator.Evaluate(t.Condition3, request.Amount) &&
            WorkflowConditionEvaluator.Evaluate(t.Condition4, request.Amount) &&
            WorkflowConditionEvaluator.Evaluate(t.Condition5, request.Amount)
        ).ToList();

        if (conditionMatches.Count == 0)
            return (null, "WF_TEMPLATE_NOT_FOUND",
                $"No workflow template found for doc={request.DocCode}, amount={request.Amount}, scope={request.WfScopeType}.");

        // Prefer specific scope over All
        var specific = conditionMatches
            .Where(t => t.WfScopeType.ToString() == request.WfScopeType)
            .ToList();

        var final = specific.Count > 0 ? specific : conditionMatches;

        if (final.Count > 1)
            return (null, "WF_TEMPLATE_AMBIGUOUS",
                $"Multiple templates matched for doc={request.DocCode}. FlowCodes: {string.Join(",", final.Select(t => t.FlowCode))}.");

        return (final[0], null, null);
    }

    public async Task<List<ResolvedApprover?>> ResolveAllApproversAsync(
        WorkflowTemplate template,
        int submitterPositionId,
        CancellationToken ct = default)
    {
        var results = new List<ResolvedApprover?>();

        foreach (var step in template.Steps.OrderBy(s => s.StepOrder))
        {
            var resolved = await approverResolver.ResolveAsync(
                submitterPositionId,
                step.ApproverType,
                step.SpecificPositionCode,
                ct);

            results.Add(resolved);
        }

        return results;
    }
}
