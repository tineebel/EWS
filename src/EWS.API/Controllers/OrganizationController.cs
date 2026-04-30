using EWS.Application.Features.Organization.Commands.BulkReEscalate;
using EWS.Application.Features.Organization.Queries.GetApprovalChain;
using EWS.Application.Features.Organization.Queries.GetEmployeeCurrentPosition;
using EWS.Application.Features.Organization.Queries.GetOrgChart;
using EWS.Application.Features.Organization.Queries.GetPositionHierarchy;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EWS.API.Controllers;

/// <summary>
/// Organization — ตรวจสอบโครงสร้างองค์กร, Hierarchy, และ Approval Chain
/// </summary>
[Route("api/[controller]")]
public class OrganizationController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Org Chart — คืนค่า Tree ของตำแหน่งทั้งหมด (full depth)
    /// ใช้สำหรับ visualize สายบังคับบัญชา
    /// </summary>
    [HttpGet("org-chart")]
    public async Task<IActionResult> GetOrgChart(
        [FromQuery] string? rootCode,
        [FromQuery] string? branchCode,
        [FromQuery] string? deptCode,
        [FromQuery] string? sectionCode,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrgChartQuery(rootCode, branchCode, deptCode, sectionCode, search), ct);
        return FromResult(result);
    }

    /// <summary>
    /// ดู Hierarchy ของตำแหน่ง ตั้งแต่ตำแหน่งนั้นขึ้นไปถึง CEO
    /// ใช้ตรวจสอบสายบังคับบัญชา และ Auto-Escalation Path
    /// </summary>
    [HttpGet("positions/{positionCode}/hierarchy")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetHierarchy(string positionCode, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPositionHierarchyQuery(positionCode), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Simulate Approval Chain — ดูว่าถ้า Position นี้ Submit เอกสาร DocCode นี้ จะต้องผ่านใครบ้าง
    /// ไม่สร้าง Record จริง — ใช้สำหรับตรวจสอบ Workflow Configuration
    /// </summary>
    [HttpGet("positions/{positionCode}/approval-chain")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetApprovalChain(
        string positionCode,
        [FromQuery] int docCode,
        [FromQuery] decimal? amount,
        [FromQuery] bool isSpecialItem = false,
        [FromQuery] bool isUrgent = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetApprovalChainQuery(positionCode, docCode, amount, isSpecialItem, isUrgent), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Bulk Re-Escalate — เมื่อพนักงานลาออก/โอนย้าย
    /// ค้นหาทุก Pending Approval ที่ assign ให้ตำแหน่งของพนักงานคนนี้ และ Re-Escalate ทั้งหมด
    /// เรียกโดย HR หลังจาก Deactivate พนักงานออกจากระบบแล้ว
    /// </summary>
    [HttpPost("employees/{employeeCode}/re-escalate-pending")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> BulkReEscalate(
        string employeeCode, [FromBody] BulkReEscalateRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new BulkReEscalateCommand(employeeCode, req.RequestedByPositionCode), ct);
        return FromResult(result);
    }

    /// <summary>
    /// ดูตำแหน่งปัจจุบันของพนักงาน (Active Assignment)
    /// </summary>
    [HttpGet("employees/{employeeCode}/current-position")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCurrentPosition(string employeeCode, CancellationToken ct)
    {
        var result = await mediator.Send(new GetEmployeeCurrentPositionQuery(employeeCode), ct);
        return FromResult(result);
    }
}

public record BulkReEscalateRequest(string RequestedByPositionCode);
