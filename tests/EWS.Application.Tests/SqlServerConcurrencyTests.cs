using EWS.Application.Common.Interfaces;
using EWS.Application.Features.Workflows.Commands.ApproveWorkflow;
using EWS.Application.Features.Workflows.Commands.SubmitWorkflow;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using EWS.Infrastructure.Persistence;
using EWS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Tests;

[Collection(SqlServerIntegrationCollection.Name)]
public class SqlServerConcurrencyTests(SqlServerIntegrationFixture fixture)
{
    private static readonly DateTime Now = new(2026, 4, 29, 12, 0, 0);

    [Fact]
    public async Task ConcurrentSubmit_GeneratesUniqueSequentialDocumentNumbers()
    {
        await using (var seedDb = fixture.CreateContext())
        {
            await SeedSubmitScenarioAsync(seedDb, TestData.SubmitterEmployeeId, TestData.ApproverEmployeeId);
        }

        const int requestCount = 8;
        var tasks = Enumerable.Range(0, requestCount)
            .Select(i => SubmitAsync(i))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.True(r.IsSuccess, r.Error));

        var documentNumbers = results.Select(r => r.Value!.DocumentNo).Order().ToList();
        Assert.Equal(requestCount, documentNumbers.Distinct().Count());
        Assert.Equal(
            Enumerable.Range(1, requestCount).Select(i => $"MEMO-2026-{i:D5}").ToList(),
            documentNumbers);

        await using var verifyDb = fixture.CreateContext();
        var sequence = await verifyDb.WorkflowDocumentSequences.SingleAsync(x => x.Prefix == "MEMO" && x.Year == 2026);
        Assert.Equal(requestCount, sequence.LastNumber);
        Assert.Equal(requestCount, await verifyDb.WorkflowInstances.CountAsync(x => x.DocumentNo.StartsWith("MEMO-2026-")));

