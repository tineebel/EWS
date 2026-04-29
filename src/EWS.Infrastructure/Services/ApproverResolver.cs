using EWS.Application.Common.Interfaces;
using EWS.Domain.Enums;
using EWS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EWS.Infrastructure.Services;

public class ApproverResolver(AppDbContext db, IDateTimeService clock) : IApproverResolver
{
    private const int MaxEscalationDepth = 10;

    public async Task<ResolvedApprover?> ResolveAsync(
        int submitterPositionId,
        ApproverType approverType,
        string? specificPositionCode,
        CancellationToken ct = default)
    {
        var targetPositionId = approverType switch
        {
            // ── ตำแหน่งเฉพาะเจาะจง (เช่น CFO) ─────────────────────────────────────────
            ApproverType.SpecificPosition => await ResolveSpecificAsync(specificPositionCode, ct),

            // ── หัวหน้าโดยตรง (parent 1 ระดับ) ────────────────────────────────────────
            ApproverType.DirectSupervisor => await ResolveParentAsync(submitterPositionId, ct),

            // ── Section Manager (Grade B1/B2 = ผู้จัดการแผนก/Theater Manager) ─────────
            // HO DOA "ผู้จัดการแผนก" / Branch DOA "ผู้จัดการสาขา"
            ApproverType.SectionManager   => await WalkUpToGradeAsync(submitterPositionId, g => g is JobGrade.B1 or JobGrade.B2, ct),

            // ── Area Manager (Grade B0 เท่านั้น — Branch DOA "Area") ──────────────────
            // มีเพียง 1 ตำแหน่งในระบบ: HOHOP09 "Area Manager (CB)"
            ApproverType.AreaManager      => await WalkUpToGradeAsync(submitterPositionId, g => g == JobGrade.B0, ct),

            // ── Department Manager (Grade A3 = ผู้อำนวยการแผนก) ──────────────────────
            // HO DOA "Department" / Branch DOA "Department"
            ApproverType.DeptManager      => await WalkUpToGradeAsync(submitterPositionId, g => g == JobGrade.A3, ct),

            // ── Division Director (Grade A2 = ผู้อำนวยการฝ่าย) ─────────────────────
            // HO/Branch DOA "Director"
            ApproverType.DivisionDirector => await WalkUpToGradeAsync(submitterPositionId, g => g == JobGrade.A2, ct),

            // ── C-Level (Grade A1 = COO/CFO/CMO/CPO/CTO) ────────────────────────────
            // HO/Branch DOA "Chief"
            ApproverType.CLevel           => await WalkUpToGradeAsync(submitterPositionId, g => g == JobGrade.A1, ct),

            // ── CEO (Grade A0 = top of hierarchy) ────────────────────────────────────
            ApproverType.Ceo              => await WalkUpToGradeAsync(submitterPositionId, g => g == JobGrade.A0, ct),

            _ => null
        };

        if (targetPositionId == null) return null;

        return await EscalateIfVacantAsync(targetPositionId.Value, ct);
    }

    // --- Resolution Strategies ---

    private async Task<int?> ResolveSpecificAsync(string? code, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(code)) return null;
        var pos = await db.Positions
            .Where(p => p.PositionCode == code && p.IsActive)
            .Select(p => (int?)p.PositionId)
            .FirstOrDefaultAsync(ct);
        return pos;
    }

    private async Task<int?> ResolveParentAsync(int positionId, CancellationToken ct)
    {
        return await db.Positions
            .Where(p => p.PositionId == positionId)
            .Select(p => p.ParentPositionId)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<int?> WalkUpToGradeAsync(
        int startPositionId,
        Func<JobGrade, bool> gradePredicate,
        CancellationToken ct)
    {
        var current = startPositionId;
        for (int depth = 0; depth < MaxEscalationDepth; depth++)
        {
            var parentId = await db.Positions
                .Where(p => p.PositionId == current && p.IsActive)
                .Select(p => p.ParentPositionId)
                .FirstOrDefaultAsync(ct);

            if (parentId == null) return null; // ถึง root แล้วไม่เจอ

            var parent = await db.Positions
                .Where(p => p.PositionId == parentId && p.IsActive)
                .Select(p => new { p.PositionId, p.JobGrade })
                .FirstOrDefaultAsync(ct);

            if (parent == null) return null;
            if (gradePredicate(parent.JobGrade)) return parent.PositionId;

            current = parent.PositionId;
        }
        return null; // MaxDepth exceeded
    }

    // --- Public Escalation (ใช้โดย Re-Escalate handlers) ---

    public Task<ResolvedApprover?> EscalateFromPositionAsync(int positionId, CancellationToken ct)
        => EscalateIfVacantAsync(positionId, ct);

    // --- Escalation (Vacant → Walk Up) ---

    private async Task<ResolvedApprover?> EscalateIfVacantAsync(int positionId, CancellationToken ct)
    {
        var now = clock.Now;
        var visited = new HashSet<int>();
        var originalPositionId = positionId;
        var current = positionId;
        int escalationDepth = 0;

        while (escalationDepth <= MaxEscalationDepth)
        {
            if (!visited.Add(current))
                return null; // cycle detected

            var pos = await db.Positions
                .Where(p => p.PositionId == current && p.IsActive)
                .Select(p => new { p.PositionId, p.PositionCode, p.PositionName, p.ParentPositionId })
                .FirstOrDefaultAsync(ct);

            if (pos == null) return null;

            // ตรวจสอบว่ามีพนักงาน Active ดำรงตำแหน่งนี้อยู่หรือไม่
            var occupant = await db.PositionAssignments
                .Where(a => a.PositionId == current
                    && a.IsActive
                    && !a.IsVacant
                    && a.StartDate <= now
                    && (a.EndDate == null || a.EndDate >= now))
                .Join(db.Employees, a => a.EmployeeId, e => e.EmployeeId,
                    (a, e) => new { e.EmployeeName, e.EmployeeId })
                .FirstOrDefaultAsync(ct);

            // ตรวจสอบ Delegation Active
            var delegation = await db.Delegations
                .Where(d => d.FromPositionId == current
                    && d.IsActive
                    && d.StartDate <= now
                    && d.EndDate >= now)
                .Select(d => new { d.ToPositionId, ToCode = d.ToPosition.PositionCode })
                .FirstOrDefaultAsync(ct);

            bool isVacant = occupant == null;

            if (!isVacant)
            {
                return new ResolvedApprover(
                    PositionId: pos.PositionId,
                    PositionCode: pos.PositionCode,
                    PositionName: pos.PositionName,
                    WasEscalated: escalationDepth > 0,
                    EscalationDepth: escalationDepth,
                    OccupantName: occupant!.EmployeeName,
                    IsVacant: false,
                    DelegatedToPositionId: delegation?.ToPositionId,
                    DelegatedToPositionCode: delegation?.ToCode,
                    EscalatedFromPositionId: escalationDepth > 0 ? originalPositionId : null
                );
            }

            // Vacant → ขึ้นไปหัวหน้า
            if (pos.ParentPositionId == null)
                return null; // ถึง root แล้วยังว่าง

            current = pos.ParentPositionId.Value;
            escalationDepth++;
        }

        return null; // MaxDepth exceeded
    }
}
