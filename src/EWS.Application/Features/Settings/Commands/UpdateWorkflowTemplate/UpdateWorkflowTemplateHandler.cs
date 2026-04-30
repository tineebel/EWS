using EWS.Application.Common.Interfaces;
using EWS.Application.Common.Models;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EWS.Application.Features.Settings.Commands.UpdateWorkflowTemplate;

public class UpdateWorkflowTemplateHandler(IAppDbContext db)
    : IRequestHandler<UpdateWorkflowTemplateCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateWorkflowTemplateCommand req, CancellationToken ct)
    {
        if (!Enum.TryParse<WfScopeType>(req.WfScopeType, out var scopeType))
            return Result<int>.Fail("WF_INVALID_SCOPE", $"Invalid WfScopeType: {req.WfScopeType}");

        var stepApproverTypes = new Dictionary<UpdateStepDto, ApproverType>();
        foreach (var stepDto in req.Steps)
        {
            if (!Enum.TryParse<ApproverType>(stepDto.ApproverType, out var approverType))
                return Result<int>.Fail("WF_INVALID_APPROVER_TYPE", $"Invalid ApproverType: {stepDto.ApproverType}");

            stepApproverTypes[stepDto] = approverType;
        }

        Result<int> result = Result<int>.Fail("WF_TEMPLATE_UPDATE_FAILED", "Workflow template update failed.");
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            // 1. Load template with steps inside the transaction
            var template = await db.WorkflowTemplates
                .Include(t => t.Steps)
                .FirstOrDefaultAsync(t => t.TemplateId == req.TemplateId, ct);

            if (template is null)
            {
                result = Result<int>.Fail("WF_TEMPLATE_NOT_FOUND", $"WorkflowTemplate {req.TemplateId} not found.");
                return;
            }

            // 2. Take snapshot of current state before any changes
            var snapshotObj = new
            {
                templateId = template.TemplateId,
                documentTypeId = template.DocumentTypeId,
                flowCode = template.FlowCode,
                flowDesc = template.FlowDesc,
                wfScopeType = template.WfScopeType.ToString(),
                hasSpecialItem = template.HasSpecialItem,
                isUrgent = template.IsUrgent,
                condition1 = template.Condition1,
                condition2 = template.Condition2,
                condition3 = template.Condition3,
                condition4 = template.Condition4,
                condition5 = template.Condition5,
                isActive = template.IsActive,
                steps = template.Steps.OrderBy(s => s.StepOrder).Select(s => new
                {
                    stepId = s.StepId,
                    stepOrder = s.StepOrder,
                    stepName = s.StepName,
                    approverType = s.ApproverType.ToString(),
                    specificPositionCode = s.SpecificPositionCode,
                    escalationDays = s.EscalationDays,
                    isRequired = s.IsRequired,
                    isActive = s.IsActive
                }).ToList()
            };
            var snapshotJson = JsonSerializer.Serialize(snapshotObj);

            // 3. Get next version number
            var maxVersion = await db.WorkflowTemplateAuditLogs
                .Where(a => a.TemplateId == req.TemplateId)
                .MaxAsync(a => (int?)a.Version, ct) ?? 0;
            var nextVersion = maxVersion + 1;

            // 4. Determine change type
            string changeType = "Updated";
            if (!req.IsActive && template.IsActive)
                changeType = "Deactivated";
            else if (req.IsActive && !template.IsActive)
                changeType = "Activated";

            // 5. Create audit log entry
            var auditLog = new WorkflowTemplateAuditLog
            {
                TemplateId = req.TemplateId,
                Version = nextVersion,
                ChangeType = changeType,
                ChangedBy = req.ChangedBy,
                ChangedAt = DateTime.UtcNow,
                SnapshotJson = snapshotJson,
                ChangeNote = req.ChangeNote
            };
            db.WorkflowTemplateAuditLogs.Add(auditLog);

            // 6. Update template fields
            template.FlowDesc = req.FlowDesc;
            template.WfScopeType = scopeType;
            template.HasSpecialItem = req.HasSpecialItem;
            template.IsUrgent = req.IsUrgent;
            template.Condition1 = req.Condition1;
            template.Condition2 = req.Condition2;
            template.Condition3 = req.Condition3;
            template.Condition4 = req.Condition4;
            template.Condition5 = req.Condition5;
            template.IsActive = req.IsActive;
            template.UpdatedAt = DateTime.UtcNow;
            template.UpdatedBy = req.ChangedBy;

            // 7. Handle steps with a two-phase reorder to avoid hitting the filtered unique index
            var commandStepIds = req.Steps
                .Where(s => s.StepId.HasValue)
                .Select(s => s.StepId!.Value)
                .ToHashSet();

            var activeExistingSteps = template.Steps
                .Where(s => s.IsActive)
                .OrderBy(s => s.StepOrder)
                .ToList();

            var now = DateTime.UtcNow;
            var tempOrderSeed = req.Steps.Count + activeExistingSteps.Count + 1000;

            foreach (var existingStep in activeExistingSteps.Where(s => commandStepIds.Contains(s.StepId)))
            {
                existingStep.StepOrder = tempOrderSeed++;
                existingStep.UpdatedAt = now;
                existingStep.UpdatedBy = req.ChangedBy;
            }

            foreach (var existingStep in activeExistingSteps.Where(s => !commandStepIds.Contains(s.StepId)))
            {
                existingStep.IsActive = false;
                existingStep.UpdatedAt = now;
                existingStep.UpdatedBy = req.ChangedBy;
            }

            await db.SaveChangesAsync(ct);

            foreach (var stepDto in req.Steps)
            {
                if (stepDto.StepId.HasValue)
                {
                    var existingStep = template.Steps.FirstOrDefault(s => s.StepId == stepDto.StepId.Value);
                    if (existingStep is not null)
                    {
                        existingStep.StepOrder = stepDto.StepOrder;
                        existingStep.StepName = stepDto.StepName;
                        existingStep.ApproverType = stepApproverTypes[stepDto];
                        existingStep.SpecificPositionCode = stepDto.SpecificPositionCode;
                        existingStep.EscalationDays = stepDto.EscalationDays;
                        existingStep.IsRequired = stepDto.IsRequired;
                        existingStep.IsActive = true;
                        existingStep.UpdatedAt = now;
                        existingStep.UpdatedBy = req.ChangedBy;
                    }
                }
                else
                {
                    var newStep = new WorkflowStep
                    {
                        TemplateId = req.TemplateId,
                        StepOrder = stepDto.StepOrder,
                        StepName = stepDto.StepName,
                        ApproverType = stepApproverTypes[stepDto],
                        SpecificPositionCode = stepDto.SpecificPositionCode,
                        EscalationDays = stepDto.EscalationDays,
                        IsRequired = stepDto.IsRequired,
                        IsActive = true,
                        CreatedAt = now,
                        CreatedBy = req.ChangedBy
                    };
                    db.WorkflowSteps.Add(newStep);
                }
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            result = Result<int>.Success(template.TemplateId);
        });

        return result;
    }
}
