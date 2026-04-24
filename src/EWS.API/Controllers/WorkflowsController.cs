using EWS.Application.Features.Workflows.Commands.ApproveWorkflow;
using EWS.Application.Features.Workflows.Commands.PreApproveWorkflow;
using EWS.Application.Features.Workflows.Commands.ReassignApprovalStep;
using EWS.Application.Features.Workflows.Commands.ReEscalateWorkflow;
using EWS.Application.Features.Workflows.Commands.RejectWorkflow;
using EWS.Application.Features.Workflows.Commands.RequestInfo;
using EWS.Application.Features.Workflows.Commands.RespondToInfoRequest;
using EWS.Application.Features.Workflows.Commands.SubmitWorkflow;
using EWS.Application.Features.Workflows.Queries.GetInfoRequestThread;
using EWS.Application.Features.Workflows.Queries.GetPendingInfoRequests;
using EWS.Application.Features.Workflows.Queries.GetWorkflowAudit;
using EWS.Application.Features.Workflows.Queries.GetWorkflowInstance;
using EWS.Application.Features.Workflows.Queries.ListWorkflowInstances;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EWS.API.Controllers;

/// <summary>
/// Workflows — Submit, Approve, Reject เอกสาร และตรวจสอบสถานะ
/// </summary>
[Route("api/[controller]")]
public class WorkflowsController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// รายการ Workflow Instances — กรองตาม Status, Position, DocCode
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] string? positionCode,
        [FromQuery] int? docCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new ListWorkflowInstancesQuery(status, positionCode, docCode, page, pageSize), ct);
        return result.IsSuccess
            ? Paginated(result.Value!)
            : Fail($"[{result.ErrorCode}] {result.Error}");
    }

    /// <summary>
    /// ดูรายละเอียด Workflow Instance พร้อม Approval Steps ปัจจุบัน
    /// </summary>
    [HttpGet("{instanceId:guid}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(Guid instanceId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetWorkflowInstanceQuery(instanceId), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Audit Trail — ประวัติทุก Event ของ Instance (Immutable)
    /// ใช้ตรวจสอบว่า Flow เดิน Step ถูกต้องหรือไม่
    /// </summary>
    [HttpGet("{instanceId:guid}/audit")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAudit(Guid instanceId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetWorkflowAuditQuery(instanceId), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Submit เอกสารเข้า Workflow — ระบบจะเลือก Flow Template อัตโนมัติจาก DocCode + Amount + Scope
    /// Response จะแสดง Approval Chain ทั้งหมดทันที
    /// </summary>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Submit([FromBody] SubmitWorkflowCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Created(result.Value!)
            : Fail($"[{result.ErrorCode}] {result.Error}");
    }

    /// <summary>
    /// อนุมัติ Step ปัจจุบัน — ตรวจสอบสิทธิ์ผ่าน AssignedPosition หรือ Active Delegation
    /// </summary>
    [HttpPost("{instanceId:guid}/approve")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Approve(
        Guid instanceId, [FromBody] ApproveWorkflowRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new ApproveWorkflowCommand(instanceId, req.ActorPositionCode, req.ActorEmployeeId, req.Comment), ct);
        return FromResult(result);
    }

    /// <summary>
    /// ปฏิเสธเอกสาร — ต้องระบุเหตุผล, Instance จะเปลี่ยนเป็น Rejected
    /// </summary>
    [HttpPost("{instanceId:guid}/reject")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reject(
        Guid instanceId, [FromBody] RejectWorkflowRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RejectWorkflowCommand(instanceId, req.ActorPositionCode, req.ActorEmployeeId, req.Comment), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Re-Escalate — เมื่อผู้อนุมัติลาออก/โอนย้ายหลัง Submit
    /// ระบบจะ walk up hierarchy หาผู้อนุมัติใหม่แทนตำแหน่งที่ว่าง
    /// </summary>
    [HttpPost("{instanceId:guid}/re-escalate")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ReEscalate(
        Guid instanceId, [FromBody] ReEscalateRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new ReEscalateWorkflowCommand(instanceId, req.RequestedByPositionCode), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Admin Reassign — กำหนดผู้อนุมัติใหม่โดยตรง เมื่อ Step ติด Stuck
    /// ใช้เมื่อ Re-Escalate แล้วไม่พบผู้อนุมัติในสาย hierarchy
    /// </summary>
    [HttpPost("{instanceId:guid}/steps/{stepOrder:int}/reassign")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Reassign(
        Guid instanceId, int stepOrder, [FromBody] ReassignStepRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new ReassignApprovalStepCommand(instanceId, stepOrder,
                req.TargetPositionCode, req.RequestedByPositionCode, req.Reason), ct);
        return FromResult(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Info Request — ขอข้อมูลระหว่าง Step ระหว่างรออนุมัติ
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Info Request — Step ปัจจุบันขอข้อมูลจาก Step ก่อนหน้า
    /// ขอได้เฉพาะ Step ที่ StepOrder น้อยกว่า ห้ามขอไปข้างหน้า
    /// </summary>
    [HttpPost("{instanceId:guid}/steps/{stepOrder:int}/request-info")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RequestInfo(
        Guid instanceId, int stepOrder, [FromBody] RequestInfoRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RequestInfoCommand(instanceId, stepOrder, req.ToStepOrder,
                req.ActorPositionCode, req.ActorEmployeeId, req.Question), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Respond to Info Request — ตอบคำถาม หรือ Forward ต่อไปยัง Step ก่อนหน้า
    /// เมื่อ root request ตอบแล้ว Step ที่รออยู่จะกลับมา Pending อัตโนมัติ
    /// </summary>
    [HttpPost("info-requests/{infoRequestId:long}/respond")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RespondToInfoRequest(
        long infoRequestId, [FromBody] RespondToInfoRequestRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RespondToInfoRequestCommand(infoRequestId,
                req.ActorPositionCode, req.ActorEmployeeId,
                req.Answer, req.ForwardToStepOrder, req.ForwardQuestion), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Inbox Info Requests — ดู Info Requests ที่ตำแหน่งนี้ต้องตอบ
    /// </summary>
    [HttpGet("info-requests/pending")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetPendingInfoRequests(
        [FromQuery] string positionCode,
        [FromQuery] Guid? instanceId,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetPendingInfoRequestsQuery(positionCode, instanceId), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Info Request Thread — ดู chain การถาม-ตอบทั้งหมดของ Instance
    /// แสดง depth ของ chain (5→3→1 = depth 0,1,2)
    /// </summary>
    [HttpGet("{instanceId:guid}/info-requests")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetInfoRequestThread(Guid instanceId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetInfoRequestThreadQuery(instanceId), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Secretary Pre-Approve — Chief ยืนยัน/ปฏิเสธเอกสารที่เลขาสร้างให้
    /// ต้องทำก่อน Flow อนุมัติจริงจึงจะเริ่มได้
    /// </summary>
    [HttpPost("{instanceId:guid}/pre-approve")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PreApprove(
        Guid instanceId, [FromBody] PreApproveWorkflowRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new PreApproveWorkflowCommand(instanceId, req.ChiefPositionCode, req.ChiefEmployeeId,
                req.IsConfirmed, req.Comment), ct);
        return FromResult(result);
    }
}

// Request body DTOs (เพื่อให้ Swagger แสดงชัดเจน)
public record ApproveWorkflowRequest(string ActorPositionCode, Guid ActorEmployeeId, string? Comment);
public record RejectWorkflowRequest(string ActorPositionCode, Guid ActorEmployeeId, string Comment);
public record PreApproveWorkflowRequest(string ChiefPositionCode, Guid ChiefEmployeeId, bool IsConfirmed, string? Comment);
public record ReEscalateRequest(string RequestedByPositionCode);
public record ReassignStepRequest(
    string TargetPositionCode,
    string RequestedByPositionCode,
    string? Reason);
public record RequestInfoRequest(
    int ToStepOrder,
    string ActorPositionCode,
    Guid ActorEmployeeId,
    string Question);
public record RespondToInfoRequestRequest(
    string ActorPositionCode,
    Guid ActorEmployeeId,
    string? Answer,
    int? ForwardToStepOrder,
    string? ForwardQuestion);
