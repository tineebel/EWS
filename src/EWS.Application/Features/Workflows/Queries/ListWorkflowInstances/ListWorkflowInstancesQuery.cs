using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Queries.ListWorkflowInstances;

public record ListWorkflowInstancesQuery(
    string? Status,
    string? PositionCode, // กรองเฉพาะ instance ที่ position นี้ต้องอนุมัติ
    int? DocCode,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<WorkflowInstanceSummaryDto>>>;

public record WorkflowInstanceSummaryDto(
    Guid InstanceId,
    string DocumentNo,
    string DocName,
    string FlowDesc,
    string Status,
    string SubmitterPositionCode,
    decimal? TotalAmount,
    DateTime SubmittedAt,
    int CurrentStepOrder,
    string? CurrentApproverPositionCode
);
