using ChoicePie.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Infrastructure.Persistent.Configurations;

public class UserInfoEntityTypeConfiguration : IEntityTypeConfiguration<UserInfo>
{
    public void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.ToTable("user_infos");

        builder.HasKey(info => info.Id);

        builder.Property(info => info.Id)
            .HasColumnName("user_info_id")
            .ValueGeneratedOnAdd();

        builder.Property(info => info.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(info => info.Username)
            .HasColumnName("account")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(info => info.Email)
            .HasColumnName("email")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(info => info.Favorite)
            .HasColumnName("favorite")
            .HasMaxLength(20);

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