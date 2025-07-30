using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class BalanceEntityTypeConfiguration : IEntityTypeConfiguration<BalanceEntity>
{
    public void Configure(EntityTypeBuilder<BalanceEntity> builder)
    {
        builder.HasKey(b => b.Id);

        builder
            .HasOne(b => b.User)
            .WithOne(b => b.Balance)
            .HasForeignKey<UserEntity>(u => u.BalanceId)
            .IsRequired();
    }
}
