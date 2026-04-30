using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Features.Workflows.Commands.SubmitWorkflow;

public class SubmitWorkflowHandler(
    IAppDbContext db,
    IWorkflowEngine engine,
    IDocumentNumberService docNoService,
    IDateTimeService clock)
    : IRequestHandler<SubmitWorkflowCommand, Result<SubmitWorkflowDto>>
{
    public async Task<Result<SubmitWorkflowDto>> Handle(SubmitWorkflowCommand request, CancellationToken ct)
    {
        var now = clock.Now;

        // 1. Validate submitter position
        var submitterPos = await db.Positions
            .Where(p => p.PositionCode == request.SubmitterPositionCode && p.IsActive)
            .Select(p => new { p.PositionId, p.WfScopeType, p.IsChiefLevel, p.SecretaryPositionId })
            .FirstOrDefaultAsync(ct);

        if (submitterPos == null)
            return Result<SubmitWorkflowDto>.Fail("WF_POSITION_NOT_FOUND",
                $"Position '{request.SubmitterPositionCode}' not found.");

        // 2. Validate submitter has active assignment
        var hasAssignment = await db.PositionAssignments
            .AnyAsync(a => a.PositionId == submitterPos.PositionId
                && !a.IsVacant
                && a.StartDate <= now && (a.EndDate == null || a.EndDate >= now)
                && (a.Employee.EndDate == null || a.Employee.EndDate >= now)
                && a.EmployeeId == request.SubmitterEmployeeId, ct);

        if (!hasAssignment)
            return Result<SubmitWorkflowDto>.Fail("WF_UNAUTHORIZED",
                "Employee does not have an active assignment for this position.");

        // 3. Validate ActingAs position (Delegation)
        int? actingAsPositionId = null;
        if (!string.IsNullOrEmpty(request.ActingAsPositionCode))
        {
            var actingPos = await db.Positions
                .Where(p => p.PositionCode == request.ActingAsPositionCode && p.IsActive)
                .Select(p => (int?)p.PositionId)
                .FirstOrDefaultAsync(ct);

            if (actingPos == null)
                return Result<SubmitWorkflowDto>.Fail("WF_POSITION_NOT_FOUND",
                    $"ActingAs position '{request.ActingAsPositionCode}' not found.");

            actingAsPositionId = actingPos;
        }

        // 4. Select Template
        var templateReq = new TemplateSelectionRequest(
            request.DocCode, request.TotalAmount,
            request.IsSpecialItem, request.IsUrgent,
            submitterPos.WfScopeType.ToString());

        var (template, errCode, errMsg) = await engine.SelectTemplateAsync(templateReq, ct);
        if (template == null)
            return Result<SubmitWorkflowDto>.Fail(errCode!, errMsg!);

        // 5. Secretary Pre-Approval Logic (ต้องทำก่อน resolve approvers)
        // Position.SecretaryPositionId = PositionId ของเลขาของตำแหน่งนั้น (Chief.SecretaryPositionId = Secretary)
        // เมื่อเลขา Submit แทน Chief ต้อง query กลับ: หา Chief ที่มี SecretaryPositionId = ตำแหน่งของเลขานี้
        int? chiefPositionId = null;
        bool requiresPreApproval = false;

        if (request.IsCreatedBySecretary)
        {
            chiefPositionId = await db.Positions
                .Where(p => p.SecretaryPositionId == submitterPos.PositionId && p.IsActive)
                .Select(p => (int?)p.PositionId)
                .FirstOrDefaultAsync(ct);

            if (!chiefPositionId.HasValue)
                return Result<SubmitWorkflowDto>.Fail("WF_SECRETARY_NO_CHIEF",
                    $"Position '{request.SubmitterPositionCode}' is not registered as a secretary for any Chief.");

            requiresPreApproval = true;
        }

        // 6. Resolve all approvers
        // กรณีเลขา Submit แทน Chief → resolve chain จาก Chief's position (ไม่ใช่ Secretary)
        // เพราะ Flow การอนุมัติควรเดินจาก Chief ขึ้นไป ไม่ใช่จาก Secretary ขึ้นไป
        var resolveFromPositionId = requiresPreApproval ? chiefPositionId!.Value : submitterPos.PositionId;
        var resolvedApprovers = await engine.ResolveAllApproversAsync(template, resolveFromPositionId, ct);

        var unresolvedStep = template.Steps
            .OrderBy(s => s.StepOrder)
            .Select((step, index) => new { Step = step, Resolved = resolvedApprovers.ElementAtOrDefault(index) })
            .FirstOrDefault(x => x.Resolved == null);

        if (unresolvedStep != null)
            return Result<SubmitWorkflowDto>.Fail("WF_APPROVER_NOT_RESOLVED",
                $"Could not resolve approver for step {unresolvedStep.Step.StepOrder} ({unresolvedStep.Step.StepName}).");

        // 7. Generate document number
        var docNo = await docNoService.GenerateAsync(request.DocCode, ct);

        // 8. Create WorkflowInstance
        var instance = new WorkflowInstance
        {
            InstanceId = Guid.NewGuid(),
            TemplateId = template.TemplateId,
            DocumentNo = docNo,
            SubmitterPositionId = submitterPos.PositionId,
            SubmitterEmployeeId = request.SubmitterEmployeeId,
            ActingAsPositionId = actingAsPositionId,
            Status = requiresPreApproval ? WorkflowStatus.Draft : WorkflowStatus.Pending,
            PreApprovalStatus = requiresPreApproval ? PreApprovalStatus.Pending : PreApprovalStatus.NotRequired,
            CreatedBySecretaryPositionId = requiresPreApproval ? submitterPos.PositionId : null,
            PreApprovalChiefPositionId = chiefPositionId,
            TotalAmount = request.TotalAmount,
            IsSpecialItem = request.IsSpecialItem,
            IsUrgent = request.IsUrgent,
            Subject = request.Subject,
            Remark = request.Remark,
            SubmittedAt = now,
            CreatedAt = now,
            CreatedBy = request.SubmitterEmployeeId.ToString()
        };

        db.WorkflowInstances.Add(instance);

        // 9. Create WorkflowApproval records (pre-resolved)
        var stepList = template.Steps.OrderBy(s => s.StepOrder).ToList();
        var approvalStepDtos = new List<SubmitApprovalStepDto>();

        for (int i = 0; i < stepList.Count; i++)
        {
            var step = stepList[i];
            var resolved = resolvedApprovers[i]!;

            db.WorkflowApprovals.Add(new WorkflowApproval
            {
                InstanceId = instance.InstanceId,
                StepId = step.StepId,
                StepOrder = step.StepOrder,
                AssignedPositionId = resolved.PositionId,
                Status = ApprovalStatus.Pending,
                EscalatedFromPositionId = resolved.EscalatedFromPositionId,
                CreatedAt = now,
                CreatedBy = request.SubmitterEmployeeId.ToString()
            });

            approvalStepDtos.Add(new SubmitApprovalStepDto(
                step.StepOrder, step.StepName,
                resolved.PositionCode, resolved.PositionName,
                resolved.OccupantName,
                resolved.OccupantNames.ToList(),
                resolved.OccupantCount,
                resolved.WasEscalated,
                resolved.DelegatedToPositionCode));
        }

        // 10. Insert WorkflowHistory
        db.WorkflowHistories.Add(new WorkflowHistory
        {
            InstanceId = instance.InstanceId,
            EventType = "Submit",
            ActorPositionId = submitterPos.PositionId,
            ActorEmployeeId = request.SubmitterEmployeeId,
            Comment = request.Subject,
            OccurredAt = now
        });

        await db.SaveChangesAsync(ct);

        return Result<SubmitWorkflowDto>.Success(new SubmitWorkflowDto(
            instance.InstanceId, docNo,
            template.FlowCode, template.FlowDesc,
            requiresPreApproval, instance.Status.ToString(),
            approvalStepDtos));
    }
}
