using EWS.Domain.Entities;

namespace EWS.Application.Common.Interfaces;

public record TemplateSelectionRequest(
    int DocCode,
    decimal? Amount,
    bool IsSpecialItem,
    bool IsUrgent,
    string WfScopeType  // "Branch" | "Ho" | "All"
);

public interface IWorkflowEngine
{
    /// <summary>
    /// เลือก WorkflowTemplate ที่ตรงกับ Document + เงื่อนไข
    /// คืน null ถ้าไม่พบ หรือ error code ถ้าพบหลายตัว
    /// </summary>
    Task<(WorkflowTemplate? Template, string? ErrorCode, string? ErrorMessage)> SelectTemplateAsync(
        TemplateSelectionRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Resolve ผู้อนุมัติทุก Step ของ Template จาก submitterPositionId
    /// </summary>
    Task<List<ResolvedApprover?>> ResolveAllApproversAsync(
        WorkflowTemplate template,
        int submitterPositionId,
        CancellationToken ct = default);
}
