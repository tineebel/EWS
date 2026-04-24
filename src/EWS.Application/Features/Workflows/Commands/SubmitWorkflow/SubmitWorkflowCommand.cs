using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Workflows.Commands.SubmitWorkflow;

public record SubmitWorkflowCommand(
    int DocCode,
    string SubmitterPositionCode,
    Guid SubmitterEmployeeId,

    /// <summary>กรณี Delegation: ตำแหน่งจริงที่มีสิทธิ์ (null = ไม่ใช่ delegation)</summary>
    string? ActingAsPositionCode,

    decimal? TotalAmount,
    bool IsSpecialItem,
    bool IsUrgent,
    string? Subject,
    string? Remark,

    /// <summary>
    /// true = เลขาสร้างแทน Chief → ต้องรอ Chief PreApprove ก่อน Flow จริงเริ่ม
    /// </summary>
    bool IsCreatedBySecretary = false
) : IRequest<Result<SubmitWorkflowDto>>;

public record SubmitWorkflowDto(
    Guid InstanceId,
    string DocumentNo,
    int FlowCode,
    string FlowDesc,
    bool RequiresPreApproval,
    string Status,
    List<SubmitApprovalStepDto> ApprovalChain
);

public record SubmitApprovalStepDto(
    int StepOrder,
    string StepName,
    string ApproverPositionCode,
    string ApproverPositionName,
    string? OccupantName,
    bool WasEscalated,
    string? DelegatedToPositionCode
);
