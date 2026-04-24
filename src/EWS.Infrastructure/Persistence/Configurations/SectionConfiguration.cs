using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections", "dbo");
        builder.HasKey(x => x.SectionId);
        builder.Property(x => x.SectCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SectName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SectNameEn).HasMaxLength(200);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(x => x.SectCode).IsUnique();

        builder.HasOne(x => x.Department)
            .WithMany(x => x.Sections)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
