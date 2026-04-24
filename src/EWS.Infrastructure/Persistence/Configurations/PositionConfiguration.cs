using EWS.Domain.Entities;
using EWS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions", "dbo");
        builder.HasKey(x => x.PositionId);

        builder.Property(x => x.PositionCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PositionName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.PositionShortName).HasMaxLength(100);
        builder.Property(x => x.JobGrade).HasConversion<int>();
        builder.Property(x => x.WfScopeType).HasConversion<int>();
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => x.PositionCode).IsUnique();

        // Self-referencing hierarchy
        builder.HasOne(x => x.ParentPosition)
            .WithMany(x => x.SubordinatePositions)
            .HasForeignKey(x => x.ParentPositionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Secretary relationship (self-referencing)
        builder.HasOne(x => x.SecretaryPosition)
            .WithMany()
            .HasForeignKey(x => x.SecretaryPositionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Section)
            .WithMany(x => x.Positions)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
