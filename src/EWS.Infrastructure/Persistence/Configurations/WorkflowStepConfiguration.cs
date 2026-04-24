using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("WorkflowSteps", "wf");
        builder.HasKey(x => x.StepId);

        builder.Property(x => x.StepName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ApproverType).HasConversion<int>();
        builder.Property(x => x.SpecificPositionCode).HasMaxLength(30);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => new { x.TemplateId, x.StepOrder })
            .IsUnique()
            .HasDatabaseName("IX_WorkflowStep_TemplateOrder");

        builder.HasOne(x => x.Template)
            .WithMany(x => x.Steps)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
