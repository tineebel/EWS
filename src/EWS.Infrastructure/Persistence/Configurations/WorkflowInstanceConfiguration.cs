using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances", "wf");
        builder.HasKey(x => x.InstanceId);

        builder.Property(x => x.DocumentNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ExternalDocRef).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Subject).HasMaxLength(500);
        builder.Property(x => x.Remark).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => x.DocumentNo).IsUnique();
        builder.HasIndex(x => new { x.Status, x.SubmittedAt })
            .HasDatabaseName("IX_WorkflowInstance_Status");

        builder.HasOne(x => x.Template)
            .WithMany(x => x.Instances)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubmitterPosition)
            .WithMany()
            .HasForeignKey(x => x.SubmitterPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubmitterEmployee)
            .WithMany()
            .HasForeignKey(x => x.SubmitterEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActingAsPosition)
            .WithMany()
            .HasForeignKey(x => x.ActingAsPositionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(x => x.PreApprovalStatus).HasConversion<int>();

        builder.HasOne(x => x.CreatedBySecretaryPosition)
            .WithMany()
            .HasForeignKey(x => x.CreatedBySecretaryPositionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.PreApprovalChiefPosition)
            .WithMany()
            .HasForeignKey(x => x.PreApprovalChiefPositionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
