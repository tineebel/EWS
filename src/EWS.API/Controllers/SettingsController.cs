using EWS.Application.Features.Settings.Commands.UpdateWorkflowTemplate;
using EWS.Application.Features.Settings.Queries.GetEmployeeDetail;
using EWS.Application.Features.Settings.Queries.GetPositionDetail;
using EWS.Application.Features.Settings.Queries.GetTemplateHistory;
using EWS.Application.Features.Settings.Queries.ListDelegations;
using EWS.Application.Features.Settings.Queries.ListDocumentTypes;
using EWS.Application.Features.Settings.Queries.ListEmployees;
using EWS.Application.Features.Settings.Queries.ListPositions;
using EWS.Application.Features.Settings.Queries.ListWorkflowTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EWS.API.Controllers;

// ── Request models ──────────────────────────────────────────────────────────
public class UpdateWorkflowTemplateRequest
{
    public string FlowDesc { get; set; } = string.Empty;
    public string WfScopeType { get; set; } = string.Empty;
    public bool HasSpecialItem { get; set; }
    public bool IsUrgent { get; set; }
    public string? Condition1 { get; set; }
    public string? Condition2 { get; set; }
    public string? Condition3 { get; set; }
    public string? Condition4 { get; set; }
    public string? Condition5 { get; set; }
    public bool IsActive { get; set; }
    public List<UpdateStepDtoRequest> Steps { get; set; } = [];
    public string? ChangeNote { get; set; }
}

public class UpdateStepDtoRequest
{
    public int? StepId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string ApproverType { get; set; } = string.Empty;
    public string? SpecificPositionCode { get; set; }
    public int EscalationDays { get; set; }
    public bool IsRequired { get; set; }
}

/// <summary>
/// Settings — Master data สำหรับ Admin WebApp
/// </summary>
[Route("api/[controller]")]
public class SettingsController(IMediator mediator) : ApiControllerBase
{
    // ── Positions ──────────────────────────────────────────────────────────────

    [HttpGet("positions")]
    public async Task<IActionResult> ListPositions(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListPositionsQuery(search, isActive, page, pageSize), ct);
        if (!result.IsSuccess) return Fail($"[{result.ErrorCode}] {result.Error}");
        return Paginated(result.Value!);
    }

    [HttpGet("positions/{positionCode}")]
    public async Task<IActionResult> GetPositionDetail(string positionCode, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPositionDetailQuery(positionCode), ct);
        return FromResult(result);
    }

    // ── Employees ──────────────────────────────────────────────────────────────

    [HttpGet("employees")]
    public async Task<IActionResult> ListEmployees(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListEmployeesQuery(search, status, page, pageSize), ct);
        if (!result.IsSuccess) return Fail($"[{result.ErrorCode}] {result.Error}");
        return Paginated(result.Value!);
    }

    [HttpGet("employees/{employeeCode}")]
    public async Task<IActionResult> GetEmployeeDetail(string employeeCode, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetEmployeeDetailQuery(employeeCode), ct);
        return FromResult(result);
    }

    // ── Document Types ─────────────────────────────────────────────────────────

    [HttpGet("document-types")]
    public async Task<IActionResult> ListDocumentTypes(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListDocumentTypesQuery(search, isActive), ct);
        return FromResult(result);
    }

    // ── Workflow Templates ─────────────────────────────────────────────────────

    [HttpGet("workflow-templates")]
    public async Task<IActionResult> ListWorkflowTemplates(
        [FromQuery] int? docCode,
        [FromQuery] bool? isActive,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListWorkflowTemplatesQuery(docCode, isActive), ct);
        return FromResult(result);
    }

    [HttpPut("workflow-templates/{templateId:int}")]
    public async Task<IActionResult> UpdateWorkflowTemplate(
        int templateId,
        [FromBody] UpdateWorkflowTemplateRequest body,
        CancellationToken ct = default)
    {
        var command = new UpdateWorkflowTemplateCommand(
            templateId,
            body.FlowDesc,
            body.WfScopeType,
            body.HasSpecialItem,
            body.IsUrgent,
            body.Condition1,
            body.Condition2,
            body.Condition3,
            body.Condition4,
            body.Condition5,
            body.IsActive,
            body.Steps.Select(s => new UpdateStepDto(
                s.StepId,
                s.StepOrder,
                s.StepName,
                s.ApproverType,
                s.SpecificPositionCode,
                s.EscalationDays,
                s.IsRequired)).ToList(),
            body.ChangeNote,
            ChangedBy: "Admin");

        var result = await mediator.Send(command, ct);
        return FromResult(result);
    }

    [HttpGet("workflow-templates/{templateId:int}/history")]
    public async Task<IActionResult> GetTemplateHistory(int templateId, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTemplateHistoryQuery(templateId), ct);
        return FromResult(result);
    }

    // ── Delegations ────────────────────────────────────────────────────────────

    [HttpGet("delegations")]
    public async Task<IActionResult> ListDelegations(
        [FromQuery] string? positionCode,
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListDelegationsQuery(positionCode, activeOnly), ct);
        return FromResult(result);
    }
}
