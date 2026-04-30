using EWS.Application.Common.Models;
using MediatR;

namespace EWS.Application.Features.Organization.Queries.GetApprovalChain;

/// <summary>
/// Simulate approval chain สำหรับตำแหน่งที่กำหนด โดยไม่สร้าง Record จริง
/// ใช้ตรวจสอบว่า Flow ถูกต้องหรือไม่
/// </summary>
public record GetApprovalChainQuery(
    string SubmitterPositionCode,
    int DocCode,
    decimal? Amount,
    bool IsSpecialItem = false,
    bool IsUrgent = false
) : IRequest<Result<ApprovalChainDto>>;

public record ApprovalChainDto(
    int DocCode,
    int FlowCode,
    string FlowDesc,
    string SelectedScope,
    string? Condition,
    List<ApprovalChainStepDto> Steps
);

public record ApprovalChainStepDto(
    int StepOrder,
    string StepName,
    string ApproverType,
    string? SpecificPositionCode,
    string ResolvedPositionCode,
    string ResolvedPositionName,
    bool WasEscalated,
    int EscalationDepth,
    string? OccupantName,
    List<string> OccupantNames,
    int OccupantCount,
    bool IsVacant,
    string? DelegatedToPositionCode,
    string? ErrorCode,
    string? ErrorMessage
);
