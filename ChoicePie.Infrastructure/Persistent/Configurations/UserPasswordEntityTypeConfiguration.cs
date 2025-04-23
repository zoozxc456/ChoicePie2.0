using ChoicePie.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Infrastructure.Persistent.Configurations;

public class UserPasswordEntityTypeConfiguration : IEntityTypeConfiguration<UserPassword>
{
    public void Configure(EntityTypeBuilder<UserPassword> builder)
    {
        builder.ToTable("user_passwords");

        builder.HasKey(info => info.Id);

        builder.Property(info => info.Id)
            .HasColumnName("user_password_id")
            .ValueGeneratedOnAdd();

        builder.Property(info => info.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(info => info.HashedPassword)
            .HasColumnName("hashed_password")
            .HasMaxLength(44)
            .IsRequired();

        builder.Property(info => info.Salted)
            .HasColumnName("salted")
            .HasMaxLength(24)
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
    }
}