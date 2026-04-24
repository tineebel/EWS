using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EWS.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Division> Divisions { get; }
    DbSet<Department> Departments { get; }
    DbSet<Section> Sections { get; }
    DbSet<Position> Positions { get; }
    DbSet<Employee> Employees { get; }
    DbSet<PositionAssignment> PositionAssignments { get; }
    DbSet<Delegation> Delegations { get; }

    DbSet<DocumentType> DocumentTypes { get; }
    DbSet<WorkflowTemplate> WorkflowTemplates { get; }
    DbSet<WorkflowStep> WorkflowSteps { get; }
    DbSet<WorkflowInstance> WorkflowInstances { get; }
    DbSet<WorkflowApproval> WorkflowApprovals { get; }
    DbSet<WorkflowHistory> WorkflowHistories { get; }
    DbSet<WorkflowInfoRequest> WorkflowInfoRequests { get; }
    DbSet<WorkflowTemplateAuditLog> WorkflowTemplateAuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
