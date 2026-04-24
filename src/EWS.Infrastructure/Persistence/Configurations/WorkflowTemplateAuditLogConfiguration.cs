using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class WorkflowTemplateAuditLogConfiguration : IEntityTypeConfiguration<WorkflowTemplateAuditLog>
{
    public void Configure(EntityTypeBuilder<WorkflowTemplateAuditLog> builder)
    {
        builder.ToTable("WorkflowTemplateAuditLogs", "wf");
        builder.HasKey(x => x.AuditId);

        builder.Property(x => x.ChangeType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ChangedBy).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SnapshotJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.ChangeNote).HasMaxLength(500);

        builder.HasIndex(x => x.TemplateId);

        builder.HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
