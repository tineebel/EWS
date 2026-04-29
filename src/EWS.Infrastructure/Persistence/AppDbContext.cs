using EWS.Application.Common.Interfaces;
using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EWS.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    // --- Schema: dbo (Organization Master Data) ---
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PositionAssignment> PositionAssignments => Set<PositionAssignment>();
    public DbSet<Delegation> Delegations => Set<Delegation>();

    // --- Schema: wf (Workflow Config + Runtime) ---
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<WorkflowTemplate> WorkflowTemplates => Set<WorkflowTemplate>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowApproval> WorkflowApprovals => Set<WorkflowApproval>();
    public DbSet<WorkflowHistory> WorkflowHistories => Set<WorkflowHistory>();
    public DbSet<WorkflowInfoRequest> WorkflowInfoRequests => Set<WorkflowInfoRequest>();
    public DbSet<WorkflowTemplateAuditLog> WorkflowTemplateAuditLogs => Set<WorkflowTemplateAuditLog>();
    public DbSet<WorkflowDocumentSequence> WorkflowDocumentSequences => Set<WorkflowDocumentSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
