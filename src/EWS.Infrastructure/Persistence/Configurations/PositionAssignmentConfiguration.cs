using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class PositionAssignmentConfiguration : IEntityTypeConfiguration<PositionAssignment>
{
    public void Configure(EntityTypeBuilder<PositionAssignment> builder)
    {
        builder.ToTable("PositionAssignments", "dbo");
        builder.HasKey(x => x.AssignmentId);

        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        // Index สำหรับการค้นหา Active Assignment
        // Active = IsActive=true AND StartDate <= NOW AND (EndDate IS NULL OR EndDate >= NOW)
        builder.HasIndex(x => new { x.PositionId, x.IsActive, x.StartDate, x.EndDate })
            .HasDatabaseName("IX_PositionAssignment_Active");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.PositionAssignments)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Position)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
