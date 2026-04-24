using EWS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWS.Infrastructure.Persistence.Configurations;

public class DelegationConfiguration : IEntityTypeConfiguration<Delegation>
{
    public void Configure(EntityTypeBuilder<Delegation> builder)
    {
        builder.ToTable("Delegations", "dbo");
        builder.HasKey(x => x.DelegationId);

        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => new { x.FromPositionId, x.IsActive, x.StartDate, x.EndDate })
            .HasDatabaseName("IX_Delegation_Active");

        builder.HasOne(x => x.FromPosition)
            .WithMany(x => x.DelegationsFrom)
            .HasForeignKey(x => x.FromPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToPosition)
            .WithMany(x => x.DelegationsTo)
            .HasForeignKey(x => x.ToPositionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
