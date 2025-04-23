using ChoicePie.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Infrastructure.Persistent.Configurations;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("user_id")
            .ValueGeneratedOnAdd();

        builder.Property(user => user.Account)
            .HasColumnName("account")
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(user => user.AuditInfo, optionBuilder =>
        {
            optionBuilder.Property(info => info.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            optionBuilder.Property(info => info.CreatedBy)
                .HasColumnName("created_by")
                .IsRequired();

            optionBuilder.Property(info => info.UpdatedAt)
                .HasColumnName("updated_at");

            optionBuilder.Property(info => info.UpdatedBy)
                .HasColumnName("updated_by");
        });

        builder.HasOne<UserInfo>(user => user.Info)
            .WithOne()
            .HasForeignKey<UserInfo>(info => info.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserPassword>(user => user.Password)
            .WithOne()
            .HasForeignKey<UserPassword>(password => password.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}