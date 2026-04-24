using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class WorkflowInfoRequestConfiguration : IEntityTypeConfiguration<WorkflowInfoRequest>
{
    public void Configure(EntityTypeBuilder<WorkflowInfoRequest> builder)
    {
        builder.ToTable("WorkflowInfoRequests", "wf");
        builder.HasKey(x => x.InfoRequestId);
        builder.Property(x => x.InfoRequestId).UseIdentityColumn();

        builder.Property(x => x.Question).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Answer).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => new { x.InstanceId, x.FromStepOrder, x.ToStepOrder, x.Status })
            .HasDatabaseName("IX_InfoRequest_Instance_Steps");

        builder.HasOne(x => x.Instance)
            .WithMany()
            .HasForeignKey(x => x.InstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromPosition)
            .WithMany()
            .HasForeignKey(x => x.FromPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToPosition)
            .WithMany()
            .HasForeignKey(x => x.ToPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing: parent/child chain
        builder.HasOne(x => x.ParentRequest)
            .WithOne(x => x.ChildRequest)
            .HasForeignKey<WorkflowInfoRequest>(x => x.ParentInfoRequestId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
