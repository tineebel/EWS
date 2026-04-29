using EWS.Application.Common.Interfaces;
using EWS.Domain.Entities;
using EWS.Domain.Enums;
using EWS.Infrastructure.Services;

namespace EWS.Application.Tests;

public class WorkflowEngineTests
{
    [Fact]
    public async Task ResolveAllApproversAsync_PreservesStepPositions_WhenMiddleStepCannotResolve()
    {
        var template = new WorkflowTemplate
        {
            Steps =
            [
                new WorkflowStep { StepOrder = 1, ApproverType = ApproverType.DirectSupervisor, StepName = "First" },
                new WorkflowStep { StepOrder = 2, ApproverType = ApproverType.DeptManager, StepName = "Missing" },
                new WorkflowStep { StepOrder = 3, ApproverType = ApproverType.Ceo, StepName = "Final" }
            ]
        };

        var resolver = new StubApproverResolver([
            new ResolvedApprover(101, "P101", "First Approver", false, 0, "Alice", false, null, null),
            null,
            new ResolvedApprover(303, "P303", "Final Approver", false, 0, "Charlie", false, null, null)
        ]);
        var engine = new WorkflowEngine(null!, resolver);

        var result = await engine.ResolveAllApproversAsync(template, submitterPositionId: 1);

        Assert.Equal(3, result.Count);
        Assert.Equal("P101", result[0]?.PositionCode);
        Assert.Null(result[1]);
        Assert.Equal("P303", result[2]?.PositionCode);
    }

    private sealed class StubApproverResolver(IReadOnlyList<ResolvedApprover?> results) : IApproverResolver
    {
        private int _index;

        public Task<ResolvedApprover?> ResolveAsync(
            int submitterPositionId,
            ApproverType approverType,
            string? specificPositionCode,
            CancellationToken ct = default)
        {
            return Task.FromResult(results[_index++]);
        }

        public Task<ResolvedApprover?> EscalateFromPositionAsync(int positionId, CancellationToken ct = default)
            => Task.FromResult<ResolvedApprover?>(null);
    }
}
