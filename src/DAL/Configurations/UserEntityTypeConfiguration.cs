using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class UserEntityTypeConfigurations : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder
            .HasOne(u => u.Balance)
            .WithOne(u => u.User)
            .HasForeignKey<BalanceEntity>(b => b.UserId)
            .IsRequired();
    }
}