        async Task<EWS.Application.Common.Models.Result<SubmitWorkflowDto>> SubmitAsync(int index)
        {
            await using var db = fixture.CreateContext();
            var handler = new SubmitWorkflowHandler(
                db,
                new WorkflowEngine(db, new ApproverResolver(db, new FixedClock(Now))),
                new DocumentNumberService(db, new FixedClock(Now)),
                new FixedClock(Now));

            return await handler.Handle(new SubmitWorkflowCommand(
                DocCode: 1001,
                SubmitterPositionCode: TestData.SubmitterPositionCode,
                SubmitterEmployeeId: TestData.SubmitterEmployeeId,
                ActingAsPositionCode: null,
                TotalAmount: 100 + index,
                IsSpecialItem: false,
                IsUrgent: false,
                Subject: $"Concurrent submit {index}",
                Remark: null,
                IsCreatedBySecretary: false), CancellationToken.None);
        }
    }

    [Fact]
    public async Task ConcurrentApprove_AllowsOnlyOneSuccessfulApproval()
    {
        Guid instanceId;
        await using (var seedDb = fixture.CreateContext())
        {
            instanceId = await SeedApproveScenarioAsync(seedDb, TestData.ApproveSubmitterEmployeeId, TestData.ApproveApproverEmployeeId);
        }

        var tasks = Enumerable.Range(0, 2)
            .Select(_ => ApproveAsync(instanceId))
            .ToArray();

        var outcomes = await Task.WhenAll(tasks);
        var successful = outcomes.Count(o => o == "success");

        Assert.Equal(1, successful);

        await using var verifyDb = fixture.CreateContext();
        var instance = await verifyDb.WorkflowInstances.SingleAsync(x => x.InstanceId == instanceId);
        var approval = await verifyDb.WorkflowApprovals.SingleAsync(x => x.InstanceId == instanceId);

        Assert.Equal(WorkflowStatus.Approved, instance.Status);
        Assert.Equal(ApprovalStatus.Approved, approval.Status);
        Assert.Single(await verifyDb.WorkflowHistories.Where(x => x.InstanceId == instanceId && x.EventType == "Complete").ToListAsync());

        async Task<string> ApproveAsync(Guid id)
        {
            try
            {
                await using var db = fixture.CreateContext();
                var handler = new ApproveWorkflowHandler(db, new FixedClock(Now));
                var result = await handler.Handle(new ApproveWorkflowCommand(
                    id,
                    ActorPositionCode: TestData.ApproveApproverPositionCode,
                    ActorEmployeeId: TestData.ApproveApproverEmployeeId,
                    Comment: "approve"), CancellationToken.None);

                return result.IsSuccess ? "success" : result.ErrorCode ?? "fail";
            }
            catch (DbUpdateConcurrencyException)
            {
                return "concurrency";
            }
        }
    }

    private static async Task SeedSubmitScenarioAsync(
        AppDbContext db,
        Guid submitterEmployeeId,
        Guid approverEmployeeId)
    {
        var section = await SeedOrganizationAsync(db);

        var approver = new Position
        {
            PositionCode = TestData.ApproverPositionCode,
            PositionName = "Approver",
            JobGrade = JobGrade.B1,
            WfScopeType = WfScopeType.Ho,
            Section = section,
            IsActive = true,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        var requester = new Position
        {
            PositionCode = TestData.SubmitterPositionCode,
            PositionName = "Requester",
            JobGrade = JobGrade.C1,
            WfScopeType = WfScopeType.Ho,
            Section = section,
            ParentPosition = approver,
            IsActive = true,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        db.Positions.AddRange(approver, requester);

        db.Employees.AddRange(
            new Employee
            {
                EmployeeId = submitterEmployeeId,
                EmployeeCode = "SUBMITTER-SUBMIT",
                EmployeeName = "Submitter",
                Status = EmployeeStatus.Active,
                StartDate = Now.AddYears(-1),
                CreatedAt = Now,
                CreatedBy = "test"
            },
            new Employee
            {
                EmployeeId = approverEmployeeId,
                EmployeeCode = "APPROVER-SUBMIT",
                EmployeeName = "Approver",
                Status = EmployeeStatus.Active,
                StartDate = Now.AddYears(-1),
                CreatedAt = Now,
                CreatedBy = "test"
            });

        db.PositionAssignments.AddRange(
            new PositionAssignment
            {
                Position = requester,
                EmployeeId = submitterEmployeeId,
                IsActive = true,
                IsVacant = false,
                StartDate = Now.AddDays(-1),
                CreatedAt = Now,
                CreatedBy = "test"
            },
            new PositionAssignment
            {
                Position = approver,
                EmployeeId = approverEmployeeId,
                IsActive = true,
                IsVacant = false,
                StartDate = Now.AddDays(-1),
                CreatedAt = Now,
                CreatedBy = "test"
            });

        var docType = new DocumentType
        {
            DocCode = 1001,
            DocName = "Memo",
            Category = "Memo",
            IsActive = true,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        db.DocumentTypes.Add(docType);
        db.WorkflowTemplates.Add(new WorkflowTemplate
        {
            DocumentType = docType,
            FlowCode = 1,
            FlowDesc = "Memo approval",
            WfScopeType = WfScopeType.Ho,
            HasSpecialItem = false,
            IsUrgent = false,
            IsActive = true,
            CreatedAt = Now,
            CreatedBy = "test",
            Steps =
            [
                new WorkflowStep
                {
                    StepOrder = 1,
                    StepName = "Direct supervisor",
                    ApproverType = ApproverType.DirectSupervisor,
                    IsActive = true,
                    IsRequired = true,
                    CreatedAt = Now,
                    CreatedBy = "test"
                }
            ]
        });

        await db.SaveChangesAsync();
    }

    private static async Task<Guid> SeedApproveScenarioAsync(
        AppDbContext db,
        Guid submitterEmployeeId,
        Guid approverEmployeeId)
    {
        var section = await SeedOrganizationAsync(db);
        var approver = new Position
        {
            PositionCode = TestData.ApproveApproverPositionCode,
            PositionName = "Approver",
            JobGrade = JobGrade.B1,
            WfScopeType = WfScopeType.Ho,
            Section = section,
            IsActive = true,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        var requester = new Position
        {
            PositionCode = TestData.ApproveSubmitterPositionCode,
            PositionName = "Requester",
            JobGrade = JobGrade.C1,
            WfScopeType = WfScopeType.Ho,
            Section = section,
            ParentPosition = approver,
            IsActive = true,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        db.Positions.AddRange(approver, requester);

        db.Employees.AddRange(
            new Employee
            {
                EmployeeId = submitterEmployeeId,
                EmployeeCode = "SUBMITTER-APPROVE",
                EmployeeName = "Submitter",
                Status = EmployeeStatus.Active,
                StartDate = Now.AddYears(-1),
                CreatedAt = Now,
                CreatedBy = "test"
            },
            new Employee
            {
                EmployeeId = approverEmployeeId,
                EmployeeCode = "APPROVER-APPROVE",
                EmployeeName = "Approver",
                Status = EmployeeStatus.Active,
                StartDate = Now.AddYears(-1),
                CreatedAt = Now,
                CreatedBy = "test"
            });
        db.PositionAssignments.Add(new PositionAssignment
        {
            Position = approver,
            EmployeeId = approverEmployeeId,
            IsActive = true,
            IsVacant = false,
            StartDate = Now.AddDays(-1),
            CreatedAt = Now,
            CreatedBy = "test"
        });

        var docType = new DocumentType
        {
            DocCode = 9001,
            DocName = "Approval Test",
            Category = "Test",
            IsActive = true,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        var template = new WorkflowTemplate
        {
            DocumentType = docType,
            FlowCode = 1,
            FlowDesc = "Approval test flow",
            WfScopeType = WfScopeType.Ho,
            IsActive = true,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        var step = new WorkflowStep
        {
            Template = template,
            StepOrder = 1,
            StepName = "Approve",
            ApproverType = ApproverType.DirectSupervisor,
            IsActive = true,
            IsRequired = true,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        var instanceId = Guid.NewGuid();
        db.WorkflowInstances.Add(new WorkflowInstance
        {
            InstanceId = instanceId,
            Template = template,
            DocumentNo = "TEST-2026-00001",
            SubmitterPosition = requester,
            SubmitterEmployeeId = submitterEmployeeId,
            Status = WorkflowStatus.Pending,
            PreApprovalStatus = PreApprovalStatus.NotRequired,
            SubmittedAt = Now,
            CreatedAt = Now,
            CreatedBy = "test"
        });
        db.WorkflowApprovals.Add(new WorkflowApproval
        {
            InstanceId = instanceId,
            Step = step,
            StepOrder = 1,
            AssignedPosition = approver,
            Status = ApprovalStatus.Pending,
            CreatedAt = Now,
            CreatedBy = "test"
        });

        await db.SaveChangesAsync();
        return instanceId;
    }

    private static async Task<Section> SeedOrganizationAsync(AppDbContext db)
    {
        var division = new Division
        {
            DivisionCode = Guid.NewGuid().ToString("N")[..12],
            DivisionName = "Test Division",
            CreatedAt = Now,
            CreatedBy = "test"
        };
        var department = new Department
        {
            DeptCode = Guid.NewGuid().ToString("N")[..12],
            DeptName = "Test Department",
            Division = division,
            CreatedAt = Now,
            CreatedBy = "test"
        };
        var section = new Section
        {
            SectCode = Guid.NewGuid().ToString("N")[..12],
            SectName = "Test Section",
            Department = department,
            CreatedAt = Now,
            CreatedBy = "test"
        };

        db.Sections.Add(section);
        await db.SaveChangesAsync();
        return section;
    }

    private sealed class FixedClock(DateTime now) : IDateTimeService
    {
        public DateTime Now => now;
    }

    private static class TestData
    {
        public static readonly Guid SubmitterEmployeeId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid ApproverEmployeeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid ApproveSubmitterEmployeeId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public static readonly Guid ApproveApproverEmployeeId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        public const string SubmitterPositionCode = "REQ-SUBMIT";
        public const string ApproverPositionCode = "APP-SUBMIT";
        public const string ApproveSubmitterPositionCode = "REQ-APPROVE";
        public const string ApproveApproverPositionCode = "APP-APPROVE";
    }
}
