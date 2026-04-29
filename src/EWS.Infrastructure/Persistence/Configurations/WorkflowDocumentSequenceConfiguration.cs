using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class WorkflowDocumentSequenceConfiguration : IEntityTypeConfiguration<WorkflowDocumentSequence>
{
    public void Configure(EntityTypeBuilder<WorkflowDocumentSequence> builder)
    {
        builder.ToTable("WorkflowDocumentSequences", "wf");
        builder.HasKey(x => x.SequenceId);

        builder.Property(x => x.Prefix).HasMaxLength(30).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => new { x.Prefix, x.Year })
            .IsUnique()
            .HasDatabaseName("IX_WorkflowDocumentSequence_PrefixYear");
    }
}
