using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments", "dbo");
        builder.HasKey(x => x.DepartmentId);
        builder.Property(x => x.DeptCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DeptShortCode).HasMaxLength(20);
        builder.Property(x => x.DeptName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DeptNameEn).HasMaxLength(200);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(x => x.DeptCode).IsUnique();

        builder.HasOne(x => x.Division)
            .WithMany(x => x.Departments)
            .HasForeignKey(x => x.DivisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
