using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class WorkflowTemplateConfiguration : IEntityTypeConfiguration<WorkflowTemplate>
{
    public void Configure(EntityTypeBuilder<WorkflowTemplate> builder)
    {
        builder.ToTable("WorkflowTemplates", "wf");
        builder.HasKey(x => x.TemplateId);

        builder.Property(x => x.FlowDesc).HasMaxLength(300).IsRequired();
        builder.Property(x => x.WfScopeType).HasConversion<int>();
        builder.Property(x => x.Condition1).HasMaxLength(100);
        builder.Property(x => x.Condition2).HasMaxLength(100);
        builder.Property(x => x.Condition3).HasMaxLength(100);
        builder.Property(x => x.Condition4).HasMaxLength(100);
        builder.Property(x => x.Condition5).HasMaxLength(100);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        // doc_code + flow_code ต้อง unique
        builder.HasIndex(x => new { x.DocumentTypeId, x.FlowCode }).IsUnique();

        builder.HasOne(x => x.DocumentType)
            .WithMany(x => x.Templates)
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
