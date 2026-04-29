using EWS.Application.Common.Interfaces;
using EWS.Application.Features.Workflows.Commands.SubmitWorkflow;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using EWS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Tests;

public class SubmitWorkflowHandlerTests
{
    [Fact]
    public async Task Handle_UnresolvedApprover_FailsAndDoesNotCreateInstance()
    {
        await using var db = CreateDbContext();
        var employeeId = Guid.NewGuid();
        var now = new DateTime(2026, 4, 29, 12, 0, 0);

        db.Positions.Add(new Position
        {
            PositionId = 1,
            PositionCode = "REQ01",
            PositionName = "Requester",
            WfScopeType = WfScopeType.Ho,
            IsActive = true
        });
        db.PositionAssignments.Add(new PositionAssignment
        {
            AssignmentId = 1,
            PositionId = 1,
            EmployeeId = employeeId,
            IsActive = true,
            IsVacant = false,
            StartDate = now.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var template = new WorkflowTemplate
        {
            TemplateId = 10,
            FlowCode = 100,
            FlowDesc = "Test flow",
            Steps =
            [
                new WorkflowStep { StepId = 1, StepOrder = 1, StepName = "Missing approver" }
            ]
        };
        var engine = new StubWorkflowEngine(template, [null]);
        var handler = new SubmitWorkflowHandler(
            db,
            engine,
            new StubDocumentNumberService(),
            new FixedClock(now));

        var command = new SubmitWorkflowCommand(
            DocCode: 1001,
            SubmitterPositionCode: "REQ01",
            SubmitterEmployeeId: employeeId,
            ActingAsPositionCode: null,
            TotalAmount: 100,
            IsSpecialItem: false,
            IsUrgent: false,
            Subject: "Test",
            Remark: null,
            IsCreatedBySecretary: false);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("WF_APPROVER_NOT_RESOLVED", result.ErrorCode);
        Assert.Empty(db.WorkflowInstances);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class StubWorkflowEngine(
        WorkflowTemplate template,
        List<ResolvedApprover?> approvers) : IWorkflowEngine
    {
        public Task<(WorkflowTemplate? Template, string? ErrorCode, string? ErrorMessage)> SelectTemplateAsync(
            TemplateSelectionRequest request,
            CancellationToken ct = default)
        {
            return Task.FromResult<(WorkflowTemplate?, string?, string?)>((template, null, null));
        }

        public Task<List<ResolvedApprover?>> ResolveAllApproversAsync(
            WorkflowTemplate template,
            int submitterPositionId,
            CancellationToken ct = default)
        {
            return Task.FromResult(approvers);
        }
    }

    private sealed class StubDocumentNumberService : IDocumentNumberService
    {
        public Task<string> GenerateAsync(int docCode, CancellationToken ct = default)
            => Task.FromResult("DOC-2026-00001");
    }

    private sealed class FixedClock(DateTime now) : IDateTimeService
    {
        public DateTime Now => now;
    }
}
