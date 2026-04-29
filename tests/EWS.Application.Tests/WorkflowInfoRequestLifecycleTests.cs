using EWS.Application.Common.Interfaces;
using EWS.Application.Features.Workflows.Commands.ApproveWorkflow;
using EWS.Application.Features.Workflows.Commands.RequestInfo;
using EWS.Application.Features.Workflows.Commands.RespondToInfoRequest;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using EWS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Tests;

public class WorkflowInfoRequestLifecycleTests
{
    private static readonly DateTime Now = new(2026, 4, 29, 12, 0, 0);

    [Fact]
    public async Task Approve_CancelsOpenInfoRequestsFromCurrentStep()
    {
        await using var db = CreateDbContext();
        var actorEmployeeId = Guid.NewGuid();
        var instanceId = await SeedWorkflowWithOpenInfoRequestAsync(db, actorEmployeeId);
        var handler = new ApproveWorkflowHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new ApproveWorkflowCommand(
            instanceId,
            ActorPositionCode: "STEP2",
            ActorEmployeeId: actorEmployeeId,
            Comment: "approve without waiting"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(InfoRequestStatus.Cancelled, (await db.WorkflowInfoRequests.SingleAsync()).Status);
    }

    [Fact]
    public async Task RequestInfo_DuplicateOpenRequestToSameStep_Fails()
    {
        await using var db = CreateDbContext();
        var actorEmployeeId = Guid.NewGuid();
        var instanceId = await SeedWorkflowWithOpenInfoRequestAsync(db, actorEmployeeId);
        var handler = new RequestInfoHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new RequestInfoCommand(
            instanceId,
            FromStepOrder: 2,
            ToStepOrder: 1,
            ActorPositionCode: "STEP2",
            ActorEmployeeId: actorEmployeeId,
            Question: "Same question again"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("WF_INFO_DUPLICATE", result.ErrorCode);
    }

    [Fact]
    public async Task RespondInfo_Forward_CreatesChildAndMarksParentForwarded()
    {
        await using var db = CreateDbContext();
        var actorEmployeeId = Guid.NewGuid();
        var infoRequestId = await SeedForwardableInfoRequestAsync(db, actorEmployeeId);
        var handler = new RespondToInfoRequestHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new RespondToInfoRequestCommand(
            infoRequestId,
            ActorPositionCode: "STEP2",
            ActorEmployeeId: actorEmployeeId,
            Answer: null,
            ForwardToStepOrder: 1,
            ForwardQuestion: "Please clarify"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Forwarded", result.Value!.Action);
        Assert.NotNull(result.Value.ChildInfoRequestId);

        var requests = await db.WorkflowInfoRequests.OrderBy(r => r.InfoRequestId).ToListAsync();
        Assert.Equal(2, requests.Count);
        Assert.Equal(InfoRequestStatus.Forwarded, requests[0].Status);
        Assert.Equal(InfoRequestStatus.Open, requests[1].Status);
        Assert.Equal(requests[0].InfoRequestId, requests[1].ParentInfoRequestId);
    }

    [Fact]
    public async Task RespondInfo_ChildAnswer_ReopensParentRequest()
    {
        await using var db = CreateDbContext();
        var actorEmployeeId = Guid.NewGuid();
        var childInfoRequestId = await SeedForwardedParentWithOpenChildAsync(db, actorEmployeeId);
        var handler = new RespondToInfoRequestHandler(db, new FixedClock(Now));

        var result = await handler.Handle(new RespondToInfoRequestCommand(
            childInfoRequestId,
            ActorPositionCode: "STEP1",
            ActorEmployeeId: actorEmployeeId,
            Answer: "Child answer",
            ForwardToStepOrder: null,
            ForwardQuestion: null), CancellationToken.None);

        Assert.True(result.IsSuccess);

        var parent = await db.WorkflowInfoRequests.SingleAsync(r => r.InfoRequestId == 1);
        var child = await db.WorkflowInfoRequests.SingleAsync(r => r.InfoRequestId == 2);
        Assert.Equal(InfoRequestStatus.Open, parent.Status);
        Assert.Equal(InfoRequestStatus.Closed, child.Status);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<Guid> SeedWorkflowWithOpenInfoRequestAsync(AppDbContext db, Guid step2EmployeeId)
    {
        var instanceId = Guid.NewGuid();
        SeedPositionsAndEmployee(db, step2EmployeeId, employeePositionId: 2);

        db.WorkflowInstances.Add(new WorkflowInstance
        {
            InstanceId = instanceId,
            DocumentNo = "DOC-2026-00010",
            Status = WorkflowStatus.Pending,
            PreApprovalStatus = PreApprovalStatus.NotRequired,
            SubmittedAt = Now
        });
        db.WorkflowApprovals.AddRange(
            new WorkflowApproval { ApprovalId = 1, InstanceId = instanceId, StepOrder = 1, AssignedPositionId = 1, Status = ApprovalStatus.Approved },
            new WorkflowApproval { ApprovalId = 2, InstanceId = instanceId, StepOrder = 2, AssignedPositionId = 2, Status = ApprovalStatus.Pending });
        db.WorkflowInfoRequests.Add(new WorkflowInfoRequest
        {
            InfoRequestId = 1,
            InstanceId = instanceId,
            FromStepOrder = 2,
            FromPositionId = 2,
            ToStepOrder = 1,
            ToPositionId = 1,
            Question = "Need detail",
            Status = InfoRequestStatus.Open
        });

        await db.SaveChangesAsync();
        return instanceId;
    }

    private static async Task<long> SeedForwardableInfoRequestAsync(AppDbContext db, Guid step2EmployeeId)
    {
        var instanceId = Guid.NewGuid();
        SeedPositionsAndEmployee(db, step2EmployeeId, employeePositionId: 2);

        db.WorkflowInstances.Add(new WorkflowInstance
        {
            InstanceId = instanceId,
            DocumentNo = "DOC-2026-00011",
            Status = WorkflowStatus.Pending,
            PreApprovalStatus = PreApprovalStatus.NotRequired,
            SubmittedAt = Now
        });
        db.WorkflowApprovals.AddRange(
            new WorkflowApproval { ApprovalId = 1, InstanceId = instanceId, StepOrder = 1, AssignedPositionId = 1, Status = ApprovalStatus.Approved },
            new WorkflowApproval { ApprovalId = 2, InstanceId = instanceId, StepOrder = 2, AssignedPositionId = 2, Status = ApprovalStatus.Approved },
            new WorkflowApproval { ApprovalId = 3, InstanceId = instanceId, StepOrder = 3, AssignedPositionId = 3, Status = ApprovalStatus.Pending });
        db.WorkflowInfoRequests.Add(new WorkflowInfoRequest
        {
            InfoRequestId = 1,
            InstanceId = instanceId,
            FromStepOrder = 3,
            FromPositionId = 3,
            ToStepOrder = 2,
            ToPositionId = 2,
            Question = "Need detail",
            Status = InfoRequestStatus.Open
        });

        await db.SaveChangesAsync();
        return 1;
    }

    private static async Task<long> SeedForwardedParentWithOpenChildAsync(AppDbContext db, Guid step1EmployeeId)
    {
        var instanceId = Guid.NewGuid();
        SeedPositionsAndEmployee(db, step1EmployeeId, employeePositionId: 1);

        db.WorkflowInstances.Add(new WorkflowInstance
        {
            InstanceId = instanceId,
            DocumentNo = "DOC-2026-00012",
            Status = WorkflowStatus.Pending,
            PreApprovalStatus = PreApprovalStatus.NotRequired,
            SubmittedAt = Now
        });
        db.WorkflowInfoRequests.AddRange(
            new WorkflowInfoRequest
            {
                InfoRequestId = 1,
                InstanceId = instanceId,
                FromStepOrder = 3,
                FromPositionId = 3,
                ToStepOrder = 2,
                ToPositionId = 2,
                Question = "Parent question",
                Status = InfoRequestStatus.Forwarded,
                ChildInfoRequestId = 2
            },
            new WorkflowInfoRequest
            {
                InfoRequestId = 2,
                InstanceId = instanceId,
                FromStepOrder = 2,
                FromPositionId = 2,
                ToStepOrder = 1,
                ToPositionId = 1,
                Question = "Child question",
                Status = InfoRequestStatus.Open,
                ParentInfoRequestId = 1
            });

        await db.SaveChangesAsync();
        return 2;
    }

    private static void SeedPositionsAndEmployee(AppDbContext db, Guid employeeId, int employeePositionId)
    {
        db.Positions.AddRange(
            new Position { PositionId = 1, PositionCode = "STEP1", PositionName = "Step 1", IsActive = true },
            new Position { PositionId = 2, PositionCode = "STEP2", PositionName = "Step 2", IsActive = true },
            new Position { PositionId = 3, PositionCode = "STEP3", PositionName = "Step 3", IsActive = true });
        db.Employees.Add(new Employee
        {
            EmployeeId = employeeId,
            EmployeeCode = "E001",
            EmployeeName = "Owner",
            Status = EmployeeStatus.Active,
            StartDate = Now.AddYears(-1)
        });
        db.PositionAssignments.Add(new PositionAssignment
        {
            AssignmentId = employeePositionId,
            PositionId = employeePositionId,
            EmployeeId = employeeId,
            IsActive = true,
            IsVacant = false,
            StartDate = Now.AddDays(-1)
        });
    }

    private sealed class FixedClock(DateTime now) : IDateTimeService
    {
        public DateTime Now => now;
    }
}
