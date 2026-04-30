using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Organization.Queries.GetApprovalChain;

public class GetApprovalChainHandler(IAppDbContext db, IWorkflowEngine engine)
    : IRequestHandler<GetApprovalChainQuery, Result<ApprovalChainDto>>
{
    public async Task<Result<ApprovalChainDto>> Handle(GetApprovalChainQuery request, CancellationToken ct)
    {
        var submitter = await db.Positions
            .Where(p => p.PositionCode == request.SubmitterPositionCode && p.IsActive)
            .Select(p => new { p.PositionId, p.WfScopeType })
            .FirstOrDefaultAsync(ct);

        if (submitter == null)
            return Result<ApprovalChainDto>.Fail("ORG_POSITION_NOT_FOUND",
                $"Submitter position '{request.SubmitterPositionCode}' not found.");

        var templateRequest = new TemplateSelectionRequest(
            request.DocCode, request.Amount,
            request.IsSpecialItem, request.IsUrgent,
            submitter.WfScopeType.ToString());

        var (template, errCode, errMsg) = await engine.SelectTemplateAsync(templateRequest, ct);
        if (template == null)
            return Result<ApprovalChainDto>.Fail(errCode!, errMsg!);

        var resolved = await engine.ResolveAllApproversAsync(template, submitter.PositionId, ct);
        var stepResults = new List<ApprovalChainStepDto>();
        var index = 0;
        foreach (var step in template.Steps.OrderBy(s => s.StepOrder))
        {
            var r = resolved.ElementAtOrDefault(index++);

            stepResults.Add(new ApprovalChainStepDto(
                step.StepOrder, step.StepName,
                step.ApproverType.ToString(), step.SpecificPositionCode,
                r?.PositionCode ?? "-", r?.PositionName ?? "-",
                r?.WasEscalated ?? false, r?.EscalationDepth ?? 0,
                r?.OccupantName,
                r?.OccupantNames.ToList() ?? [],
                r?.OccupantCount ?? 0,
                r?.IsVacant ?? true,
                r?.DelegatedToPositionCode,
                r == null ? "WF_APPROVER_NOT_RESOLVED" : null,
                r == null ? "Could not resolve approver for this step." : null
            ));
        }

        return Result<ApprovalChainDto>.Success(new ApprovalChainDto(
            request.DocCode, template.FlowCode, template.FlowDesc,
            template.WfScopeType.ToString(), template.Condition1,
            stepResults));
    }
}
