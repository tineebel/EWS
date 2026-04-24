using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class WorkflowHistoryConfiguration : IEntityTypeConfiguration<WorkflowHistory>
{
    public void Configure(EntityTypeBuilder<WorkflowHistory> builder)
    {
        builder.ToTable("WorkflowHistories", "wf");
        builder.HasKey(x => x.HistoryId);

        // BIGINT IDENTITY — รองรับ Volume สูง
        builder.Property(x => x.HistoryId)
            .UseIdentityColumn();

        builder.Property(x => x.EventType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        // DataSnapshot เก็บ JSON — ไม่จำกัดขนาด
        builder.Property(x => x.DataSnapshot).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.InstanceId, x.OccurredAt })
            .HasDatabaseName("IX_WorkflowHistory_Instance");

        builder.HasOne(x => x.Instance)
            .WithMany(x => x.Histories)
            .HasForeignKey(x => x.InstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActorPosition)
            .WithMany()
            .HasForeignKey(x => x.ActorPositionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ActorEmployee)
            .WithMany()
            .HasForeignKey(x => x.ActorEmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
