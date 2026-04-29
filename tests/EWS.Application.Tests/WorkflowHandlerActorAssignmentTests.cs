using EWS.Application.Features.Workflows.Commands.ApproveWorkflow;
using EWS.Application.Features.Workflows.Commands.RejectWorkflow;
using EWS.Application.Features.Workflows.Commands.RequestInfo;
using EWS.Application.Features.Workflows.Commands.RespondToInfoRequest;
using EWS.Application.Common.Interfaces;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using EWS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Tests;

public class WorkflowHandlerActorAssignmentTests
{
    private static readonly DateTime Now = new(2026, 4, 29, 12, 0, 0);

    [Fact]
    public async Task Approve_ActorEmployeeDoesNotHoldActorPosition_FailsUnauthorized()
    {
        await using var db = CreateDbContext();
        var instanceId = await SeedPendingApprovalAsync(db);
        var handler = new ApproveWorkflowHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new ApproveWorkflowCommand(
            instanceId,
            ActorPositionCode: "APPROVER",
            ActorEmployeeId: Guid.NewGuid(),
            Comment: "approve"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("WF_UNAUTHORIZED", result.ErrorCode);
    }

    [Fact]
    public async Task Approve_ActorEmployeeHoldsActorPosition_CompletesWorkflow()
    {
        await using var db = CreateDbContext();
        var actorEmployeeId = Guid.NewGuid();
        var instanceId = await SeedPendingApprovalAsync(db, actorEmployeeId);
        var handler = new ApproveWorkflowHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new ApproveWorkflowCommand(
            instanceId,
            ActorPositionCode: "APPROVER",
            ActorEmployeeId: actorEmployeeId,
            Comment: "approve"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsCompleted);
        Assert.Equal("Approved", result.Value.Status);
        Assert.Equal(WorkflowStatus.Approved, (await db.WorkflowInstances.SingleAsync()).Status);
        Assert.Equal(ApprovalStatus.Approved, (await db.WorkflowApprovals.SingleAsync()).Status);
    }

    [Fact]
    public async Task Reject_ActorEmployeeDoesNotHoldActorPosition_FailsUnauthorized()
    {
        await using var db = CreateDbContext();
        var instanceId = await SeedPendingApprovalAsync(db);
        var handler = new RejectWorkflowHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new RejectWorkflowCommand(
            instanceId,
            ActorPositionCode: "APPROVER",
            ActorEmployeeId: Guid.NewGuid(),
            Comment: "reject"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("WF_UNAUTHORIZED", result.ErrorCode);
    }

    [Fact]
    public async Task RequestInfo_ActorEmployeeDoesNotHoldFromPosition_FailsUnauthorized()
    {
        await using var db = CreateDbContext();
        var instanceId = await SeedInfoRequestScenarioAsync(db);
        var handler = new RequestInfoHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new RequestInfoCommand(
            instanceId,
            FromStepOrder: 2,
            ToStepOrder: 1,
            ActorPositionCode: "STEP2",
            ActorEmployeeId: Guid.NewGuid(),
            Question: "Need more detail"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("WF_UNAUTHORIZED", result.ErrorCode);
        Assert.Empty(db.WorkflowInfoRequests);
    }

    [Fact]
    public async Task RequestInfo_ActorEmployeeHoldsFromPosition_CreatesOpenRequest()
    {
        await using var db = CreateDbContext();
        var actorEmployeeId = Guid.NewGuid();
        var instanceId = await SeedInfoRequestScenarioAsync(db, step2EmployeeId: actorEmployeeId);
        var handler = new RequestInfoHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new RequestInfoCommand(
            instanceId,
            FromStepOrder: 2,
            ToStepOrder: 1,
            ActorPositionCode: "STEP2",
            ActorEmployeeId: actorEmployeeId,
            Question: "Need more detail"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Open", result.Value!.Status);
        Assert.Single(db.WorkflowInfoRequests);
    }

    [Fact]
    public async Task RespondInfo_ActorEmployeeDoesNotHoldToPosition_FailsUnauthorized()
    {
        await using var db = CreateDbContext();
        var infoRequestId = await SeedOpenInfoRequestAsync(db);
        var handler = new RespondToInfoRequestHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new RespondToInfoRequestCommand(
            infoRequestId,
            ActorPositionCode: "STEP1",
            ActorEmployeeId: Guid.NewGuid(),
            Answer: "Answer",
            ForwardToStepOrder: null,
            ForwardQuestion: null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("WF_UNAUTHORIZED", result.ErrorCode);
    }

    [Fact]
    public async Task RespondInfo_ActorEmployeeHoldsToPosition_ClosesRequest()
    {
        await using var db = CreateDbContext();
        var actorEmployeeId = Guid.NewGuid();
        var infoRequestId = await SeedOpenInfoRequestAsync(db, step1EmployeeId: actorEmployeeId);
        var handler = new RespondToInfoRequestHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new RespondToInfoRequestCommand(
            infoRequestId,
            ActorPositionCode: "STEP1",
            ActorEmployeeId: actorEmployeeId,
            Answer: "Answer",
            ForwardToStepOrder: null,
            ForwardQuestion: null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Answered", result.Value!.Action);
        Assert.Equal(InfoRequestStatus.Closed, (await db.WorkflowInfoRequests.SingleAsync()).Status);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<Guid> SeedPendingApprovalAsync(AppDbContext db, Guid? actorEmployeeId = null)
    {
        var instanceId = Guid.NewGuid();
        var employeeId = actorEmployeeId ?? Guid.NewGuid();

        db.Positions.Add(new Position
        {
            PositionId = 10,
            PositionCode = "APPROVER",
            PositionName = "Approver",
            IsActive = true
        });
        db.Employees.Add(new Employee
        {
            EmployeeId = employeeId,
            EmployeeCode = "E001",
            EmployeeName = "Approver",
            Status = EmployeeStatus.Active,
            StartDate = Now.AddYears(-1)
        });

        if (actorEmployeeId.HasValue)
        {
            db.PositionAssignments.Add(new PositionAssignment
            {
                AssignmentId = 10,
                PositionId = 10,
                EmployeeId = employeeId,
                IsActive = true,
                IsVacant = false,
                StartDate = Now.AddDays(-1)
            });
        }

        db.WorkflowInstances.Add(new WorkflowInstance
        {
            InstanceId = instanceId,
            DocumentNo = "DOC-2026-00001",
            Status = WorkflowStatus.Pending,
            PreApprovalStatus = PreApprovalStatus.NotRequired,
            SubmittedAt = Now
        });
        db.WorkflowApprovals.Add(new WorkflowApproval
        {
            ApprovalId = 1,
            InstanceId = instanceId,
            StepOrder = 1,
            AssignedPositionId = 10,
            Status = ApprovalStatus.Pending
        });

        await db.SaveChangesAsync();
        return instanceId;
    }

    private static async Task<Guid> SeedInfoRequestScenarioAsync(
        AppDbContext db,
        Guid? step2EmployeeId = null)
    {
        var instanceId = Guid.NewGuid();
        var employeeId = step2EmployeeId ?? Guid.NewGuid();

        db.Positions.AddRange(
            new Position { PositionId = 1, PositionCode = "STEP1", PositionName = "Step 1", IsActive = true },
            new Position { PositionId = 2, PositionCode = "STEP2", PositionName = "Step 2", IsActive = true });
        db.Employees.Add(new Employee
        {
            EmployeeId = employeeId,
            EmployeeCode = "E002",
            EmployeeName = "Step 2 Owner",
            Status = EmployeeStatus.Active,
            StartDate = Now.AddYears(-1)
        });

        if (step2EmployeeId.HasValue)
        {
            db.PositionAssignments.Add(new PositionAssignment
            {
                AssignmentId = 2,
                PositionId = 2,
                EmployeeId = employeeId,
                IsActive = true,
                IsVacant = false,
                StartDate = Now.AddDays(-1)
            });
        }

        db.WorkflowInstances.Add(new WorkflowInstance
        {
            InstanceId = instanceId,
            DocumentNo = "DOC-2026-00002",
            Status = WorkflowStatus.Pending,
            PreApprovalStatus = PreApprovalStatus.NotRequired,
            SubmittedAt = Now
        });
        db.WorkflowApprovals.AddRange(
            new WorkflowApproval
            {
                ApprovalId = 1,
                InstanceId = instanceId,
                StepOrder = 1,
                AssignedPositionId = 1,
                Status = ApprovalStatus.Approved
            },
            new WorkflowApproval
            {
                ApprovalId = 2,
                InstanceId = instanceId,
                StepOrder = 2,
                AssignedPositionId = 2,
                Status = ApprovalStatus.Pending
            });

        await db.SaveChangesAsync();
        return instanceId;
    }

    private static async Task<long> SeedOpenInfoRequestAsync(
        AppDbContext db,
        Guid? step1EmployeeId = null)
    {
        var instanceId = Guid.NewGuid();
        var employeeId = step1EmployeeId ?? Guid.NewGuid();

        db.Positions.AddRange(
            new Position { PositionId = 1, PositionCode = "STEP1", PositionName = "Step 1", IsActive = true },
            new Position { PositionId = 2, PositionCode = "STEP2", PositionName = "Step 2", IsActive = true });
        db.Employees.Add(new Employee
        {
            EmployeeId = employeeId,
            EmployeeCode = "E001",
            EmployeeName = "Step 1 Owner",
            Status = EmployeeStatus.Active,
            StartDate = Now.AddYears(-1)
        });

        if (step1EmployeeId.HasValue)
        {
            db.PositionAssignments.Add(new PositionAssignment
            {
                AssignmentId = 1,
                PositionId = 1,
                EmployeeId = employeeId,
                IsActive = true,
                IsVacant = false,
                StartDate = Now.AddDays(-1)
            });
        }

        db.WorkflowInstances.Add(new WorkflowInstance
        {
            InstanceId = instanceId,
            DocumentNo = "DOC-2026-00003",
            Status = WorkflowStatus.Pending,
            PreApprovalStatus = PreApprovalStatus.NotRequired,
            SubmittedAt = Now
        });
        db.WorkflowInfoRequests.Add(new WorkflowInfoRequest
        {
            InfoRequestId = 1,
            InstanceId = instanceId,
            FromStepOrder = 2,
            FromPositionId = 2,
            ToStepOrder = 1,
            ToPositionId = 1,
            Question = "Need details",
            Status = InfoRequestStatus.Open
        });

        await db.SaveChangesAsync();
        return 1;
    }

    private sealed class FixedClock(DateTime now) : IDateTimeService
    {
        public DateTime Now => now;
    }
}
