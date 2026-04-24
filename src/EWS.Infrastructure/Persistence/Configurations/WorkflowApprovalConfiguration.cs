using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class WorkflowApprovalConfiguration : IEntityTypeConfiguration<WorkflowApproval>
{
    public void Configure(EntityTypeBuilder<WorkflowApproval> builder)
    {
        builder.ToTable("WorkflowApprovals", "wf");
        builder.HasKey(x => x.ApprovalId);

        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => new { x.InstanceId, x.StepOrder })
            .HasDatabaseName("IX_WorkflowApproval_InstanceStep");

        builder.HasOne(x => x.Instance)
            .WithMany(x => x.Approvals)
            .HasForeignKey(x => x.InstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Step)
            .WithMany()
            .HasForeignKey(x => x.StepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedPosition)
            .WithMany()
            .HasForeignKey(x => x.AssignedPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActorEmployee)
            .WithMany()
            .HasForeignKey(x => x.ActorEmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ActorActingAsPosition)
            .WithMany()
            .HasForeignKey(x => x.ActorActingAsPositionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
