using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", "dbo");
        builder.HasKey(x => x.EmployeeId);

        builder.Property(x => x.EmployeeCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.EmployeeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EmployeeNameEn).HasMaxLength(200);
        builder.Property(x => x.Nickname).HasMaxLength(100);
        builder.Property(x => x.Tel).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.ImagePath).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => x.EmployeeCode).IsUnique();
    }
}
