using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Queries.GetWorkflowInstance;

public class GetWorkflowInstanceHandler(IAppDbContext db)
    : IRequestHandler<GetWorkflowInstanceQuery, Result<WorkflowInstanceDetailDto>>
{
    public async Task<Result<WorkflowInstanceDetailDto>> Handle(
        GetWorkflowInstanceQuery request, CancellationToken ct)
    {
        var instance = await db.WorkflowInstances
            .Where(x => x.InstanceId == request.InstanceId)
            .Select(x => new
            {
                x.InstanceId, x.DocumentNo, x.Status, x.PreApprovalStatus,
                x.TotalAmount, x.IsSpecialItem, x.IsUrgent, x.Subject,
                x.SubmittedAt, x.CompletedAt,
                SubmitterCode = x.SubmitterPosition.PositionCode,
                SubmitterName = x.SubmitterPosition.PositionName,
                DocCode = x.Template.DocumentType.DocCode,
                DocName = x.Template.DocumentType.DocName,
                x.Template.FlowCode, x.Template.FlowDesc
            })
            .FirstOrDefaultAsync(ct);

        if (instance == null)
            return Result<WorkflowInstanceDetailDto>.Fail("WF_INSTANCE_NOT_FOUND", "Instance not found.");

        var approvals = await db.WorkflowApprovals
            .Where(a => a.InstanceId == request.InstanceId)
            .OrderBy(a => a.StepOrder)
            .Select(a => new WorkflowApprovalStepDto(
                a.StepOrder, a.Step.StepName,
                a.AssignedPosition.PositionCode,
                a.AssignedPosition.PositionName,
                db.PositionAssignments
                    .Where(pa => pa.PositionId == a.AssignedPositionId && pa.IsActive && !pa.IsVacant)
                    .Select(pa => pa.Employee.EmployeeName)
                    .FirstOrDefault(),
                a.Status.ToString(),
                a.ActorEmployee != null ? a.ActorEmployee.EmployeeName : null,
                a.ActionAt, a.Comment
            ))
            .ToListAsync(ct);

        var currentStep = approvals.FirstOrDefault(a => a.Status == "Pending");

        return Result<WorkflowInstanceDetailDto>.Success(new WorkflowInstanceDetailDto(
            instance.InstanceId, instance.DocumentNo,
            instance.DocCode, instance.DocName,
            instance.FlowCode, instance.FlowDesc,
            instance.Status.ToString(), instance.PreApprovalStatus.ToString(),
            instance.TotalAmount, instance.IsSpecialItem, instance.IsUrgent,
            instance.Subject, instance.SubmitterCode, instance.SubmitterName,
            instance.SubmittedAt, instance.CompletedAt,
            currentStep?.StepOrder ?? 0,
            currentStep?.ApproverPositionCode,
            approvals));
    }
}
