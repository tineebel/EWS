using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class DivisionConfiguration : IEntityTypeConfiguration<Division>
{
    public void Configure(EntityTypeBuilder<Division> builder)
    {
        builder.ToTable("Divisions", "dbo");
        builder.HasKey(x => x.DivisionId);
        builder.Property(x => x.DivisionCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DivisionName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DivisionNameEn).HasMaxLength(200);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(x => x.DivisionCode).IsUnique();
    }
}
